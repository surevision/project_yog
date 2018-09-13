using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

[Serializable]
public class GamePlayer : GameCharacterBase {
    public override void update() {
        base.update();
        if (!this.isMoving()) {
            float step = 0.16f / 16.0f;
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

    public override float screenX() {
        return this.realX + 0.5f * 0.16f;
    }
}
