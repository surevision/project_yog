﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;

public class SceneMap : SceneBase {
    public WindowMessage windowMessage;
    public Image snap;
    public bool prepareFreeze = false;  // 标记截屏准备
    public string mapToLoad = "";  // 标记要加载地图


    private GameObject currMapObj = null;
    private GameObject player = null;

	void Start () {
        base.Start();
        sceneName = "Map";


        GameTemp.gameVariables = new GameVariables();
        GameTemp.gameSwitches = new GameSwitches();
        GameTemp.gameSelfSwitches = new GameSelfSwtiches();
        GameTemp.gameScreen = new GameScreen();
        GameTemp.gameMessage = new GameMessage();
        GameTemp.gameMap = new GameMap();

        loadMap("Map1");

    }

    /// <summary>
    /// 屏幕截图
    /// </summary>
    public Texture2D snapScreen() {
        Texture2D texture2D = new Texture2D(Screen.width, Screen.height, TextureFormat.ARGB32, false);
        texture2D.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        return texture2D;
    }

    public void setupFreeze() {
        this.prepareFreeze = true;
    }

    public void prepareLoadMap(string mapName) {
        this.mapToLoad = mapName;
    }

    private void loadMap(string mapName) {
        GameObject map = Instantiate<GameObject>(Resources.Load<GameObject>(string.Format("prefabs/maps/{0}", mapName)));
        map.name = mapName;
        Destroy(this.currMapObj);
        map.transform.SetParent(GameObject.Find("Map").transform);
        this.currMapObj = map;

        GameTemp.gameMap.setupMap(map);

        this.player = Instantiate<GameObject>(Resources.Load<GameObject>("prefabs/characters/players/Player"));
        this.player.transform.SetParent(map.transform.Find(GameMap.layers[(int)GameMap.Layers.LayerPlayer]));
        if (GameTemp.gamePlayer == null) {
            // 根据prefab初始化角色
            GameTemp.gamePlayer = (GamePlayer)this.player.GetComponent<SpritePlayer>().character;
            GameTemp.gamePlayer.setCellPosition(new Vector2Int(-6, -4));
            GameTemp.gamePlayer.setupCollider(this.player.GetComponent<SpritePlayer>());    // 玩家碰撞盒
        } else {
            this.player.GetComponent<SpritePlayer>().setPlayer(GameTemp.gamePlayer);
        }

        GameObject.Find("Main Camera").GetComponent<CameraControl>().target = player;
        windowMessage.gameObject.transform.localScale = new Vector3(1, 0, windowMessage.gameObject.transform.localScale.z);

    }

    public IEnumerator TakeSnapshot() {
        WaitForEndOfFrame frameEnd = new WaitForEndOfFrame();
        yield return frameEnd;
        Destroy(this.snap.sprite);
        Texture2D texture = snapScreen();
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        this.snap.sprite = sprite;
    }


    protected override void updateRender() {
        base.updateRender();

        // 检测截屏
        if (this.prepareFreeze) {
            this.prepareFreeze = false;
            TakeSnapshot();
        }

        // 检测切换地图
        if (!"".Equals(this.mapToLoad)) {
            this.loadMap(this.mapToLoad);
            this.mapToLoad = "";
            return;
        }

        // 刷新角色
        this.player.GetComponent<SpritePlayer>().update();

        // 刷新事件
        foreach (GameEvent e in GameTemp.gameMap.events) {
            e.getEventSprite().update();
        }

        // 刷新视野
        GameObject.Find("Main Camera").GetComponent<Camera>().orthographicSize = GameTemp.gameScreen.currView;

        // 刷新对话
        windowMessage.update();

        // 刷新渐变
        if (GameTemp.gameScreen.isInTransition()) {
            this.snap.color = new Color(1, 1, 1, GameTemp.gameScreen.transitionProgress);
        }

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
        GameTemp.gamePlayer.update();
        GameTemp.gameMap.update();
        GameTemp.gameScreen.update();
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

}
