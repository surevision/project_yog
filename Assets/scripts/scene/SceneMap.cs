using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneMap : SceneBase {
    private GameObject currMapObj = null;
    private GameObject player = null;

	void Start () {
        base.Start();
        sceneName = "Map";

        string mapName = "Map002";
        GameObject map = Instantiate<GameObject>(Resources.Load<GameObject>(string.Format("prefabs/maps/{0}", mapName)));
        map.name = mapName;
        GameObject.Find("Map").transform.DetachChildren();
        map.transform.SetParent(GameObject.Find("Map").transform);
        this.currMapObj = map;

        this.player = Instantiate<GameObject>(Resources.Load<GameObject>("prefabs/characters/players/Player"));
        this.player.transform.SetParent(map.transform.Find("LayerEvents"));

        GameObject.Find("Main Camera").GetComponent<CameraControl>().target = player;

        GameTemp.gameScreen = new GameScreen();
        GameTemp.gameMap = new GameMap();
        GameTemp.gameMap.setupMap(map);


        foreach (Intersection.Polygon polygon in GameTemp.gameMap.mapInfo.passageColliders) {
            foreach (Vector2 point in polygon.points) {
                Debug.Log(point);
            }
        }


        GameTemp.gamePlayer = (GamePlayer)this.player.GetComponent<SpritePlayer>().character;

        GameTemp.gamePlayer.setupCollider(this.player.GetComponent<SpritePlayer>());    // 玩家碰撞盒

        Vector2 startPos = GameTemp.gameMap.getTileWorldPos(5, 4);
        GameTemp.gamePlayer.setPos(startPos.x, startPos.y);

    }

    protected override void updateRender() {
        base.updateRender();
        this.player.GetComponent<SpritePlayer>().update();
        GameObject.Find("Main Camera").GetComponent<Camera>().orthographicSize = GameTemp.gameScreen.currView;

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
        }
        

    }

    protected override void updateLogic() {
        base.updateLogic();
        GameTemp.gamePlayer.update();
        GameTemp.gameScreen.update();
        if (Input.GetKeyDown(KeyCode.F5)) {
            GameTemp.gameScreen.toggleView();
        }
    }

    public GameObject getMapNode() {
        return this.currMapObj;
    }

}
