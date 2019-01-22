using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CameraControl : MonoBehaviour {

	public GameObject _playerLightMask;
	private Material _playerLightMaskMaterial;

    public Material _screenMaterial = null;

	private Camera camera;
	private GameObject _target;
    private GameObject _player;
    private Vector3 _targetPos;

    private bool forcePos = false;

    private Vector3 _minTile;
    private Vector3 _maxTile;

    public GameObject target {
        get { return _target; }
        set { _target = value; }
    }
    public GameObject player {
        get { return _player; }
        set { _player = value; }
    }
    public Vector3 targetPos {
        get { return _targetPos; }
        set { _targetPos = value; }
	}
	public GameObject playerLightMask {
		get {
			return _playerLightMask;
		}
		set {
			_playerLightMask = value;
			_playerLightMaskMaterial = _playerLightMask.GetComponent<MeshRenderer>().material;
		}
	}

	public Vector3 minTile {
        get {
            return _minTile;
        }

        set {
            _minTile = value;
        }
    }

    public Vector3 maxTile {
        get {
            return _maxTile;
        }

        set {
            _maxTile = value;
        }
    }

    private float maxX, minX, maxY, minY;
	void Start() {
		camera = this.gameObject.GetComponent<Camera>();
		_playerLightMaskMaterial = this.playerLightMask.GetComponent<SpriteRenderer>().material;
	}
	public void setupPos(Vector3 minTile, Vector3 maxTile) {
        float width, height;
        height = GameObject.Find("Main Camera").GetComponent<Camera>().orthographicSize * 2f;
        width = height * GameObject.Find("Main Camera").GetComponent<Camera>().aspect;
        this.minX = minTile.x + width / 2f;
        this.maxX = maxTile.x - width / 2f;
        this.minY = minTile.y + height / 2f;
        this.maxY = maxTile.y - height / 2f;
    }

    public void update() {
        if (this.playerLightMask != null) {
            if (this.playerLightMask.activeSelf != GameTemp.gameScreen.maskVisible) {
                this.playerLightMask.SetActive(GameTemp.gameScreen.maskVisible);
            }
        }
        //强制转向坐标
        if (forcePos) {
            // 更新摄像机位置
            float x = Mathf.Clamp(this.targetPos.x, this.minX, this.maxX);
            float y = Mathf.Clamp(this.targetPos.y, this.minY, this.maxY);
            this.transform.position = new Vector3(
                x,
                y,
                -10
                );
        } else {
            // 转向目标
            if (this.target != null) {
                // 更新摄像机位置
                float x = Mathf.Clamp(this.target.transform.position.x, this.minX, this.maxX);
                float y = Mathf.Clamp(this.target.transform.position.y, this.minY, this.maxY);
                this.transform.position = new Vector3(
                    x,
                    y,
                    -10
                    );
                // 更新遮罩位置
                if (GameTemp.gameScreen.maskVisible && this.target == this.player && this.playerLightMask != null) {
                    Vector3 size = this.player.GetComponent<SpriteRenderer>().bounds.size;
					this.playerLightMask.transform.position = new Vector3(
						x,
						y,
						this.player.transform.position.z
						);
					Vector3 pos = camera.WorldToViewportPoint(this.player.transform.position);
					float _x = (x - this.target.transform.position.x) / (Util.WIDTH / Util.PPU);
					float _y = (y - this.target.transform.position.y) / (Util.HEIGHT / Util.PPU);
					Vector2 offset = new Vector2(_x, _y);
					this._playerLightMaskMaterial.SetTextureOffset("_Mask", offset);
				}
            }
        }
    }

    /// <summary>
    /// 画面后处理，修改色调
    /// </summary>
    /// <param name="src"></param>
    /// <param name="dest"></param>
    void OnRenderImage(RenderTexture src, RenderTexture dest) {
        if (_screenMaterial) {
            _screenMaterial.SetFloat("_Brightness", 1f);  // 亮度
            _screenMaterial.SetFloat("_Saturation", 1f);    // 饱和度
            _screenMaterial.SetFloat("_Contrast", 1f);      // 对比度
            _screenMaterial.SetInt("_Hue", 0);  // 色调0~360
            Graphics.Blit(src, dest, _screenMaterial);
        } else {
            //直接绘制
            Graphics.Blit(src, dest);
        }
    }
}
