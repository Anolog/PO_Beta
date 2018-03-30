using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hitbox : MonoBehaviour
{

    protected CharacterStats Attacker;
    protected Ability.AbilityType Type;
    protected int AbilityDamage;
    public new string name = "";

    public virtual void Initialize(CharacterStats attacker, Ability.AbilityType type, int abilityDamage, float lifeTime)
    {
        Attacker = attacker;
        Type = type;
        AbilityDamage = abilityDamage;

        Destroy(gameObject, lifeTime);
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (Attacker != null) // if the rock thrower dies before his projectile hits something there is an error so we have this
        {
            if (other.gameObject.tag == "Decoration")
            {
                #region Play Hit Decoration Sound

                // Play a hit sound
                // TODO: find decoration hit sound
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

                AudioManager.Sounds sound = GameManager.audioManager.GetSoundFromEffect("Decoration Hit", true);
                GameManager.audioManager.PlaySoundAtPosition(sound, tempListener.transform, contactPoint);
                //goat

                #endregion
            }

            if (other.gameObject.tag == "Enemy" && Attacker.gameObject.tag == "Player" ||
            other.gameObject.tag == "Player" && Attacker.gameObject.tag == "Enemy" ||
            other.gameObject.tag == "Player" && Attacker.gameObject.tag == "Boss" ||
            other.gameObject.tag == "Boss" && Attacker.gameObject.tag == "Player")
            {
                if (other.gameObject.GetComponent<CharacterStats>() != null)
                {
                    other.gameObject.GetComponent<CharacterStats>().TakeDamage(Attacker, Type, AbilityDamage);
                }
            }

            if (Type == Ability.AbilityType.Ranged)
            {

                if (other.gameObject.tag != Attacker.gameObject.tag && 
                    other.gameObject.tag != "Hitbox" &&
                    other.gameObject.tag != "SpawnCollider" &&
                    other.gameObject.tag != "DownedPlayer")
                {
                    Destroy(gameObject);
                }

            }
        }
    }
}
