using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Item : BaseItem {
	public Item() {

	}
	public Item(int id, string name, string desc, int commonEventId, bool consumable, string note) : base(id, name, desc, note) {
        this.consumable = consumable;
		this.commonEventId = commonEventId;
	}
    /// <summary>
    /// 消耗品
    /// </summary>
    public bool consumable = false;
	/// <summary>
	/// 使用后触发的公共事件id，0不触发
	/// </summary>
	public int commonEventId = 0;
}
