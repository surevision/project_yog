using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SystemData {
    public string startMap;    // 起始地图
    public int startX;         // 起始坐标x
    public int startY;         // 起始坐标y
    public GameCharacterBase.DIRS startDir; // 起始方向
}
