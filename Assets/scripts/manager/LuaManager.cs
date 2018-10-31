using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LuaManager {
    public static XLua.LuaEnv LuaEnv = new XLua.LuaEnv();
    public static void disposeLuaEnv() {
        LuaEnv.Dispose();
    }

    public static XLua.LuaTable getInterpreterEnvTable(GameInterpreter interpreter) {

        XLua.LuaTable scriptEnv = LuaManager.LuaEnv.NewTable();
        // 为每个脚本设置一个独立的环境，可一定程度上防止脚本间全局变量、函数冲突
        XLua.LuaTable meta = LuaManager.LuaEnv.NewTable();
        meta.Set("__index", LuaManager.LuaEnv.Global);
        scriptEnv.SetMetaTable(meta);
        meta.Dispose();

        scriptEnv.Set("self", interpreter);
        scriptEnv.Set("gameVariables", GameTemp.gameVariables);
        scriptEnv.Set("gameSwitches", GameTemp.gameSwitches);
        scriptEnv.Set("gameSelfSwitches", GameTemp.gameSelfSwitches);
        scriptEnv.Set("gamePlayer", GameTemp.gamePlayer);
        scriptEnv.Set("gameMap", GameTemp.gameMap);

        return scriptEnv;
    }
}
