using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicKnockbackAbility : Ability {

    public static float RECOVERY_TIME = 0.15f;
    public static float CAST_TIME = 0.2f;
    public static float COOL_DOWN_TIME = 8f;
    public static float BASE_DAMAGE = 5f;
    public static float LIFE_TIME = 1.6f;
    public static string ABILITY_NAME = "Knockback";
    public static float REDUCED_COOLDOWN = 0.5f;

    public BasicKnockbackAbility(CharacterStats character)
        : base(character)
    {
        m_Type = AbilityType.Melee;
        m_CastTime = CAST_TIME;
        m_RecoveryTime = RECOVERY_TIME;
        m_CoolDownTime = COOL_DOWN_TIME;
        m_BaseDamage = BASE_DAMAGE;
        AbilityName = ABILITY_NAME;
        m_ReducedCooldown = REDUCED_COOLDOWN;
        m_DefaultCooldown = COOL_DOWN_TIME;
        Damage = m_BaseDamage + m_Character.Damage;

        m_Lifetime = LIFE_TIME;
    }

    public override void Use()
    {
        if (m_CoolDownTimer < Time.time)
        {
            Animator animator = m_Character.gameObject.GetComponentInChildren<Animator>();

            animator.SetTrigger("UseKnockback");

            if (m_Character.CompareTag("Player"))
            {
                animator.SetBool("Jump", false);
            }

            base.Use();
        }
        else
        {
            GameManager.audioManager.PlaySound(AudioManager.Sounds.ABILITY_STILL_ON_CD);
        }
    }

    protected override void ActivateEffect()
    {
        Vector3 pos = m_Character.transform.position;
        Quaternion rot = m_Character.transform.rotation;

        pos.y += 1.0f;

        Hitbox = (GameObject)Object.Instantiate(Resources.Load("DamageHitboxes/BasicKnockbackAbilityHitbox"), pos, rot);
        Hitbox.GetComponent<BasicKnockbackHitbox>().Initialize(m_Character, m_Type, (int)Damage, m_Lifetime);

        #region Play Sound
        // play a yelling sound or something like that
        GameManager.audioManager.PlaySoundAtPosition(AudioManager.Sounds.KNOCKBACK, m_Character.transform, m_Character.transform.position); 
        #endregion
    }
}
