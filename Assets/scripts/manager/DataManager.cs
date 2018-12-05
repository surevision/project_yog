﻿using System;
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
            fs.Read(lenBytes, 0, 4);
            offset += lenBytes.Length;
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
            fs.Close();

            // 绑定新数据
            GameTemp.gameVariables = gameVariables;
            GameTemp.gameSwitches = gameSwitches;
            GameTemp.gameSelfSwitches = gameSelfSwitches;
            GameTemp.gameScreen = gameScreen;
            GameTemp.gameMessage = new GameMessage();
            GameTemp.gameMap = gameMap;
            GameTemp.gamePlayer = gamePlayer;

        } catch (Exception e) {
            Debug.Log(string.Format("load data {0} fail", fileName));
            Debug.Log(e.Message + e.StackTrace);
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

    public static void loadAndReStart(int saveNum) {
        loadGameData(string.Format("save{0}.sav", saveNum));
        ((SceneMap)SceneManager.Scene).loadMap(GameTemp.gameMap.mapName, true);
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