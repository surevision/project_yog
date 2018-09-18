using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[XLua.LuaCallCSharp]
public class GameSwitches {
    private Dictionary<int, bool> data;
    public GameSwitches() {
        this.data = new Dictionary<int, bool>();
    }

    public bool this[int index] {
        get {
            return this.get(index);
        }
        set { this.data.Add(index, value); }
    }

    public bool get(int index) {
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

}
