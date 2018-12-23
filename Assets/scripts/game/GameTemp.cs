using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[XLua.LuaCallCSharp]
public class GameTemp {
    // 游戏数据

    public static GameVariables gameVariables;
    public static GameSwitches gameSwitches;
    public static GameSelfSwitches gameSelfSwitches;

    public static GameScreen gameScreen;

    public static GameMap gameMap;
    public static GamePlayer gamePlayer;
    public static GameParty gameParty;
    public static GameMessage gameMessage;

    // 运行时临时数据
    public static bool transforming;    // 场所移动中
    public static string startMapName = "";


}
