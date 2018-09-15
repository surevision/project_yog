using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

[Serializable]
public class GamePlayer : GameCharacterBase {
    public override void update() {
        base.update();
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
