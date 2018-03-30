using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamagePickup : ItemPickup
{
    public float Duration = 15.0f;

    protected override void OnPickup(GameObject player)
    {
        GameManager.audioManager.PlaySound(AudioManager.Sounds.DAMAGE_PICKUP);
        player.GetComponent<CharacterStats>().DamageItemPickup(Duration);
    }
}
