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

    public override float offsetStepX() {
        return base.offsetStepX();
    }
    public override float offsetStepY() {
        return base.offsetStepY();
    }

    public float offsetScreenX() {
        return 0.5f * 0.16f;
    }
    public float offsetScreenY() {
        return 0.01f * 2f;
    }

    public override float screenX() {
        return this.realX + offsetScreenX();
    }
    public override float screenY() {
        return this.realY + offsetScreenY();
    }
}
