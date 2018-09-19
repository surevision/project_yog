using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(EventCommand))]
public class EventCommandEditor : Editor {
    protected EventCommand m_Target;

    private GameInterpreter.CommandTypes code;

    protected virtual void initTarget(Object target) {
        // target可以让我们得到当前绘制的Component对象
        m_Target = (EventCommand)target;
    }

    // 重载OnInspectorGUI()来绘制自己的编辑器
    public override void OnInspectorGUI() {
        this.initTarget(target);
        // DrawDefaultInspector告诉Unity按照默认的方式绘制面板，这种方法在我们仅仅想要自定义某几个属性的时候会很有用
        DrawDefaultInspector();
        DrawArgsAndNames();
    }

    private void DrawArgsAndNames() {
        if (this.code != this.m_Target.code) {
            // 自动填充args初始值
            this.initArgs();
            this.code = this.m_Target.code;
        }
        this.m_Target.gameObject.name = this.calcName();    // 更改节点名字
    }

    private string calcName() {
        return this.m_Target.code.ToString();
    }

    private void initArgs() {
        //this.m_Target.args = new string[1];
    }
}