using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager {
    private static GameObject nodeBGM;
    private static GameObject nodeBGS;
    private static GameObject nodeSE;

    private const int SE_CHANNELS = 10;

    public static void setup(GameObject _nodeBGM, GameObject _nodeBGS, GameObject _nodeSE) {
        nodeBGM = _nodeBGM;
        nodeBGS = _nodeBGS;
        nodeSE = _nodeSE;

        // SE相关处理
        // 清理子节点
        while (nodeSE.transform.childCount > 0) {
            Transform child = nodeSE.transform.GetChild(0);
            child.parent = null;
            GameObject.Destroy(child.gameObject);
        }
        // 初始化SE节点AudioSource
        for (int i = 0; i < SE_CHANNELS; i += 1) {
            GameObject node = new GameObject();
            node.transform.parent = nodeSE.transform;
            node.AddComponent<AudioSource>();
        }
    }

    /// <summary>
    /// 播放bgm
    /// </summary>
    /// <param name="name"></param>
    public static void PlayBGM(string name) {
        string path = string.Format("audios/bgm/{0}", name);
        AudioClip clip = Resources.Load<AudioClip>(path);
        nodeBGM.GetComponent<AudioSource>().Stop();
        nodeBGM.GetComponent<AudioSource>().clip = clip;
        nodeBGM.GetComponent<AudioSource>().PlayDelayed(0);
    }
    public static void StopBGM() {
        nodeBGM.GetComponent<AudioSource>().Stop();
    }

    /// <summary>
    /// 播放bgs
    /// </summary>
    /// <param name="name"></param>
    public static void PlayBGS(string name) {
        string path = string.Format("audios/bgs/{0}", name);
        AudioClip clip = Resources.Load<AudioClip>(path);
        nodeBGS.GetComponent<AudioSource>().Stop();
        nodeBGS.GetComponent<AudioSource>().clip = clip;
        nodeBGS.GetComponent<AudioSource>().PlayDelayed(0);
    }
    public static void StopBGS() {
        nodeBGS.GetComponent<AudioSource>().Stop();
    }

    /// <summary>
    /// 播放se
    /// </summary>
    /// <param name="name"></param>
    public static void PlaySE(string name) {
        string path = string.Format("audios/se/{0}", name);
        AudioClip clip = Resources.Load<AudioClip>(path);

        int index = 0;
        for (int i = 0; i < SE_CHANNELS; i += 1) {
            if (nodeSE.transform.GetChild(i).GetComponent<AudioSource>().clip == null ||
                    !nodeSE.transform.GetChild(i).GetComponent<AudioSource>().isPlaying) {
                index = i;
                break;
            }
        }

        Debug.Log(string.Format("play on channel {0} name {1}", index, name));

        GameObject node = nodeSE.transform.GetChild(index).gameObject;
        node.GetComponent<AudioSource>().Stop();
        node.GetComponent<AudioSource>().clip = clip;
        node.GetComponent<AudioSource>().Play();
    }


}
