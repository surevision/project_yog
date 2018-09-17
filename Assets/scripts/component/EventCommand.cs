using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

[Serializable]
public class EventCommand : MonoBehaviour {
    [SerializeField]
    private GameInterpreter.CommandTypes _code;
    public GameInterpreter.CommandTypes code {
        get { return _code; }
    }
    [SerializeField]
    private string _value;
    public string value {
        get { return _value; }
    }
}
