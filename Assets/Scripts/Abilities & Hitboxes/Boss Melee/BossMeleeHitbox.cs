using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossMeleeHitbox : Hitbox {
    protected override void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
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

            AudioManager.Sounds sound = GameManager.audioManager.GetSoundFromEffect("Player Hit", true);
            GameManager.audioManager.PlaySoundAtPosition(sound, tempListener.transform, contactPoint);
        }

        base.OnTriggerEnter(other);
    }
}
