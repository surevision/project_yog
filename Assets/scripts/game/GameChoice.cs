using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class GameChoice {
    public class Choice {
        public string content;
        public string jumpTag;
    }
    
    private string _title;
    public string title {
        get { return _title; }
        set { _title = this.dealTransStr(value); }
    }

    public List<Choice> choices = new List<Choice>();

    public delegate void FinishCallback(int choice);

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
            text = regVar.Replace(text, GameTemp.gameVariables.getObj(varId).ToString(), 1);
        }
        return text;
    }

    public void prepareChoice(string title) {
        this.title = title;
        this.choices.Clear();
    }
    public void addChoice(string content, string jumpTag) {
        Choice choice = new Choice();
        choice.content = content;
        choice.jumpTag = jumpTag;
        this.choices.Add(choice);
    }

    public bool isBusy() {
        return (this.title != null && (!"".Equals(this.title)));
    }

    public void onFinish(int choice) {
        Debug.Log(string.Format("select choice {0}: {1}", choice, this.choices[choice]));
        this.title = "";
        if (this._onFinishCallback != null) {
            this._onFinishCallback.Invoke(choice);
        }
    }
}
