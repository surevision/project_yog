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
            return this.data[index];
        }
        set { this.data[index] = value; }
    }
}
