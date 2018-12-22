using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BaseItem {
	/// <summary>
	/// ID
	/// </summary>
	public int id = 0;

	/// <summary>
	/// 物品名
	/// </summary>
	public string name = "";

	/// <summary>
	/// 物品描述
	/// </summary>
	public string description = "";

	/// <summary>
	/// 备注
	/// </summary>
	public string note = "";
}
