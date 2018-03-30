using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoCdsPickup : ItemPickup {

    public float Duration = 15.0f;

    override protected void OnPickup(GameObject player)
    {
        GameManager.audioManager.PlaySound(AudioManager.Sounds.COOL_DOWN_PICKUP);
        player.GetComponent<CharacterStats>().ReduceCooldowns(Duration);
    }
}
