using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedItemPickup : ItemPickup
{
    public float Duration = 8.0f;
    public float Speed = 2.0f;
    public float Acceleration = 1.0f;

    override protected void OnPickup(GameObject player)
    {
        GameManager.audioManager.PlaySound(AudioManager.Sounds.SPEED_PICKUP);


        player.GetComponent<CharacterStats>().IncreaseMovementSpeed(Speed, Duration);
        player.GetComponent<CharacterStats>().IncreaseAcceleration(Acceleration, Duration);

    }
}
