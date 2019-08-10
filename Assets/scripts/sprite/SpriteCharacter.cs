using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class SpriteCharacter : SpriteBase {

    [SerializeField]
    private Sprite baseFrame;

    public Sprite BaseFrame {
        get {
            return baseFrame;
        }
        set {
            baseFrame = value;
        }
    }

    //[SerializeField]
    //private GameCharacterBase _character;

    public virtual GameCharacterBase character 
    {
        get { return null; }
    }


    protected string characterName = "";

    // 渲染相关
    private Sprite[] sprites;

	// Use this for initialization
	void Start () {
	}

    public override void update() {
        base.update();
        this.updateBitmap();
        this.updatePattern();
        this.updatePosition();
        this.character.isDirty = false;
    }

    private void updatePosition() {
        this.transform.rotation = Quaternion.Euler(new Vector3(0, 0, this.character.angle));
        if (this.character.isDirty) {
            this.GetComponent<SpriteRenderer>().color = new Color(
                this.GetComponent<SpriteRenderer>().color.r,
                this.GetComponent<SpriteRenderer>().color.g,
                this.GetComponent<SpriteRenderer>().color.b,
                this.character.opacity / 255.0f
                );
		}
		// 强制层级设置（角色之上 角色之下）
		if (this.character.GetType() == typeof(GameEvent)) {
			if (this.character.priorityType == GameCharacterBase.PRIORITIES.BELOW) {
				// 角色之下
				this.GetComponent<SpriteRenderer>().sortingOrder = 190;
			}
			if (this.character.priorityType == GameCharacterBase.PRIORITIES.UPPER) {
				// 角色之下
				this.GetComponent<SpriteRenderer>().sortingOrder = 210;
			}
		}
		this.transform.position = new Vector3(this.character.screenX(), this.character.screenY(), this.character.screenY());
    }

    /// <summary>
    /// 检测换图
    /// </summary>
    protected virtual void updateBitmap() {
        if (!this.characterName.Equals(this.character.characterName)) {
            this.changeBitmap(this.character.direction, this.character.pattern, true);
            this.characterName = this.character.characterName;
        }
    }

    /// <summary>
    /// 更新步行图
    /// </summary>
    private void updatePattern() {
        int pattern = this.character.pattern < 3 ? this.character.pattern : 1;  // 0 1 2 1 循环
        if (this.character.isDirty) {
            this.changeBitmap(this.character.direction, pattern);
        }
    }

    /// <summary>
    /// 根据图像修改行走图配置
    /// </summary>
    /// <param name="direction">方向</param>
    /// <param name="pattern">踏步图序号</param>
    /// <param name="isNew">是否是新图案</param>
    private void changeBitmap(GameCharacterBase.DIRS direction, int pattern, bool isNew = false) {
        SpriteRenderer sr = this.transform.GetComponent<SpriteRenderer>();
        if (isNew) {
            string path = string.Format("graphics/character/{0}", this.character.characterName);
            this.sprites = Resources.LoadAll<Sprite>(path);
        }
        int index = ((int)direction / 2 - 1) * 3 + (pattern % 3);
        if ("".Equals(this.character.characterName)) {
            sr.sprite = null;
        } else {
            sr.sprite = this.sprites[index];
        }
    }
}