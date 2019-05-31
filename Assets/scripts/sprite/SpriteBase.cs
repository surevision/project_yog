using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteBase : MonoBehaviour {

	// 动画相关
	class SpriteAnimInfo {
		public int animFrame = -1;
		public Sprite[] animSprites = null;
		public string animName = "";
		public bool animLoop = false;
	}
	class Anime : MonoBehaviour {	// 占位类
	}


	private List<SpriteAnimInfo> animeInfos = new List<SpriteAnimInfo>();

    public virtual void update() {
		// 刷新动画
		if (this.hasAnim()) {
			for (int i = this.animeInfos.Count - 1; i >= 0; i -= 1) {
				SpriteAnimInfo info = this.animeInfos[i];
				GameObject animObj = this.gameObject.transform.GetComponentsInChildren<Anime>()[i].gameObject;
				info.animFrame += 1;
				if (info.animFrame > info.animSprites.Length * this.animRate()) {	// 播放完毕
					if (info.animLoop) {
						// 循环动画
						info.animFrame = 0;
					} else {
						// 删除动画
						this.animeInfos.RemoveAt(i);
						GameObject.Destroy(animObj);
					}
				} else {	// 显示下一帧
					if (info.animFrame % this.animRate() == 0) {
						SpriteRenderer sr = animObj.GetComponent<SpriteRenderer>();
						sr.sprite = info.animSprites[info.animFrame / this.animRate()];
					}
				}
			}
		}
    }

	/// <summary>
	/// 动画减速
	/// </summary>
	/// <returns>The rate.</returns>
	private int animRate() {
		return 2;
	}

	public virtual bool hasAnim() {
		return this.animeInfos.Count > 0;
	}

	/// <summary>
	/// 播放动画
	/// </summary>
	/// <param name="animName">Animation name.</param>
	/// <param name="animLoop">If set to <c>true</c> animation loop.</param>
	public virtual void playAnim(string animName, bool animLoop = false) {
		SpriteAnimInfo animInfo = new SpriteAnimInfo();
		animInfo.animName = animName;
		animInfo.animFrame = 0;
		animInfo.animLoop = animLoop;
		this.animeInfos.Add(animInfo);
		this.startAnim(animInfo);
	}

	/// <summary>
	/// 停止一个循环动画
	/// </summary>
	/// <param name="animName">Animation name.</param>
	public virtual void stopLoopAnim(string animName) {
		for (int i = this.animeInfos.Count - 1; i >= 0; i -= 1) {
			SpriteAnimInfo info = this.animeInfos[i];
			if (info.animName.Equals(animName)) {
				GameObject animObj = this.gameObject.transform.GetComponentsInChildren<Anime>()[i].gameObject;
				this.animeInfos.RemoveAt(i);
				GameObject.Destroy(animObj);
			}
		}
	}

	private void startAnim(SpriteAnimInfo animInfo) {
		GameObject animSpriteObj = new GameObject("anim");
		animSpriteObj.AddComponent<Anime>();
		SpriteRenderer sr = animSpriteObj.AddComponent<SpriteRenderer>();
		string path = string.Format("graphics/animation/{0}", animInfo.animName);
		animInfo.animSprites = Resources.LoadAll<Sprite>(path);
		sr.sprite = animInfo.animSprites[0];
		SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
		sr.sortingLayerName = spriteRenderer.sortingLayerName;
		sr.sortingOrder = spriteRenderer.sortingOrder + 1;
		animSpriteObj.transform.SetParent(this.gameObject.transform);
		animSpriteObj.transform.localPosition = new Vector3(0, this.GetComponent<SpriteRenderer>().bounds.size.y / 2, 0);
	}
}
