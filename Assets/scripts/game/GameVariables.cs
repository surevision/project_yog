﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameVariables {
    private Dictionary<int, object> data;
    public GameVariables() {
        this.data = new Dictionary<int, object>();
    }

    public int this[int index] {
        get {
            if (this.data[index] == null) {
                return 0;
            }
            try {
                return (int)this.data[index];
            } catch (System.Exception e) {
                Debug.Log(string.Format("get fail at index {0}", index));
                return 0;
            }
        }
        set { this.data[index] = value; }
    }

    public object getObj(int index) {
        return this.data[index];
    }
    
}
