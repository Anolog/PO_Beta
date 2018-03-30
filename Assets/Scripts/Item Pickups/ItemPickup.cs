using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ItemPickup : MonoBehaviour {

    public ItemManager ItemSpawner;

	// Use this for initialization
	protected void Start () {
		
	}
	
	// Update is called once per frame
	protected void Update () {
        //make it rotate here assuming design agrees with that
        Vector3 rotation = transform.rotation.eulerAngles;

        rotation.y += 90 * Time.deltaTime;

        transform.rotation = Quaternion.Euler(rotation);
	}

    abstract protected void OnPickup(GameObject player);

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            ItemSpawner.ItemPickedUp(gameObject);
            OnPickup(other.gameObject);
            Destroy(gameObject);
        }
    }
}
