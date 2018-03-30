using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyBombHitbox : Hitbox
{
    public bool StunOnHit = false;
    public float FinalScale = 0;
    protected override void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Enemy" && Attacker.gameObject.tag == "Player" ||
            other.gameObject.tag == "Boss" && Attacker.gameObject.tag == "Player")
        {
            if (other.gameObject.GetComponent<CharacterStats>() != null)
            {
                RaycastHit hit;
                Vector3 dir = other.gameObject.transform.position - gameObject.transform.position;
                Ray ray = new Ray(gameObject.transform.position, dir);

                if (Physics.Raycast(ray, out hit, 2))
                {
                    if (hit.transform == other.gameObject.transform)
                    {
                        other.gameObject.GetComponent<CharacterStats>().TakeDamage(Attacker, Type, AbilityDamage);

                        if (StunOnHit == true)
                        {
                            other.gameObject.GetComponent<CharacterStats>().KnockbackCharacter(Vector3.zero, 3.0f, Attacker);
                        }
                        else
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

                            GameManager.audioManager.PlaySoundAtPosition(Attacker.GetHitSound(other.gameObject), tempListener.transform, contactPoint); 
                            //goat
                            #endregion
                        }
                    }
                }
            }
        }
    }

    public void Explode()
    {
        transform.localScale = Vector3.zero;
        gameObject.GetComponent<SphereCollider>().enabled = false;
        StartCoroutine(ExpandCoroutine());
    }

    IEnumerator ExpandCoroutine()
    {
        while(transform.localScale.x < FinalScale)
        {
            transform.localScale += 8 * new Vector3(Time.deltaTime, Time.deltaTime, Time.deltaTime) * FinalScale;
            yield return null;
        }

        //Debug.Log("Done");
        gameObject.GetComponent<SphereCollider>().enabled = true;
        yield return null;
    }
}
