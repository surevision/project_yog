using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(SpriteCharacter))]
public class CharacterBaseEditor : Editor
{
    SpriteCharacter m_Target;

    // 重载OnInspectorGUI()来绘制自己的编辑器
    public override void OnInspectorGUI()
    {
        // target可以让我们得到当前绘制的Component对象
        m_Target = (SpriteCharacter)target;

        // DrawDefaultInspector告诉Unity按照默认的方式绘制面板，这种方法在我们仅仅想要自定义某几个属性的时候会很有用
        DrawDefaultInspector();
        DrawMySpriteCharacterAttr();
    }

    void CalcPos(Vector3 pos, ref int x, ref int y, ref float realX, ref float realY)
    {
        x = (int)pos.x;
        y = (int)pos.y;
        realX = pos.x;
        realY = pos.y;
    }

    void DrawMySpriteCharacterAttr()
    {
        // 修改坐标
        if (m_Target.transform.position.x != m_Target.character.realX ||
            m_Target.transform.position.y != m_Target.character.realY)
        {
            Undo.RecordObject(m_Target, "Change Character Position");
            CalcPos(
                m_Target.transform.position,
                ref m_Target.character.x,
                ref m_Target.character.y,
                ref m_Target.character.realX,
                ref m_Target.character.realY
                );
        }

        SpriteRenderer sr = m_Target.transform.GetComponent<SpriteRenderer>();
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
        if (index / 3 != (int)m_Target.character.direction / 2 &&
                sr.sprite == m_Target.BaseFrame) {

            string path = string.Format("graphics/character/{0}", m_Target.character.characterName);
            Sprite[] sprites = Resources.LoadAll<Sprite>(path);
            int pattern = m_Target.character.pattern;
            int i = ((int)m_Target.character.direction / 2 - 1) * 3 + (pattern % 3);
            Debug.Log(string.Format("i, {0}", i));
            m_Target.BaseFrame = sprites[i];
            sr.sprite = sprites[i];
        }

        // 根据图像修改行走图配置

        if (sr.sprite != m_Target.BaseFrame)
        {
            // 改变状态前，使用该方法来记录操作，以便之后Undo
            Undo.RecordObject(m_Target, "Change Character BaseFrame");
            sr.sprite = m_Target.BaseFrame;
            //Debug.Log("fileName:" + fileName + " index:" + index);
            //if (frameName.StartsWith("$") || frameName.StartsWith("!$") || frameName.StartsWith("$!"))
            //{
            //    m_Target.character.characterName = fileName;
            //    m_Target.character.characterIndex = 0;
            //    m_Target.character.direction = dirs[(index / 3) % 3];
            //}
            //else
            //{
            //    m_Target.character.characterName = fileName;
            //    m_Target.character.characterIndex = (index / (3 * 4 * 4)) * 4 + ((index % (3 * 4)) / 3);
            //    m_Target.character.direction = dirs[((index % (3 * 4 * 4)) / (3 * 4)) % (3 * 4)];
            //}

            // 统一为单角色图片
            m_Target.character.characterName = fileName;
            m_Target.character.characterIndex = 0;
            m_Target.character.direction = dirs[(index / 3) % 3];
            m_Target.character.pattern = index % 3;
        }
    }
}