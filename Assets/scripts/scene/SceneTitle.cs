using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;

public class SceneTitle : SceneBase {
    [Serializable]
    public enum TransitionType {
        NONE = -1,
        BLACK = 1,
        WHITE = 2
    }

    // debug
    bool debug = true;
    private GameObject menuNode = null;         // 菜单
    private GameObject asyncUINode = null;         // 异步刷新的ui
	public Image transBlack;				// 黑屏渐变
	public Image transWhite;				// 白屏渐变

    // 菜单相关
    private UISetBase.UISetMessenger uiSetMessenger = null;
    private UISetBase uiSet = null;
    private List<AsyncUIBase> asyncUis = null;

    // 渐变相关
    private TransitionType _transitionType = TransitionType.BLACK;
    private int _transitionProgress = -1;
    private int _transitionDuration = -1;
    public float transitionProgress {
        get { return 1.0f * Mathf.Max(0, _transitionProgress) / _transitionDuration; }
    }
    public delegate void onTransitionFinish();  // 渐变完成回调
    public onTransitionFinish transitionFinishCallback;

	protected override void Start () {
        base.Start();
        sceneName = "Title";

		// 初始化数据库
		DataManager.loadAllData();

        // 初始化音频
        AudioManager.setup(
            GameObject.Find("BGM"),
            GameObject.Find("BGS"),
            GameObject.Find("SEs")
        );
        // 标记菜单节点
        this.menuNode = GameObject.Find("Menu");
        this.asyncUINode = GameObject.Find("AsyncUI");
        this.asyncUis = new List<AsyncUIBase>();
        this.uiSetMessenger = new UISetBase.UISetMessenger(this.menuNode, "");
        this.uiSetMessenger.setSwitchDelegate(this.switchUIDelegate);

        this.switchToUI("title");

    }

    public void startNewGame() {

        GameTemp.gameVariables = new GameVariables();
        GameTemp.gameSwitches = new GameSwitches();
        GameTemp.gameSelfSwitches = new GameSelfSwitches();
        GameTemp.gameParty = new GameParty();
        GameTemp.gameScreen = new GameScreen();
        GameTemp.gameMessage = new GameMessage();
        GameTemp.gameChoice = new GameChoice();
        GameTemp.gameMap = new GameMap();

        GameTemp.startMapName = DataManager.systemData.startMap;
        SceneManager.gotoScene("Map");
    }

    public void startLoadedGame() {
        GameTemp.startMapName = "";
        SceneManager.gotoScene("Map");
    }

    protected override void updateRender() {
        base.updateRender();

        if (this.isInTransition()) {
            // 刷新渐变
            if (this._transitionProgress > 0) {
                this._transitionProgress = Mathf.Max(this._transitionProgress - 1, 0);
                if (this._transitionProgress == 0) {
                    this._transitionProgress = -1;
                    if (this.transitionFinishCallback != null) {
                        transitionFinishCallback.Invoke();
                    }
                }
            }
			switch (this._transitionType) {
			case TransitionType.BLACK:
				this.transBlack.color = new Color (0, 0, 0, this.transitionProgress);
				break;
			case TransitionType.WHITE:
				this.transWhite.color = new Color (1, 1, 1, this.transitionProgress);
				break;
			}
        }

        if (this.isUIRunning()) {
            return;
        }

    }

    protected override void updateLogic() {
        base.updateLogic();

        // 刷新渐变
        if (this.isInTransition()) {
            return;
        }

        // 刷新异步ui
        foreach (AsyncUIBase asyncUi in this.asyncUis) {
            asyncUi.update();
        }
        this.asyncUis.RemoveAll(ui => ui.isDead());

        // 刷新菜单
        if (this.isUIRunning()) {
            this.uiSet.update();
            return;
        }

        if (InputManager.isTrigger(InputManager.GameKey.F5)) {
            GameTemp.gameScreen.toggleView();
        }
    }

    protected override void updateAfterFrame() {
        base.updateAfterFrame();
    }


    /// <summary>
    /// 展示异步ui
    /// </summary>
    /// <param name="ui"></param>
    public void showAsyncUi(AsyncUIBase ui) {
        this.asyncUis.Add(ui);
        ui.start();
    }
    /// <summary>
    /// 切换UI
    /// </summary>
    /// <param name="newUIName">新ui名</param>
    public void switchToUI(string newUIName, string args = "") {
        UISetBase origUI = this.uiSet;
        this.uiSetMessenger.switchToUI(newUIName, args);
        if (origUI != null) {
            origUI.terminate();
        }
    }
    /// <summary>
    /// 切换ui委托
    /// </summary>
    /// <param name="uiName">User interface name.</param>
    public void switchUIDelegate(string uiSetName, string args = "") {
        this.uiSetMessenger.uiSetName = uiSetName;
		if ("title".Equals(uiSetName)) {
			// 菜单
			this.uiSet = new UISetTitle(this.uiSetMessenger);
            this.uiSet.start();
        } else if ("load".Equals(uiSetName)) {
            // 读档
            this.uiSet = new UISetLoad(this.uiSetMessenger);
            this.uiSet.start();
        } else if ("save".Equals(uiSetName)) {
            // 存档
            this.uiSet = new UISetSave(this.uiSetMessenger);
            ((UISetSave)this.uiSet).fromMap = "map".Equals(args);
            this.uiSet.start();
		} else {
			// 关闭ui
			this.uiSet = null;
		}
    }

    /// <summary>
    /// 是否在处理菜单逻辑
    /// </summary>
    /// <returns></returns>
    public bool isUIRunning() {
        return this.uiSet != null;
    }

    /// <summary>
    /// 初始化渐变
    /// </summary>
    /// <param name="duration"></param>
    /// <param name="callback"></param>
	public void setupTransition(TransitionType transType, int duration, onTransitionFinish callback) {
		this._transitionType = transType;
        this._transitionDuration = duration;
        this._transitionProgress = this._transitionDuration;
		this.transitionFinishCallback = callback;
		// 初始化渐变节点显隐状态
		this.transBlack.color = new Color(0, 0, 0, 0);
		this.transWhite.color = new Color(1, 1, 1, 0);
    }

    /// <summary>
    /// 是否在执行渐变
    /// </summary>
    /// <returns></returns>
    public bool isInTransition() {
        return this._transitionProgress != -1;
    }


}
