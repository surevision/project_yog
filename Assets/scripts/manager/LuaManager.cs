using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LuaManager {
    public static XLua.LuaEnv LuaEnv = new XLua.LuaEnv();
    public static void disposeLuaEnv() {
        LuaEnv.Dispose();
    }
}
