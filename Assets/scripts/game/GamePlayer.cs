using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;

[Serializable]
[XLua.LuaCallCSharp]
public class GamePlayer : GameCharacterBase {

	protected override float getMoveSpeed() {
		if (InputManager.isPress (InputManager.GameKey.SHIFT)) {
			return Mathf.Min(5, this.moveSpeed + 1);
		}
		return this.moveSpeed;
	}

    public override void update() {
        base.update();
		if (!GameTemp.transforming &&
			!GameTemp.gameMap.interpreter.isRunning() &&
			!this.isMoving()) {
            switch (InputManager.DIR4()) {
                case InputManager.GameKey.DOWN :
                    this.moveStraight(DIRS.DOWN);
                    break;
                case InputManager.GameKey.LEFT :
                    this.moveStraight(DIRS.LEFT);
                    break;
                case InputManager.GameKey.RIGHT :
                    this.moveStraight(DIRS.RIGHT);
                    break;
                case InputManager.GameKey.UP :
                    this.moveStraight(DIRS.UP);
                    break;
                default:
                    break;
            }
        }
    }

    /// <summary>
    /// 更新脚步动画
    /// </summary>
    protected override void updateAnimation() {
        this.updateAnimeCount();
		if (this.animeCount > 24 - this.getMoveSpeed() * 2) {
            this.updateAnimePattern();  // 改图
            this.animeCount = 0;
            this.isDirty = true;
			if (this.pattern == 2 || this.pattern == 0) {
                // 脚步声
				if (this.getMoveSpeed() > 3) {
					AudioManager.PlaySE("se_maoudamashii_se_footstep01"); // NOTE 配表？
				} else {
					AudioManager.PlaySE("se_maoudamashii_se_footstep02"); // NOTE 配表？
				}
            }
        }
    }

	public SpritePlayer getPlayerSprite() {
		return ((SceneMap)SceneManager.Scene).getPlayerNode().GetComponent<SpritePlayer>();
	}

	/// <summary>
	/// 播放动画
	/// </summary>
	/// <param name="animName">Animation name.</param>
	/// <param name="loop">If set to <c>true</c> loop.</param>
	public override void playAnim(string animName, bool loop = false)
	{
		base.playAnim(animName, loop);
		SpritePlayer sprite = getPlayerSprite();
		sprite.playAnim(animName, loop);
	}

	/// <summary>
	/// 停止一个循环动画，不考虑序号
	/// </summary>
	/// <param name="animName">Animation name.</param>
	public override void stopLoopAnim(string animName)
	{
		base.stopLoopAnim(animName);
		SpritePlayer sprite = getPlayerSprite();
		sprite.stopLoopAnim(animName);
	}

	/// <summary>
	/// 读档初始化精灵
	/// </summary>
	public void loadInitSprite() {
		SpritePlayer sprite = getPlayerSprite();
		// 恢复循环动画
		foreach (string animName in this.loopAnimNames) {
			sprite.playAnim(animName, true);
		}
	}

    /// <summary>
    /// 初始化包围盒数据
    /// </summary>
    /// <param name="sprite"></param>
    public override void setupCollider(SpriteCharacter sprite) {
        List<Vector2> points = new List<Vector2>();
        Vector3 size = sprite.GetComponent<SpriteRenderer>().bounds.size;
        float resize = 0.036f;

        points.Add(new Vector2((resize + Util.GRID_WIDTH / Util.PPU - resize) / 2, resize));
        points.Add(new Vector2(resize, resize + (Util.GRID_WIDTH / Util.PPU - resize) / 2));
        points.Add(new Vector2((resize + Util.GRID_WIDTH / Util.PPU - resize) / 2, Util.GRID_WIDTH / Util.PPU - resize));
        points.Add(new Vector2(Util.GRID_WIDTH / Util.PPU - resize, resize + (Util.GRID_WIDTH / Util.PPU - resize) / 2));

//        points.Add(new Vector2(resize, resize + 8 / Util.PPU));
//        points.Add(new Vector2(resize, Util.GRID_WIDTH / Util.PPU - resize * 2));
//        points.Add(new Vector2(Util.GRID_WIDTH / Util.PPU - resize, Util.GRID_WIDTH / Util.PPU - resize * 2));
//        points.Add(new Vector2(Util.GRID_WIDTH / Util.PPU - resize, resize));

//        points.Add(new Vector2(0, 8 / Util.PPU));
//        points.Add(new Vector2(0, Util.GRID_WIDTH / Util.PPU));
//        points.Add(new Vector2(Util.GRID_WIDTH / Util.PPU, Util.GRID_WIDTH / Util.PPU));
//        points.Add(new Vector2(Util.GRID_WIDTH / Util.PPU, 8 / Util.PPU));
        this.colliderPolygon = new Intersection.Polygon(points);
    }

    public override float getStep() {
        return getBaseStep() / 16;
    }

    public float offsetScreenX() {
        return 0.5f * (Util.GRID_WIDTH / Util.PPU);
    }
    public float offsetScreenY() {
        return Util.GRID_WIDTH / Util.PPU / 16;
    }

    public override float screenX() {
        return this.realX + offsetScreenX();
    }
    public override float screenY() {
        return this.realY + offsetScreenY();
    }

}
