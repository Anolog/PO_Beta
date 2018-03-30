using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VortexAbility : Ability {

    public static float RECOVERY_TIME = 0.15f;
    public static float CAST_TIME = 0.1f;
    public static float COOL_DOWN_TIME = 8f;
    public static float BASE_DAMAGE = 10f;
    public static float LIFE_TIME = 1f;
    public static string ABILITY_NAME = "Vortex";
    public static float REDUCED_COOLDOWN = 0.7f;

    private GameObject m_Weapon;
    public VortexAbility(CharacterStats character)
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

    protected override void ActivateEffect()
    {
        Vector3 pos = m_Character.transform.position;
        Quaternion rot = m_Character.transform.rotation;

        pos.y += m_Character.gameObject.GetComponent<CapsuleCollider>().height * 0.5f;

        if (m_Character.gameObject.transform.Find("Bow").gameObject.activeSelf)
        {
            m_Weapon = m_Character.gameObject.transform.Find("Bow").gameObject;
        }
        else
        {
            m_Weapon = m_Character.gameObject.transform.Find("SwordAndShield").gameObject;
        }
        m_Weapon.SetActive(false);

        Hitbox = (GameObject)Object.Instantiate(Resources.Load("DamageHitboxes/VortexAbilityHitbox"), pos, rot);
        Hitbox.GetComponent<VortexHitbox>().Initialize(m_Character, m_Type, (int)Damage, m_Lifetime, m_Weapon);

        Animator animator = m_Character.gameObject.GetComponentInChildren<Animator>();

        animator.SetLayerWeight(animator.GetLayerIndex("UpperBodyLayer"), 1f);

        animator.SetBool("UseVortex", true);

        #region Play Sound
        GameManager.audioManager.PlaySoundAtPosition(AudioManager.Sounds.VORTEX, m_Character.transform, m_Character.transform.position); 
        #endregion
    }
}
