using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindowMessage : WindowBase {
    private string _text;

    public string text {
        get { return _text; }
        set { _text = value; }
    }

}
