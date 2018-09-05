using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteCharacter : SpriteBase {

    [SerializeField]
    public GameCharacterBase character;

    [SerializeField]
    private Sprite baseFrame;

    public Sprite BaseFrame
    {
        get
        {
            return baseFrame;
        }
        set
        {
            baseFrame = value;
        }
    }

	// Use this for initialization
	void Start () {
        //SpriteRenderer sr = transform.GetComponent<SpriteRenderer>();
        //Sprite[] sprites = Resources.LoadAll<Sprite>("Characters/!Flame");
        //Sprite sprite = sprites[10];
        //sr.sprite = sprite;
        
	}
	
	// Update is called once per frame
	void Update () {
		
	}


}