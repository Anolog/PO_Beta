using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossMeleeAbility : Ability {

    public static float RECOVERY_TIME = 0.15f;
    public static float CAST_TIME = 0.2f;
    public static float COOL_DOWN_TIME = 0.35f;
    public static float BASE_DAMAGE = 40;
    public static float LIFE_TIME = 0.1f;

    public BossMeleeAbility(CharacterStats character)
        : base(character)
    {
        m_Type = AbilityType.Melee;
        m_CastTime = CAST_TIME;
        m_RecoveryTime = RECOVERY_TIME;
        m_CoolDownTime = COOL_DOWN_TIME;
        m_BaseDamage = BASE_DAMAGE;
        m_DefaultCooldown = COOL_DOWN_TIME;
        Damage = m_BaseDamage + m_Character.Damage;

        m_Lifetime = LIFE_TIME;
    }

    public override void Use()
    {
        if (m_CoolDownTimer < Time.time)
        {
            base.Use();

            m_Character.gameObject.GetComponentInChildren<Animator>().SetTrigger("UseMelee");

            //Debug.Log("InUse");
        }
    }

    protected override void ActivateEffect()
    {
        Vector3 pos = m_Character.transform.position;
        Quaternion rot = m_Character.transform.rotation;


        Hitbox = (GameObject)Object.Instantiate(Resources.Load("DamageHitboxes/BossMeleeAbilityHitbox"), pos, rot);
        Hitbox.GetComponent<Hitbox>().Initialize(m_Character, m_Type, (int)Damage, m_Lifetime);

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

        AudioManager.Sounds sound = GameManager.audioManager.GetSoundFromEffect("Boss Attack", true);
        GameManager.audioManager.PlaySoundAtPosition(sound, tempListener.transform, m_Character.transform.position);

    }
}
