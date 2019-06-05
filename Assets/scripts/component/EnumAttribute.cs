using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
using System.Reflection;
using System.Text.RegularExpressions;
#endif

/// <summary>
/// 设置枚举名称
/// </summary>
#if UNITY_EDITOR
[AttributeUsage(AttributeTargets.Field)]
#endif
public class EnumAttirbute : PropertyAttribute
{
	/// <summary>
	/// 枚举名称
	/// </summary>
	public string name;
	public EnumAttirbute(string name)
	{
		this.name = name;
	}
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(EnumAttirbute))]
public class EnumNameDrawer : PropertyDrawer
{
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		// 替换属性名称
		EnumAttirbute enumAttirbute = (EnumAttirbute)attribute;
		label.text = enumAttirbute.name;

		bool isElement = Regex.IsMatch(property.displayName, "Element \\d+");
		if (isElement)
		{
			label.text = property.displayName;
		}


		if (property.propertyType == SerializedPropertyType.Enum)
		{
			DrawEnum(position, property, label);
		} 
		else if (property.isArray)
		{
			label.text = property.displayName;
		}
		else
		{
			EditorGUI.PropertyField(position, property, label, true);
		}
	}

	/// <summary>
	/// 重新绘制枚举类型属性
	/// </summary>
	/// <param name="position"></param>
	/// <param name="property"></param>
	/// <param name="label"></param>
	private void DrawEnum(Rect position, SerializedProperty property, GUIContent label)
	{
		EditorGUI.BeginChangeCheck();
		Type type = fieldInfo.FieldType;

		string[] names = property.enumNames;
		string[] values = new string[names.Length];
		while(type.IsArray)
		{
			type = type.GetElementType();
		}

		for (int i = 0; i < names.Length; ++i)
		{
			FieldInfo info = type.GetField(names[i]);
			EnumAttirbute[] enumAttributes = (EnumAttirbute[])info.GetCustomAttributes(typeof(EnumAttirbute), false);
			values[i] = enumAttributes.Length == 0 ? names[i] : enumAttributes[0].name;
		}

		int index = EditorGUI.Popup(position, label.text, property.enumValueIndex, values);
		if (EditorGUI.EndChangeCheck() && index != -1)
		{
			property.enumValueIndex = index;
		}
	}
}
#endif