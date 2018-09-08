using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SpritePlayer))]
public class CharacterPlayerEditor : CharacterBaseEditor {

    protected override void initTarget(Object target) {
        m_Target = (SpritePlayer)target;
    }
}
