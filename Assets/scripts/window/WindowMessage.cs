using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
    
	private bool showFast = false;	// 快速显示所有
    private int waitCount = 0;	// 等待帧数
	private int openness = 0;	// 展开的程度
    private bool openning = true;
	private bool closing = false;
	private bool finished = false;
    private int showIndex = 0;
    
	public void startMessage() {
		this.text = GameTemp.gameMessage.text;
		this.showFast = false;	// 快速显示所有
		this.waitCount = 0;	// 等待帧数
		this.openness = 0;	// 展开的程度
		this.openning = true;
		this.closing = false;
		this.finished = false;
		// 初始化显示控件
		if (this.richText) {
            this.richText.text = this.text;		// 初始化文字
            this.richText.clear();
            this.richText.refresh();
		}
        this.showIndex = 0;	// 当前显示的文字
	}

    public void update() {
        if (!GameTemp.gameMessage.isBusy()) {
            return;
        }
        if (this.finished && !this.closing) {
            if (Input.GetKeyDown(KeyCode.Space)) {	// 按下确认
                this.closing = true;	// 开始关闭
                this.finished = false;
            }
            return;
        }
        if (!this.openning && !this.closing) {
            // 未显示完
            if (Input.GetKeyDown(KeyCode.Space)) {
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
                this.openness += 16;
                this.gameObject.transform.localScale = new Vector3(1, Mathf.Min(this.openness, 255) / 255.0f, this.gameObject.transform.localScale.z);
                if (this.openness >= 255) {
                    this.openning = false;
                }
            } else if (this.closing) {	// 刷新关闭
                this.openness -= 16;
                this.gameObject.transform.localScale = new Vector3(1, Mathf.Min(this.openness, 255) / 255.0f, this.gameObject.transform.localScale.z);
                if (this.openness <= 0) {
                    this.closing = false;
                    GameTemp.gameMessage.onFinish();	// 显示完毕回调
                }
            }
        }
    }

}
