﻿using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;


[Serializable]
public class GameEvent : GameCharacterBase {

    /// <summary>
    /// 事件id，从1开始
    /// </summary>
    private int _eventId;
    public int eventId {
        get { return _eventId; }
        set { _eventId = value; }
    }

    private int _mapId;
    public int mapId {
        get { return _mapId; }
        set { _mapId = value; }
    }

    private bool _starting;
    public bool starting {
        get { return _starting; }
        set { _starting = value; }
    }
    private bool _erased;                                // 暂时消除标记
    public bool erased {
        get { return _erased; }
        set { _erased = value; }
    }
    private int _page = -1;                                   // 当前事件页
    public int page {
        get { return _page; }
        protected set { _page = value; }
    }

    private GameInterpreter.TriggerTypes _trigger;       // 记录当前的开始条件
    public GameInterpreter.TriggerTypes trigger {
        get { return _trigger; }
        set { _trigger = value; }
    }

    [NonSerialized]
    private List<EventCommand> _list;                    // 当前指令列表
    public List<EventCommand> list {
        get { return _list; }
        set { _list = value; }
    }

    private GameInterpreter interpreter;                // 并行事件用解释器

    public void setup(int id) {
        this.eventId = id;

        SpriteEvent sprite = getEventSprite();
        foreach (EventPage page in sprite.GetComponentsInChildren<EventPage>()) {
            page.gameObject.SetActive(true);
            page.transform.GetComponent<SpriteRenderer>().color = new Color(0, 0, 0, 0);
        }
        this.x = sprite.transform.position.x;
        this.y = sprite.transform.position.y;
        this.realX = this.x;
        this.realY = this.y;

        this.page = -1;
        this.list = null;
        this.erased = false;
    }

    /// <summary>
    /// 取事件页信息
    /// </summary>
    /// <param name="page"></param>
    /// <returns></returns>
    private EventPage getPageInfo(int page) {
        return this.getEventSprite().GetComponentsInChildren<EventPage>()[page];
    }

    /// <summary>
    /// 取指令列表
    /// </summary>
    /// <param name="page"></param>
    /// <returns></returns>
    private List<EventCommand> getCommands(int page) {
        List<EventCommand> result = new List<EventCommand>();
        foreach (EventCommand e in this.getPageInfo(page).GetComponentsInChildren<EventCommand>()) {
            if (e.gameObject.activeInHierarchy) {
                result.Add(e);
            }
        }
        return result;
    }

    /// <summary>
    /// 读档恢复list数据
    /// </summary>
    /// <param name="page"></param>
    public void loadCommands(int page) {
        this.list = this.getCommands(page);
    }

    /// <summary>
    /// 读档恢复本事件调用的公共事件数据
    /// </summary>
    public void loadInterpreterCommonEventList() {
        if (this.interpreter != null && this.interpreter.childInterpreter != null) {
            this.interpreter.childInterpreter.loadList(
                ((SceneMap)SceneManager.Scene).getCommonEventCmd(this.interpreter.childInterpreter.eventPageForList)
            );
        }
    }

    /// <summary>
    /// 检查条件满足
    /// </summary>
    /// <param name="page">事件页序号</param>
    /// <returns></returns>
    public bool isConditionMet(int page) {
        EventPage pageInfo = this.getPageInfo(page);
        // 检查开关
        for (int i = 0; i < pageInfo.activeSwitches.Length; i += 1) {
            int switchId = pageInfo.activeSwitches[i].index;
            if (!GameTemp.gameSwitches[switchId]) {
                return false;
            }
        }
        // 检查变量
        for (int i = 0; i < pageInfo.activeVariables.Length; i += 1) {
            int variableId = pageInfo.activeVariables[i].index;
            int variableValue = pageInfo.activeVariables[i].value;
            if (GameTemp.gameVariables[variableId] < variableValue) {
                return false;
            }
        }
        // 检查独立开关
        for (int i = 0; i < pageInfo.activeSelfSwitches.Length; i += 1) {
            GameInterpreter.SelfSwitchCode switchId = pageInfo.activeSelfSwitches[i].index;  // ABCD
            if (!GameTemp.gameSelfSwitches[GameSelfSwitches.key(this.mapId, this.eventId, switchId)]) {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// 启动事件页
    /// </summary>
    /// <param name="page"></param>
    public void setupPage(int page) {
        Debug.Log(string.Format("setupPage {0}", page));
        this.page = page;
        if (this.page == -1) {
            this.list = null;
            this.interpreter = null;
            this.characterName = "";    // 清理图像
        } else {
            // 显示控制
            EventPage[] pages = this.getEventSprite().GetComponentsInChildren<EventPage>();

            // 只显示本页对应精灵
            EventPage currPage = pages[page];
            this.characterName = currPage.characterName;
            this.direction = currPage.direction;
            this.pattern = currPage.pattern;
            this.priorityType = currPage.priorityType;
            this.through = currPage.through;
            this.moveSpeed = currPage.moveSpeed;
            this.moveFrequency = currPage.moveFrequency;
            this.walkAnime = currPage.walkAnime;
            this.stepAnime = currPage.stepAnime;
            this.directionFix = currPage.directionFix;
            this.list = this.getCommands(page);
            this.trigger = this.getPageInfo(page).trigger;
            if (this.trigger == GameInterpreter.TriggerTypes.Async) {
                // 并行处理：
                this.interpreter = new GameInterpreter(0, false);
            } else {
                this.interpreter = null;
            }
        }
    }

    /// <summary>
    /// 标记启动
    /// </summary>
    public void start() {
        this.starting = true;   // 标记本事件开始执行
        this.lockup();
        if (!GameTemp.gameMap.interpreter.isRunning()) {
			Debug.Log("setupStartingEvent");
            GameTemp.gameMap.interpreter.setupStartingEvent();  // 调用解释器执行
        }
    }
    
    public void clearStarting() {
        this.starting = false;
    }

    public void refresh() {
        int newPage = -1;
        if (!this.erased) {
            // 没有暂时消除的情况下
            EventPage[] pages = this.getEventSprite().GetComponentsInChildren<EventPage>();
            // 按页码从大到小遍历
            for (int i = pages.Length - 1; i >= 0; i -= 1) {
                if (this.isConditionMet(i)) {
                    newPage = i;
                    break;
                }
            }
        }
        if (this.page != newPage) {
            this.clearStarting();
            this.setupPage(newPage);    // 设置新启动页,-1时清除
        }
        this.checkTriggerAuto();    // 检查自动启动
    }

    private void checkTriggerAuto() {
        if (!this.isNullPage() && !this.erased && this.trigger == GameInterpreter.TriggerTypes.Auto) {
            this.start();
        }
    }

    /// <summary>
    /// 暂时消除
    /// </summary>
    public void erase() {
        this.erased = true;
        this.refresh();
    }

    /// <summary>
    /// 处于空指令页
    /// </summary>
    /// <returns></returns>
    public bool isNullPage() {
        return this.list == null;
    }

    /// <summary>
    /// 取事件对应的精灵节点
    /// </summary>
    /// <returns></returns>
    public SpriteEvent getEventSprite() {
        return ((SceneMap)SceneManager.Scene)
            .getMapNode()
            .transform
            .Find(GameMap.layers[(int)GameMap.Layers.LayerEvents])
            .GetComponentsInChildren<SpriteEvent>()[this.eventId - 1];
    }

    public override void update() {
        base.update();
        if (!this.erased) {
            // 检查自动执行
            this.checkTriggerAuto();
        } else {
        }
        if (this.interpreter != null) {
            // 检查并行执行
            if (!this.interpreter.isRunning()) {
                this.interpreter.setup(this.list, this.eventId, this.page);
            }
            this.interpreter.update();
        }
    }

    /// <summary>
    /// 初始化包围盒数据
    /// </summary>
    /// <param name="sprite"></param>
    public override void setupCollider(SpriteCharacter sprite) {
//        Debug.Log(sprite.GetComponent<SpriteRenderer>().bounds.size);
        // TODO 事件碰撞区域
        base.setupCollider(sprite);
        return;
        if (!sprite.GetComponent<SpriteRenderer>().bounds.size.Equals(Vector3.zero)) {
            List<Vector2> points = new List<Vector2>();
            Vector3 size = sprite.GetComponent<SpriteRenderer>().bounds.size;
            float resize = 0.01f;
            points.Add(new Vector2(resize, resize));
            points.Add(new Vector2(resize, size.y / 4 * 3 - resize));
            points.Add(new Vector2(size.x - resize, size.y / 4 * 3 - resize));
            points.Add(new Vector2(size.x - resize, resize));
            this.colliderPolygon = new Intersection.Polygon(points);
        } else {
            base.setupCollider(sprite);
        }
    }

    public float offsetScreenX() {
        return 0.5f * (Util.GRID_WIDTH / Util.PPU);
    }
    public float offsetScreenY() {
        return 0;
    }

    public override float screenX() {
        return this.realX + offsetScreenX();
    }
    public override float screenY() {
        return this.realY + offsetScreenY();
    }
    
}
