using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneMap : SceneBase {
    private GameObject currMapObj = null;

    private GameObject player = null;

	void Start () {
        base.Start();
        sceneName = "Main";

        GameObject map = Instantiate<GameObject>(Resources.Load<GameObject>("prefabs/maps/Map001"));
        GameObject.Find("Map").transform.DetachChildren();
        map.transform.SetParent(GameObject.Find("Map").transform);
        this.currMapObj = map;

        this.player = Instantiate<GameObject>(Resources.Load<GameObject>("prefabs/characters/players/Player"));
        this.player.transform.SetParent(map.transform.Find("LayerEvents"));
    }

    protected override void updateLogic () {
        base.updateLogic();
        this.player.GetComponent<SpriteCharacter>().update();
	}
}
