using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Overlay : MonoBehaviour {

    public bool ShowFramerate = false;
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (ShowFramerate)
        {
            GetComponentInChildren<UnityEngine.UI.Text>().text = (1.0f / Time.deltaTime).ToString();
        }
    }
}
