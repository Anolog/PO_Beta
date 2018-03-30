using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportTest : MonoBehaviour {

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
            StartCoroutine(FadeIn());
        }
    }

    IEnumerator FadeIn()
    {
        float startTime = Time.time;

        Material mat = gameObject.GetComponentInChildren<Renderer>().material;

        while (Time.time < startTime + 1)
        {
            float val = (Time.time - startTime);

            mat.SetFloat("_FadeTime", 1 - val);


            yield return null;
        }

        yield return null;
    }
}
