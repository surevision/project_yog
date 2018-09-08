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


    private string characterName = "";
    private int characterIndex = 0;
    private int characterPattern = 1;

    // 渲染相关
    private Sprite[] sprites;

	// Use this for initialization
	void Start () {
	}

    public override void update() {
        base.update();
        this.character.update();
        this.updateBitmap();
        this.updatePattern();
    }

    /// <summary>
    /// 检测换图
    /// </summary>
    private void updateBitmap() {
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
        this.changeBitmap(this.character.direction, pattern);
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
        sr.sprite = this.sprites[index];
    }
}