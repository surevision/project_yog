using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CameraControl : MonoBehaviour {

    public GameObject playerLightMask;

    private GameObject _target;

    private Vector3 _minTile;
    private Vector3 _maxTile;

    public GameObject target {
        get { return _target; }
        set { _target = value; }
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

    public void setupPos(Vector3 minTile, Vector3 maxTile) {
        float width, height;
        height = GameObject.Find("Main Camera").GetComponent<Camera>().orthographicSize * 2f;
        width = height * GameObject.Find("Main Camera").GetComponent<Camera>().aspect;
        this.minX = minTile.x + width / 2f;
        this.maxX = maxTile.x - width / 2f;
        this.minY = minTile.y + height / 2f;
        this.maxY = maxTile.y - height / 2f;
    }

	public void update () {
        if (this.target != null) {
            float x = Mathf.Clamp(this.target.transform.position.x, this.minX, this.maxX);
            float y = Mathf.Clamp(this.target.transform.position.y, this.minY, this.maxY);
            this.transform.position = new Vector3(
                x,
                y,
                -10
                );
            if (this.playerLightMask != null) {
                Vector3 size = this.target.GetComponent<SpriteRenderer>().bounds.size;
                this.playerLightMask.transform.position = new Vector3(
                    this.target.transform.position.x,
                    this.target.transform.position.y + size.y * 0.5f,
                    this.target.transform.position.z
                    );
            }
        }
    }
}
