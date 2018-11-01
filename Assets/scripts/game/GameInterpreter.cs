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
        MoveByRoute = 209,              // 设置移动路线
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

    /// <summary>
    /// 移动路径指令
    /// </summary>
    [Serializable]
    [XLua.LuaCallCSharp]
    public class MoveRoute {
        public enum Cmd {
            ROUTE_END               = 0, // 移动路径的终点
            ROUTE_MOVE_DOWN         = 1, // 向下移动
            ROUTE_MOVE_LEFT         = 2, // 向左移动
            ROUTE_MOVE_RIGHT        = 3, // 向右移动
            ROUTE_MOVE_UP           = 4, // 向上移动
            ROUTE_MOVE_LOWER_L      = 5, // 左下移动
            ROUTE_MOVE_LOWER_R      = 6, // 右下移动
            ROUTE_MOVE_UPPER_L      = 7, // 坐上移动
            ROUTE_MOVE_UPPER_R      = 8, // 右上移动
            ROUTE_MOVE_RANDOM       = 9, // 随机移动
            ROUTE_MOVE_TOWARD       = 10, // 接近玩家
            ROUTE_MOVE_AWAY         = 11, // 远离玩家
            ROUTE_MOVE_FORWARD      = 12, // 前进一步
            ROUTE_MOVE_BACKWARD     = 13, // 后退一步
            ROUTE_JUMP              = 14, // 跳跃
            ROUTE_WAIT              = 15, // 等待
            ROUTE_TURN_DOWN         = 16, // 脸朝向下
            ROUTE_TURN_LEFT         = 17, // 脸朝向左
            ROUTE_TURN_RIGHT        = 18, // 脸朝向右
            ROUTE_TURN_UP           = 19, // 脸朝向上
            ROUTE_TURN_90D_R        = 20, // 右转 90 度
            ROUTE_TURN_90D_L        = 21, // 左转 90 度
            ROUTE_TURN_180D         = 22, // 后转180 度
            ROUTE_TURN_90D_R_L      = 23, // 随机向左右转
            ROUTE_TURN_RANDOM       = 24, // 随机转换方向
            ROUTE_TURN_TOWARD       = 25, // 朝向玩家
            ROUTE_TURN_AWAY         = 26, // 背向玩家
            ROUTE_SWITCH_ON         = 27, // 开启开关
            ROUTE_SWITCH_OFF        = 28, // 关闭开关
            ROUTE_CHANGE_SPEED      = 29, // 更改移动速度
            ROUTE_CHANGE_FREQ       = 30, // 更改移动频度
            ROUTE_WALK_ANIME_ON     = 31, // 开启步行动画
            ROUTE_WALK_ANIME_OFF    = 32, // 关闭步行动画
            ROUTE_STEP_ANIME_ON     = 33, // 开启踏步动画
            ROUTE_STEP_ANIME_OFF    = 34, // 关闭踏步动画
            ROUTE_DIR_FIX_ON        = 35, // 开启固定朝向
            ROUTE_DIR_FIX_OFF       = 36, // 关闭固定朝向
            ROUTE_THROUGH_ON        = 37, // 开启穿透
            ROUTE_THROUGH_OFF       = 38, // 关闭穿透
            ROUTE_TRANSPARENT_ON    = 39, // 开启透明化
            ROUTE_TRANSPARENT_OFF   = 40, // 关闭透明化
            ROUTE_CHANGE_GRAPHIC    = 41, // 更改图像
            ROUTE_CHANGE_OPACITY    = 42, // 更改不透明度
            ROUTE_CHANGE_BLENDING   = 43, // 更改合成方式
            ROUTE_PLAY_SE           = 44, // 播放声效
            ROUTE_SCRIPT            = 45, // 脚本
        }

        [SerializeField]
        public Cmd code;

        [SerializeField]
        public List<string> args;

        public MoveRoute() { }

        public MoveRoute(int code) {
            this.code = (Cmd)code;
            this.args = null;
        }

        public MoveRoute(int code, string args) {
            this.code = (Cmd)code;
            this.args = new List<string>();
            this.args.Add(args);
        }

        public MoveRoute(int code, XLua.LuaTable argsTable) {
            this.code = (Cmd)code;
            this.args = new List<string>();
            for (int i = 0; i < argsTable.Length; i += 1) {
                this.args.Add(argsTable.Get<string>(i + 1));
            }
        }

        public override string ToString() {
            return base.ToString() + " code: " + this.code + " args: " + this.args;
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
    public GameCharacterBase getCharacter(int charId) {
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
    /// 场所移动的渐变结束回调
    /// </summary>
    private void onTransformFinish() {
        GameTemp.transforming = false;
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
                //Debug.Log(string.Format("this.movingCharacter.isForcedMoving() {0}", this.movingCharacter.isForcedMoving()));
                if (this.movingCharacter.isForcedMoving()) {  // 移动中
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
            } else {
                if (!this.executeCmd()) {   // 执行并前往下一个指令
                    return;
                }
                this.index += 1;
            }
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
                this.setup(e.list, e.eventId);
                e.starting = false;
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
                    return this.command_condition();
                case CommandTypes.Loop:    // 112 循环开始
                    Debug.Log(string.Format("CommandTypes.Loop", this.currentParam));
                    return this.command_loop();
                case CommandTypes.Break:    // 113 跳出循环
                    Debug.Log(string.Format("CommandTypes.Break", this.currentParam));
                    return this.command_break();
                case CommandTypes.CommandBreak: // 115 中断事件处理 
                    Debug.Log(string.Format("CommandTypes.CommandBreak", this.currentParam));
                    return this.command_break_event();
                case CommandTypes.CommonEvent:  // 117 公共事件 
                    Debug.Log(string.Format("CommandTypes.CommonEvent", this.currentParam));
                    return true;
                case CommandTypes.Label:  // 118 设置标签
                    Debug.Log(string.Format("CommandTypes.Label", this.currentParam));
                    return this.command_label();
                case CommandTypes.GotoLabel:  // 119 跳转到标签
                    Debug.Log(string.Format("CommandTypes.GotoLabel", this.currentParam));
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
                case CommandTypes.Transformation:   // 201 场所移动
                    Debug.Log(string.Format("CommandTypes.Transformation", this.currentParam));
                    return this.command_transformation();
                case CommandTypes.MoveByRoute:   // 209 设置移动路线
                    Debug.Log(string.Format("CommandTypes.MoveByRoute", this.currentParam));
                    return this.command_moveByRoute();
                case CommandTypes.Erase:   // 216 暂时消除事件
                    Debug.Log(string.Format("CommandTypes.Erase", this.currentParam));
                    return this.command_erase();
                case CommandTypes.Shake:    // 225 画面震动
                    Debug.Log(string.Format("CommandTypes.Shake", this.currentParam));
                    return this.command_shake();
                case CommandTypes.Wait: // 230 等待
                    Debug.Log(string.Format("CommandTypes.Wait", this.currentParam));
                    return this.command_wait();
                case CommandTypes.PlayBGM: // 241 播放bgm
                    Debug.Log(string.Format("CommandTypes.PlayBGM", this.currentParam));
                    return this.command_play_bgm();
                case CommandTypes.PlaySE: // 250 播放se
                    Debug.Log(string.Format("CommandTypes.PlaySE", this.currentParam));
                    return this.command_play_se();
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
            return true;
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

        XLua.LuaTable scriptEnv = LuaManager.getInterpreterEnvTable(this);

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
    /// 115 中断事件处理
    /// </summary>
    /// <returns></returns>
    public bool command_break_event() {
        this.index = this.list.Count;
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
        string opa = this.currentParam[1].Trim();
        int value = 0;
        if (this.currentParam[2].StartsWith("v")) {
            value = GameTemp.gameVariables[int.Parse(this.currentParam[2].Substring(1))];
        } else {
            value = int.Parse(this.currentParam[2]);
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
        string code = this.currentParam[0].Trim().ToUpper();
        bool value = bool.Parse(this.currentParam[1]);

        string key = GameSelfSwtiches.key(this.mapId, this.eventId, GameSelfSwtiches.code(code));
        GameTemp.gameSelfSwitches[key] = value;
        GameTemp.gameMap.needRefresh = true;
        return true;
    }

    /// <summary>
    /// 201 场所移动
    /// mapname
    /// x
    /// y
    /// dir 不填不改变朝向
    /// 渐变过渡帧数，可不填
    /// </summary>
    /// <returns></returns>
    public bool command_transformation() {
        GameTemp.transforming = true;
        Vector2Int newPos = new Vector2Int(int.Parse(this.currentParam[1].Trim()), int.Parse(this.currentParam[2].Trim()));
        GameTemp.gamePlayer.setCellPosition(newPos);
        if (this.currentParam.Length >= 4 && (!"".Equals(this.currentParam[3]))) {
            GameTemp.gamePlayer.direction = (GameCharacterBase.DIRS)int.Parse(this.currentParam[3]);
        }
        if (this.currentParam.Length >= 5 && (!"".Equals(this.currentParam[4]))) {
            ((SceneMap)SceneManager.Scene).setupFreeze();
            ((SceneMap)SceneManager.Scene).setupTransition(int.Parse(this.currentParam[4]), onTransformFinish);
        } else {
            GameTemp.transforming = false;  // 立即判定结束移动
        }
        ((SceneMap)SceneManager.Scene).prepareLoadMap(this.currentParam[0]);
        return true;
    }

    /// <summary>
    /// 209 设置移动路线
    /// lua移动指令
    /// 移动对象
    /// 是否等待结束
    /// </summary>
    /// <returns></returns>
    public bool command_moveByRoute() {

        XLua.LuaTable scriptEnv = LuaManager.getInterpreterEnvTable(this);

        object[] result = LuaManager.LuaEnv.DoString(this.currentParam[0], string.Format("command_move_by_route_{0}", this.eventId), scriptEnv);

        // 移动对象
        GameCharacterBase character = this.getCharacter(int.Parse(this.currentParam[1]));

        // 是否等待移动结束
        if (bool.Parse(this.currentParam[2])) {
            this.movingCharacter = character;
        }

        // 处理移动指令
        List<MoveRoute> list = new List<MoveRoute>();
        for (int i = 0; i < ((XLua.LuaTable)result[0]).Length; i += 1) {
            list.Add(((XLua.LuaTable)result[0]).Get<MoveRoute>(i + 1));
            Debug.Log(list[list.Count - 1]);
        }

        character.setMoveRoute(list);

        return true;
    }

    /// <summary>
    /// 216 暂时消除事件
    /// </summary>
    /// <returns></returns>
    public bool command_erase() {
        ((GameEvent)this.getCharacter(-1)).erase();
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
    ///  241 播放bgm
    ///  文件名
    /// </summary>
    public bool command_play_bgm() {
        AudioManager.PlayBGM(this.currentParam[0]);
        return true;
    }

    /// <summary>
    ///  250 播放se
    ///  文件名
    /// </summary>
    public bool command_play_se() {
        AudioManager.PlaySE(this.currentParam[0]);
        return true;
    }

    /// <summary>
    ///  355 eval
    ///  lua脚本
    /// </summary>
    public bool command_evalScript() {
        string src = this.currentParam[0];

        Debug.Log(src);

        XLua.LuaTable scriptEnv = LuaManager.getInterpreterEnvTable(this);

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
