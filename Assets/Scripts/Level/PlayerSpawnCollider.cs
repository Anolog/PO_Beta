using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawnCollider : MonoBehaviour {

    private int m_NumPlayersIn = 0;

    public GameObject Blocker;
    public GameObject Game;
    public GameObject Door;
    public bool AllPlayersOut = false;

    private void Start()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
            m_NumPlayersIn++;   
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            m_NumPlayersIn--;

            if (m_NumPlayersIn <= 0)
            {
                Blocker.GetComponent<MeshCollider>().enabled = true;
                AllPlayersOut = true;
                Game.GetComponent<ItemManager>().enabled = true;
                Door.GetComponent<Animator>().SetBool("ShutDoor", true);
            }
        }
    }
}
