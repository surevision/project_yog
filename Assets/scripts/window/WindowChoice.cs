using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WindowChoice : WindowBase {
    private string _text;

    public string text {
        get { return _text; }
        set { 
            _text = value;
        }
    }

    public SLRichText richText;
    public GameObject choicePanel;


    private List<GameObject> choiceItems = new List<GameObject>();
    private List<GameChoice.Choice> choices = new List<GameChoice.Choice>();

    private const int OpenSpeed = 255 / 8;   //展开速度
    private bool showFast = false;      // 快速显示所有
    private int waitCount = 0;          // 等待帧数
    private int openness = 0;           // 展开的程度
    private bool openning = true;
    private bool closing = false;
    private bool finished = false;
    private bool needClose = true;
    private int showIndex = 0;
    private int choiceIndex = 0;

    public void startMessage(bool needOpen, bool needClose) {
        this.text = GameTemp.gameChoice.title;
        this.choices = GameTemp.gameChoice.choices;
        this.showFast = false;  // 快速显示所有
        this.waitCount = 0; // 等待帧数
        this.openness = 0;  // 展开的程度
        this.openning = true;
        this.closing = false;
        this.finished = false;
        this.choiceIndex = 0;
        if (!needOpen) {
            this.openness = 255;
            this.openning = false;
        }
        this.needClose = needClose;
        // 初始化显示控件
        if (this.richText) {
            Debug.Log("has rich text");
            this.richText.text = this.text;     // 初始化文字
            this.richText.clear();
            this.richText.refresh();
        } else {
            Debug.Log("rich text not set");
        }
        // 初始化选项控件
        foreach (GameObject item in this.choiceItems) {
            GameObject.Destroy(item);
        }
        this.choiceItems.Clear();
        foreach (GameChoice.Choice choice in this.choices) {
            GameObject choiceItem = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("prefabs/ui/messages/ChoiceItem"));
            TMP_Text label = choiceItem.transform.Find("text").GetComponent<TMP_Text>();
            label.text = choice.content;
            choiceItem.transform.SetParent(this.choicePanel.transform);
            choiceItem.transform.Find("cursor").gameObject.SetActive(false);
            choiceItem.SetActive(false);
            this.choiceItems.Add(choiceItem);
        }
        this.showIndex = 0; // 当前显示的文字
    }

    public void update() {
        if (!GameTemp.gameChoice.isBusy()) {
            return;
        }
        if (this.finished && !this.closing) {
            if (InputManager.isTrigger(InputManager.GameKey.C)) {   // 按下确认
                this.closing = true;    // 开始关闭
                this.finished = false;
            }
            if (InputManager.isTrigger(InputManager.GameKey.UP)) {   // 
                this.choiceIndex = Mathf.Max(this.choiceIndex - 1, 0);
            }
            if (InputManager.isTrigger(InputManager.GameKey.DOWN)) {   // 
                this.choiceIndex = Mathf.Min(this.choiceIndex + 1, this.choices.Count - 1);
            }
            for (int i = 0; i < this.choiceItems.Count; i += 1) {
                GameObject choiceItem = this.choiceItems[i];
                choiceItem.transform.Find("cursor").gameObject.SetActive(false);
                if (i == this.choiceIndex) {
                    choiceItem.transform.Find("cursor").gameObject.SetActive(true);
                }
            }
            return;
        }
        if (!this.openning && !this.closing) {
            // 未显示完
            if (InputManager.isTrigger(InputManager.GameKey.C)) {
                this.showIndex = this.richText.contents.Count;  // 快速显示
				this.finished = true;
				for (int i = 0; i < this.choices.Count; i += 1) {
					if (this.choiceItems[i]) {
						this.choiceItems[i].SetActive(true);
						this.choiceItems[i].transform.localScale = new Vector3(1, 1, this.choiceItems[i].transform.localScale.z);
					}
				}
				this.waitCount = 1;
            }
            if (this.waitCount > 0 && !this.finished) {
                this.waitCount -= 1;    // 执行等待
            } else {
                if (this.showIndex > this.richText.contents.Count) {
                    // 显示完
                    this.finished = true;
                    // 展示选项
                    for (int i = 0; i < this.choices.Count; i += 1) {
                        if (this.choiceItems[i]) {
                            this.choiceItems[i].SetActive(true);
                            this.choiceItems[i].transform.localScale = new Vector3(1, 1, this.choiceItems[i].transform.localScale.z);
                        }
                    }

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
            if (this.openning) {    // 刷新打开
                this.openness += OpenSpeed;
                this.gameObject.transform.localScale = new Vector3(1, Mathf.Min(this.openness, 255) / 255.0f, this.gameObject.transform.localScale.z);
                if (this.openness >= 255) {
                    this.gameObject.transform.localScale = new Vector3(1, 1, this.gameObject.transform.localScale.z);
                    this.openning = false;
                }
            } else if (this.closing) {  // 刷新关闭
                if (!this.needClose) {
                    this.closing = false;
                    GameTemp.gameMessage.onFinish();    // 显示完毕回调
                } else {
                    this.openness -= OpenSpeed;
                    this.gameObject.transform.localScale = new Vector3(1, Mathf.Min(this.openness, 255) / 255.0f, this.gameObject.transform.localScale.z);
                    if (this.openness <= 0) {
                        this.gameObject.transform.localScale = new Vector3(1, 0, this.gameObject.transform.localScale.z);
                        this.closing = false;
                        GameTemp.gameChoice.onFinish(this.choiceIndex);    // 显示完毕回调
                    }
                }
            }
        }
    }

}
