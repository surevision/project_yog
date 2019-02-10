using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 读档菜单
/// </summary>
public class UISetLoad : UISetBase {
    private const int FileNums = 16;    // 存档个数
    private const int CursorTopY = 512;
    private const int ContentTopY = -8;
    private const int ContentSpaceY = 16;
    private const int ContentSpaceX = 0;

    private GameObject menuNode = null; // 加载的节点
    private GameObject contentNode = null; // 容器节点
    private List<Transform> items = new List<Transform>();    // 选项节点

    private int _menuIndex = -1;

    /// <summary>
    /// 选中的选项id
    /// </summary>
    /// <value>选中的选项id，设置时自动更新ui</value>
    public int menuIndex
    {
        get {
            return _menuIndex;
        }
        set {
            _menuIndex = value;
            if (value >= 0) {
                // 更新光标
                this.updateCursor();
            }
        }
    }

    public UISetLoad() : base() {
    }

    public UISetLoad(UISetBase.UISetMessenger messenger) : base(messenger) {
    }


    public override void start() {
        base.start();
        // 加载菜单节点
        this.menuNode = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("prefabs/ui/menus/FilesMenu"));
        this.menuNode.transform.SetParent(this.messenger.uiNode.transform);
        this.menuNode.name = "LoadMenu";
        // 读取节点信息
        this.contentNode = this.menuNode.transform.Find("Panel/mask/content").gameObject;

        this.initItems();
    }
    public override void update() {

        // 按键处理
        while (true) {
            // 移动光标
            if (InputManager.isTrigger(InputManager.GameKey.DOWN)) {
                if (this.items.Count <= 1) {
                    break;
                }
                this.menuIndex = Mathf.Min(this.menuIndex + 1, this.items.Count - 1);
                break;
            }
            if (InputManager.isTrigger(InputManager.GameKey.UP)) {
                if (this.items.Count <= 1) {
                    break;
                }
                this.menuIndex = Mathf.Max(this.menuIndex - 1, 0);
                break;
            }
            // 移动光标
            if (InputManager.isTrigger(InputManager.GameKey.RIGHT)) {
                if (this.items.Count <= 1) {
                    break;
                }
                this.menuIndex = Mathf.Min(this.menuIndex + 1, this.items.Count - 1);
                break;
            }
            if (InputManager.isTrigger(InputManager.GameKey.LEFT)) {
                if (this.items.Count <= 1) {
                    break;
                }
                this.menuIndex = Mathf.Max(this.menuIndex - 1, 0);
                break;
            }
            if (this.menuIndex >= 0 && InputManager.isTrigger(InputManager.GameKey.C)) {
                if (DataManager.loadAndReStart(this.menuIndex + 1)) {
                    this.messenger.switchToUI("");
                    break;
                }
            }
            // 退出菜单
            if (InputManager.isTrigger(InputManager.GameKey.B)) {
                this.messenger.switchToUI("menu");
                break;
            }
            break;
        }
        base.update();
    }
    public override void terminate() {
        GameObject.Destroy(this.menuNode);
        base.terminate();
    }

    /// <summary>
    /// 创建项目节点
    /// </summary>
    private void initItems() {
        GameObject itemObjPrefab = Resources.Load<GameObject>(string.Format("prefabs/ui/menus/FileNode"));
        for (int i = 0; i < FileNums; i += 1) {
            GameObject itemObj = GameObject.Instantiate(itemObjPrefab);
            // HeaderData
            Dictionary<string, string> headerData = DataManager.loadGameDataHeader(string.Format("save{0}.sav", i + 1));
            itemObj.transform.Find("FileName").GetComponent<TMPro.TMP_Text>().text = string.Format("存档 {0}", i + 1);
            itemObj.transform.Find("Info").gameObject.SetActive(false);
            if (headerData != null) {
                string mapName = headerData["mapName"];
                string saveTime = headerData["saveTime"];
                itemObj.transform.Find("Info").gameObject.SetActive(true);
                itemObj.transform.Find("Info").GetComponent<TMPro.TMP_Text>().text = string.Format("地图：{0} 存档时间：{1}", mapName, saveTime);
            }
            itemObj.transform.SetParent(this.contentNode.transform);
            this.items.Add(itemObj.transform);
        }
        if (this.items.Count > 0) {
            this._menuIndex = 0;
        }
        this.updateCursor();
    }

    /// <summary>
    /// 更新光标位置
    /// </summary>
    private void updateCursor() {
        for (int i = 0; i < FileNums; i += 1) {
            this.items[i].Find("cursor").gameObject.SetActive(false);
        }
        if (this.menuIndex >= 0 && this.menuIndex < this.items.Count) {
            GameObject cursorNode = this.items[this.menuIndex].Find("cursor").gameObject;
            cursorNode.SetActive(true);
            // 超出屏幕的处理
            bool modifyFlag = false;
            float nextY = cursorNode.transform.parent.parent.GetComponent<RectTransform>().TransformPoint(
                cursorNode.transform.parent.GetComponent<RectTransform>().localPosition).y;
            Debug.Log(string.Format("next y: {0}", nextY));
            if (nextY > CursorTopY) {
                // 下次移动会超出上方
                this.contentNode.transform.position = new Vector3(this.contentNode.transform.position.x,
                    this.contentNode.transform.position.y - (120 + ContentSpaceY),
                    this.contentNode.transform.position.z);
                modifyFlag = true;
            }
            if (nextY <= CursorTopY - 4 * (120 + ContentSpaceY)) {
                // 下次移动会超出下方
                this.contentNode.transform.position = new Vector3(this.contentNode.transform.position.x,
                    this.contentNode.transform.position.y + (120 + ContentSpaceY),
                    this.contentNode.transform.position.z);
                modifyFlag = true;
            }
        }
    }
}
