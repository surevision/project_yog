using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;

public class SceneMap : SceneBase {

    public bool prepareFreeze = false;  // 标记截屏准备
    public string mapToLoad = "";  // 标记要加载地图

    // unity对象相关
    private GameObject currMapObj = null;   // 地图
    private GameObject player = null;       // 玩家
    public WindowMessage windowMessage;     // 文字显示窗口
    public Image snap;                      // 截屏
    public Dictionary<int, Image> pictures = null;            // 图片

    // 渐变相关
    private int _transitionProgress = -1;
    private int _transitionDuration = -1;
    public float transitionProgress {
        get { return 1.0f * Mathf.Max(0, _transitionProgress) / _transitionDuration; }
    }
    public delegate void onTransitionFinish();  // 渐变完成回调
    public onTransitionFinish transitionFinishCallback;

	void Start () {
        base.Start();
        sceneName = "Map";

        // 初始化音频
        AudioManager.setup(
            GameObject.Find("BGM"),
            GameObject.Find("BGS"),
            GameObject.Find("SEs")
        );

        GameTemp.gameVariables = new GameVariables();
        GameTemp.gameSwitches = new GameSwitches();
        GameTemp.gameSelfSwitches = new GameSelfSwtiches();
        GameTemp.gameScreen = new GameScreen();
        GameTemp.gameMessage = new GameMessage();
        GameTemp.gameMap = new GameMap();

        loadMap("Map1");

    }

    public void prepareLoadMap(string mapName) {
        this.mapToLoad = mapName;
    }

    private void loadMap(string mapName) {
        // 加载地图prefab
        GameObject map = Instantiate<GameObject>(Resources.Load<GameObject>(string.Format("prefabs/maps/{0}", mapName)));
        CameraControl cameraControl = GameObject.Find("Main Camera").GetComponent<CameraControl>();
        cameraControl.target = null;
        cameraControl.player = null;
        map.name = mapName;
		GameObject.Destroy(this.player);
		GameObject.Destroy(this.currMapObj);
        map.transform.SetParent(GameObject.Find("Map").transform);
        map.transform.position = Vector3.zero;
        this.currMapObj = map;

        GameTemp.gameMap.setupMap(map);

        // 创建玩家
        this.player = Instantiate<GameObject>(Resources.Load<GameObject>("prefabs/characters/players/Player"));
        this.player.transform.SetParent(map.transform.Find(GameMap.layers[(int)GameMap.Layers.LayerPlayer]));
        if (GameTemp.gamePlayer == null) {
            // 根据prefab初始化角色
            GameTemp.gamePlayer = (GamePlayer)this.player.GetComponent<SpritePlayer>().character;
            GameTemp.gamePlayer.setCellPosition(new Vector2Int(-6, -4));
            GameTemp.gamePlayer.setupCollider(this.player.GetComponent<SpritePlayer>());    // 玩家碰撞盒

            this.updateLogic();
            this.updateRender();
        } else {
            this.player.GetComponent<SpritePlayer>().setPlayer(GameTemp.gamePlayer);
        }

        // 绑定摄像机到玩家
        cameraControl.target = player;
        cameraControl.player = player;
        cameraControl.minTile = GameTemp.gameMap.mapInfo.minTileWorld;
        cameraControl.maxTile = GameTemp.gameMap.mapInfo.maxTileWorld;
        cameraControl.setupPos(cameraControl.minTile, cameraControl.maxTile);

        // 初始化文章窗口
        windowMessage.gameObject.transform.localScale = new Vector3(1, 0, windowMessage.gameObject.transform.localScale.z);

        // 初始化显示图片
        if (this.pictures == null) {
			this.pictures = new Dictionary<int, Image>();
		}
		// 创建现有数据记录的图片
        foreach (GamePicture pic in GameTemp.gameScreen.pictures.Values) {
			this.attachPicImage(pic);
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
                if (this._transitionProgress == 0) {
                    this._transitionProgress = -1;
                    if (this.transitionFinishCallback != null) {
                        transitionFinishCallback.Invoke();
                    }
                }
            }
            this.snap.color = new Color(1, 1, 1, this.transitionProgress);
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

        // debug
        bool debug = true;
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

            }
        }
        

    }

    protected override void updateLogic() {
        base.updateLogic();

        GameTemp.gameScreen.update();
        if (this.isInTransition()) {
            return;
        }
        GameTemp.gamePlayer.update();
        GameTemp.gameMap.update();
        if (Input.GetKeyDown(KeyCode.F5)) {
            GameTemp.gameScreen.toggleView();
        }
    }

    protected override void updateAfterFrame() {
        base.updateAfterFrame();
    }

    public GameObject getMapNode() {
        return this.currMapObj;
    }

	/// <summary>
	/// 显示图片
	/// </summary>
	/// <param name="picture"></param>
	public void attachPicImage(GamePicture picture) {
		GameObject picImage = new GameObject();
		Image img = picImage.AddComponent<Image>();
		Sprite sprite = Resources.Load<Sprite>(string.Format("graphics/pictures/{0}", picture.picName));
		img.sprite = sprite;
		img.GetComponent<RectTransform>().sizeDelta = new Vector2(sprite.bounds.extents.x * sprite.pixelsPerUnit, 
																	sprite.bounds.extents.y * sprite.pixelsPerUnit);
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
    /// 初始化渐变
    /// </summary>
    /// <param name="duration"></param>
    /// <param name="callback"></param>
    public void setupTransition(int duration, onTransitionFinish callback) {
        this._transitionDuration = duration;
        this._transitionProgress = this._transitionDuration;
        this.transitionFinishCallback = callback;
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
