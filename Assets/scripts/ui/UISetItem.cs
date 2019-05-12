using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISetItem : UISetBase {
    private const int CursorTopY = 128;
    private const int ContentTopY = -8;
    private const int ContentSpaceY = 12;
    private const int ContentSpaceX = 0;

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
                this.targetPos = this.items[_menuIndex].transform.position;
                this.modifyTargetPos();
                // 更新物品说明
                this.helpText.text = GameTemp.gameParty.items[this._menuIndex].item.description;
            }
        }
    }


	private GameObject menuNode = null; // 加载的节点
    private Vector3 targetPos = Vector3.zero;   // 光标目标位置
    private List<Transform> items = new List<Transform>();    // 选项节点
    private GameObject cursorNode = null;   // 光标节点
    private GameObject contentNode = null;   // 容器节点
    private TMPro.TMP_Text helpText = null; // 说明文本

	public UISetItem() : base() {
	}

	public UISetItem(UISetBase.UISetMessenger messenger) : base(messenger) {
	}

	public override void start() {
		base.start();
		InputManager.clear();
		// 加载菜单节点
		this.menuNode = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("prefabs/ui/menus/ItemMenu"));
		this.menuNode.transform.SetParent(this.messenger.uiNode.transform);
		this.menuNode.name = "ItemMenu";
		// 读取节点信息
        this.contentNode = this.menuNode.transform.Find("PanelItems/mask/content").gameObject;
		this.cursorNode = this.menuNode.transform.Find("PanelItems/mask/cursor").gameObject;
        this.helpText = this.menuNode.transform.Find("PanelInfo/mask/content/Text").GetComponent<TMPro.TMP_Text>();
        this.helpText.text = "";
        this.targetPos = Vector3.zero;

        this.initItems();
	}
    public override void terminate() {
		base.terminate();
		GameObject.Destroy(this.menuNode);
	}

	/// <summary>
	/// 刷新
	/// </summary>
    public override void update() {
        // 光标移动中
        if (this.isMoving()) {
            this.targetPos = this.items[this.menuIndex].transform.position;
			this.cursorNode.transform.position = new Vector3(this.targetPos.x, this.targetPos.y, this.targetPos.z);
            this.targetPos = Vector3.zero;
			return;
		}
		// 按键处理
        while (true) {
            // 移动光标
            if (InputManager.isTrigger(InputManager.GameKey.DOWN)) {
                if (this.items.Count <= 1) {
                    break;
                }
                this.menuIndex = Mathf.Min(this.menuIndex + 5, this.items.Count - 1);
                break;
            }
            if (InputManager.isTrigger(InputManager.GameKey.UP)) {
                if (this.items.Count <= 1) {
                    break;
                }
                this.menuIndex = Mathf.Max(this.menuIndex - 5, 0);
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
			// 确定
            if (InputManager.isTrigger(InputManager.GameKey.C)) {
                if (this.items.Count == 0) {
                    break;
                }
                bool consume = false;
                if (((Item)GameTemp.gameParty.items[this.menuIndex].item).commonEventId != 0) {
                    GameTemp.gameMap.commonEventId = ((Item)GameTemp.gameParty.items[this.menuIndex].item).commonEventId;
                    this.messenger.switchToUI("");

                    // 标记已使用效果
                    consume = true;
                }
                if (consume && ((Item)GameTemp.gameParty.items[this.menuIndex].item).consumable) {
                    GameTemp.gameParty.items[this.menuIndex].num -= 1;
                    if (GameTemp.gameParty.items[this.menuIndex].num <= 0) {
                        GameTemp.gameParty.items.RemoveAt(this.menuIndex);
                    }
                }
				break;
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

    private void modifyTargetPos() {
        Debug.Log(string.Format("curr index {0}", this.menuIndex));
        // 超出范围处理
        bool modifyFlag = false;
        if (this.cursorNode.GetComponent<RectTransform>().localPosition.y + this.cursorNode.transform.InverseTransformPoint(this.targetPos).y > CursorTopY) {
            // 下次移动会超出上方
            this.contentNode.transform.position = new Vector3(this.contentNode.transform.position.x,
                this.contentNode.transform.position.y - (48 + 12),
                this.contentNode.transform.position.z);
            modifyFlag = true;
        }
        if (this.cursorNode.GetComponent<RectTransform>().localPosition.y + this.cursorNode.transform.InverseTransformPoint(this.targetPos).y < CursorTopY - 4 * (48 + 12)) {
            // 下次移动会超出下方
            this.contentNode.transform.position = new Vector3(this.contentNode.transform.position.x,
                this.contentNode.transform.position.y + (48 + 12),
                this.contentNode.transform.position.z);
            modifyFlag = true;
        }
    }

	private bool isMoving() {
        return this.targetPos != Vector3.zero;
	}

    /// <summary>
    /// 创建物品节点
    /// </summary>
    private void initItems() {
        GameObject itemObjPrefab = Resources.Load<GameObject>(string.Format("prefabs/ui/menus/ItemNode"));
        Debug.Log(string.Format("item len {0}", GameTemp.gameParty.items.Count));
        foreach (GameParty.BagItem item in GameTemp.gameParty.items) {
            GameObject itemObj = GameObject.Instantiate(itemObjPrefab);
            itemObj.GetComponent<TMPro.TMP_Text>().text = item.item.name;
            itemObj.transform.GetChild(0).GetComponent<TMPro.TMP_Text>().text = item.num.ToString();
            itemObj.transform.SetParent(this.contentNode.transform);
            this.items.Add(itemObj.transform);

            // 不可使用的物品灰化
            if (((Item)item.item).commonEventId == 0) {
				itemObj.GetComponent<TMPro.TMP_Text>().color = new Color(0.8f, 0.8f, 0.8f);
            }
        }
        if (GameTemp.gameParty.items.Count > 0) {
            this._menuIndex = 0;
            this.helpText.text = GameTemp.gameParty.items[0].item.description;
        }
    }

}
