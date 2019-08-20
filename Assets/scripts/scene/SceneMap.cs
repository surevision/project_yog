using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;

public class SceneMap : SceneBase {
    // debug
    bool debug = true;

	[Serializable]
	public enum TransitionType {
		NONE = -1,
		SNAP = 0,
		BLACK = 1,
		WHITE = 2
	}

    public bool prepareFreeze = false;  // 标记截屏准备
    public string mapToLoad = "";  // 标记要加载地图

    // unity对象相关
    private GameObject currMapObj = null;   // 地图
    private GameObject player = null;       // 玩家
    private GameObject menuNode = null;         // 菜单
    private GameObject asyncUINode = null;         // 异步刷新的ui
    private GameObject commonEventNode = null;  // 公共事件
    public WindowMessage windowMessage;     // 文字显示窗口
    public WindowChoice windowChoice;       // 选择项窗口
    public Image snap;                      // 截屏
	public Image transBlack;				// 黑屏渐变
	public Image transWhite;				// 白屏渐变
    public Dictionary<int, Image> pictures = null;            // 图片

    // 菜单相关
    private UISetBase.UISetMessenger uiSetMessenger = null;
    private UISetBase uiSet = null;
    private List<AsyncUIBase> asyncUis = null;

    // 渐变相关
	private TransitionType _transitionType = TransitionType.SNAP;
    private int _transitionProgress = -1;
    private int _transitionDuration = -1;
    public float transitionProgress {
        get { return 1.0f * Mathf.Max(0, _transitionProgress) / _transitionDuration; }
    }
    public delegate void onTransitionFinish();  // 渐变完成回调
    public onTransitionFinish transitionFinishCallback;

	protected override void Start () {
        base.Start();
        sceneName = "Map";

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
        // 加载公共事件
        this.commonEventNode = Instantiate<GameObject>(Resources.Load<GameObject>(string.Format("prefabs/maps/CommonEventNode")));
        this.commonEventNode.transform.parent = GameObject.Find("CommonEvents").transform;

        if (GameTemp.startMapName != null && !"".Equals(GameTemp.startMapName)) {
            // 新游戏
            loadMap(GameTemp.startMapName);
        } else {
            // 读取游戏
            loadMap(GameTemp.gameMap.mapName, true);
        }

    }

    public void prepareLoadMap(string mapName) {
        this.mapToLoad = mapName;
    }

    public void loadMap(string mapName, bool isLoad = false) {
        // 加载地图prefab
        GameObject map = Instantiate<GameObject>(Resources.Load<GameObject>(string.Format("prefabs/maps/{0}", mapName)));
        CameraControl cameraControl = GameObject.Find("Main Camera").GetComponent<CameraControl>();
        cameraControl.player = null;
        map.name = mapName;
		GameObject.Destroy(this.player);
		GameObject.Destroy(this.currMapObj);
        map.transform.SetParent(GameObject.Find("Map").transform);
        map.transform.position = Vector3.zero;
		this.currMapObj = map;

		GameTemp.gameMap.setupMap(map, isLoad);

        // 创建玩家
        this.player = Instantiate<GameObject>(Resources.Load<GameObject>("prefabs/characters/players/Player"));
        this.player.transform.SetParent(map.transform.Find(GameMap.layers[(int)GameMap.Layers.LayerPlayer]));
        if (isLoad) {
            this.player.GetComponent<SpritePlayer>().setPlayer(GameTemp.gamePlayer);
			GameTemp.gamePlayer.loadInitSprite();
        } else {
            if (GameTemp.gamePlayer == null) {  // 起始地图
                // 根据prefab初始化角色
                GameTemp.gamePlayer = (GamePlayer)this.player.GetComponent<SpritePlayer>().character;
                GameTemp.gamePlayer.setCellPosition(new Vector2Int(DataManager.systemData.startX, DataManager.systemData.startY));
                GameTemp.gamePlayer.direction = DataManager.systemData.startDir;
                GameTemp.gameMap.showMapName = DataManager.systemData.showMapName;
				GameTemp.gamePlayer.setupCollider(this.player.GetComponent<SpritePlayer>());    // 玩家碰撞盒
				this.updateLogic();
				this.updateRender();
            } else {
				this.player.GetComponent<SpritePlayer>().setPlayer(GameTemp.gamePlayer);
            }
		}

		// 绑定摄像机到玩家
		cameraControl.player = this.player;		// 刷新视野
		cameraControl.gameObject.GetComponent<Camera>().orthographicSize = GameTemp.gameScreen.currView;
		// 初始化摄像机范围
		cameraControl.minTile = GameTemp.gameMap.mapInfo.minTileWorld;
		cameraControl.maxTile = GameTemp.gameMap.mapInfo.maxTileWorld;
		cameraControl.setupPos(cameraControl.minTile, cameraControl.maxTile);
		Debug.Log("force cameraControl.update();");
		cameraControl.update();

        // 初始化文章窗口
        windowMessage.gameObject.transform.localScale = new Vector3(1, 0, windowMessage.gameObject.transform.localScale.z);
        // 初始化选择窗口
        windowChoice.gameObject.SetActive(false);

        // 初始化显示图片
        if (this.pictures == null) {
			this.pictures = new Dictionary<int, Image>();
		}
		// 创建现有数据记录的图片
        foreach (GamePicture pic in GameTemp.gameScreen.pictures.Values) {
			this.attachPicImage(pic);
		}
        // 展示地图名
        if (!isLoad && GameTemp.gameMap.showMapName) {
            this.showMapName();
        }
    }

    protected override void updateRender() {
        base.updateRender();

        // 检测截屏
        if (this.prepareFreeze) {
            this.prepareFreeze = false;
			GameObject.Destroy(this.snap.sprite);
            Texture2D texture = snapScreen();
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            this.snap.sprite = sprite;
            return;
        }

		if (this.isInTransition()) {
			// 刷新渐变
			if (this._transitionProgress > 0) {
				this._transitionProgress = Mathf.Max(this._transitionProgress - 1, 0);
//				Debug.Log(string.Format("this._transitionProgress {0}", this._transitionProgress));
				if (this._transitionProgress == 0) {
					this._transitionProgress = -1;
					if (this.transitionFinishCallback != null) {
						transitionFinishCallback.Invoke();
					}
				}
			}
			switch (this._transitionType) {
			case TransitionType.SNAP:
				this.snap.color = new Color(1, 1, 1, this.transitionProgress);
				break;
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

        // 检测切换地图
        if (!"".Equals(this.mapToLoad)) {
            this.loadMap(this.mapToLoad);
            this.mapToLoad = "";
        }

        // 刷新角色
        this.player.GetComponent<SpritePlayer>().update();

        // 刷新事件
        foreach (GameEvent e in GameTemp.gameMap.events) {
            e.getEventSprite().update();
        }

		// 刷新图片
		foreach (GamePicture pic in GameTemp.gameScreen.pictures.Values) {
			Image img = this.pictures[pic.num];
			GameObject imgObj = img.gameObject;
			imgObj.GetComponent<RectTransform>().localPosition = new Vector3(pic.x, pic.y, pic.num); // 位置
			imgObj.transform.rotation = Quaternion.Euler(new Vector3(0, 0, pic.rotation));  // 旋转
			img.color = new Color(1, 1, 1, pic.opacity / 255.0f);   // 透明
			imgObj.transform.localScale = new Vector3(pic.zoomX, pic.zoomY, 1);	// 缩放
		}

        // 刷新视野
        GameObject.Find("Main Camera").GetComponent<Camera>().orthographicSize = GameTemp.gameScreen.currView;

        // 刷新摄像机
        GameObject.Find("Main Camera").GetComponent<CameraControl>().update();

        // 刷新对话
        windowMessage.update();
        // 刷新选择项
        windowChoice.update();

        if (debug) {
            Intersection.Polygon testPolygon = GameTemp.gamePlayer.currCollider();
            Vector2 start = testPolygon.points[0];
            foreach (Vector2 point in testPolygon.points) {
                Debug.DrawLine(new Vector3(start.x, start.y, this.player.transform.position.z),
					new Vector3(point.x, point.y, this.player.transform.position.z), Color.blue);
                start = point;
            }
            start = testPolygon.points[0];
            Vector2 end = testPolygon.points[testPolygon.length - 1];
            Debug.DrawLine(new Vector3(start.x, start.y, this.player.transform.position.z),
				new Vector3(end.x, end.y, this.player.transform.position.z), Color.blue);
			
			testPolygon = GameTemp.gamePlayer.currMidCollider();
			start = testPolygon.points[0];
			foreach (Vector2 point in testPolygon.points) {
				Debug.DrawLine(new Vector3(start.x, start.y, this.player.transform.position.z),
					new Vector3(point.x, point.y, this.player.transform.position.z), Color.yellow);
				start = point;
			}
			start = testPolygon.points[0];
			end = testPolygon.points[testPolygon.length - 1];
			Debug.DrawLine(new Vector3(start.x, start.y, this.player.transform.position.z),
				new Vector3(end.x, end.y, this.player.transform.position.z), Color.yellow);


            foreach (Intersection.Polygon polygon in GameTemp.gameMap.mapInfo.passageColliders) {
                start = polygon.points[0];
                foreach (Vector2 point in polygon.points) {
                    Debug.DrawLine(new Vector3(start.x, start.y, this.player.transform.position.z),
                        new Vector3(point.x, point.y, this.player.transform.position.z), Color.red);
                    start = point;
                }
                start = polygon.points[0];
                end = polygon.points[polygon.length - 1];
                Debug.DrawLine(new Vector3(start.x, start.y, this.player.transform.position.z),
                    new Vector3(end.x, end.y, this.player.transform.position.z), Color.red);

            }


			foreach (GameEvent e in GameTemp.gameMap.events) {
				Intersection.Polygon polygon = e.currCollider();
				start = polygon.points[0];
				foreach (Vector2 point in polygon.points) {
					Debug.DrawLine(new Vector3(start.x, start.y, this.player.transform.position.z),
						new Vector3(point.x, point.y, this.player.transform.position.z), Color.green);
					start = point;
				}
				start = polygon.points[0];
				end = polygon.points[polygon.length - 1];
				Debug.DrawLine(new Vector3(start.x, start.y, this.player.transform.position.z),
					new Vector3(end.x, end.y, this.player.transform.position.z), Color.green);
				
				polygon = e.currMidCollider();
				start = polygon.points[0];
				foreach (Vector2 point in polygon.points) {
					Debug.DrawLine(new Vector3(start.x, start.y, this.player.transform.position.z),
						new Vector3(point.x, point.y, this.player.transform.position.z), Color.green);
					start = point;
				}
				start = polygon.points[0];
				end = polygon.points[polygon.length - 1];
				Debug.DrawLine(new Vector3(start.x, start.y, this.player.transform.position.z),
					new Vector3(end.x, end.y, this.player.transform.position.z), Color.green);

            }
        }
    }

    protected override void updateLogic() {
        base.updateLogic();

        // 刷新渐变
		if (this.isInTransition()) {
			GameTemp.gameScreen.updateCurrPos();
			// 检测切换地图
			if (!"".Equals(this.mapToLoad)) {
				return;
			}
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

        GameTemp.gameScreen.update();
        GameTemp.gamePlayer.update();
        GameTemp.gameMap.update();
        if (InputManager.isTrigger(InputManager.GameKey.F5)) {
            GameTemp.gameScreen.toggleView();
        }

        if (debug) {
            if (InputManager.isTrigger(InputManager.GameKey.F9)) {
                this.uiSetMessenger.switchToUI("save");
            }
        }
    }

    protected override void updateAfterFrame() {
        base.updateAfterFrame();
    }

    public GameObject getMapNode() {
        return this.currMapObj;
    }

	public GameObject getPlayerNode() {
		return this.player;
	}

    /// <summary>
    /// 按id获得公共事件指令
    /// </summary>
    /// <returns></returns>
    /// <param name="id">事件索引，从0开始</param>
    public List<EventCommand> getCommonEventCmd(int id) {
        return new List<EventCommand>(this.commonEventNode.transform.GetChild(id).GetComponentsInChildren<EventCommand>());
    }

    /// <summary>
    /// 按事件名获得公共事件指令
    /// </summary>
    /// <returns></returns>
    /// <param name="name">事件名称</param>
    public List<EventCommand> getCommonEventCmd(string name, out int id) {
        int index = 0;
        for (int i = 0; i < this.commonEventNode.transform.childCount; i += 1) {
            if (this.commonEventNode.transform.GetChild(i).name.Equals(name)) {
                index = i;
                break;
            }
        }
        id = index;
        return new List<EventCommand>(this.commonEventNode.transform.GetChild(index).GetComponentsInChildren<EventCommand>());
    }

    /// <summary>
    /// 显示文章
    /// </summary>
    /// <param name="needOpen">If set to <c>true</c> need open.</param>
    /// <param name="needClose">If set to <c>true</c> need close.</param>
    public void startMessage(bool needOpen, bool needClose) {
        windowMessage.startMessage(needOpen, needClose);
    }

    /// <summary>
    /// 显示选择项
    /// </summary>
    /// <param name="needOpen">If set to <c>true</c> need open.</param>
    /// <param name="needClose">If set to <c>true</c> need close.</param>
    public void startChoice(bool needOpen, bool needClose) {
        windowChoice.gameObject.SetActive(true);
        windowChoice.startMessage(needOpen, needClose);
    }

	/// <summary>
	/// 显示图片
	/// </summary>
	/// <param name="picture"></param>
	public void attachPicImage(GamePicture picture) {
		Debug.Log(string.Format("attach pic {0}", picture.picName));
		GameObject picImage = new GameObject();
		Image img = picImage.AddComponent<Image>();
		Sprite sprite = Resources.Load<Sprite>(string.Format("graphics/pictures/{0}", picture.picName));
		img.sprite = sprite;
		Debug.Log(string.Format("pic size: {0}, {1}", sprite.textureRect.width, sprite.textureRect.height));
		img.GetComponent<RectTransform>().sizeDelta = new Vector2(sprite.textureRect.width, sprite.textureRect.height);
		img.transform.position = new Vector3(picture.x, picture.y, picture.num);
		img.color = new Color(
			1.0f,
			1.0f,
			1.0f,
			picture.opacity / 255.0f
			);
		img.transform.rotation = Quaternion.Euler(new Vector3(0, 0, picture.rotation));
		picImage.transform.SetParent(GameObject.Find("Pics").transform);
		if (this.pictures.ContainsKey(picture.num)) {
			// 已存在
			GameObject.Destroy(this.pictures[picture.num]);
            this.pictures.Remove(picture.num);
		}
		this.pictures.Add(picture.num, img);
	}

	/// <summary>
	/// 删除图片
	/// </summary>
	/// <param name="num"></param>
	public void erasePicImage(int num) {
		if (this.pictures.ContainsKey(num)) {
			// 已存在
			Debug.Log(string.Format("Destroy picImg {0}", num));
			GameObject.Destroy(this.pictures[num].gameObject);
			this.pictures.Remove(num);
		}
	}

    /// <summary>
    /// 显示地图名
    /// </summary>
    public void showMapName() {
        this.showAsyncUi(new AsyncUIMapName("MapName", this.asyncUINode));
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
		if ("menu".Equals(uiSetName)) {
			// 菜单
			this.uiSet = new UISetMenu(this.uiSetMessenger);
            this.uiSet.start();
        } else if ("item".Equals(uiSetName)) {
            // 物品
            this.uiSet = new UISetItem(this.uiSetMessenger);
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
		} else if ("gameover".Equals(uiSetName)) {
			// GameOver
			this.uiSet = new UIGameOver(this.uiSetMessenger);
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
		this.snap.color = new Color(1, 1, 1, 0);
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

    /// <summary>
    /// 屏幕截图
    /// </summary>
    public Texture2D snapScreen() {
        Camera camera = GameObject.Find("Main Camera").GetComponent<Camera>();
        Rect rect = new Rect(0, 0, Screen.width, Screen.height);
        RenderTexture rt = new RenderTexture((int)rect.width, (int)rect.height, 0);
        camera.targetTexture = rt;
        camera.Render();
        RenderTexture.active = rt;
        Texture2D screenShot = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.RGB24, false);
        screenShot.ReadPixels(rect, 0, 0);
        screenShot.Apply();
        camera.targetTexture = null;
        RenderTexture.active = null;
        return screenShot;
    }


    public void setupFreeze() {
        this.prepareFreeze = true;
    }

}
