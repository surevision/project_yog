using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CameraControl : MonoBehaviour {

	public GameObject _playerLightMask;

	[SerializeField]
	private Material _playerLightMaskMaterial;

    public Material _screenMaterial = null;

	[SerializeField]
	private Camera camera;

	public GameObject player;

    private Vector3 _minTile;
    private Vector3 _maxTile;

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
//		this.camera = this.gameObject.GetComponent<Camera>();
//		this._playerLightMaskMaterial = this.playerLightMask.GetComponent<SpriteRenderer>().material;
	}
	public void setupPos(Vector3 minTile, Vector3 maxTile) {
        float width, height;
		height = this.camera.orthographicSize * 2f;
		width = height * this.camera.aspect;
        this.minX = minTile.x + width / 2f;
        this.maxX = maxTile.x - width / 2f;
        this.minY = minTile.y + height / 2f;
        this.maxY = maxTile.y - height / 2f;
    }

    public void update() {
        if (this.camera == null) {
            return;
        }
		if (this.player == null) {
			return;
		}
        if (this.playerLightMask != null) {
            if (this.playerLightMask.activeSelf != GameTemp.gameScreen.maskVisible) {
                this.playerLightMask.SetActive(GameTemp.gameScreen.maskVisible);
            }
        }
		// 转向目标// 更新摄像机位置
		float x = Mathf.Clamp(GameTemp.gameScreen.currTargetPos.x, this.minX, this.maxX);
		float y = Mathf.Clamp(GameTemp.gameScreen.currTargetPos.y, this.minY, this.maxY);
//		Debug.Log(string.Format("x, y, {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}", 
//			x, y, GameTemp.gameScreen.currTargetPos.x, this.minX, this.maxX, GameTemp.gameScreen.currTargetPos.y, this.minY, this.maxY));
		this.transform.position = new Vector3(
			x + GameTemp.gameScreen.shakePosX / (float)Util.PPU,
			y + GameTemp.gameScreen.shakePosY / (float)Util.PPU,
			-10
		);
		// 更新遮罩位置
		if (GameTemp.gameScreen.maskVisible && this.playerLightMask != null) {
			GameObject player = this.player;
			this.playerLightMask.transform.position = new Vector3(
				x,
				y,
				this.player.transform.position.z
			);
			float _x = (x - GameTemp.gameScreen.currTargetPos.x) / (Util.WIDTH / Util.PPU);
			float _y = (y - GameTemp.gameScreen.currTargetPos.y) / (Util.HEIGHT / Util.PPU);
			//                    Debug.Log(string.Format("x {0} y {1}", _x, _y));
			Vector2 offset = new Vector2(_x, _y);
			// this._playerLightMaskMaterial.SetTextureOffset("_Mask", offset);
			List<Vector4> values = new List<Vector4>();
			values.Add(new Vector4(_x, _y, 0, 0));
			// TODO 摇曳灯光暂无处理
			Dictionary<int, Vector4> lights = GameTemp.gameScreen.getLights();
			foreach (var pair in lights) {
				_x = (x - pair.Value.x) / (Util.WIDTH / Util.PPU);
				_y = (y - pair.Value.y) / (Util.HEIGHT / Util.PPU);
				values.Add(new Vector4(_x, _y, 0, 0));
			}
			for (int i = values.Count; i < 10; i += 1) {
				values.Add(Vector4.zero);
			}
			this._playerLightMaskMaterial.SetVectorArray("_Positions", values);
			this._playerLightMaskMaterial.SetFloat("_ActivePosition", Mathf.Min(10, lights.Count + 1));
		}
    }

    /// <summary>
    /// 画面后处理，修改色调
    /// </summary>
    /// <param name="src"></param>
    /// <param name="dest"></param>
    void OnRenderImage(RenderTexture src, RenderTexture dest) {
        if (GameTemp.gameScreen != null && _screenMaterial) {
            _screenMaterial.SetFloat("_Brightness", GameTemp.gameScreen.currScreenColorInfo.brightness);  // 亮度
            _screenMaterial.SetFloat("_Saturation", GameTemp.gameScreen.currScreenColorInfo.saturation);    // 饱和度
            _screenMaterial.SetFloat("_Contrast", GameTemp.gameScreen.currScreenColorInfo.contrast);      // 对比度
            _screenMaterial.SetInt("_Hue", GameTemp.gameScreen.currScreenColorInfo.hue);  // 色调0~360
            Graphics.Blit(src, dest, _screenMaterial);
        } else {
            //直接绘制
            Graphics.Blit(src, dest);
        }
    }
}
