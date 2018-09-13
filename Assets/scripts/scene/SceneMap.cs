using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneMap : SceneBase {
    private GameObject currMapObj = null;
    private GameObject player = null;

	void Start () {
        base.Start();
        sceneName = "Main";

        GameObject map = Instantiate<GameObject>(Resources.Load<GameObject>("prefabs/maps/Map002"));
        GameObject.Find("Map").transform.DetachChildren();
        map.transform.SetParent(GameObject.Find("Map").transform);
        this.currMapObj = map;

        this.player = Instantiate<GameObject>(Resources.Load<GameObject>("prefabs/characters/players/Player"));
        this.player.transform.SetParent(map.transform.Find("LayerEvents"));

        GameObject.Find("Main Camera").GetComponent<CameraControl>().target = player;

        GameTemp.gameScreen = new GameScreen();
        GameTemp.gameMap = new GameMap();
        GameTemp.gameMap.setupMap(map);
        GameTemp.gamePlayer = (GamePlayer)this.player.GetComponent<SpritePlayer>().character;
    }

    protected override void updateRender() {
        base.updateRender();
        this.player.GetComponent<SpritePlayer>().update();
        GameObject.Find("Main Camera").GetComponent<Camera>().orthographicSize = GameTemp.gameScreen.currView;
    }

    protected override void updateLogic() {
        base.updateLogic();
        GameTemp.gamePlayer.update();
        GameTemp.gameScreen.update();
        if (Input.GetKeyDown(KeyCode.F5)) {
            GameTemp.gameScreen.toggleView();
        }
    }

}
