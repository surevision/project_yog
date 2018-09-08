using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

[Serializable]
public class GamePlayer : GameCharacterBase {
    private int moveCnt = 0;
    public override void update() {
        this.moveCnt += 1;
        if (this.moveCnt > 60 * 1) {
            this.moveCnt = 0;
            this.direction = (DIRS)((((int)(this.direction) / 2 % 4) + 1) * 2);
        }
        base.update();
    }
}
