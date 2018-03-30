using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealHitbox : Hitbox
{
    protected PlayerStats User;

    public virtual void Initialize(PlayerStats user, int healAmount, float lifeTime)
    {
        User = user;
        AbilityDamage = healAmount;

        Destroy(gameObject, lifeTime);
    }

    protected override void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player" && User.gameObject.tag == "Player")
        {
            if (other.gameObject.GetComponent<PlayerStats>() != null)
            {
                //other.gameObject.GetComponent<PlayerStats>().Heal(AbilityDamage);
                other.gameObject.GetComponent<PlayerStats>().TakeHealing(User, AbilityDamage);

                // Play a heal sound
                //GameManager.audioManager.PlaySoundAtPosition(AudioManager.Sounds.MONO_TEST, other.gameObject.transform, other.gameObject.transform.position);
            }
        }
    }
}
