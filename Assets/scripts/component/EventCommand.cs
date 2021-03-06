﻿using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

[Serializable]
public class EventCommand : MonoBehaviour {
	[SerializeField]                                                    
	[EnumAttirbute("指令")]
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
    public string getTitle() {
        Dictionary<GameInterpreter.CommandTypes, string> d = new Dictionary<GameInterpreter.CommandTypes, string> {
            // 1
            {GameInterpreter.CommandTypes.ShowArticle, "显示文章"},
            {GameInterpreter.CommandTypes.ShowChoice, "显示选择项"},
            {GameInterpreter.CommandTypes.Condition,"条件分歧"},
            {GameInterpreter.CommandTypes.Loop,"循环开始"},
            {GameInterpreter.CommandTypes.Break,"跳出循环"},
            {GameInterpreter.CommandTypes.CommandBreak,"终止事件处理"},
            {GameInterpreter.CommandTypes.CommonEvent,"公共事件"},
            {GameInterpreter.CommandTypes.Comment,"注释"},
            {GameInterpreter.CommandTypes.Label,"设置跳转标记"},
            {GameInterpreter.CommandTypes.GotoLabel,"跳转到标记"},
            {GameInterpreter.CommandTypes.SetSwitch,"开关操作"},
            {GameInterpreter.CommandTypes.SetVariable,"变量操作"},
            {GameInterpreter.CommandTypes.SetSelfSwitch,"独立开关"},
            {GameInterpreter.CommandTypes.DealItem,"增减物品"},

            // 2
            {GameInterpreter.CommandTypes.Transformation,"场所移动"},
            {GameInterpreter.CommandTypes.SetPos,"设置事件位置"},
            {GameInterpreter.CommandTypes.MoveByRoute,"设置移动路径"},
            {GameInterpreter.CommandTypes.Erase,"暂时消除事件"},
            {GameInterpreter.CommandTypes.Shake,"屏幕震动"},
			{GameInterpreter.CommandTypes.Wait,"等待"},
			{GameInterpreter.CommandTypes.ShowPic,"显示图片"},
            {GameInterpreter.CommandTypes.TransPic,"移动图片"},
            {GameInterpreter.CommandTypes.ErasePic,"消除图片"},
            {GameInterpreter.CommandTypes.ChangeScreenColor,"更改画面色调"},
			{GameInterpreter.CommandTypes.PlayBGM,"播放BGM"},
            {GameInterpreter.CommandTypes.StopBGM,"停止BGM"},
            {GameInterpreter.CommandTypes.PlaySE,"播放SE"},
            {GameInterpreter.CommandTypes.StopSE,"停止SE"},
            {GameInterpreter.CommandTypes.ShowMapName,"开启/关闭地图名"},

            // 3
            {GameInterpreter.CommandTypes.EvalScript,"脚本"},

            // 4
            {GameInterpreter.CommandTypes.EndIf,"分歧结束"},
            {GameInterpreter.CommandTypes.EndLoop,"以上反复"}
        };

        string title = d[this.code];
        if (this.code == GameInterpreter.CommandTypes.Condition) {
        } else if (this.code == GameInterpreter.CommandTypes.MoveByRoute) {
        } else if (this.code == GameInterpreter.CommandTypes.EvalScript) {
        } else if (this.code == GameInterpreter.CommandTypes.EndIf) {
        } else if (this.code == GameInterpreter.CommandTypes.ShowArticle) {
            if (this.args[0] != null) {
                title = title + ": " + this.args[0].Replace("\n", "").Substring(0, Mathf.Min(this.args[0].Replace("\n", "").Length, 20));
            }
        } else if (this.code == GameInterpreter.CommandTypes.ShowChoice) {
            if (this.args[0] != null) {
                title = title + ": " + this.args[0].Replace("\n", "").Substring(0, Mathf.Min(this.args[0].Replace("\n", "").Length, 20));
            }
        } else if (this.args.Length == 0) {
        } else {
            title = title + ": " + string.Join(", ", this.args);
        }
        return title;
    }
}
