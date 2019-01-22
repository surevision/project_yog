using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameScreen {

	// 显示图片
	public Dictionary<int, GamePicture> pictures;

    // 遮罩
    public bool maskVisible = false;

	// 缩放相关
	private const float NormalView = 1.6f;
	private const float HalfView = 2.4f;

	private float targetView = NormalView;
	private bool isMiniView = false;

	private float _currView = NormalView;

    // 画面色调
    [Serializable]
    public class ScreenColorInfo {
        /// <summary>
        /// 亮度
        /// </summary>
        public float brightness = 1.0f;
        /// <summary>
        /// 饱和度
        /// </summary>
        public float saturation = 1.0f;
        /// <summary>
        /// 对比度
        /// </summary>
        public float contrast = 1.0f;
        /// <summary>
        /// 色调
        /// </summary>
        public int hue = 0;
    }

    /// <summary>
    /// 当前色调
    /// </summary>
    public ScreenColorInfo currScreenColorInfo = null;
    /// <summary>
    /// 转换到的色调
    /// </summary>
    private ScreenColorInfo targetScreenColorInfo = null;
    /// <summary>
    /// 色调转换时间
    /// </summary>
    private int screenColorTransformFrames = 0;

	public GameScreen() {
		this.currView = NormalView;
        this.currScreenColorInfo = new ScreenColorInfo();
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
    /// 显示光遮罩
    /// </summary>
    public void showMask() {
        this.maskVisible = true;
    }

    /// <summary>
    /// 隐藏光遮罩
    /// </summary>
    public void hideMask() {
        this.maskVisible = false;
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

    /// <summary>
    /// 转换画面色调
    /// </summary>
    /// <param name="info">Info.</param>
    /// <param name="time">Time.</param>
    public void screenColorChange(ScreenColorInfo info, int time) {
        this.targetScreenColorInfo = info;
        this.screenColorTransformFrames = time;
    }

    public bool isColorChanging() {
        return this.targetScreenColorInfo != null;
    }

    public void update() {

		// 刷新图片
		foreach(GamePicture pic in this.pictures.Values) {
			pic.update();
		}

        // 刷新色调
        if (this.isColorChanging()) {
            this.screenColorTransformFrames -= 1;
            if (this.screenColorTransformFrames == 0) {
                this.currScreenColorInfo = this.targetScreenColorInfo;
                this.targetScreenColorInfo = null;
            } else {
                this.currScreenColorInfo.brightness += 
                    (this.targetScreenColorInfo.brightness - this.currScreenColorInfo.brightness) / this.screenColorTransformFrames;
                this.currScreenColorInfo.contrast += 
                    (this.targetScreenColorInfo.contrast - this.currScreenColorInfo.contrast) / this.screenColorTransformFrames;
                this.currScreenColorInfo.saturation += 
                    (this.targetScreenColorInfo.saturation - this.currScreenColorInfo.saturation) / this.screenColorTransformFrames;
                this.currScreenColorInfo.hue += 
                    (this.targetScreenColorInfo.hue - this.currScreenColorInfo.hue) / this.screenColorTransformFrames;
            }
        }

        // 刷新视野
        if (currView < targetView) {
            currView = Mathf.Min(currView + (Util.GRID_WIDTH / Util.PPU) / 8, targetView);
        }
        if (currView > targetView) {
            currView = Mathf.Max(currView - (Util.GRID_WIDTH / Util.PPU) / 8, targetView);
        }
    }
}
