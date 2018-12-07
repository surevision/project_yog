using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SLRichText : MonoBehaviour {

    private const string C_CHR = "$";
    private const float OffsetX = 0;
    private const float OffsetY = 0;

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
    private int _defaultFontSize = 24;
    private float _lineHeight = 12;
    private float _height = 0;
    
    private List<GameObject> _texts;
    private List<GameObject> _images;
    private List<GameObject> _contents;
    public List<GameObject> contents {
        get { return _contents; }
    }
    private List<GameObject> _currentLine;

    private int _strIndex = 0;
    private int _currentColor = 0;
    private int _currentFontSize = 12;

    private RectTransform rectTransform;

    private class RenderPos{
        public float x = OffsetX;
        public float y = OffsetY;
        public float baseY = 0;         // 本行基准y
        public float maxHeight = 0;     // 记录本行最大高度
        public float lastHeight = 0;     // 上个元素的高度
    }

    private RenderPos _renderPos = new RenderPos();

	// Use this for initialization
    void Start() {

        this.rectTransform = this.container.GetComponent<RectTransform>();

        Debug.Log(string.Format("rectTransform {0}, {1}", rectTransform.sizeDelta.x, rectTransform.sizeDelta.y));

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
            this._texts = new List<GameObject>();
        }

        // 图片
        if (this._images == null) {
            this._images = new List<GameObject>();
        }
        
        // 包含文字图片等实际顺序的数组
        if (this._contents == null) {
            this._contents = new List<GameObject>();
        }
        
        // 渲染中的本行元素
        if (this._currentLine == null) {
            this._currentLine = new List<GameObject>();
        }


		this._strIndex = 0; // 遍历的当前字符
		this._currentColor = 0;  // 当前文字颜色
		this._currentFontSize = this._defaultFontSize;   // 当前文字大小
		
		for (int i = 0; i < this._texts.Count; i += 1) {
			GameObject.Destroy((GameObject)this._texts[i]);
		}
		for (int i = 0; i < this._images.Count; i += 1) {
			GameObject.Destroy((GameObject)this._images[i]);
		}
        
        this._texts.Clear();
        this._images.Clear();
        this._contents.Clear();
        this._currentLine.Clear();

        this._renderPos.x = -rectTransform.sizeDelta.x / 2 + this._defaultFontSize / 2 + OffsetX;
        this._renderPos.y = rectTransform.sizeDelta.y / 2 - (this._defaultFontSize + OffsetY);
        this._renderPos.baseY = this._renderPos.y;
        this._renderPos.maxHeight = this._currentFontSize;
	}

    public Color getColor(int code = 0) {
		Color[] colors = {
					this._defaultFontColor,
                    new Color(1, 1, 1, 1),  		// 白色
                    new Color(0, 0, 0, 1),            	// 黑色
                    new Color(1, 0, 0, 1),          	// 红色
                    new Color(0, 1, 0, 1),          	// 绿色
                    new Color(0, 0, 1, 1),          	// 蓝色
                    new Color(1, 1, 0, 1),         	// 黄色
                    new Color(1, 0, 1, 1),         	// 紫色
                    new Color(0, 1, 1, 1)         	// 青色
        };
        if (code < colors.Length) {
            return colors[code];
        }
		return colors[0];
	}

    public void refresh() {
        System.DateTime dt = System.DateTime.Now;
        //Debug.Log(string.Format("start at {0}", System.DateTime.Now));
		while (this._strIndex < this.text.Length) {
			this.progressContentString();
		}
		this.nextLine();
        this._height = Mathf.Max(-(this._renderPos.y - (rectTransform.sizeDelta.y / 2 - (this._defaultFontSize + OffsetY))), this._minHeight);
		if (this.isSingleLine) {
            this.rectTransform.sizeDelta = new Vector2(this._width, this._height);
		}
        this.adjustContentsY();
        //Debug.Log(string.Format("end at {0}", System.DateTime.Now));
        //Debug.Log(string.Format("dt {0}", System.DateTime.Now - dt));

	}

    /// <summary>
    /// 处理文本
    /// </summary>
    private void progressContentString() {
		// get one word
		string str = this.text.Substring(this._strIndex, 1);
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
			// Debug.Log(params);
			if (match.Success) {
				progressFlag = this.changeFontColor(int.Parse(match.Groups[1].Value));
				progressLen = match.Groups[1].Value.Length;
			} else {
				Debug.Log("fail changeFontColor");
			}
		} else if ("S".Equals(chr)) {
			// 修改字号
            Regex reg = new Regex(@"<(\d+)>");
            Match match = reg.Match(contentToCheck);
            // Debug.Log(params);
            if (match.Success) {
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
			//Debug.Log(string.Format("fail progress!, last code : {0}", chr));
			this._strIndex -= 2; // $X
			string str = this.text.Substring(this._strIndex, 1);
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
        //Debug.Log(string.Format("create Label {0}, size {1}", chr, this._currentFontSize));
		GameObject node = Instantiate<GameObject>(Resources.Load<GameObject>("prefabs/ui/messages/MessagChar"));
		node.name = chr;
		TMP_Text uiText = node.GetComponent<TMP_Text>();

        uiText.SetText(chr);
        
        RectTransform uiTextTransform = uiText.GetComponent<RectTransform>();
        uiText.fontSize = (int)(this._currentFontSize);
        uiTextTransform.sizeDelta = new Vector2(uiText.fontSize, uiText.fontSize);
        //float s = Util.getWidthScale();
        //uiTextTransform.localScale = new Vector3(s, s, uiTextTransform.localScale.z);

        uiText.color = this.getColor(this._currentColor);
        uiText.color = new Color(uiText.color.r, uiText.color.g, uiText.color.b, 0.0f); // 初始隐藏
        node.transform.SetParent(this.container.transform);
        node.transform.localScale = new Vector3(1, 1, node.transform.localScale.z);

        if (!this.isSingleLine && uiTextTransform.sizeDelta.x > this._width) {
			node.transform.localScale = new Vector3(this._width / uiTextTransform.sizeDelta.x, this._width / uiTextTransform.sizeDelta.x, 1);
		}
		if (!this.isSingleLine) {
            //Debug.Log(string.Format("curr x {0} {1}", chr, this._renderPos.x + uiTextTransform.sizeDelta.x / 2));
            if (this._renderPos.x - (-rectTransform.sizeDelta.x / 2 + OffsetX) + uiTextTransform.sizeDelta.x > this._width) {
				this.nextLine();
			}
        }
        this._renderPos.lastHeight = this._currentFontSize;
        this._renderPos.x += this._currentFontSize / 2;
        float lastMaxHeight = this._renderPos.maxHeight;
        this._renderPos.maxHeight = (this._renderPos.maxHeight > this._currentFontSize) ? this._renderPos.maxHeight : this._currentFontSize;
        if (lastMaxHeight != this._renderPos.maxHeight) {
            this._renderPos.y = this._renderPos.baseY + lastMaxHeight / 2 - this._renderPos.maxHeight / 2;
        }
        this.setElemPosition(node, this._renderPos.x, this._renderPos.y);
        this._renderPos.x += this._currentFontSize / 2;
        if (this.isSingleLine) {
			this._width = Mathf.Max(this._width, this._renderPos.x);
		}
		this._texts.Add(node);
		this._contents.Add(node);
		this._currentLine.Add(node);
		return true;
	}

    /// <summary>
    /// 修改颜色
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
	private bool changeFontColor(int code) {
        Debug.Log(string.Format("change color to {0}", code));
        this._currentColor = code;
        return true;
	}

    /// <summary>
    /// 修改字号
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    private bool changeFontSize(int code) {
        Debug.Log(string.Format("change font size to {0}", code));
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
			// Debug.Log("progress image %s", path);
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
        //Debug.Log(string.Format("setElemPosition {0}, {1}", _x, _y));
        RectTransform elemTransform = elem.GetComponent<RectTransform>();
        float x = _x;
        float y = _y;
        elem.transform.localPosition = new Vector3(x, y, elem.transform.localPosition.z);
        //Debug.Log(string.Format("after setElemPosition {0}, {1}", elem.transform.localPosition.x, elem.transform.localPosition.y));
	}

    private void setElemLocalPositionY(GameObject elem, float _y) {
        RectTransform elemTransform = elem.GetComponent<RectTransform>();
        elem.transform.localPosition = new Vector3(elem.transform.localPosition.x, _y, elemTransform.localPosition.z);
	}
    /// <summary>
    /// 换行
    /// </summary>
	private void nextLine() {
        // 调整当前行元素高度，适应maxHeight
        for (int i = 0; i < this._currentLine.Count; i += 1) {
            GameObject elem = this._currentLine[i];
            RectTransform elemTransform = elem.GetComponent<RectTransform>();
            this.setElemLocalPositionY(this._currentLine[i], this._renderPos.y - this._renderPos.maxHeight / 2 + elemTransform.sizeDelta.y / 2);
        }
        this._renderPos.x = -rectTransform.sizeDelta.x / 2 + this._defaultFontSize / 2 + OffsetX;
        this._renderPos.y -= this._lineHeight + this._renderPos.maxHeight / 2;
        this._renderPos.maxHeight = this._lineHeight; // 恢复lineheight
        this._renderPos.baseY = this._renderPos.y;
		this._currentLine.Clear();
	}

	private void adjustContentsY() {
		for (int i = 0; i < this._contents.Count; i += 1) {
			GameObject elem = (GameObject)this._contents[i];
			//elem.transform.position = new Vector3(elem.transform.position.x, elem.transform.position.y, elem.transform.position.z);
		}
	}
}
