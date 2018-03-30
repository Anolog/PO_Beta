using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DonutShaderTest : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            StopAllCoroutines();
            StartCoroutine(DoStuff());
        }
	}

    IEnumerator DoStuff()
    {
        float startTime = Time.time;
        transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
        Material mat = gameObject.GetComponent<Renderer>().material;

        while(Time.time < startTime + 1.5f)
        {
            float val = (Time.time - startTime);
            float scale = 17.5f *  Mathf.Clamp01(3 * val);
            transform.localScale = new Vector3(scale, scale, scale);
            mat.SetFloat("_Fill", 5 + 2 * val);// 0.5f + 4.5f * Mathf.Pow(val, 6));
            mat.SetFloat("_Duration", Mathf.Pow(val, 0.4f) * 1.7f * 4.8f);
            mat.SetFloat("_Dim", 0.2f * Mathf.Cos(val * Mathf.PI * 2) + 0.2f);
            yield return null;
        }

        yield return null;
    }
}
