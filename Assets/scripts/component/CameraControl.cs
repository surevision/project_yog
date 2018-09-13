using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour {
    private GameObject _target;

    public GameObject target {
        get { return _target; }
        set { _target = value; }
    }
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void LateUpdate () {
        if (this.target != null) {
            this.transform.position = new Vector3(
                this.target.transform.position.x,
                this.target.transform.position.y,
                -10
                );
        }
	}
}
