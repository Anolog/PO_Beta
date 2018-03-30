using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponPosition : MonoBehaviour {

    [SerializeField]
    protected Transform m_helperTransform;

    public Transform GetHelperTransform() { return m_helperTransform; }

    public bool shouldUpdate = true;

	// Update is called once per frame
	void Update () {

        if (shouldUpdate)
        {
            gameObject.transform.position = m_helperTransform.position;
            gameObject.transform.rotation = m_helperTransform.rotation;
        }
	}
}
