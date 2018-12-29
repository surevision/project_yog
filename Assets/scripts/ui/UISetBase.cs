using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// UI控制组基类
/// </summary>
public class UISetBase {
    protected static float minDistance = Util.GRID_WIDTH / Util.PPU / 10000f;    // 移动判定重叠的最小间隔
    /// <summary>
    /// 传递ui信息结构
    /// </summary>
    public class UISetMessenger {
        public GameObject uiNode;
        public string uiSetName;
        public UISetMessenger(GameObject uiNode, string uiSetName) {
            this.uiNode = uiNode;
            this.uiSetName = uiSetName;
        }
        public delegate void SwitchToUIDelegate(string uiSetName);
        public SwitchToUIDelegate switchDelegate = null;
        public void setSwitchDelegate(SwitchToUIDelegate _delegate) {
            this.switchDelegate = _delegate;
        }
        /// <summary>
        /// 切换到ui
        /// </summary>
        /// <param name="uiSetName"></param>
        public void switchToUI(string uiSetName) {
            if (this.switchDelegate != null) {
                switchDelegate.Invoke(uiSetName);
            }
        }
    }
    private string uiSetName = "";   // ui名
    protected UISetMessenger messenger = null;
    public UISetBase() {
    }
    public UISetBase(UISetMessenger messenger) {
        this.uiSetName = new string(messenger.uiSetName.ToCharArray());
        this.messenger = messenger;
    }

    public virtual void start() {
        Debug.Log(string.Format("start {0}", this.uiSetName));
    }

    public virtual void update() {
        //Debug.Log(string.Format("update {0}", this.uiSetName));
        if (!this.uiSetName.Equals(this.messenger.uiSetName)) {
            this.terminate();
            return;
        }
    }

    public virtual void terminate() {
        Debug.Log(string.Format("terminate {0}", this.uiSetName));
    }
}
