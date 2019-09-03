using System;
using System.Security.Cryptography;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

/// <summary>
/// 存读档管理类
/// </summary>
public class DataManager {
	// 游戏数据库相关
    public static List<Item> dataItems;	// 物品
    public static SystemData systemData;	// 系统设定
	public static void loadAllData() {
		XLua.LuaTable env = LuaManager.getSimpleEnvTable();

		// 物品数据
		dataItems = new List<Item>();
		object[] results = LuaManager.LuaEnv.DoString("return require \"data/items\"", string.Format("load_data_{0}", "item"), env);
		if (results != null && results.Length > 0) {
			XLua.LuaTable resultTable = ((XLua.LuaTable)results[0]);
			for (int i = 0; i < resultTable.Length; i += 1) {
				dataItems.Add(resultTable.Get<int, Item>(i + 1));	// 从1开始
			}
		}
        // 系统设定
        systemData = new SystemData();
        results = LuaManager.LuaEnv.DoString("return require \"data/system\"", string.Format("load_data_{0}", "system"), env);
        if (results != null && results.Length > 0) {
            systemData = (SystemData)results[0];
        }
		// TODO 测试
		Debug.Log(string.Format("load item finish, len {0}", dataItems.Count));
		foreach (Item item in dataItems) {
			Debug.Log(string.Format("load item {0}", item.name));
		}

	}
	// 写入一个对象
	private static int flushObjData(object obj, FileStream fs, int offset) {
        MemoryStream ms = new MemoryStream();
        int len = 0;
        // flush data
        IFormatter formater = new BinaryFormatter();
        // 处理Vector2
        SurrogateSelector ss = new SurrogateSelector();
        Vector2SerializationSurrogate v2ss = new Vector2SerializationSurrogate();
        ss.AddSurrogate(typeof(Vector2),
                        new StreamingContext(StreamingContextStates.All),
                        v2ss);
        formater.SurrogateSelector = ss;
        formater.Serialize(ms, obj);
        len = (int)ms.Length;
        // 写入长度
        fs.Write(BitConverter.GetBytes(len), 0, 4);
        fs.Write(ms.GetBuffer(), 0, (int)ms.Length); // 转入fileStream
        fs.Flush();
        ms.Close();
        return len;
    }

    // 读取一个对象
    private static object loadObjData(FileStream fs, int offset, int length) {
        Debug.Log(string.Format("load offset {0}, len {1}", offset, length));
        int len = 0;
        byte[] readData = new byte[length];
        fs.Seek(offset, SeekOrigin.Begin);
        len = fs.Read(readData, 0, length);
        MemoryStream ms = new MemoryStream(readData);
        ms.Seek(0, SeekOrigin.Begin);
        len = (int)ms.Length;
        Debug.Log(string.Format("mem len {0}", len));
        IFormatter formater = new BinaryFormatter();
        // 处理Vector2
        SurrogateSelector ss = new SurrogateSelector();
        Vector2SerializationSurrogate v2ss = new Vector2SerializationSurrogate();
        ss.AddSurrogate(typeof(Vector2),
                        new StreamingContext(StreamingContextStates.All),
                        v2ss);
        formater.SurrogateSelector = ss;
        object result = formater.Deserialize(ms);
        ms.Close();
        return result;
    }


    /// <summary>
    /// 序列化存档数据到文件
    /// </summary>
    /// <param name="fileName"></param>
    private static void dumpGameData(string fileName) {
        // 
        // header
        // HeaderData
        // 
        // GameVariables
        // GameSwitches
        // GameSelfSwitches
        // GameScreen
        // GameMap

        Debug.Log(string.Format("dump data to file {0}", fileName));

        FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate);  // 创建文件 

        // 序列化内容
        int offset = 0;
        // header
        int len = 0;
        fs.Write(BitConverter.GetBytes(len), offset, 4);
        offset += BitConverter.GetBytes(len).Length;
        // HeaderData
        Dictionary<string, string> headerData = new Dictionary<string, string>();
        headerData.Add("mapName", ((SceneMap)SceneManager.Scene).getMapNode().GetComponent<MapExInfo>().showName);
        headerData.Add("saveTime", DateTime.Now.ToString("yyyy-M-d HH:mm:ss"));
        offset += flushObjData(headerData, fs, offset);
        // GameVariables
        offset += flushObjData(GameTemp.gameVariables, fs, offset);
        // GameSwitches
        offset += flushObjData(GameTemp.gameSwitches, fs, offset);
        // GameSelfSwitches
        offset += flushObjData(GameTemp.gameSelfSwitches, fs, offset);
        // GameScreen
        offset += flushObjData(GameTemp.gameScreen, fs, offset);
        // GameMap
        offset += flushObjData(GameTemp.gameMap, fs, offset);
        // GamePlayer
		offset += flushObjData(GameTemp.gamePlayer, fs, offset);
		// GameParty
		offset += flushObjData(GameTemp.gameParty, fs, offset);
		// bgm
		offset += flushObjData(GameTemp.bgmName, fs, offset);
        fs.Close();
    }

    /// <summary>
    /// 读取存档头部数据
    /// </summary>
    /// <param name="fileName"></param>
    public static Dictionary<string, string> loadGameDataHeader(string fileName) {
        // 
        // header
        // 
        // GameVariables
        // GameSwitches
        // GameSelfSwitches
        // GameScreen
        // GameMap
        FileStream fs = null;
        try {
            fs = new FileStream(fileName, FileMode.Open);  // 读取文件 
            // 反序列化内容
            byte[] lenBytes = new byte[4];
            int len = 0;
            int offset = 0;
            // header
            fs.Read(lenBytes, 0, 4);
            offset += lenBytes.Length;
            // HeaderData
            fs.Read(lenBytes, 0, 4);
            offset += 4;
            len = BitConverter.ToInt32(lenBytes, 0);
            Dictionary<string, string> headerData = (Dictionary<string, string>)loadObjData(fs, offset, len);
            offset += len;
            fs.Close();
            return headerData;

        } catch (Exception e) {
            Debug.Log(string.Format("load data {0} fail", fileName));
            Debug.Log(e.Message + e.StackTrace);
            if (fs != null) {
                fs.Close();
            }
            return null;
        }
    }
    /// <summary>
    /// 读取存档数据，不处理异常
    /// </summary>
    /// <param name="fileName"></param>
    private static void loadGameData(string fileName) {
        // 
        // header
        // HeaderData
        // 
        // GameVariables
        // GameSwitches
        // GameSelfSwitches
        // GameScreen
        // GameMap
        FileStream fs = new FileStream(fileName, FileMode.Open);  // 读取文件 
        // 反序列化内容
        byte[] lenBytes = new byte[4];
        int len = 0;
        int offset = 0;
        // header
        fs.Read(lenBytes, 0, 4);
        offset += lenBytes.Length;
        // HeaderData
        fs.Read(lenBytes, 0, 4);
        offset += 4;
        len = BitConverter.ToInt32(lenBytes, 0);
        Dictionary<string, string> headerData = (Dictionary<string, string>)loadObjData(fs, offset, len);
        offset += len;
        // GameVariables
        fs.Read(lenBytes, 0, 4);
        offset += 4;
        len = BitConverter.ToInt32(lenBytes, 0);
        GameVariables gameVariables = (GameVariables)loadObjData(fs, offset, len);
        offset += len;
        // GameSwitches
        fs.Read(lenBytes, 0, 4);
        offset += 4;
        len = BitConverter.ToInt32(lenBytes, 0);
        GameSwitches gameSwitches = (GameSwitches)loadObjData(fs, offset, len);
        offset += len;
        // GameSelfSwitches
        fs.Read(lenBytes, 0, 4);
        offset += 4;
        len = BitConverter.ToInt32(lenBytes, 0);
        GameSelfSwitches gameSelfSwitches = (GameSelfSwitches)loadObjData(fs, offset, len);
        offset += len;
        // GameScreen
        fs.Read(lenBytes, 0, 4);
        offset += 4;
        len = BitConverter.ToInt32(lenBytes, 0);
        GameScreen gameScreen = (GameScreen)loadObjData(fs, offset, len);
        offset += len;
        // GameMap
        fs.Read(lenBytes, 0, 4);
        offset += 4;
        len = BitConverter.ToInt32(lenBytes, 0);
        GameMap gameMap = (GameMap)loadObjData(fs, offset, len);
        offset += len;
        // GamePlayer
        fs.Read(lenBytes, 0, 4);
        offset += 4;
        len = BitConverter.ToInt32(lenBytes, 0);
        GamePlayer gamePlayer = (GamePlayer)loadObjData(fs, offset, len);
		offset += len;
		// GameParty
		fs.Read(lenBytes, 0, 4);
		offset += 4;
		len = BitConverter.ToInt32(lenBytes, 0);
		GameParty gameParty = (GameParty)loadObjData(fs, offset, len);
		offset += len;
		// bgm
		fs.Read(lenBytes, 0, 4);
		offset += 4;
		len = BitConverter.ToInt32(lenBytes, 0);
		string bgmName = (string)loadObjData(fs, offset, len);
		offset += len;
        fs.Close();

        // 绑定新数据
        GameTemp.gameVariables = gameVariables;
        GameTemp.gameSwitches = gameSwitches;
        GameTemp.gameSelfSwitches = gameSelfSwitches;
        GameTemp.gameScreen = gameScreen;
        GameTemp.gameMessage = new GameMessage();
        GameTemp.gameChoice = new GameChoice();
        GameTemp.gameMap = gameMap;
        GameTemp.gamePlayer = gamePlayer;
        GameTemp.gameParty = gameParty;
		GameTemp.bgmName = bgmName;
    }

    /// <summary>
    /// 存档
    /// </summary>
    /// <param name="saveNum"></param>
    public static void save(int saveNum) {
        dumpGameData(string.Format("save{0}.sav", saveNum));
    }

    /// <summary>
    /// 读取数据
    /// </summary>
    /// <param name="saveNum">存档编号从1开始</param>
    public static bool load(int saveNum) {
        try {
            loadGameData(string.Format("save{0}.sav", saveNum));
            return true;
        } catch (Exception e) {
            Debug.Log(string.Format("failed to load save{0}.sav.", saveNum));
            Debug.Log(e.Message + e.StackTrace);
            return false;
        }
    }

    /// <summary>
    /// 读档并以存档数据开始游戏
    /// </summary>
    /// <returns></returns>
    /// <param name="saveNum">存档编号从1开始</param>
    public static bool loadAndReStart(int saveNum) {
        try {
            loadGameData(string.Format("save{0}.sav", saveNum));
			Debug.Log(string.Format("restore bgm {0}", GameTemp.bgmName));
            if (SceneManager.Scene.GetType() == typeof(SceneTitle)) {
                ((SceneTitle)SceneManager.Scene).startLoadedGame();
            } else {
                ((SceneMap)SceneManager.Scene).loadMap(GameTemp.gameMap.mapName, true);
            }
            return true;
        } catch (Exception e) {
            Debug.Log(string.Format("failed to load save{0}.sav.", saveNum));
            Debug.Log(e.Message + e.StackTrace);
            return false;
        }
    }
}

sealed class Vector2SerializationSurrogate : ISerializationSurrogate {

    // Method called to serialize a Vector2 object
    public void GetObjectData(System.Object obj,
                              SerializationInfo info, StreamingContext context) {

        Vector2 v1 = (Vector2)obj;
        info.AddValue("x", v1.x);
        info.AddValue("y", v1.y);
        Debug.Log(v1);
    }

    // Method called to deserialize a Vector2 object
    public System.Object SetObjectData(System.Object obj,
                                       SerializationInfo info, StreamingContext context,
                                       ISurrogateSelector selector) {

        Vector2 v1 = (Vector2)obj;
        v1.x = (float)info.GetValue("x", typeof(float));
        v1.y = (float)info.GetValue("y", typeof(float));
        obj = v1;
        return obj;   // Formatters ignore this return value //Seems to have been fixed!
    }
}
