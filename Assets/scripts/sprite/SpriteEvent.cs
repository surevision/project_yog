using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteEvent : SpriteCharacter {

    [SerializeField]
    private GameEvent _event;

    public override GameCharacterBase character {
        get { return _event; }
    }

    protected override void updateBitmap() {
        bool reCollider = false;
        if (!this.characterName.Equals(this.character.characterName)) {
            reCollider = true;
        }
        base.updateBitmap();
        if (reCollider) {
            this.character.setupCollider(this);
        }
    }

    public void setEvent(GameEvent _event) {
        this._event = _event;
    }
}
