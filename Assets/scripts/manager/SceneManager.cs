using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneManager {
    private static SceneBase scene;
    public static SceneBase Scene
    {
        get
        {
            return scene;
        }

        set
        {
            Debug.Log(string.Format(@"scene set {0}", value.sceneName));
            scene = value;
        }
    }
    public static void gotoScene(string scene)
    {
        if (!Scene.sceneName.Equals(scene))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(scene);
        }
    }
    public static void exitGame() {
        UnityEngine.Application.Quit();
    }
}
