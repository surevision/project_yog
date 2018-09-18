using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneBase : MonoBehaviour {
    [HideInInspector]
    public string sceneName;

    internal static float lastGCTime = 0;
    internal const float GCInterval = 1;//1 second 

	// Use this for initialization
	protected virtual void Start () {
        sceneName = "Base"; // this.GetType().Name;
        SceneManager.Scene = this;
	}
	
	// Update is called once per frame
	void Update () {
        updateRender();
    }

    void FixedUpdate() {
        updateLogic();

        // lua gc
        if (Time.time - SceneBase.lastGCTime > GCInterval) {
            LuaManager.LuaEnv.Tick();
            SceneBase.lastGCTime = Time.time;
        }
    }

    protected virtual void updateRender() {

    }

    protected virtual void updateLogic() {

    }
}
