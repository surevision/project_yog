using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WindowMessage : WindowBase {
    private string _text;

    public string text {
        get { return _text; }
        set { 
            _text = value;
        }
    }

    public SLRichText richText;
    public Sprite bgNormal;
    public Sprite bgBlack;

    private const int OpenSpeed = 255 / 8;   //展开速度
	private bool showFast = false;	    // 快速显示所有
    private int waitCount = 0;	        // 等待帧数
	private int openness = 0;	        // 展开的程度
    private bool openning = true;
	private bool closing = false;
	private bool finished = false;
    private bool needClose = true;
    private int showIndex = 0;
    
    public void startMessage(bool needOpen, bool needClose) {
		this.text = GameTemp.gameMessage.text;
		this.showFast = false;	// 快速显示所有
		this.waitCount = 0;	// 等待帧数
		this.openness = 0;	// 展开的程度
		this.openning = true;
		this.closing = false;
		this.finished = false;
        if (!needOpen) {
            this.openness = 255;
            this.openning = false;
        }
        this.needClose = needClose;
        // 设定显示位置
        Vector2Int posInt = this.getPos(GameTemp.gameMessage.position);
        this.gameObject.GetComponent<RectTransform>().position = new Vector3(posInt.x, posInt.y, this.gameObject.GetComponent<RectTransform>().position.z);
        // 设定背景
        this.setBg(GameTemp.gameMessage.background);
		// 初始化显示控件
		if (this.richText) {
            this.richText.text = this.text;		// 初始化文字
            this.richText.clear();
            this.richText.refresh();
		}
        this.showIndex = 0;	// 当前显示的文字
	}

    /// <summary>
    /// 屏幕位置坐标
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    private Vector2Int getPos(GameMessage.ScreenPos pos) {
        int x = 400;
        int y = 0;
        switch (pos) {
            case GameMessage.ScreenPos.Bottom:
                y = 114;
                break;
            case GameMessage.ScreenPos.Middle:
                y = 300;
                break;
            case GameMessage.ScreenPos.Top:
                y = 454;
                break;
        }
        return new Vector2Int(x, y);
    }

    /// <summary>
    /// 背景
    /// </summary>
    /// <param name="bgType"></param>
    /// <returns></returns>
    private void setBg(GameMessage.BgType bgType) {
		Image img = this.gameObject.transform.Find("skin").gameObject.GetComponent<Image>();
        switch (bgType) {
            case GameMessage.BgType.Normal:
		        img.sprite = bgNormal;
                break;
            case GameMessage.BgType.Black:
                img.sprite = bgBlack;
                break;
            case GameMessage.BgType.None:
                img.sprite = null;
                break;
        }
    }

    public void update() {
        if (!GameTemp.gameMessage.isBusy()) {
            return;
        }
        if (this.finished && !this.closing) {
            if (InputManager.isTrigger(InputManager.GameKey.C)) {	// 按下确认
                this.closing = true;	// 开始关闭
                this.finished = false;
            }
            return;
        }
        if (!this.openning && !this.closing) {
            // 未显示完
            if (InputManager.isTrigger(InputManager.GameKey.C)) {
                this.showIndex = this.richText.contents.Count;	// 快速显示
                this.finished = true;
            }
            if (this.waitCount > 0 && !this.finished) {
                this.waitCount -= 1;	// 执行等待
            } else {
                if (this.showIndex > this.richText.contents.Count) {
                    // 显示完
                    this.finished = true;
                } else {
                    // 按次序显示文字
                    for (int i = 0; i < this.showIndex; i += 1) {
                        if (this.richText.contents[i]) {
                            if (this.richText.contents[i].GetComponent<TMP_Text>() != null ) { // TODO只处理文字
                                this.richText.contents[i].GetComponent<TMP_Text>().color =
                                    new Color(this.richText.contents[i].GetComponent<TMP_Text>().color.r,
                                        this.richText.contents[i].GetComponent<TMP_Text>().color.g,
                                        this.richText.contents[i].GetComponent<TMP_Text>().color.b,
                                        1);
                            }
                        }
                    }
                    this.showIndex += 1;
                    this.waitCount = 4;
                }
            }
        } else {
            if (this.openning) {	// 刷新打开
                this.openness += OpenSpeed;
                this.gameObject.transform.localScale = new Vector3(1, Mathf.Min(this.openness, 255) / 255.0f, this.gameObject.transform.localScale.z);
                if (this.openness >= 255) {
                    this.gameObject.transform.localScale = new Vector3(1, 1, this.gameObject.transform.localScale.z);
                    this.openning = false;
                }
            } else if (this.closing) {	// 刷新关闭
                if (!this.needClose) {
                    this.closing = false;
                    GameTemp.gameMessage.onFinish();    // 显示完毕回调
                } else {
                    this.openness -= OpenSpeed;
                    this.gameObject.transform.localScale = new Vector3(1, Mathf.Min(this.openness, 255) / 255.0f, this.gameObject.transform.localScale.z);
                    if (this.openness <= 0) {
                        this.gameObject.transform.localScale = new Vector3(1, 0, this.gameObject.transform.localScale.z);
                        this.closing = false;
                        GameTemp.gameMessage.onFinish();    // 显示完毕回调
                    }
                }
            }
        }
    }

}
