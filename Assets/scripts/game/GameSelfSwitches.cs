﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[XLua.LuaCallCSharp]
[Serializable]
public class GameSelfSwitches {

    public static string[] CodeNames = {
        "",
        "A",
        "B",
        "C",
        "D",
    };

    private Dictionary<string, bool> data;
    public GameSelfSwitches() {
        this.data = new Dictionary<string, bool>();
    }

    public bool this[string index] {
        get {
            return this.get(index);
        }
        set {
            if (this.data.ContainsKey(index)) {
                this.data[index] = value;
            } else {
                this.data.Add(index, value);
            }
        }
    }

    public bool get(string index) {
        if (!this.data.ContainsKey(index)) {
            return false;
        }
        try {
            return this.data[index];
        } catch (System.Exception e) {
            Debug.Log(string.Format("get fail at index {0}", index));
            return false;
        }
    }

    public bool forceSet(string mapName, int eventId, string code, bool value) {
        string key = GameSelfSwitches.key(mapName, eventId, GameSelfSwitches.code(code));
        this[key] = value;
        return value;
    }

    /// <summary>
    /// 由字符ABCD构造一个对应的开关类型id
    /// </summary>
    /// <param name="codeName"></param>
    /// <returns></returns>
    public static GameInterpreter.SelfSwitchCode code(string codeName) {
        int index = 0;
        foreach(string name in CodeNames) {
            if (name.Trim().Equals(codeName)) {
                break;
            }
            index += 1;
        }
        return (GameInterpreter.SelfSwitchCode)index;
    }

    /// <summary>
    /// 构造一个key
    /// </summary>
    /// <param name="mapName"></param>
    /// <param name="eventId"></param>
    /// <param name="switchId"></param>
    /// <returns></returns>
    public static string key(string mapName, int eventId, GameInterpreter.SelfSwitchCode switchId) {
        return string.Format("{0},{1},{2}", mapName, eventId, (int)switchId);
    }

	public void debugPrint() {
		foreach (var pair in this.data) {
			Debug.Log(string.Format("key {0}, value {1}", pair.Key, pair.Value));
		}
	}
}
