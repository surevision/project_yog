using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISetItem : UISetBase {
    private const int CursorTopY = 128;
    private const int ContentTopY = -8;
    private const int ContentSpaceY = 12;
    private const int ContentSpaceX = 0;


	public int menuIndex = 0;   // 选中的选项id

	private GameObject menuNode = null; // 加载的节点
    private Vector3 targetPos = Vector3.zero;   // 光标目标位置
    private List<Transform> items = new List<Transform>();    // 选项节点
    private GameObject cursorNode = null;   // 光标节点
    private GameObject contentNode = null;   // 内容节点

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
        for (int i = 0; i < this.contentNode.transform.childCount; i += 1) {
            this.items.Add(this.contentNode.transform.GetChild(i));
		}
		this.cursorNode = this.menuNode.transform.Find("PanelItems/mask/cursor").gameObject;
        this.targetPos = Vector3.zero;
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
                this.menuIndex += 5;
                this.menuIndex = Mathf.Min(this.menuIndex, this.items.Count - 1);
                this.targetPos = this.items[this.menuIndex].transform.position;
                this.modifyTargetPos();
                break;
            }
            if (InputManager.isTrigger(InputManager.GameKey.UP)) {
                this.menuIndex -= 5;
                this.menuIndex = Mathf.Max(this.menuIndex, 0);
                this.targetPos = this.items[this.menuIndex].transform.position;
                this.modifyTargetPos();
                break;
            }
            // 移动光标
            if (InputManager.isTrigger(InputManager.GameKey.RIGHT)) {
                this.menuIndex += 1;
                this.menuIndex = Mathf.Min(this.menuIndex, this.items.Count - 1);
                this.targetPos = this.items[this.menuIndex].transform.position;
                this.modifyTargetPos();
                break;
            }
            if (InputManager.isTrigger(InputManager.GameKey.LEFT)) {
                this.menuIndex -= 1;
                this.menuIndex = Mathf.Max(this.menuIndex, 0);
                this.targetPos = this.items[this.menuIndex].transform.position;
                this.modifyTargetPos();
                break;
            }
			// 确定
			if (InputManager.isTrigger(InputManager.GameKey.C)) {
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

}
