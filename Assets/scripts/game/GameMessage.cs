using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class GameMessage {
    private string _text;
    public string text {
        get { return _text; }
        set { _text = this.dealTransStr(value); }
    }
    public delegate void FinishCallback();

    private FinishCallback _onFinishCallback;

    public void setFinishCallback(FinishCallback callback) {
        this._onFinishCallback = callback;
    }


    private string dealTransStr(string text) {
        if (text == null) {
            return "";
        }
        Regex regVar = new Regex(@"\$v<(\d+)>", RegexOptions.IgnoreCase);
        while (regVar.IsMatch(text)) {
            Match matchVar = regVar.Match(text);
            int varId = int.Parse(matchVar.Groups[1].Value);
            text = regVar.Replace(text, GameTemp.gameVariables[varId].ToString(), 1);
        }
        return text;
    }

    public bool isBusy() {
        return (this.text != null && (!"".Equals(this.text)));
    }

    public void onFinish() {
        this.text = "";
        if (this._onFinishCallback != null) {
            this._onFinishCallback.Invoke();
        }
    }
}
