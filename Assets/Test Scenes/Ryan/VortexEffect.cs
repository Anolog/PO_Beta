using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VortexEffect : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
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

        while (Time.time < startTime + 1)
        {
            float val = 1f * (Time.time - startTime);
            float scale = 64 * (1 - Mathf.Clamp01(val));
            transform.localScale = new Vector3(scale, scale, scale);
            mat.SetFloat("_Fill", 1 + 2 * val);// 0.5f + 4.5f * Mathf.Pow(val, 6));
            mat.SetFloat("_Duration", val * 1.7f * 4.8f);
            mat.SetFloat("_Dim", 0.2f * val);
            mat.SetFloat("_Angle", 3 * val);
            yield return null;
        }

        yield return null;
    }
}
