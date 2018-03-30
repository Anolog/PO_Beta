using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FPSCounter : MonoBehaviour {

    Text Txt;

	// Use this for initialization
	void Start () {
        Txt = GetComponent<Text>();
	}
	
	// Update is called once per frame
	void Update () {

        int fps = (int)(1 / Time.deltaTime);
        int fixedFPS = (int)(1 / Time.fixedDeltaTime);

        Txt.text = "FPS: " + fps + "\n" +
            "Fixed FPS: " + fixedFPS;
	}
}
