using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Item : BaseItem {
	public Item() {

	}
	public Item(int id, string name, string desc, int commonEventId, string note) : base(id, name, desc, note) {
		this.commonEventId = commonEventId;
	}
	/// <summary>
	/// 使用后触发的公共事件id，0不触发
	/// </summary>
	public int commonEventId = 0;
}
