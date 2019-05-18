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

    // 屏幕震动相关

    public enum ShakeDir {
        X = 1,
        Y = 2,
        XY = 3
    }
    private int shakePower = 0; // 震动强度
    private int shakeDuration = 0;  // 帧计数
    private int shakeDirX = 0;  // X移动方向
    private int shakeDirY = 0;  // Y移动方向
    private int shakeRandOffsetX = 0;   // X终点偏移
    private int shakeRandOffsetY = 0;   // Y终点偏移
    public int shakePosX = 0;   // X震动位置
    public int shakePosY = 0;   // Y震动位置

	// 屏幕指向相关
	public int targetToGameObject;	// 指向 GameObject， -1 不跟随， 0 玩家， 1~n 事件
	public Vector2 targetPositionFrom;
	public Vector2 targetPositionTo;		// 移向目标坐标
	public Vector2 currTargetPos;	// 当前指向坐标
	public int targetToDuration;	// 移向目标指定时间
	public int targetToFrame;	// 移向目标已用帧数

	public GameScreen() {
		this.currView = NormalView;
        this.currScreenColorInfo = new ScreenColorInfo();
		this.pictures = new Dictionary<int, GamePicture>();
		this.targetToGameObject = 0;
		this.targetToDuration = -1;
		this.currTargetPos = Vector2.zero;
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

    /// <summary>
    /// 是否正在更改画面色调
    /// </summary>
    /// <returns></returns>
    public bool isColorChanging() {
        return this.targetScreenColorInfo != null;
    }

    /// <summary>
    /// 震动
    /// </summary>
    /// <param name="power"></param>
    /// <param name="duration"></param>
    /// <param name="dir"></param>
    public void startShake(int power, int duration, ShakeDir dir) {
        this.shakePower = power;
        this.shakeDirX = 0;
        this.shakeDirY = 0;
        switch (dir) {
        case ShakeDir.X:
            this.shakeDirX = 1;
            break;
        case ShakeDir.Y:
            this.shakeDirY = 1;
            break;
        case ShakeDir.XY:
            this.shakeDirX = 1;
            this.shakeDirY = 1;
            break;
        }
        this.shakeDuration = duration;
        this.shakeRandOffsetX = UnityEngine.Random.Range(-power, power);
        this.shakeRandOffsetY = UnityEngine.Random.Range(-power, power);
        this.shakePosX = 0;
        this.shakePosY = 0;
    }

    /// <summary>
    /// 是否正在震动屏幕
    /// </summary>
    /// <returns></returns>
    public bool isShaking() {
        return this.shakeDuration > 0;
	}

	/// <summary>
	/// 是否正在将镜头转向坐标
	/// </summary>
	/// <returns><c>true</c>, if targeting to position was ised, <c>false</c> otherwise.</returns>
	public bool isTargetingToPos() {
		return this.targetToDuration > 0;
	}

	/// <summary>
	/// 跟随角色
	/// </summary>
	/// <param name="id">Identifier. 0 主角 1~n 事件</param>
	public void targetToCharacter(int id) {
		if (id >= 0) {
			this.targetToGameObject = id;
		} else {
			Debug.Log(string.Format("wrong id {id}", id));
		}
	}

	/// <summary>
	/// 移向坐标
	/// </summary>
	/// <param name="x">The x coordinate.</param>
	/// <param name="y">The y coordinate.</param>
	/// <param name="duration">Duration.</param>
	public void targetToPos(float x, float y, int duration) {
		this.targetToGameObject = -1;
		this.targetPositionFrom = this.currTargetPos;
		this.targetPositionTo = new Vector2(x, y);
		this.targetToDuration = duration;
		this.targetToFrame = 0;
	}

	/// <summary>
	/// 移向角色坐标
	/// </summary>
	/// <param name="id">Identifier.</param>
	/// <param name="duration">Duration.</param>
	public void targetToPosByCharId(int id, int duration) {

		if (id >= 0) {
			Vector2 v2 = Vector2.zero;
			if (id == 0) {	// 跟随主角
				Vector3 v3 = ((SceneMap)SceneManager.Scene).getPlayerNode().transform.position;
				v2 = new Vector2(v3.x, v3.y);
			} else {	// 跟随事件
				Vector3 v3 = ((GameEvent)GameTemp.gameMap.getCharacter(id)).getEventSprite().transform.position;
				v2 = new Vector2(v3.x, v3.y);
			}
			this.targetToGameObject = -1;
			this.targetPositionFrom = this.currTargetPos;
			this.targetPositionTo = v2;
			this.targetToDuration = duration;
			this.targetToFrame = 0;
		}
	}

	public void updateCurrPos() {

		// 刷新镜头移动
		if (this.isTargetingToPos()) {	// 移向位置
			this.targetToFrame += 1;
			this.currTargetPos = Vector2.Lerp(this.targetPositionFrom, this.targetPositionTo, 1.0f * this.targetToFrame / this.targetToDuration);
			if (this.targetToFrame == this.targetToDuration) {
				this.targetToDuration = -1;
			}
		} else {
			// 跟随角色
			if (this.targetToGameObject >= 0) {
				if (this.targetToGameObject == 0) {	// 跟随主角
					Vector3 v3 = ((SceneMap)SceneManager.Scene).getPlayerNode().transform.position;
					this.currTargetPos = new Vector2(v3.x, v3.y);
				} else {	// 跟随事件
					Vector3 v3 = ((GameEvent)GameTemp.gameMap.getCharacter(this.targetToGameObject)).getEventSprite().transform.position;
					this.currTargetPos = new Vector2(v3.x, v3.y);
				}
			}
		}
		
	}

    public void update() {
		// 更新镜头位置
		this.updateCurrPos();
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

		// 刷新图片
		foreach(GamePicture pic in this.pictures.Values) {
			pic.update();
		}

        // 刷新震动
        if (this.isShaking()) {
            // 移动位置X
            this.shakePosX += this.shakeDirX;
            if (this.shakePosX > this.shakePower * 2) {
                this.shakeDirX = -1;
                this.shakeRandOffsetX = UnityEngine.Random.Range(-this.shakePower, this.shakePower);
            } else if (this.shakePosX < -this.shakePower * 2) {
                this.shakeDirX = 1;
                this.shakeRandOffsetX = UnityEngine.Random.Range(-this.shakePower, this.shakePower);
            }
            // 移动位置y
            this.shakePosY += this.shakeDirY;
            if (this.shakePosY > this.shakePower * 2) {
                this.shakeDirY = -1;
                this.shakeRandOffsetY = UnityEngine.Random.Range(-this.shakePower, this.shakePower);
            } else if (this.shakePosY < -this.shakePower * 2) {
                this.shakeDirY = 1;
                this.shakeRandOffsetY = UnityEngine.Random.Range(-this.shakePower, this.shakePower);
            }
            // 帧步进
            this.shakeDuration -= 1;
            // 位置还原
            if (this.shakeDuration == 0) {
                this.shakePosX = 0;
                this.shakePosY = 0;
            }
        }
    }

}
