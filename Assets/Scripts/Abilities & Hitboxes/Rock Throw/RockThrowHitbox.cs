using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RockThrowHitbox : Hitbox {

    protected override void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            #region Play Hit Sound
            // Play a hit sound

            // find the closest point on my collider that my attacker is so that the sound is played directionally properly 
            Vector3 contactPoint = other.gameObject.GetComponent<Collider>().ClosestPointOnBounds(transform.position); // roughly where the collision happened

            // find which player is the listener
            GameObject tempListener = GameManager.playerManager.PlayerList()[0];
            float dist = float.MaxValue;
            foreach (GameObject player in GameManager.playerManager.PlayerList())
            {
                if (Vector3.Distance(player.transform.position, contactPoint) < dist)
                {
                    dist = Vector3.Distance(player.transform.position, contactPoint);
                    tempListener = player;
                }
            }

            GameManager.audioManager.PlaySoundAtPosition(AudioManager.Sounds.ROCK_ATTACK_HIT, tempListener.transform, contactPoint); 
            #endregion
        }

        base.OnTriggerEnter(other);
    }
}
