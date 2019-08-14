using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowFPS : MonoBehaviour
{
	void Start()
	{
	    //Application.targetFrameRate=60;
	}
	void OnGUI()
	{
		GUI.Label(new Rect(0, 0, 100, 100), (1.0f / Time.smoothDeltaTime).ToString("f2"));
	}
}