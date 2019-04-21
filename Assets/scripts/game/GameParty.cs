using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 存数玩家相关数据
/// </summary>
[Serializable]
public class GameParty {
    [Serializable]
    public class BagItem {
        public BaseItem item;
        public int num;
        public int tag;
        public string extData;
    }
    /// <summary>
    /// 物品
    /// </summary>
    public List<BagItem> items;
    public GameParty() {
        this.items = new List<BagItem>();
    }

    /// <summary>
    /// 获得物品
    /// </summary>
    /// <param name="id">物品id</param>
    /// <param name="num">数量</param>
    /// <param name="tag">使用id和tag确定一个背包物品</param>
    /// <param name="extData">附带数据</param>
    public void gainItem(int id, int num, int tag = 0, string extData = "") {
        BaseItem item = null;
        foreach (BaseItem _item in DataManager.dataItems) {
            if (_item.id == id) {
                item = _item;
                break;
            }
        }
        if (item != null) {
            int index = -1;
            for (int i = 0; i < this.items.Count; i += 1) {
                if (this.items[i].item.id == id && this.items[i].tag == tag) {
                    index = i;
                    break;
                }
            }
            if (index != -1) {
                this.items[index].num += num;
            } else {
                BagItem bagItem = new BagItem();
                bagItem.item = item;
                bagItem.num = num;
                bagItem.tag = tag;
                bagItem.extData = extData;
                this.items.Add(bagItem);
            }
        }
    }

    /// <summary>
    /// 失去物品
    /// </summary>
    /// <param name="id">物品id</param>
    /// <param name="num">数量</param>
    /// <param name="tag">使用id和tag确定一个背包物品</param>
    /// <param name="extData">附带数据</param>
    public void loseItem(int id, int num, int tag = 0, string extData = "") {
        BaseItem item = null;
        foreach (BaseItem _item in DataManager.dataItems) {
            if (_item.id == id) {
                item = _item;
                break;
            }
        }
        if (item != null) {
            int index = -1;
            for (int i = 0; i < this.items.Count; i += 1) {
                if (this.items[i].item.id == id && this.items[i].tag == tag) {
                    index = i;
                    break;
                }
            }
            if (index != -1) {
                this.items[index].num -= num;
                if (this.items[index].num <= 0) {
                    this.items.RemoveAt(index);
                }
            }
        }
    }

    /// <summary>
    /// 是否有物品
    /// </summary>
    /// <returns><c>true</c>, if item was hased, <c>false</c> otherwise.</returns>
    /// <param name="id">Identifier.</param>
    public bool hasItem(int id) {
        for (int i = 0; i < this.items.Count; i += 1) {
            if (this.items[i].item.id == id) {
                return true;
            }
        }
        return false;
    }
}
