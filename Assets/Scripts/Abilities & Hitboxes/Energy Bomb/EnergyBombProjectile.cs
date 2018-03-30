using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyBombProjectile : Hitbox
{
    private GameObject Hitbox;
    public float ExplosionDamage = 13;
    public float DistancetoExplode = 3;
    public float ExplosionLifetime = 0.5f;
    public float TimeLeftUntilExplosion = 3.0f;

    public override void Initialize(CharacterStats attacker, Ability.AbilityType type, int abilityDamage, float lifeTime)
    {
        base.Initialize(attacker, type, abilityDamage, lifeTime);
        ExplosionDamage += attacker.Damage;
    }

    protected override void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<CharacterStats>() != Attacker.gameObject.GetComponent<CharacterStats>() && !other.gameObject.CompareTag("SpawnCollider")
            && !other.CompareTag("Hitbox"))
        {
            Hitbox = (GameObject)Instantiate(Resources.Load("DamageHitboxes/EnergyBombHitbox"), gameObject.transform.position, gameObject.transform.rotation);
            for (int i = 0; i < 2; i++)
            {
                Hitbox box = Hitbox.transform.GetChild(i).GetComponent<Hitbox>();
                box.Initialize(Attacker, Type, (int)ExplosionDamage, ExplosionLifetime);
                ((EnergyBombHitbox)box).Explode();
            }
            #region Play explosion Sound
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
            #endregion

            foreach (GameObject player in GameManager.playerManager.PlayerList())
            {
                float distance = Vector3.Distance(player.transform.position, gameObject.transform.position);

                if (distance > 10)
                    continue;

                float duration = Mathf.Sqrt(0.1f * dist);
                float lowFi = 0.5f * Mathf.Pow(dist * 0.1f, 2);
                float hiFi = 0.5f * dist * 0.1f;
                player.GetComponent<PlayerStats>().Rumble(lowFi, hiFi, duration);
            }

            Destroy(gameObject);
            Destroy(Hitbox, ExplosionLifetime);
        }
    }
}
