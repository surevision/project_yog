using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapExInfo : MonoBehaviour {

    // 雾图形定义
    [Serializable]
    public class Fog {
        public string name;
        public int speedX;
        public int speedY;
    }
    // 远景图定义
    [Serializable]
    public class Parallax {
        [Serializable]
        public enum MoveType {
            Fixed = 0,  // 固定
            FollowMap = 1, // 跟随地图
            FollowMapHalf = 2,  // 1/2速度跟随地图卷动
            Auto = 3    // 自动循环卷动
        }
        public string fogName;
        public MoveType moveType;
        // 移动方式为自动时的速度
        public int speedX;
        public int speedY;
    }
    // 显示名称
    public string showName = "";
	// 雾图形
    public Fog[] fogs;
    // 远景图形
    public Parallax[] parallaxes;
    // BGM
    public string bgm = "";
    // BGS
    public string bgs = "";
    // 备注
    [MultilineAttribute]
    public string notes = "";
}
