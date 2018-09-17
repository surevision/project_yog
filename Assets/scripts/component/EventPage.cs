using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

[Serializable]
public class EventPage : SpriteBase {

    /// <summary>
    /// 出现条件 变量
    /// </summary>
    public GameInterpreter.ActiveVariable[] activeVariables;

    /// <summary>
    /// 出现条件 开关
    /// </summary>
    public GameInterpreter.ActiveSwitch[] activeSwitches;

    /// <summary>
    /// 出现条件 独立开关
    /// </summary>
    public GameInterpreter.ActiveSelfSwitch[] activeSelfSwitches;

    /// <summary>
    /// 开始条件
    /// </summary>
    public GameInterpreter.TriggerTypes trigger = GameInterpreter.TriggerTypes.Confirm;

    // 行走图相关
    public string characterName = "";                                                           // 图片名
    public GameCharacterBase.DIRS direction = GameCharacterBase.DIRS.DOWN;                      // 方向
    public int pattern = 1;                                                                     // 图案
    public GameCharacterBase.PRIORITIES priorityType = GameCharacterBase.PRIORITIES.BELOW;      // 优先级类型
    public bool through = false;                // 穿透
    public int moveSpeed = 3;                   // 移动速度
    public int moveFrequency = 0;               // 移动频度
    public bool walkAnime = true;               // 步行动画
    public bool stepAnime = false;              // 踏步动画
    public bool directionFix = false;           // 固定朝向

    public Sprite BaseFrame = null;

    public EventCommand[] getCommands() {
        return this.transform.GetComponentsInChildren<EventCommand>();
    }

    public EventCommand getCommand(int commandIndex) {
        return this.getCommands()[commandIndex];
    }

}
