﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[XLua.LuaCallCSharp]
public class GameSelfSwtiches {

    public static string[] CodeNames = {
        "A",
        "B",
        "C",
        "D",
    };

    private Dictionary<string, bool> data;
    public GameSelfSwtiches() {
        this.data = new Dictionary<string, bool>();
    }

    public bool this[string index] {
        get {
            return this.get(index);
        }
        set { this.data.Add(index, value); }
    }

    public bool get(string index) {
        if (!this.data.ContainsKey(index) == null) {
            return false;
        }
        try {
            return this.data[index];
        } catch (System.Exception e) {
            Debug.Log(string.Format("get fail at index {0}", index));
            return false;
        }
    }

    /// <summary>
    /// 由字符ABCD构造一个对应的开关类型id
    /// </summary>
    /// <param name="codeName"></param>
    /// <returns></returns>
    public static GameInterpreter.SelfSwitchCode code(string codeName) {
        int index = 0;
        foreach(string name in CodeNames) {
            if (name.Equals(codeName)) {
                break;
            }
            index += 1;
        }
        return (GameInterpreter.SelfSwitchCode)index;
    }

    /// <summary>
    /// 构造一个key
    /// </summary>
    /// <param name="mapId"></param>
    /// <param name="eventId"></param>
    /// <param name="switchId"></param>
    /// <returns></returns>
    public static string key(int mapId, int eventId, GameInterpreter.SelfSwitchCode switchId) {
        return string.Format("{0},{1},{2}", mapId, eventId, switchId);
    }
}
