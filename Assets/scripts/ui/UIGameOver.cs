using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIGameOver : UISetBase {
	private GameObject menuNode = null; // 加载的节点
	public UIGameOver() : base() {
	}

	public UIGameOver(UISetBase.UISetMessenger messenger) : base(messenger) {
	}

	public override void start() {
		base.start();
		InputManager.clear();
		// 加载菜单节点
		this.menuNode = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("prefabs/ui/menus/GameOver"));
		this.menuNode.transform.SetParent(this.messenger.uiNode.transform);
		this.menuNode.name = "GameOver";
	}
	public override void terminate() {
		base.terminate();
		GameObject.Destroy(this.menuNode);
	}

	/// <summary>
	/// 刷新
	/// </summary>
	public override void update() {
		// 按键处理
		while (true) {
			// 确定
			if (InputManager.isTrigger(InputManager.GameKey.C) || InputManager.isTrigger(InputManager.GameKey.B)) {
				SceneManager.gotoScene("Title");
				break;
			}
			break;
		}
		base.update();
	}

}
