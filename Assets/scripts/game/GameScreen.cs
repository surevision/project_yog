using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameScreen {

	// 显示图片
	public Dictionary<int, GamePicture> pictures;

	// 缩放相关
	private const float NormalView = 0.8f;
	private const float HalfView = NormalView / 2;

	private float targetView = NormalView;
	private bool isMiniView = false;

	private float _currView = NormalView;

	public GameScreen() {
		this.currView = NormalView;
		this.pictures = new Dictionary<int, GamePicture>();
	}

	public float currView {
		get { return _currView; }
		set { _currView = value; }
	}

	public void toggleView() {
		if (isMiniView) {
			this.showNormalView();
		} else {
			this.showHalfView();
		}
	}

	public void showNormalView() {
		isMiniView = false;
		targetView = NormalView;
	}

	public void showHalfView() {
		isMiniView = true;
		targetView = HalfView;
	}

	public GamePicture getPicture(int num) {
		if (this.pictures.ContainsKey(num)) {
			return this.pictures[num];
		}
		Debug.Log(string.Format("not found pic num {0}", num));
		return null;
	}

	/// <summary>
	/// 显示图片
	/// </summary>
	/// <param name="pic"></param>
    public void showPicture(GamePicture pic) {
        Debug.Log(string.Format("show pic {0}", pic.num));
        if (!this.pictures.ContainsKey(pic.num)) {
            // 操作精灵
            ((SceneMap)SceneManager.Scene).attachPicImage(pic);
            this.pictures.Add(pic.num, pic);
        } else {
            Debug.Log(string.Format("pic {0} already exists", pic.num));
        }
	}

	/// <summary>
	/// 消除图片
	/// </summary>
	/// <param name="num"></param>
	public void erasePicture(int num) {
		if (this.pictures.ContainsKey(num)) {
			this.pictures.Remove(num);
			// 操作精灵
			((SceneMap)SceneManager.Scene).erasePicImage(num);
		}
	}

    public void update() {

		// 刷新图片
		foreach(GamePicture pic in this.pictures.Values) {
			pic.update();
		}

        // 刷新视野
        if (currView < targetView) {
            currView = Mathf.Min(currView + 0.16f / 8, targetView);
        }
        if (currView > targetView) {
            currView = Mathf.Max(currView - 0.16f / 8, targetView);
        }
    }
}
