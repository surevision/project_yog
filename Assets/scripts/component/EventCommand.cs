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
    [MultilineAttribute]
    private string[] _args;
    public string[] args {
        get { return _args; }
        set { _args = value; }
    }
}
