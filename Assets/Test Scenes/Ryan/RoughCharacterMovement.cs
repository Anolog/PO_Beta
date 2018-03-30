using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoughCharacterMovement : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        transform.position += transform.forward * Input.GetAxis("Vertical") * Time.deltaTime * 10;

        Vector3 rotation = transform.rotation.eulerAngles;
        rotation.y += Input.GetAxis("Horizontal") * Time.deltaTime * 30;
        transform.rotation = Quaternion.Euler(rotation);

        Debug.Log(Input.GetAxis("Vertical").ToString() + ", " + Input.GetAxis("Horizontal"));
    }
}
