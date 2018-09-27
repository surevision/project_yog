using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpritePlayer : SpriteCharacter {

    [SerializeField]
    private GamePlayer _player;

    public override GameCharacterBase character {
        get { return _player; }
    }

    public void setPlayer(GamePlayer player) {
        this._player = player;
    }
}
