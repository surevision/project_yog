using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[XLua.LuaCallCSharp]
public class GameTemp {
    public static GameVariables gameVariables;
    public static GameSwitches gameSwitches;
    public static GameSelfSwtiches gameSelfSwitches;

    public static GameScreen gameScreen;

    public static GameMap gameMap;
    public static GamePlayer gamePlayer;
    public static GameMessage gameMessage;

    public static GameVariables getVariables() {
        return gameVariables;
    }
}
