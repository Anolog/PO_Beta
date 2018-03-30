using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateToCamera : MonoBehaviour
{

    Quaternion m_Rotation;
    Vector3 m_Up;
    public GameObject PseudoBB;

    //Vector3 m_LookTarget;

    public void SetObject(GameObject obj)
    {
        PseudoBB = obj;
        m_Rotation = obj.transform.rotation;
        m_Up = PseudoBB.transform.up;
    }

    // Use this for initialization
    void Start()
    {
        if (PseudoBB != null)
            m_Rotation = PseudoBB.transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnPreRender()
    {
        //Debug.Log(obj.transform.rotation);

        //determine where the camera forward vector collides with the plane defined by this object's right and up vectors
        Vector3 camToObj = PseudoBB.transform.position - transform.position;
        float normalDot = Vector3.Dot(camToObj, PseudoBB.transform.up);
        //float forwardDot = Vector3.Dot(obj.transform.forward, obj.transform.forward);
        float distance = normalDot * 1;
        Vector3 collisionPoint = PseudoBB.transform.up * distance + transform.position;
        //m_LookTarget = collisionPoint;

        //rotate quad to face collision point
        Vector3 collisionPointVector = collisionPoint - PseudoBB.transform.position;
        
        //float angle = Mathf.Atan2(collisionPointVector.y, collisionPointVector.x) * Mathf.Rad2Deg;
        //float angle = Vector3.Angle(-PseudoBB.transform.forward, collisionPointVector);

        //PseudoBB.transform.rotation = m_Rotation * Quaternion.Euler(0, 90 - angle, 0);
        PseudoBB.transform.rotation = Quaternion.LookRotation(-collisionPointVector, m_Up);
        //Debug.Log(angle);
       
    }

    private void OnPostRender()
    {
        PseudoBB.transform.rotation = m_Rotation;
        //Debug.Log(PseudoBB.transform.rotation);
    }
}
