using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSmashAbility : Ability {

    public static float RECOVERY_TIME = 0.15f;
    public static float CAST_TIME = 0.2f;
    public static float COOL_DOWN_TIME = 8f;
    public static float BASE_DAMAGE = 24;
    public static float LIFE_TIME = 2.0f;
    public static string ABILITY_NAME = "Weapon Smash";
    public static float REDUCED_COOLDOWN = 1.6f;

    float m_HeightOffset = 1.352f; //Hardcoded for now - probably want a helper eventually
    float m_RotationOffset = -37.741f; //Hardcoded for now

    public WeaponSmashAbility(CharacterStats character)
        : base(character)
    {
        m_Type = AbilityType.Melee;
        m_CastTime = CAST_TIME;
        m_RecoveryTime = RECOVERY_TIME;
        m_CoolDownTime = COOL_DOWN_TIME;
        m_BaseDamage = BASE_DAMAGE;
        AbilityName = ABILITY_NAME;
        m_DefaultCooldown = COOL_DOWN_TIME;
        m_ReducedCooldown = REDUCED_COOLDOWN;

        Damage = m_BaseDamage + m_Character.Damage;

        m_Lifetime = LIFE_TIME;
    }

    protected override void ActivateEffect()
    {
        Vector3 pos = m_Character.transform.position;

        pos.y += m_HeightOffset;

        Quaternion rot = Quaternion.Euler(m_RotationOffset, m_Character.transform.rotation.eulerAngles.y, m_Character.transform.rotation.eulerAngles.z);

        Hitbox = (GameObject)Object.Instantiate(Resources.Load("DamageHitboxes/WeaponSmashAbilityHitbox"), pos, rot);
        Hitbox.GetComponent<Hitbox>().Initialize(m_Character, m_Type, (int)Damage, m_Lifetime);
        Hitbox.GetComponent<WeaponSmashHitbox>().SetupWeaponSmashInitailOffsets(m_HeightOffset, m_RotationOffset);

        m_Character.usingWeaponSmash = true;
        m_Character.Grounded = false;
        m_Character.gameObject.GetComponent<UnityEngine.AI.NavMeshAgent>().enabled = false;

        Animator animator = m_Character.gameObject.GetComponentInChildren<Animator>();

        if (m_Character.Abilities[0].AbilityName == "Sword and Shield")
        {
            animator.SetBool("UseWeaponSmashSword", true);
        }
        else
        {
            animator.SetBool("UseWeaponSmashBow", true);
        }

        #region Play Sound
        // play a yelling sound or something like that
        GameManager.audioManager.PlaySoundAtPosition(AudioManager.Sounds.WEAPON_SMASH, m_Character.transform, m_Character.transform.position); 
        #endregion
    }
}
