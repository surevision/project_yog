using System.Collections;
using System.Collections.Generic;
using System;
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

    public void setCellPosition(Vector2Int cellPos) {
        Vector2 pos = GameTemp.gameMap.getTileWorldPos(cellPos.x, cellPos.y);
        this.x = pos.x;
        this.y = pos.y;
        this.realX = pos.x;
        this.realY = pos.y;
    }

    /// <summary>
    /// 初始化包围盒数据
    /// </summary>
    /// <param name="sprite"></param>
    public override void setupCollider(SpriteCharacter sprite) {
        List<Vector2> points = new List<Vector2>();
        Vector3 size = sprite.GetComponent<SpriteRenderer>().bounds.size;
        float resize = 0.01f;
        points.Add(new Vector2(resize, resize));
        points.Add(new Vector2(resize, size.y / 4 * 3 - resize));
        points.Add(new Vector2(size.x - resize, size.y / 4 * 3 - resize));
        points.Add(new Vector2(size.x - resize, resize));
        this.colliderPolygon = new Intersection.Polygon(points);
    }
    
    public float offsetScreenX() {
        return 0.5f * 0.16f;
    }
    public float offsetScreenY() {
        return 0;
    }

    public override float screenX() {
        return this.realX + offsetScreenX();
    }
    public override float screenY() {
        return this.realY + offsetScreenY();
    }
}
