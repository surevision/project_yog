﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
public class CopyGameObjectPathEditor : Editor {
    /// <summary>
    /// 剪切板
    /// </summary>
    public class ClipBoard
    {
        /// <summary>
        /// 将信息复制到剪切板当中
        /// </summary>
        public static void Copy(string format,params object[] args)
        {
            string result = string.Format(format,args);
            TextEditor editor = new TextEditor();
            editor.content = new GUIContent( result);
            editor.OnFocus();
            editor.Copy();
        }
    }
    [MenuItem("Commands/Copy/ObjectPath")]
    private static void CopyGameObjectPath()
    {
        UnityEngine.Object obj = Selection.activeObject;
        if (obj == null)
        {
            Debug.LogError("You must select Obj first!");
            return;
        }
        string result = AssetDatabase.GetAssetPath(obj);
        if (string.IsNullOrEmpty(result))//如果不是资源则在场景中查找
        {
            Transform selectChild = Selection.activeTransform;
            if (selectChild != null)
            {
                result = selectChild.name;
                while (selectChild.parent != null)
                {
                    selectChild = selectChild.parent;
                    result = string.Format("{0}/{1}", selectChild.name, result);
                }
            }
        }
        ClipBoard.Copy(result);
        Debug.Log(string.Format("The gameobject:{0}'s path has been copied to the clipboard!", obj.name));
        Debug.Log(string.Format("The gameobject:{0}'s path：{1}", obj.name, result));
    }
}
