using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[XLua.LuaCallCSharp]
public class GameTemp {
    // 游戏数据

    public static GameVariables gameVariables = null;
    public static GameSwitches gameSwitches = null;
    public static GameSelfSwitches gameSelfSwitches = null;

    public static GameScreen gameScreen = null;

    public static GameMap gameMap = null;
    public static GamePlayer gamePlayer = null;
    public static GameParty gameParty = null;
    public static GameMessage gameMessage = null;
    public static GameChoice gameChoice = null;

    // 运行时临时数据
    public static bool transforming = false;    // 场所移动中
    public static string startMapName = "";


}
