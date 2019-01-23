using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 显示地图名ui，简单渐隐
/// </summary>
public class AsyncUIMapName : AsyncUIBase {

    private GameObject node;
    private GameObject nameLabel;
    private GameObject bgNode;

    private enum Phase {
        Open = 0,
        Stay,
        Close,
        End
    }
    private int[] Delays = new int[]{30, 60, 30, 1};    //显示，保持，消失持续帧数
    private Phase phase = Phase.Open;
    private int timeStep = 0;

    public AsyncUIMapName() {
    }

    public AsyncUIMapName(string name, GameObject uiNode)
        : base(name, uiNode) {
    }

    public override void start() {
        base.start();
        this.timeStep = 0;
        this.node = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("prefabs/ui/asyncs/MapName"));
        this.node.transform.SetParent(this.uiNode.transform);
        this.node.name = "MapName";
        this.nameLabel = this.node.transform.Find("Panel/bg/name").gameObject;
        this.bgNode = this.node.transform.Find("Panel/bg").gameObject;
        this.nameLabel.GetComponent<TMPro.TMP_Text>().SetText(((SceneMap)SceneManager.Scene).getMapNode().GetComponent<MapExInfo>().showName);
        Color color = this.bgNode.GetComponent<Image>().color;
        this.bgNode.GetComponent<Image>().color = new Color(color.r, color.g, color.b, 0);
        color = this.nameLabel.GetComponent<TMPro.TMP_Text>().color;
        this.nameLabel.GetComponent<TMPro.TMP_Text>().color = new Color(color.r, color.g, color.b, 0);
    }

    public override void update() {
        base.update();
        this.timeStep += 1;
        int frames = 0;
        for (int i = 0; i < Delays.Length; i += 1) {
            frames += Delays[i];
            if (this.timeStep < frames) {
                this.phase = (Phase)i;
                break;
            }
        }
        Color color = this.bgNode.GetComponent<Image>().color;
        Color fontColor = this.nameLabel.GetComponent<TMPro.TMP_Text>().color;
        switch (this.phase) {
            case Phase.Open:
                this.bgNode.GetComponent<Image>().color = new Color(color.r, color.g, color.b, 1.0f * this.timeStep / Delays[(int)Phase.Open]);
                this.nameLabel.GetComponent<TMPro.TMP_Text>().color = new Color(fontColor.r, fontColor.g, fontColor.b, 1.0f * this.timeStep / Delays[(int)Phase.Open]);
                break;
            case Phase.Stay:
                break;
            case Phase.Close:
                this.bgNode.GetComponent<Image>().color = new Color(
                        color.r, 
                        color.g, 
                        color.b, 
                        1.0f - 1.0f * 
                            (this.timeStep - Delays[(int)Phase.Open] - Delays[(int)Phase.Stay]) / Delays[(int)Phase.Close]);
                this.nameLabel.GetComponent<TMPro.TMP_Text>().color = new Color(
                        color.r,
                        color.g,
                        color.b,
                        1.0f - 1.0f *
                            (this.timeStep - Delays[(int)Phase.Open] - Delays[(int)Phase.Stay]) / Delays[(int)Phase.Close]);
                break;
            case Phase.End:
                this.terminate();
                break;
        }
    }

    public override void terminate() {
        base.terminate();
        GameObject.Destroy(this.node);
    }
}
