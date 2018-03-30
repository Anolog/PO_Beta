using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealAbility : Ability
{

    public static float RECOVERY_TIME = 0.15f;
    public static float CAST_TIME = 0.0f;
    public static float COOL_DOWN_TIME = 15f;
    public static float HEAL_AMOUNT = 45;
    public static float LIFE_TIME = 0.5f;
    public static string ABILITY_NAME = "Heal";
    public static float REDUCED_COOLDOWN = 0.8f;

    protected float m_HealAmount;

    private GameObject m_Weapon;

    public HealAbility(CharacterStats character)
        : base(character)
    {
        m_Type = AbilityType.Melee;
        m_CastTime = CAST_TIME;
        m_RecoveryTime = RECOVERY_TIME;
        m_CoolDownTime = COOL_DOWN_TIME;
        AbilityName = ABILITY_NAME;
        m_ReducedCooldown = REDUCED_COOLDOWN;
        m_DefaultCooldown = COOL_DOWN_TIME;
        m_HealAmount = HEAL_AMOUNT;

        m_Lifetime = LIFE_TIME;
    }

    protected override void ActivateEffect()
    {
        Vector3 pos = m_Character.transform.position;
        Quaternion rot = m_Character.transform.rotation;

        // If I am an enemy move the position of my hitbox away from my pivot point, 
        // so that the pivot point of the hitbox is at the ground
        if (m_Character.gameObject.tag == "Enemy")
        {
            pos.y -= 1;
        }

        Hitbox = (GameObject)Object.Instantiate(Resources.Load("HealingHitboxes/HealAbilityHitbox"), pos, rot);
        Hitbox.GetComponent<HealHitbox>().Initialize((PlayerStats)m_Character, (int)m_HealAmount, m_Lifetime);

        Animator animator = m_Character.gameObject.GetComponentInChildren<Animator>();

        animator.SetTrigger("UseHeal");

        IEnumerator hideWeapons = HideWeaponsCoroutine(0.5f);
        m_Character.StartCoroutine(hideWeapons);

        m_Character.usingHeal = true;

        #region Play healing sound
        // Play some sourt of healing sound
        GameManager.audioManager.PlaySoundAtPosition(AudioManager.Sounds.HEAL, m_Character.transform, m_Character.transform.position); 
        #endregion
    }

    IEnumerator HideWeaponsCoroutine(float duration)
    {
        if (m_Character.gameObject.transform.Find("Bow").gameObject.activeSelf)
        {
            m_Weapon = m_Character.gameObject.transform.Find("Bow").gameObject;
        }
        else
        {
            m_Weapon = m_Character.gameObject.transform.Find("SwordAndShield").gameObject;
        }
        m_Weapon.SetActive(false);

        yield return new WaitForSeconds(duration);

        m_Weapon.SetActive(true);
        m_Character.usingHeal = false;

        yield return null;
    }
}
