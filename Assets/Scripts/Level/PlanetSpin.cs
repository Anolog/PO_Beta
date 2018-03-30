using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetSpin : MonoBehaviour {

    float spinSpeed = 1f;
	// Update is called once per frame
	void Update () {
        Vector3 rot = transform.rotation.eulerAngles;
        rot.y += spinSpeed * Time.deltaTime;
        Quaternion qRot = Quaternion.Euler(rot.x, rot.y, rot.z);
        transform.rotation = qRot;
	}
}
