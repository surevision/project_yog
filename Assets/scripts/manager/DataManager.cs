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
    private static string keyStr = "deadcafe";

    // 写入一个des加密对象
    private static int flushDesData(object obj, FileStream fs, int offset) {
        byte[] key = Encoding.ASCII.GetBytes(keyStr);
        DESCryptoServiceProvider provider = new DESCryptoServiceProvider(); // des工厂
        provider.Mode = CipherMode.ECB;//兼容其他语言的Des加密算法  
        provider.Padding = PaddingMode.Zeros;//自动补0
        MemoryStream tempMs = new MemoryStream();
        MemoryStream ms = new MemoryStream();   // 加密中间流
        CryptoStream cs = new CryptoStream(ms, provider.CreateDecryptor(key, key), CryptoStreamMode.Write);
        int len = 0;
        // flush
        IFormatter formater = new BinaryFormatter();
        formater.Serialize(tempMs, obj);
        cs.Write(tempMs.GetBuffer(), 0, (int)tempMs.Length);
        cs.FlushFinalBlock();
        len = (int)ms.Length;
        // 写入长度
        fs.Write(BitConverter.GetBytes(len), 0, 4);
        fs.Write(ms.ToArray(), 0, (int)ms.Length); // 转入fileStream
        fs.Flush();
        cs.Close();
        ms.Close();
        return len;
    }

    // 读取一个des加密对象
    private static object loadDesData(FileStream fs, int offset, int length) {
        byte[] key = Encoding.ASCII.GetBytes(keyStr);
        DESCryptoServiceProvider provider = new DESCryptoServiceProvider(); // des工厂
        provider.Mode = CipherMode.ECB;//兼容其他语言的Des加密算法  
        provider.Padding = PaddingMode.Zeros;//自动补0
        MemoryStream ms = new MemoryStream();   // 加密中间流
        CryptoStream cs = new CryptoStream(ms, provider.CreateDecryptor(key, key), CryptoStreamMode.Write);
        int len = 0;
        byte[] readData = new byte[length];
        len = fs.Read(readData, offset, length);
        cs.Write(readData, 0, length);
        cs.FlushFinalBlock();
        len = (int)ms.Length;
        IFormatter formater = new BinaryFormatter();
        object result = formater.Deserialize(cs);
        cs.Close();
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
        // 
        // GameVariables
        // GameSwitches
        // GameSelfSwitches
        // GameScreen
        // GameMap

        Debug.Log(string.Format("dump data to file {0}", fileName));

        FileStream fs = new FileStream(fileName, FileMode.Create);  // 创建文件 

        // 序列化内容
        int offset = 0;
        // header
        int len = 0;
        fs.Write(BitConverter.GetBytes(len), offset, 4);
        offset += BitConverter.GetBytes(len).Length;
        // GameVariables
        offset += flushDesData(GameTemp.gameVariables, fs, offset);
        // GameSwitches
        offset += flushDesData(GameTemp.gameSwitches, fs, offset);
        // GameSelfSwitches
        offset += flushDesData(GameTemp.gameSelfSwitches, fs, offset);
        // GameScreen
        offset += flushDesData(GameTemp.gameScreen, fs, offset);
        // GameMap
        offset += flushDesData(GameTemp.gameMap, fs, offset);
        fs.Close();
    }

    /// <summary>
    /// 读取存档数据
    /// </summary>
    /// <param name="fileName"></param>
    private static void loadGameData(string fileName) {
        // 
        // header
        // 
        // GameVariables
        // GameSwitches
        // GameSelfSwitches
        // GameScreen
        // GameMap

        try {
            FileStream fs = new FileStream(fileName, FileMode.Open);  // 读取文件 
            // 反序列化内容
            byte[] lenBytes = new byte[4];
            int len = 0;
            int offset = 0;
            // header
            fs.Read(lenBytes, offset, 4);
            offset += lenBytes.Length;
            // GameVariables
            fs.Read(lenBytes, offset, 4);
            offset += 4;
            len = BitConverter.ToInt32(lenBytes, 0);
            GameVariables gameVariables = (GameVariables)loadDesData(fs, offset, len);
            offset += len;
            // GameSwitches
            fs.Read(lenBytes, offset, 4);
            offset += 4;
            len = BitConverter.ToInt32(lenBytes, 0);
            GameSwitches gameSwitches = (GameSwitches)loadDesData(fs, offset, len);
            offset += len;
            // GameSelfSwitches
            fs.Read(lenBytes, offset, 4);
            offset += 4;
            len = BitConverter.ToInt32(lenBytes, 0);
            GameSelfSwitches gameSelfSwitches = (GameSelfSwitches)loadDesData(fs, offset, len);
            offset += len;
            // GameScreen
            fs.Read(lenBytes, offset, 4);
            offset += 4;
            len = BitConverter.ToInt32(lenBytes, 0);
            GameScreen gameScreen = (GameScreen)loadDesData(fs, offset, len);
            offset += len;
            // GameMap
            fs.Read(lenBytes, offset, 4);
            offset += 4;
            len = BitConverter.ToInt32(lenBytes, 0);
            GameMap gameMap = (GameMap)loadDesData(fs, offset, len);
            offset += len;
            fs.Close();
        } catch (Exception e) {
            Debug.Log(string.Format("load data {0} fail", fileName));
            Debug.Log(e.Message);
        }
    }

    /// <summary>
    /// 存档
    /// </summary>
    /// <param name="saveNum"></param>
    public static void save(int saveNum) {
        dumpGameData(string.Format("save{0}.sav", saveNum));
    }

    /// <summary>
    /// 读档
    /// </summary>
    /// <param name="saveNum"></param>
    public static void load(int saveNum) {
        loadGameData(string.Format("save{0}.sav", saveNum));
    }
}
