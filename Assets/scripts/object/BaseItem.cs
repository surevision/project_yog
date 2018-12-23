using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BaseItem {
	public BaseItem() { }
	public BaseItem(int id, string name, string desc, string note) {
		this.id = id;
		this.name = name;
		this.description = desc;
		this.note = note;
	}
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
