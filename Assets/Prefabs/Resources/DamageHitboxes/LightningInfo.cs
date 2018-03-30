using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningInfo : MonoBehaviour {



	// Use this for initialization
	void Start () {
        
	}
	
	// Update is called once per frame
	void Update () {
        GetComponentInChildren<Renderer>().material.SetFloat("_Scale", transform.localScale.y * 0.03f);
    }
}
