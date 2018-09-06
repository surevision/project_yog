using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public class GameCharacterBase
{
    public enum DIRS
    {
        NONE = 0,
        DOWN = 2,
        LEFT = 4,
        RIGHT = 6,
        UP = 8
    }
    public enum PRIORITIES
    {
        BELOW,
        SAME,
        UPPER
    }
    public int id;                              // ID
    public int x;                               // 地图 X 坐标（格子坐标）
    public int y;                               // 地图 Y 坐标（格子坐标）
    public float realX;                         // 地图 X 坐标（实际像素坐标）
    public float realY;                         // 地图 Y 坐标（实际像素坐标）
    public int moveSpeed;                       // 移动速度
    public string characterName;                // 文件名
    public int characterIndex = 0;              // 图像序号
    public int moveFrequency = 0;               // 移动频度
    public bool walkAnime = false;              // 步行动画
    public bool stepAnime = false;              // 踏步动画
    public bool directionFix = false;           // 固定朝向
    public int opacity = 255;                   // 不透明度
    public DIRS direction = DIRS.DOWN;          // 方向
    public int pattern = 1;                     // 图案
    public PRIORITIES priorityType = PRIORITIES.BELOW;            // 优先级类型
    public bool through = false;                    // 穿透
    public int animationId = -1;                    // 动画 ID
    public int balloonId = -1;                      // 心情图标 ID
    public bool transparent = false;                // 透明状态
    
    private DIRS originalDirection = DIRS.DOWN;      // 原方向
    private int originalPattern = 1;                 // 原图案
    private float animeCount = 0;                    // 动画计数
    private int stopCount = 0;                       // 停止计数
    private bool locked = false;                     // 锁的标志
    private DIRS prelockDirection = DIRS.NONE;       // 被锁上前的方向
    private bool moveSucceed = true;                 // 移动成功的标志


    public GameCharacterBase() {
        this.originalDirection = DIRS.DOWN;       // 原方向
        this.originalPattern = 1;                 // 原图案
        this.animeCount = 0;                      // 动画计数
        this.stopCount = 0;                       // 停止计数
        this.locked = false;                      // 锁的标志
        this.prelockDirection = DIRS.NONE;        // 被锁上前的方向
        this.moveSucceed = true;                  // 移动成功的标志
    }

    /// <summary>
    /// 每帧移动距离
    /// </summary>
    /// <returns></returns>
    private float distancePerFrame() {
        return Mathf.Pow(2, this.moveSpeed) / 256.0f;
    }

    /// <summary>
    /// 更新
    /// </summary>
    public virtual void update() {
        this.updateAnimation();
        if (this.isMoving()) {
            this.updateMove();
            return;
        }
        this.updateStop();
    }
    /// <summary>
    /// 是否在移动中
    /// </summary>
    /// <returns></returns>
    protected virtual bool isMoving() {
        return this.realX != this.x || this.realY != this.y;
    }

    /// <summary>
    /// 更新脚步动画
    /// </summary>
    protected virtual void updateAnimation() {
        this.updateAnimeCount();
        if (this.animeCount > 18 - this.moveSpeed * 2) {
            this.updateAnimePattern();  // 改图
            this.animeCount = 0;
        }
    }

    protected virtual void updateAnimeCount() {
        if (this.isMoving() && this.stepAnime) {
            // 行走的情况
            this.animeCount += 1.5f;
        } else if (this.stepAnime || this.pattern != this.originalPattern) {
            this.animeCount += 1.0f;
        }
    }

    /// <summary>
    /// 更新动画计数
    /// </summary>
    protected virtual void updateAnimePattern() {
        if (!this.stepAnime && this.stopCount > 0) {
            // 无踏步动画
            this.pattern = this.originalPattern;
        } else {
            this.pattern = (this.pattern + 1) % 4;
        }

    }

    /// <summary>
    /// 更新移动位置
    /// </summary>
    protected virtual void updateMove() {
        if (this.x < this.realX) {
            this.realX = Mathf.Max(this.realX - this.distancePerFrame(), this.x);
        }
        if (this.x > this.realX) {
            this.realX = Mathf.Min(this.realX + this.distancePerFrame(), this.x);
        }
        if (this.y < this.realY) {
            this.realY = Mathf.Max(this.realY - this.distancePerFrame(), this.y);
        }
        if (this.y > this.realY) {
            this.realY = Mathf.Min(this.realY + this.distancePerFrame(), this.y);
        }
    }

    /// <summary>
    /// 更新停止
    /// </summary>
    protected virtual void updateStop() {
        if (!this.locked) {
            this.stopCount += 1;
        }
    }
}
