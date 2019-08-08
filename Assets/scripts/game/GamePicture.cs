using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[XLua.LuaCallCSharp]
[Serializable]
public class GamePicture {
	// 属性
	public string picName = "";    // 文件名
	public int num;    // 编号
	public int x;   // 坐标
	public int y;  // 坐标
	public int opacity; // 透明度
	public int rotation;   // 旋转角度
	public float zoomX; // 缩放x
	public float zoomY; // 缩放y

	// 动态
	private int animeWait;	// 渐变时间
	private int targetX;
	private int targetY;
	private int targetOpacity;
	private int targetRotation;
	private float targetZoomX;
	private float targetZoomY;

	public GamePicture(int num, string picName, int x, int y) {
		this.num = num;
		this.picName = picName;
		this.x = x;
		this.y = y;
		this.opacity = 255;
		this.rotation = 0;
		this.zoomX = 1;
		this.zoomY = 1;
		this.animeWait = -1;
	}

	/// <summary>
	/// 设置动画时间
	/// </summary>
	/// <param name="wait"></param>
	public void setAnimDuration(int wait) {
		this.animeWait = wait;
	}

	// 移动图片
	public void moveTo(int x, int y) {
		this.targetX = x;
		this.targetY = y;
	}
	// 渐变
	public void fadeTo(int opacity) {
		this.targetOpacity = opacity;
	}
	// 旋转
	public void rotateTo(int rotation) {
		this.targetRotation = rotation;
	}
	// 缩放
	public void zoomTo(float zx, float zy) {
		this.targetZoomX = zx;
		this.targetZoomY = zy;
	}

	// 更新动态
	public void update() {
		if (this.animeWait > 0) {
			this.x += (this.targetX - this.x) / this.animeWait;
			this.y += (this.targetY - this.y) / this.animeWait;
			this.opacity += (this.targetOpacity - this.opacity) / this.animeWait;
			this.rotation += (this.targetRotation - this.rotation) / this.animeWait;
			this.zoomX += (this.targetZoomX - this.zoomX) / this.animeWait;
			this.zoomY += (this.targetZoomY - this.zoomY) / this.animeWait;
		} else if (this.animeWait == 0) {
			this.x = this.targetX;
			this.y = this.targetY;
			this.opacity = this.targetOpacity;
			this.rotation = this.targetRotation;
			this.zoomX = this.targetZoomX;
			this.zoomY = this.targetZoomY;
		}
		// 更新计数
		if (this.animeWait >= 0) {
			this.animeWait -= 1;
		}
	}
}
