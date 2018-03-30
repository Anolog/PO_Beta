using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChargeHitbox : Hitbox
{
    const int FORCE_MULTIPLIER = 3000;

    public Vector3 ChargeVelocity;
    public Vector3 ChargeStart;

    public void SetUpCharge(Vector3 chargeVelocity, Vector3 chargeStart)
    {
        ChargeStart = chargeStart;
        ChargeVelocity = chargeVelocity;
        name = "Charge";
    }

    public void Update()
    {
        Vector3 pos = Attacker.transform.position;

        pos.y += 0.9f;
        pos.z += 0.2f * Attacker.transform.forward.z;
        pos.x += 0.2f * Attacker.transform.forward.x;

        transform.position = pos;

        Quaternion rot = Attacker.transform.rotation;
        transform.rotation = rot;

        Vector3 forward = Attacker.gameObject.transform.forward;

        Attacker.gameObject.GetComponent<Rigidbody>().velocity = new Vector3(ChargeVelocity.x * forward.x, Attacker.gameObject.GetComponent<Rigidbody>().velocity.y, ChargeVelocity.z * forward.z);

        if (Vector3.Distance(ChargeStart, Attacker.gameObject.transform.position) >= 10)
        {
            Attacker.isControllable = true;
            Attacker.usingCharge = false;
            DestroyObject(gameObject);
        }
    }

    override protected void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Hitbox")
        {
            if (other.gameObject.GetComponentInParent<Hitbox>().name == "Knockback")
            {
                Attacker.isControllable = true;
                Attacker.usingCharge = false;
                DestroyObject(gameObject);
            }
        }

        if (other.gameObject.tag == "Enemy" && Attacker.gameObject.tag == "Player" ||
            other.gameObject.tag == "Boss" && Attacker.gameObject.tag == "Player")
        {
            if (other.gameObject.GetComponent<CharacterStats>() != null)
            {
                Vector3 direction = other.gameObject.transform.position - transform.position;
                direction.Normalize();

                other.gameObject.GetComponent<CharacterStats>().TakeDamage(Attacker, Type, AbilityDamage);
                other.gameObject.GetComponent<CharacterStats>().KnockbackCharacter(direction * FORCE_MULTIPLIER, 1, Attacker);
                                                       
                #region Play hit sound
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

                GameManager.audioManager.PlaySoundAtPosition(AudioManager.Sounds.CHARGE_HIT, tempListener.transform, contactPoint); 
                #endregion
            }
        }

        if (other.gameObject.tag != "Enemy" && other.gameObject.tag != "Player" && other.gameObject.tag != "Boss" && other.gameObject.tag != "SpawnCollider" && other.gameObject.tag != "Hitbox")
        {
            Vector3 contactPoint = other.gameObject.GetComponent<Collider>().ClosestPointOnBounds(transform.position); // roughly where the collision happened

            Vector3 direction = contactPoint - Attacker.transform.position;
            direction.y = 0;

            direction.Normalize();

            float dot = Vector3.Dot(direction, Attacker.transform.forward);
            if (dot >= 0.6)
            {
                Attacker.isControllable = true;
                Attacker.usingCharge = false;
                DestroyObject(gameObject);
            }

            ((PlayerStats)Attacker).Rumble(0.4f, 0.4f, 0.75f);

            //contactPoint.Normalize();
            //float dot = Vector3.Dot(contactPoint, Attacker.transform.forward);
            //if (dot >= -0.5)
            //{
            //    Attacker.isControllable = true;
            //    Attacker.usingCharge = false;
            //    DestroyObject(gameObject);
            //}
        }
    }

    private void OnDestroy()
    {
        Animator animator = Attacker.gameObject.GetComponentInChildren<Animator>();
        //animator.SetLayerWeight(animator.GetLayerIndex("UpperBodyLayer"), 0f);
        animator.SetBool("UseChargeSword", false);
        animator.SetBool("UseChargeBow", false);
    }
}
