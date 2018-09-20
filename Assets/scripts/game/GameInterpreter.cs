using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

/// <summary>
/// 事件解释器
/// </summary>
public class GameInterpreter {

    /// <summary>
    /// 独立开关枚举
    /// </summary>
    [Serializable]
    public enum SelfSwitchCode {
        A = 1,
        B = 2,
        C = 3,
        D = 4
    }

    /// <summary>
    /// 开始条件
    /// </summary>
    [Serializable]
    public enum TriggerTypes {
        Confirm = 0, // 确定键
        EnterCollide = 1, // 碰撞接触
        ExitCollide = 2, // 碰撞分离
        Auto = 3, // 自动执行
        Async = 4, // 并行处理
    }

    /// <summary>
    /// 指令类型
    /// </summary>
    [Serializable]
    public enum CommandTypes {
        // 1
        ShowArticle = 101,              // 显示文章
        Condition = 111,                // 条件分歧
        Loop = 112,                     // 循环开始
        Break = 113,                    // 中断循环
        CommandBreak = 115,             // 中断事件处理
        CommonEvent = 117,              // 公共事件
        Label = 118,                    // 跳转标志
        GotoLabel = 119,                // 跳转
        SetSwitch = 121,                // 开关操作
        SetVariable = 122,              // 变量操作
        SetSelfSwitch = 123,            // 独立开关操作
        Comment = 140,                  // 注释

        // 2
        Transformation = 201,           // 场所移动
        SetPos = 203,                   // 设置事件位置
        MoveByRoute = 205,              // 设置移动路线
        Erase = 214,                    // 暂时消除事件
        Shake = 225,                    // 画面震动
        Wait = 230,                     // 等待
        PlayBGM = 241,                  // 播放BGM
        FadeoutBGM = 242,               // 淡出BGM
        PlaySE = 250,                   // 播放SE
        StopSE = 251,                   // 停止SE

        // 3
        EvalScript = 355,               // 脚本

        // 4
        EndIf = 412,		            // 分歧结束
        EndLoop = 413,		            // 以上反复（循环）

    }

    /// <summary>
    /// 出现条件 开关
    /// </summary>
    [Serializable]
    public class ActiveSwitch {
        [SerializeField]
        public int index;
    }

    /// <summary>
    /// 出现条件：变量
    /// </summary>
    [Serializable]
    public class ActiveVariable {
        [SerializeField]
        public int index;
        [SerializeField]
        public int value;
    }

    /// <summary>
    /// 出现条件：独立开关
    /// </summary>
    [Serializable]
    public class ActiveSelfSwitch {
        [SerializeField]
        public SelfSwitchCode index;

        public ActiveSelfSwitch() { }

        public ActiveSelfSwitch(SelfSwitchCode index) {
            this.index = index;
        }
    }

    private int depth;
    private bool isMain;

    private int mapId = 0;  // 启动时的地图id
    public int origEventId = 0;	// 启动时的事件id
    public int eventId = 0;	// 事件id
    public List<EventCommand> list = null;	// 执行内容
    public int index = 0;	// 指令索引
    public int lastLoopIndex = 0;   // 上一个循环开始指令索引
    public Dictionary<string, int> gotoMarks = new Dictionary<string,int>(); //标签跳转记录
    public bool messageWaiting = false;	// 等待文章结束
    public GameCharacterBase movingCharacter = null;		// 等待移动结束
    public int waitCount = 0;	// 等待帧数
    public GameInterpreter childInterpreter = null;	// 子解释器（公共事件）

    public CommandTypes currentCode = 0;
    public string[] currentParam;

    public GameInterpreter(int depth, bool isMain) {
        this.depth = depth;
        this.isMain = isMain;
    }

    public void clear() {
        this.mapId = 0; // 启动时的地图id
        this.origEventId = 0;   // 启动时的事件id
        this.eventId = 0;   // 事件id
        this.list = null;   // 执行内容
        this.index = 0; // 当前指令索引
        this.lastLoopIndex = 0;   // 上一个循环开始指令索引
        this.gotoMarks = new Dictionary<string,int>(); //标签跳转索引记录
        this.messageWaiting = false;    // 等待文章结束
        this.movingCharacter = null;        // 等待移动结束
        this.waitCount = 0; // 等待帧数
        this.childInterpreter = null;   // 子解释器（公共事件）

        this.currentCode = 0;
    }

    public void setup(List<EventCommand> list, int eventId = 0) {
        this.clear();
        this.mapId = GameTemp.gameMap.mapInfo.mapId;
        this.origEventId = eventId;
        this.eventId = eventId;
        this.list = list;
    }
    public bool isRunning() {
        return this.list != null;
    }

    /// <summary>
    /// 根据编号取gamePlayer/gameEvent
    /// </summary>
    /// <param name="charId">char</param>
    /// <returns></returns>
    private GameCharacterBase getCharacter(int charId) {
        if (charId == -1) {
            // 本事件
            return GameTemp.gameMap.events[this.eventId];
        } else if (charId == 0) {
            // 玩家
            return GameTemp.gamePlayer;
        } else {
            // 指定事件
            return GameTemp.gameMap.events[charId];
        }
    }

    /// <summary>
    /// 结束执行
    /// </summary>
    private void commandEnd() {
        this.list = null;
        if (this.isMain && this.eventId > 0) {
            GameTemp.gameMap.events[this.eventId].unlock();
        }
    }

    /// <summary>
    /// 跳转到分歧结束
    /// </summary>
    public void commandConditionSkip() {
        while (this.list[this.index].code != CommandTypes.EndIf) {
            this.index += 1;
        }
    }
    /// <summary>
    /// 跳转到循环结束
    /// </summary>
    public void commandLoopSkip() {
        while (this.list[this.index].code != CommandTypes.EndLoop) {
            this.index += 1;
        }
    }

    public void update() {
        while (true) {  // while跳过无用指令
            if (GameTemp.gameMap.mapInfo.mapId != this.mapId) { // 地图id和启动地图有差异
                this.eventId = 0;
            }
            if (this.messageWaiting) {  // 文章显示中
                return;
            }
            if (this.movingCharacter != null) { // 等待移动
                if (this.movingCharacter.isMoving()) {  // 移动中
                    return;
                }
                this.movingCharacter = null;
            }
            if (this.waitCount > 0) {   // 等待n帧
                this.waitCount -= 1;
                return;
            }
            // 执行相关
            if (this.list == null) {
                // 尝试启动地图事件
                this.setupStartingEvent();
                if (this.list == null) {
                    // 没有可启动的事件
                    return;
                }
            }
            if (!this.executeCmd()) {   // 执行并前往下一个指令
                return;
            }
            this.index += 1;
        }
    }

    /// <summary>
    /// 启动事件
    /// </summary>
    public void setupStartingEvent() {
        if (GameTemp.gameMap.needRefresh) {
            GameTemp.gameMap.refresh();
        }
        foreach (GameEvent e in GameTemp.gameMap.events) {
            if (e.starting) {

                e.starting = false;
                this.setup(e.list, eventId);
                return;
            }
        }
    }

    /// <summary>
    /// 执行指令
    /// </summary>
    /// <returns></returns>
    public bool executeCmd() {
        if (this.index >= this.list.Count) {
            // 所有指令执行完成
            Debug.Log("commandEnd");
            this.commandEnd();
            return true;
        } else {
            EventCommand eventCmd = this.list[this.index];
            this.currentCode = eventCmd.code;
            this.currentParam = eventCmd.args;
            switch (eventCmd.code) {
                case CommandTypes.ShowArticle:  // 101 显示文章 
                    Debug.Log(string.Format("CommandTypes.ShowArticle", this.currentParam));
                    return this.command_showArticle();
                case CommandTypes.Condition:    // 111 条件分歧 
                    Debug.Log(string.Format("CommandTypes.Condition", this.currentParam));
                    this.command_condition();
                    return true;
                case CommandTypes.Loop:    // 112 循环开始
                    Debug.Log(string.Format("Loop.Condition", this.currentParam));
                    return this.command_loop();
                case CommandTypes.Break:    // 113 跳出循环
                    Debug.Log(string.Format("Loop.Condition", this.currentParam));
                    return this.command_break();
                case CommandTypes.CommandBreak: // 115 中断事件处理 
                    Debug.Log(string.Format("CommandTypes.CommandBreak", this.currentParam));
                    return true;
                case CommandTypes.CommonEvent:  // 117 公共事件 
                    Debug.Log(string.Format("CommandTypes.CommonEvent", this.currentParam));
                    return true;
                case CommandTypes.Label:  // 118 设置标签
                    Debug.Log(string.Format("CommandTypes.Label", this.currentParam));
                    return this.command_label();
                case CommandTypes.GotoLabel:  // 119 跳转到标签
                    Debug.Log(string.Format("CommandTypes.Label", this.currentParam));
                    return this.command_gotolabel();
                case CommandTypes.SetSwitch:    // 121 开关操作
                    Debug.Log(string.Format("CommandTypes.SetSwitch", this.currentParam));
                    return this.command_setSwitch();
                case CommandTypes.SetVariable:  // 122 变量操作
                    Debug.Log(string.Format("CommandTypes.SetVariable", this.currentParam));
                    return this.command_setVariable();
                case CommandTypes.SetSelfSwitch:    // 123 独立开关操作
                    Debug.Log(string.Format("CommandTypes.SetSelfSwitch", this.currentParam));
                    return this.command_setSelfSwitch();
                case CommandTypes.Comment:  // 140 注释 
                    Debug.Log(string.Format("CommandTypes.Comment", this.currentParam));
                    return true;
                case CommandTypes.Shake:    // 225 画面震动
                    Debug.Log(string.Format("CommandTypes.Shake", this.currentParam));
                    return this.command_shake();
                case CommandTypes.Wait: // 230 等待
                    Debug.Log(string.Format("CommandTypes.Wait", this.currentParam));
                    return this.command_wait();
                case CommandTypes.EvalScript:   // 355 脚本
                    Debug.Log(string.Format("CommandTypes.EvalScript", this.currentParam));
                    return this.command_evalScript();
                case CommandTypes.EndLoop:   // 413 以上反复
                    Debug.Log(string.Format("CommandTypes.EndLoop", this.currentParam));
                    return this.command_endLoop();
                default:
                    return true;
            }
        }
    }

    /// <summary>
    /// 101 显示文章
    /// text
    /// </summary>
    public bool command_showArticle() {
        Debug.Log(string.Format("command_showArticle"));
        if (!GameTemp.gameMessage.isBusy()) {
            string text = this.currentParam[0];
            GameTemp.gameMessage.text = text;
            this.messageWaiting = true;
            GameTemp.gameMessage.setFinishCallback(this.onMessageFinish);
            ((SceneMap)SceneManager.Scene).windowMessage.startMessage();
            this.index += 1;
        }
        return false;
    }

    public void onMessageFinish() {
        this.messageWaiting = false;
    }

    /// <summary>
    /// 111 条件分歧
    /// lua, 3 lua条件判定，跳过3条指令
    /// </summary>
    /// <returns></returns>
    public bool command_condition() {
        string condition = this.currentParam[0];

        Debug.Log(condition);

        XLua.LuaTable scriptEnv = LuaManager.LuaEnv.NewTable();
        // 为每个脚本设置一个独立的环境，可一定程度上防止脚本间全局变量、函数冲突
        XLua.LuaTable meta = LuaManager.LuaEnv.NewTable();
        meta.Set("__index", LuaManager.LuaEnv.Global);
        scriptEnv.SetMetaTable(meta);
        meta.Dispose();

        scriptEnv.Set("self", this);
        scriptEnv.Set("gameVariables", GameTemp.gameVariables);
        scriptEnv.Set("gameSwitches", GameTemp.gameSwitches);
        scriptEnv.Set("gameSelfSwitches", GameTemp.gameSelfSwitches);
        scriptEnv.Set("gamePlayer", GameTemp.gamePlayer);
        scriptEnv.Set("gameMap", GameTemp.gameMap);

        bool result = (bool)LuaManager.LuaEnv.DoString(condition, string.Format("event_condition_{0}", this.eventId), scriptEnv)[0];

        if (!result) {
            this.commandConditionSkip(); // 跳转到最近的end
        }
        return true;
    }

    /// <summary>
    /// 112 循环开始
    /// 循环无法嵌套
    /// </summary>
    /// <returns></returns>
    public bool command_loop() {
        this.lastLoopIndex = this.index;
        return true;
    }

    /// <summary>
    /// 113 跳出循环
    /// 循环无法嵌套
    /// </summary>
    /// <returns></returns>
    public bool command_break() {
        this.commandLoopSkip();
        return true;
    }

    /// <summary>
    /// 118 设置标签
    /// </summary>
    /// <returns></returns>
    public bool command_label() {
        string labelName = this.currentParam[0];
        this.gotoMarks.Add(labelName, this.index);
        return true;
    }

    /// <summary>
    /// 119 跳转到标签
    /// </summary>
    /// <returns></returns>
    public bool command_gotolabel() {
        string labelName = this.currentParam[0];
        if (this.gotoMarks.ContainsKey(labelName)) {
            this.index = this.gotoMarks[labelName];
        }
        return true;
    }


    /// <summary>
    /// 121 开关操作
    /// id,true/false
    /// </summary>
    /// <returns></returns>
    public bool command_setSwitch() {
        Debug.Log(string.Format("command_setSwitch"));
        int switchId = int.Parse(this.currentParam[0]);
        bool value = bool.Parse(this.currentParam[1]);
        GameTemp.gameSwitches[switchId] = value;
        GameTemp.gameMap.needRefresh = true;
        return true;
    }

    /// <summary>
    /// 122 变量操作
    /// 5,=,2
    /// 5,=,v2
    /// </summary>
    /// <returns></returns>
    public bool command_setVariable() {
        Debug.Log(string.Format("command_setVariable"));
        int variableId = int.Parse(this.currentParam[0]);
        string opa = this.currentParam[1];
        int value = 0;
        if (this.currentParam[2].StartsWith("v")) {
            value = GameTemp.gameVariables[int.Parse(this.currentParam[2].Substring(1))];
        } else {
            int.Parse(this.currentParam[2]);
        }
        if ("=".Equals(opa)) {
            GameTemp.gameVariables[variableId] = value;
        } else if ("+=".Equals(opa)) {
            GameTemp.gameVariables[variableId] += value;
        } else if ("-=".Equals(opa)) {
            GameTemp.gameVariables[variableId] -= value;
        } else if ("*=".Equals(opa)) {
            GameTemp.gameVariables[variableId] *= value;
        } else if ("/=".Equals(opa)) {
            GameTemp.gameVariables[variableId] /= value;
        } else {
        }
        GameTemp.gameMap.needRefresh = true;
        return true;
    }

    /// <summary>
    /// 123 独立开关操作
    /// A,true
    /// </summary>
    /// <returns></returns>
    public bool command_setSelfSwitch() {
        Debug.Log(string.Format("command_setSelfSwitch"));
        string code = this.currentParam[0].ToUpper();
        bool value = bool.Parse(this.currentParam[1]);

        string key = GameSelfSwtiches.key(this.mapId, this.eventId, GameSelfSwtiches.code(code));
        GameTemp.gameSelfSwitches[key] = value;
        GameTemp.gameMap.needRefresh = true;
        return true;
    }

    /// <summary>
    /// 225 画面震动
    /// </summary>
    /// <returns></returns>
    public bool command_shake() {
        return true;
    }

    /// <summary>
    ///  230 等待
    ///  帧数
    /// </summary>
    public bool command_wait() {
        this.waitCount = int.Parse(this.currentParam[0]);
        return true;
    }


    /// <summary>
    ///  355 eval
    ///  lua脚本
    /// </summary>
    public bool command_evalScript() {
        string src = this.currentParam[0];

        Debug.Log(src);

        XLua.LuaTable scriptEnv = LuaManager.LuaEnv.NewTable();
        // 为每个脚本设置一个独立的环境，可一定程度上防止脚本间全局变量、函数冲突
        XLua.LuaTable meta = LuaManager.LuaEnv.NewTable();
        meta.Set("__index", LuaManager.LuaEnv.Global);
        scriptEnv.SetMetaTable(meta);
        meta.Dispose();

        scriptEnv.Set("self", this);
        scriptEnv.Set("gameVariables", GameTemp.gameVariables);
        scriptEnv.Set("gameSwitches", GameTemp.gameSwitches);
        scriptEnv.Set("gameSelfSwitches", GameTemp.gameSelfSwitches);
        scriptEnv.Set("gamePlayer", GameTemp.gamePlayer);
        scriptEnv.Set("gameMap", GameTemp.gameMap);

        LuaManager.LuaEnv.DoString(src, string.Format("event_eval_{0}", this.eventId), scriptEnv);

        return true;
    }


    /// <summary>
    ///  413 以上反复
    ///  循环无法嵌套
    /// </summary>
    public bool command_endLoop() {
        // 跳转到上次的loop位置
        this.index = this.lastLoopIndex;
        return true;
    }

}
