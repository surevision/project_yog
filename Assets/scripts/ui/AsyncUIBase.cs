using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 异步处理的UI
/// </summary>
public class AsyncUIBase {
    protected GameObject uiNode = null;
    protected string uiName = "";   // ui名
    protected bool deadFlag = false;  // 销毁标记
    public AsyncUIBase() {
        this.uiNode = null;
        this.uiName = "";
        this.deadFlag = false;
    }
    public AsyncUIBase(string name, GameObject uiNode) : base() {
        this.uiNode = uiNode;
        this.uiName = new string(name.ToCharArray());
    }

    public bool isDead() {
        return this.deadFlag;
    }

    public virtual void start() {
        Debug.Log(string.Format("start async ui {0}", this.uiName));
    }

    public virtual void update() {

    }

    public virtual void terminate() {
        Debug.Log(string.Format("terminate async ui {0}", this.uiName));
        this.deadFlag = true;
    }
}
