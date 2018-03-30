using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMeleeAbility : Ability {

    public static float RECOVERY_TIME = 0.15f;
    public static float CAST_TIME = 0.5f;
    public static float COOL_DOWN_TIME = 0.0f;
    public static float BASE_DAMAGE = 0;
    public static float LIFE_TIME = 0.1f;

    public EnemyMeleeAbility(CharacterStats character)
        : base(character)
    {
        m_Type = AbilityType.Melee;
        m_CastTime = CAST_TIME;
        m_BaseDamage = BASE_DAMAGE;
        m_RecoveryTime = RECOVERY_TIME;

        float attackTime = m_Character.AttSpd;
        m_DefaultCooldown = attackTime;
        m_CoolDownTime = attackTime;
        Damage = m_BaseDamage + m_Character.Damage;

        m_Lifetime = LIFE_TIME;
    }

    protected override void ActivateEffect()
    {
        Vector3 pos = m_Character.gameObject.transform.position;
        Quaternion rot = m_Character.transform.rotation;

        pos += m_Character.transform.forward / 1.5f;

        Hitbox = (GameObject)Object.Instantiate(Resources.Load("DamageHitboxes/MeleeEnemyAttackHitbox"), pos, rot);
        Hitbox.GetComponent<Hitbox>().Initialize(m_Character, m_Type, (int)Damage, m_Lifetime);

        #region Play attacking Sound
        // play an attacking sound
        // find which player is the listener
        GameObject tempListener = GameManager.playerManager.PlayerList()[0];
        float dist = float.MaxValue;
        foreach (GameObject player in GameManager.playerManager.PlayerList())
        {
            if (Vector3.Distance(player.transform.position, m_Character.transform.position) < dist)
            {
                dist = Vector3.Distance(player.transform.position, m_Character.transform.position);
                tempListener = player;
            }
        }

        //only playing the sound if he hits right now, which he pretty much always should
        AudioManager.Sounds sound = GameManager.audioManager.GetSoundFromEffect("Plant Attack", false);
        GameManager.audioManager.PlaySoundAtPosition(sound, tempListener.transform, m_Character.transform.position); 
        #endregion
    }


}
