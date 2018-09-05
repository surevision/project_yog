using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneMap : SceneBase {
    private GameObject currMapObj = null;
	// Use this for initialization
	void Start () {
        base.Start();
        sceneName = "Main";

        GameObject map = Instantiate<GameObject>(Resources.Load<GameObject>("prefabs/maps/Map001"));
        map.transform.tag = "map";
        GameObject.Find("Map").transform.DetachChildren();
        map.transform.SetParent(GameObject.Find("Map").transform);
        this.currMapObj = map;
    }
	
	// Update is called once per frame
	void updateLogic () {
		
	}
}
