using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvulnDamagePickup : ItemPickup
{
    public float Duration = 15.0f;

    override protected void OnPickup(GameObject player)
    {
        GameManager.audioManager.PlaySound(AudioManager.Sounds.INVINC_PICKUP);
        player.GetComponent<CharacterStats>().InvulnDamageItemPickup(Duration);
    }
}
