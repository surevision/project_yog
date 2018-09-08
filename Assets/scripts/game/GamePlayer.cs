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
                Debug.Log("key down");
                this.direction = DIRS.DOWN;
                this.y -= 1;
            }
            if (Input.GetKey(KeyCode.LeftArrow)) {
                this.direction = DIRS.LEFT;
                this.x -= 1;
            }
            if (Input.GetKey(KeyCode.RightArrow)) {
                this.direction = DIRS.RIGHT;
                this.x += 1;
            }
            if (Input.GetKey(KeyCode.UpArrow)) {
                this.direction = DIRS.UP;
                this.y += 1;
            }
        }
    }
}
