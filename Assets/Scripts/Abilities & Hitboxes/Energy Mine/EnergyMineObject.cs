using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Legacy code, for if we want it to explode when the player touches it.
public class EnergyMineObject : Hitbox
{
    private GameObject hitbox;
    public float ExplosionDamage = 10.0f;
    public float ExplosionLifetime = 0.5f;
    private float m_LifeTime;
    //public float m_ExplosionCountdown = 1.0f;
    //public float m_GracePeriod = 2.0f;
    

    public override void Initialize(CharacterStats attacker, Ability.AbilityType type, int abilityDamage, float lifetime )
    {
        Attacker = attacker;
        Type = type;
        AbilityDamage = abilityDamage;

        ExplosionDamage += attacker.Damage;
        m_LifeTime = lifetime + Time.time;
        //m_ExplosionCountdown = lifetime + Time.time;
        //m_GracePeriod += Time.time;

    }

    private void Update()
    {
        if (m_LifeTime <= Time.time)
        {
            CreateExplosion();
        }
    }

    
    //private void OnCollisionEnter(Collision collision)
    //{
    //    return;
    //    /*
    //    if ((collision.gameObject.tag == "Player" && Attacker.gameObject.tag == "Boss") && m_GracePeriod <= Time.time)
    //    {
    //        if (collision.gameObject.GetComponent<CharacterStats>() != null)
    //        {
    //            collision.gameObject.GetComponent<CharacterStats>().TakeDamage(Attacker, Type, AbilityDamage);
    //        }

    //        foreach (ContactPoint contact in collision.contacts)
    //        {
    //            CreateExplosion();
    //        }
    //    }
        
    //*/
    //}

    //protected override void OnTriggerEnter(Collider other)
    //{
    //    return;
    //}

    private void CreateExplosion()
    {
        hitbox = (GameObject)Instantiate(Resources.Load("DamageHitboxes/EnergyMineHitbox"), gameObject.transform.position, gameObject.transform.rotation);
        hitbox.GetComponent<EnergyMineHitbox>().Initialize(Attacker, Type, (int)ExplosionDamage, ExplosionLifetime);

        #region Play Explosion Sound
        // play an explosion sound
        // find which player is the listener
        GameObject tempListener = GameManager.playerManager.PlayerList()[0];
        float dist = float.MaxValue;
        foreach (GameObject player in GameManager.playerManager.PlayerList())
        {
            if (Vector3.Distance(player.transform.position, gameObject.transform.position) < dist)
            {
                dist = Vector3.Distance(player.transform.position, gameObject.transform.position);
                tempListener = player;
            }
        }
        GameManager.audioManager.PlaySoundAtPosition(AudioManager.Sounds.ENERGY_BOMB_EXPLOSION, tempListener.transform, gameObject.transform.position);
        //goat
        #endregion

        Destroy(gameObject);
    }
}
