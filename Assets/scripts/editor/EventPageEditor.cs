using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(EventPage))]
public class EventPageEditor : Editor {
    protected EventPage m_Target;

    protected virtual void initTarget(Object target) {
        // target可以让我们得到当前绘制的Component对象
        m_Target = (EventPage)target;
    }

    // 重载OnInspectorGUI()来绘制自己的编辑器
    public override void OnInspectorGUI() {
        this.initTarget(target);
        // DrawDefaultInspector告诉Unity按照默认的方式绘制面板，这种方法在我们仅仅想要自定义某几个属性的时候会很有用
        DrawDefaultInspector();
        DrawMySpriteCharacterAttr();
    }

    void CalcPos(Vector3 pos, ref float x, ref float y, ref float realX, ref float realY) {
        x = (int)pos.x;
        y = (int)pos.y;
        realX = pos.x;
        realY = pos.y;
    }

    private void DrawMySpriteCharacterAttr() {

        SpriteRenderer sr = m_Target.transform.GetComponent<SpriteRenderer>();
        if (m_Target.BaseFrame != null) {
            string frameName = m_Target.BaseFrame.name;
            Regex reg = new Regex(@"^(.+)_(\d+)$");
            Match match = reg.Match(frameName);
            string fileName = match.Groups[1].Value;
            int index = int.Parse(match.Groups[2].Value);

            // 检测朝向变化
            GameCharacterBase.DIRS[] dirs = {
                GameCharacterBase.DIRS.DOWN,
                GameCharacterBase.DIRS.LEFT,
                GameCharacterBase.DIRS.RIGHT,
                GameCharacterBase.DIRS.UP,
            };

            if (index / 3 != (int)m_Target.direction / 2 &&
                    sr.sprite == m_Target.BaseFrame) {

                string path = string.Format("graphics/character/{0}", m_Target.characterName);
                Sprite[] sprites = Resources.LoadAll<Sprite>(path);
                int pattern = m_Target.pattern;
                int i = ((int)m_Target.direction / 2 - 1) * 3 + (pattern % 3);
                //Debug.Log(string.Format("i, {0}", i));
                m_Target.BaseFrame = sprites[i];
                sr.sprite = sprites[i];
            }

            // 根据图像修改行走图配置
            if (sr.sprite != m_Target.BaseFrame) {
                // 改变状态前，使用该方法来记录操作，以便之后Undo
                Undo.RecordObject(m_Target, "Change Character BaseFrame");
                sr.sprite = m_Target.BaseFrame;
                // 统一为单角色图片
                m_Target.characterName = fileName;
                m_Target.direction = dirs[(index / 3) % 3];
                m_Target.pattern = index % 3;
            }
        }
    }
}