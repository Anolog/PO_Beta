using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPickupItem : ItemPickup {

    override protected void OnPickup(GameObject player)
    {
        GameManager.audioManager.PlaySound(AudioManager.Sounds.HEALTH_PICKUP);
        player.GetComponent<CharacterStats>().TakeHealing(null, 50);
    }
}
