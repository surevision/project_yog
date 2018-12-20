using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// UI控制组基类
/// </summary>
public class UISetBase {
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
    private UISetMessenger messenger = null;
    public UISetBase(UISetMessenger messenger) {
        this.uiSetName = new string(messenger.uiSetName.ToCharArray());
        this.messenger = messenger;
    }

    public virtual void start() {
    }

    public virtual void update() {
        if (!this.uiSetName.Equals(this.messenger.uiSetName)) {
            this.terminate();
            return;
        }
    }

    private virtual void terminate() {
    }
}
