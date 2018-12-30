using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;

[Serializable]
public class GamePlayer : GameCharacterBase {

    public override void update() {
        base.update();
        if (GameTemp.gameMap.interpreter.isRunning()) {
            return;
        }
        if (!this.isMoving()) {
            if (Input.GetKey(KeyCode.DownArrow)) {
                this.moveStraight(DIRS.DOWN);
            }
            if (Input.GetKey(KeyCode.LeftArrow)) {
                this.moveStraight(DIRS.LEFT);
            }
            if (Input.GetKey(KeyCode.RightArrow)) {
                this.moveStraight(DIRS.RIGHT);
            }
            if (Input.GetKey(KeyCode.UpArrow)) {
                this.moveStraight(DIRS.UP);
            }
        }
    }

    /// <summary>
    /// 更新脚步动画
    /// </summary>
    protected override void updateAnimation() {
        this.updateAnimeCount();
        if (this.animeCount > 24 - this.moveSpeed * 2) {
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
        float resize = 0.02f;
        points.Add(new Vector2(resize, resize + 8 / Util.PPU));
        points.Add(new Vector2(resize, Util.GRID_WIDTH / Util.PPU - resize));
        points.Add(new Vector2(Util.GRID_WIDTH / Util.PPU - resize, Util.GRID_WIDTH / Util.PPU - resize));
        points.Add(new Vector2(Util.GRID_WIDTH / Util.PPU - resize, resize));

//        points.Add(new Vector2(0, 8 / Util.PPU));
//        points.Add(new Vector2(0, Util.GRID_WIDTH / Util.PPU));
//        points.Add(new Vector2(Util.GRID_WIDTH / Util.PPU, Util.GRID_WIDTH / Util.PPU));
//        points.Add(new Vector2(Util.GRID_WIDTH / Util.PPU, 8 / Util.PPU));
        this.colliderPolygon = new Intersection.Polygon(points);
    }

    public override float getStep() {
        return getBaseStep() / 32;
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
