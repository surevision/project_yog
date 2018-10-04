using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CameraControl : MonoBehaviour {
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
        width = GameObject.Find("Main Camera").GetComponent<Camera>().orthographicSize;
        height = width * GameObject.Find("Main Camera").GetComponent<Camera>().aspect;
        float _width = maxTile.x - minTile.x;
        float _height = _width * GameObject.Find("Main Camera").GetComponent<Camera>().aspect;
        this.minX = minTile.x;
        this.maxX = maxTile.x;
        this.minY = (minTile.y + maxTile.y) / 2 - _height;
        this.maxY = _height + (minTile.y + maxTile.y) / 2;
    }

    // Use this for initialization
    void Start () {
		
	}
	
	public void update () {
        float x = Mathf.Clamp(this.target.transform.position.x, this.minX, this.maxX);
        float y = Mathf.Clamp(this.target.transform.position.y, this.minY, this.maxY);
        if (this.target != null) {
            this.transform.position = new Vector3(
                x,
                y,
                -10
                );
        }
	}
}
