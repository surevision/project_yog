using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;

[Serializable]
public class GamePlayer : GameCharacterBase {

	protected override float getMoveSpeed() {
		if (InputManager.isPress (InputManager.GameKey.SHIFT)) {
			return Mathf.Min(5, this.moveSpeed + 1);
		}
		return this.moveSpeed;
	}

    public override void update() {
        base.update();
        if (GameTemp.gameMap.interpreter.isRunning()) {
            return;
        }
        if (!this.isMoving()) {
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
            if (this.pattern == 2) {
                // 脚步声
                AudioManager.PlaySE("step");
            }
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
