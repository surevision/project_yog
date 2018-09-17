using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteEvent : SpriteCharacter {

    [SerializeField]
    private GameEvent _event;

    public override GameCharacterBase character {
        get { return _event; }
    }
}
