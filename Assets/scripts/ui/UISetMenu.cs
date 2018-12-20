using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISetMenu : UISetBase {
    public int menuIndex = 0;   // 选中的选项id

    private GameObject menuNode = null; // 加载的节点
    private Vector3 targetPos = Vector3.zero;   // 光标目标位置
    private List<Transform> items = new List<Transform>();    // 选项节点
    private GameObject cursorNode = null;   // 光标节点

    public UISetMenu() : base() {
    }

    public UISetMenu(UISetBase.UISetMessenger messenger) : base(messenger) {
    }

    public override void start() {
        base.start();
        // 加载菜单节点
        this.menuNode = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("prefabs/ui/menus/MainMenu"));
        this.menuNode.transform.SetParent(this.messenger.uiNode.transform);
        this.menuNode.name = "MainMenu";
        // 读取节点信息
        Transform contentNode = this.menuNode.transform.Find("Panel/mask/content");
        Debug.Log(contentNode);
        for (int i = 0; i < contentNode.childCount; i += 1) {
            this.items.Add(contentNode.GetChild(i));
        }
        this.cursorNode = this.menuNode.transform.Find("Panel/mask/cursor").gameObject;
        Debug.Log(this.cursorNode);
        this.targetPos = this.cursorNode.transform.position;
    }
    protected override void terminate() {
        base.terminate();
        GameObject.Destroy(this.menuNode);
    }

    /// <summary>
    /// 刷新
    /// </summary>
    public override void update() {
        if (this.isMoving()) {
            // 光标移动中
            Vector3.MoveTowards(this.cursorNode.transform.position, this.targetPos, 100 * UISetBase.minDistance * 0.016f);
            return;
        }

        // 移动光标
        if (Input.GetKeyDown(KeyCode.DownArrow)) {
            this.menuIndex += 1;
            this.menuIndex %= this.items.Count;
        }
        if (Input.GetKeyDown(KeyCode.UpArrow)) {
            this.menuIndex -= 1;
            this.menuIndex += this.items.Count;
            this.menuIndex %= this.items.Count;
        }
        this.targetPos = this.items[this.menuIndex].GetComponent<RectTransform>().position;

        // 退出菜单
        if (Input.GetKeyDown(KeyCode.Escape)) {
            this.messenger.switchToUI("");
        }
        base.update();
    }

    private bool isMoving() {
        return Vector3.Distance(this.cursorNode.transform.position, this.targetPos) > UISetBase.minDistance;
    }

}
