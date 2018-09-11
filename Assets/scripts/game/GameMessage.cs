using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMessage {
    public string text = "";
    public delegate void FinishCallback();

    private FinishCallback _onFinishCallback;

    public void setFinishCallback(FinishCallback callback) {
        this._onFinishCallback = callback;
    }

    public bool isBusy() {
        return this.text == null || (!"".Equals(this.text));
    }

    public void onFinish() {
        this.text = "";
        if (this._onFinishCallback != null) {
            this._onFinishCallback.Invoke();
        }
    }
}
