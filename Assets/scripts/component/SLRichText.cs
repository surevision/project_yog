using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class SLRichText : MonoBehaviour {

    private const string C_CHR = "$";

    [SerializeField]
    private GameObject container;

    [SerializeField]
    private string _text;

    public string text {
        get { return _text; }
        set { _text = value; }
    }

    [SerializeField]
    private float _width;

    public float width {
        get { return _width; }
        set { _width = value; }
    }

    [SerializeField]
    private bool _isSingleLine;

    public bool isSingleLine {
        get { return _isSingleLine; }
        set { _isSingleLine = value; }
    }

    [SerializeField]
    private float _minHeight = 64;

    public float minHeight {
      get { return _minHeight; }
      set { _minHeight = value; }
    }


    private Color _defaultFontColor = new Color(255, 255, 255, 255);
    private int _defaultFontSize = 12;
    private float _lineHeight = 10;
    private float _height = 0;
    
    private ArrayList _texts;
    private ArrayList _images;
    private ArrayList _contents;
    private ArrayList _currentLine;

    private int _strIndex = 0;
    private int _currentColor = 0;
    private int _currentFontSize = 12;

    private RectTransform rectTransform;

    private class RenderPos{
		public float x = 0;
		public float y = 0;
		public float maxHeight = 0;   // 记录本行最大高度
    }

    private RenderPos _renderPos = new RenderPos();

	// Use this for initialization
    void Start() {

        this.rectTransform = this.container.GetComponent<RectTransform>();

        this.clear();
	}
	
	// Update is called once per frame
	void Update () {
        if (this.container == null) {
            return;
        }
	}
    
	public void clear() {
		// this._defaultFontSize = DEFAULT_FONT_SIZE;
		// this._lineHeight = DEFAULT_LINE_HEIGHT;
		this._height = this.minHeight;//this._defaultFontSize;
		if (this.isSingleLine) {
			this._width = 0;
		}

        // 文字
        if (this._texts == null) {
            this._texts = new ArrayList();
        }

        // 图片
        if (this._images == null) {
            this._images = new ArrayList();
        }
        
        // 包含文字图片等实际顺序的数组
        if (this._contents == null) {
            this._contents = new ArrayList();
        }
        
        // 渲染中的本行元素
        if (this._currentLine == null) {
            this._currentLine = new ArrayList();
        }


		this._strIndex = 0; // 遍历的当前字符
		this._currentColor = 0;  // 当前文字颜色
		this._currentFontSize = this._defaultFontSize;   // 当前文字大小
		
		for (int i = 0; i < this._texts.Count; i += 1) {
			Destroy((GameObject)this._texts[i]);
		}
		for (int i = 0; i < this._images.Count; i += 1) {
			Destroy((GameObject)this._images[i]);
		}
        
        this._texts.Clear();
        this._images.Clear();
        this._contents.Clear();
        this._currentLine.Clear();

		this._renderPos.x = 0;
        this._renderPos.y = 0;
        this._renderPos.maxHeight = this._defaultFontSize;
	}

    public Color getColor(int code = 0) {
		Color[] colors = {
					this._defaultFontColor,
                    new Color(255, 255, 255, 255),  		// 白色
                    new Color(0, 0, 0, 255),            	// 黑色
                    new Color(255, 0, 0, 255),          	// 红色
                    new Color(0, 255, 0, 255),          	// 绿色
                    new Color(0, 0, 255, 255),          	// 蓝色
                    new Color(255, 255, 0, 255),         	// 黄色
                    new Color(255, 0, 255, 255),         	// 紫色
                    new Color(0, 255, 255, 255),         	// 青色
        };
        if (code < colors.Length) {
            return colors[code];
        }
		return colors[0];
	}

    private void refresh() {
		// this.node.scaleX = 2;
		// this.node.scaleY = 2;
		while (this._strIndex < this.text.Length) {
			this.progressContentString();
		}
		this.nextLine();
		this._height = Mathf.Max(this._renderPos.y, this._minHeight);
		if (this.isSingleLine) {
            this.rectTransform.sizeDelta = new Vector2(this._width, this._height);
		}
		this.adjustContentsY();
	}

    /// <summary>
    /// 处理文本
    /// </summary>
    private void progressContentString() {
		// get one word
		var str = this.text.Substring(this._strIndex, 1);
		this._strIndex += 1;
		if ("\r".Equals(str)) {
			// 回车
			return;
		} else if ("\n".Equals(str)) {
			// 换行
			this.progressNewLine();
		} else if (C_CHR.Equals(str)) {
			// 控制符开始
			// 处理控制字符，处理中会影响_strIndex
			this.progressControlChar();
		} else {
			// 普通文字
			this.progressNormalText(str);
		}
	}
    /// <summary>
    /// 处理换行
    /// </summary>
	private void progressNewLine() {
		this.nextLine();
	}

    /// <summary>
    /// 处理特殊字符
    /// </summary>
	private void progressControlChar() {
		string chr = this.text.Substring(this._strIndex, 1);
		this._strIndex += 1;
		chr = chr.ToUpper();
		bool progressFlag = false;	// 是否正常解析
		int progressLen = 0; // 参数字符数
		// get params( first <.*> regexp)
		string contentToCheck = this.text.Substring(this._strIndex);
		if ("N".Equals(chr)) {
			this.progressNewLine();
			return;
		} else if ("C".Equals(chr)) {
			// 修改颜色
            Regex reg = new Regex(@"<(\d+)>");
            Match match = reg.Match(contentToCheck);
			// cc.log(params);
			if (match != null) {
				progressFlag = this.changeFontColor(int.Parse(match.Groups[1].Value));
				progressLen = match.Groups[1].Value.Length;
			} else {
				Debug.Log("fail changeFontColor");
			}
		} else if ("S".Equals(chr)) {
			// 修改字号
            Regex reg = new Regex(@"<(\d+)>");
            Match match = reg.Match(contentToCheck);
			// cc.log(params);
			if (match != null) {
				progressFlag = this.changeFontSize(int.Parse(match.Groups[1].Value));
				progressLen = match.Groups[1].Value.Length;
			} else {
				Debug.Log("fail changeFontSize");
			}
		}
		if (progressFlag) {
			// 正常解析
			this._strIndex += 2; // "<>"
			this._strIndex += progressLen;
		} else {
			// 处理失败，包含$在内均当做普通字符处理
			Debug.Log(string.Format("fail progress!, last code : {0}", chr));
			this._strIndex -= 2; // $X
			var str = this.text.Substring(this._strIndex, 1);
			this._strIndex += 1;
			this.progressNormalText(str);
		}
	}

    /// <summary>
    /// 处理普通文字
    /// </summary>
    /// <param name="chr"></param>
    /// <returns></returns>
    private bool progressNormalText(string chr) {
		// cc.log("create cc.Label %s, size %s", chr, this._currentFontSize);
		GameObject node = Instantiate<GameObject>(Resources.Load<GameObject>("prefabs/ui/MessagChar"));
		node.name = chr;
		Text uiText = node.GetComponent<Text>();

		uiText.text = chr;
        
        RectTransform uiTextTransform = uiText.GetComponent<RectTransform>();
        uiText.color = new Color(uiText.color.r, uiText.color.g, uiText.color.b, 1.0f);
		uiText.fontSize = this._currentFontSize;
		uiText.lineSpacing = Mathf.Max(1.0f * this._lineHeight / this._currentFontSize, 1);
        
        uiText.color = this.getColor(this._currentColor);
        node.transform.SetParent(this.container.transform);

		if (!this.isSingleLine && uiTextTransform.sizeDelta.x > this._width) {
			node.transform.localScale = new Vector3(this._width / uiTextTransform.sizeDelta.x, this._width / uiTextTransform.sizeDelta.x, 1);
		}
		if (!this.isSingleLine) {
			if (this._renderPos.x + this._width / uiTextTransform.sizeDelta.x > this._width) {
				this.nextLine();
			}
		}
		this.setElemPosition(node, this._renderPos.x, this._renderPos.y);
		this._renderPos.x += uiTextTransform.sizeDelta.x;
        this._renderPos.maxHeight = (this._renderPos.maxHeight > uiTextTransform.sizeDelta.y) ? this._renderPos.maxHeight : uiTextTransform.sizeDelta.y;
		if (this.isSingleLine) {
			this._width = Mathf.Max(this._width, this._renderPos.x);
		}
		this._texts.Add(uiText);
		this._contents.Add(node);
		this._currentLine.Add(uiText);
		return true;
	}

    /// <summary>
    /// 修改颜色
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
	private bool changeFontColor(int code) {
        this._currentColor = code;
        return true;
	}

    /// <summary>
    /// 修改字号
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
	private bool changeFontSize(int code = 12) {
		this._currentFontSize = code;
		return true;
	}

    /// <summary>
    /// 插入图片
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
	private bool progressImage(string path) {
		if (path != null && (!"".Equals(path))) {
			// 暂时不处理
			// cc.log("progress image %s", path);
			// var uiImage = ccui.ImageView.create(path, ccui.Widget.LOCAL_TEXTURE);
			// uiImage.setVisible(this._show_fast);
			// this.addChild(uiImage);
			// if (uiImage.getContentSize().width > this._width) {
			// 	uiImage:setScale(this._width / uiImage.getContentSize().width);
			// }
			// if (this._renderPos.x + uiImage.getContentSize().width > this._width) {
			// 	this.nextLine();
			// }
			// this.setElemPosition(uiImage, this._renderPos.x, this._renderPos.y);
			// this._renderPos.x += uiImage.getContentSize().width;
			// this._renderPos.maxHeight = (this._renderPos.maxHeight > uiImage.getContentSize().height) ? this._renderPos.maxHeight : uiImage.getContentSize().height
			// this._images.push(uiImage);
			// this._contents.push(uiImage);
			// this._currentLine.push(uiImage);
			return true;
		}
		return false;
	}

    /// <summary>
    /// 调整位置
    /// </summary>
    /// <param name="elem"></param>
    /// <param name="_x"></param>
    /// <param name="_y"></param>
    private void setElemPosition(GameObject elem, float _x, float _y) {
		// cc.log("setElemPosition %s, %s", _x, _y);
        RectTransform elemTransform = elem.GetComponent<RectTransform>();
		elemTransform.anchoredPosition = new Vector2(0, 1);
		// elem.active = true;
		if (_x != null) {
			float x = _x + elemTransform.sizeDelta.x * elemTransform.anchoredPosition.x;
			elem.transform.position.Set(x, elem.transform.position.y, elem.transform.position.z);
		}
		if (_y != null) {
			float y = _y - elemTransform.sizeDelta.y / 2;
			elem.transform.position.Set(elem.transform.position.x, -y, elem.transform.position.z);
		}
	}

    private void setElemPositionY(GameObject elem, float _y) {
		// cc.log("setElemPosition %s, %s", _x, _y);
        RectTransform elemTransform = elem.GetComponent<RectTransform>();
		elemTransform.anchoredPosition = new Vector2(0, 1);
		if (_y != null) {
			float y = _y - elemTransform.sizeDelta.y / 2;
			elem.transform.position.Set(elem.transform.position.x, -y, elem.transform.position.z);
		}
	}
    /// <summary>
    /// 换行
    /// </summary>
	private void nextLine() {
		// 调整当前行元素高度，适应maxHeight
		for (int i = 0; i < this._currentLine.Count; i += 1) {
			this.setElemPositionY((GameObject)this._currentLine[i], this._renderPos.y + this._renderPos.maxHeight);
		}
		this._renderPos.x = 0;
		this._renderPos.y += this._renderPos.maxHeight + this._lineHeight;
		this._renderPos.maxHeight = this._defaultFontSize; // 恢复lineheight
		this._currentLine.Clear();
	}

	private void adjustContentsY() {
		for (int i = 0; i < this._contents.Count; i += 1) {
			GameObject elem = (GameObject)this._contents[i];
			elem.transform.position.Set(elem.transform.position.x, elem.transform.position.y + this._height, elem.transform.position.z);
		}
	}
}
