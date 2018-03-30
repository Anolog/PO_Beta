using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointPickupItem : ItemPickup {

    override protected void OnPickup(GameObject player)
    {
        GameManager.audioManager.PlaySound(AudioManager.Sounds.POINTS_PICKUP);

        //create a roughly normal distribution throughout the range 10-50
        int normalDistrib = 10 + Random.Range(0, 11) + Random.Range(0, 11) + Random.Range(0, 11) + Random.Range(0, 11);

        player.GetComponent<PlayerStats>().GivePoints(normalDistrib);
    }
}
