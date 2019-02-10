using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 存档菜单
/// </summary>
public class UISetSave : UISetBase {
    private const int FileNums = 16;    // 存档个数

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
            }
        }
    }

    public UISetSave() : base() {
    }

    public UISetSave(UISetBase.UISetMessenger messenger) : base(messenger) {
    }


    public override void start() {
        base.start();
        // 加载菜单节点
        this.menuNode = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("prefabs/ui/menus/FilesMenu"));
        this.menuNode.transform.SetParent(this.messenger.uiNode.transform);
        this.menuNode.name = "SaveMenu";
        // 读取节点信息
        this.contentNode = this.menuNode.transform.Find("Panel/mask/content").gameObject;

        this.initItems();
    }
    public override void update() {
        base.update();
    }
    public override void terminate() {
        base.terminate();
        GameObject.Destroy(this.menuNode);
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
        if (GameTemp.gameParty.items.Count > 0) {
            this._menuIndex = 0;
        }
    }
}
