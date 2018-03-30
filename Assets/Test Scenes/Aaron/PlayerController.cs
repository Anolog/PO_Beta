
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Command))]
public class PlayerController : MonoBehaviour
{
    InputManager m_InputManager;

	void Start ()
    {
        //Only for testing purposes for now. Will have to be changed when gamemanager is done. 
        //m_InputManager = GameObject.FindGameObjectWithTag("InputManager").GetComponent<InputManager>();
	}
	
	void Update ()
    {
		
	}

    void GetInput()
    {

    }
}
