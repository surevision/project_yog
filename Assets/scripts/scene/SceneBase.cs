using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneBase : MonoBehaviour {
    [HideInInspector]
    public string sceneName;
	// Use this for initialization
	protected virtual void Start () {
        sceneName = "Base"; // this.GetType().Name;
        SceneManager.Scene = this;
	}
	
	// Update is called once per frame
	void Update () {
        updateLogic();
    }

    protected virtual void updateLogic() {

    }
}
