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

    public int getArgsLen() {
        Dictionary<GameInterpreter.CommandTypes, int> d = new Dictionary<GameInterpreter.CommandTypes, int> {
            // 1
            {GameInterpreter.CommandTypes.ShowArticle, 1},
            {GameInterpreter.CommandTypes.Condition,2},
            {GameInterpreter.CommandTypes.CommandBreak,0},
            {GameInterpreter.CommandTypes.CommonEvent,1},
            {GameInterpreter.CommandTypes.Comment,1},
            {GameInterpreter.CommandTypes.SetSwitch,2},
            {GameInterpreter.CommandTypes.SetVariable,3},
            {GameInterpreter.CommandTypes.SetSelfSwitch,2},

            // 2
            {GameInterpreter.CommandTypes.Transformation,4},
            {GameInterpreter.CommandTypes.SetPos,2},
            {GameInterpreter.CommandTypes.MoveByRoute,1},
            {GameInterpreter.CommandTypes.Erase,0},
            {GameInterpreter.CommandTypes.Shake,2},
            {GameInterpreter.CommandTypes.Wait,1},
            {GameInterpreter.CommandTypes.PlayBGM,2},
            {GameInterpreter.CommandTypes.FadeoutBGM,1},
            {GameInterpreter.CommandTypes.PlaySE,2},
            {GameInterpreter.CommandTypes.StopSE,1},

            // 3
            {GameInterpreter.CommandTypes.EvalScript,1},

            // 4
            {GameInterpreter.CommandTypes.EndCmd,0}
        };
        return d[this.code];
    }

    public string getTitle() {
        Dictionary<GameInterpreter.CommandTypes, string> d = new Dictionary<GameInterpreter.CommandTypes, string> {
            // 1
            {GameInterpreter.CommandTypes.ShowArticle, "显示文章"},
            {GameInterpreter.CommandTypes.Condition,"条件分歧"},
            {GameInterpreter.CommandTypes.CommandBreak,"跳出循环"},
            {GameInterpreter.CommandTypes.CommonEvent,"公共事件"},
            {GameInterpreter.CommandTypes.Comment,"注释"},
            {GameInterpreter.CommandTypes.SetSwitch,"开关操作"},
            {GameInterpreter.CommandTypes.SetVariable,"变量操作"},
            {GameInterpreter.CommandTypes.SetSelfSwitch,"独立开关"},

            // 2
            {GameInterpreter.CommandTypes.Transformation,"场所移动"},
            {GameInterpreter.CommandTypes.SetPos,"设置事件位置"},
            {GameInterpreter.CommandTypes.MoveByRoute,"设置移动路径"},
            {GameInterpreter.CommandTypes.Erase,"暂时消除事件"},
            {GameInterpreter.CommandTypes.Shake,"屏幕震动"},
            {GameInterpreter.CommandTypes.Wait,"等待"},
            {GameInterpreter.CommandTypes.PlayBGM,"播放BGM"},
            {GameInterpreter.CommandTypes.FadeoutBGM,"淡出BGM"},
            {GameInterpreter.CommandTypes.PlaySE,"播放SE"},
            {GameInterpreter.CommandTypes.StopSE,"停止SE"},

            // 3
            {GameInterpreter.CommandTypes.EvalScript,"脚本"},

            // 4
            {GameInterpreter.CommandTypes.EndCmd,"end"}
        };

        string title = d[this.code];
        if (this.code == GameInterpreter.CommandTypes.Condition) {
        } else if(this.code == GameInterpreter.CommandTypes.MoveByRoute) {
        } else if (this.code == GameInterpreter.CommandTypes.EvalScript) {
        } else if (this.code == GameInterpreter.CommandTypes.ShowArticle) {
            if (this.args[0] != null) {
                title = title + ": " + this.args[0].Replace("\n", "").Substring(0, Mathf.Min(this.args[0].Replace("\n", "").Length, 20));
            }
        } else {
            title = title + ": " + string.Join(", ", this.args);
        }
        return title;
    }
}
