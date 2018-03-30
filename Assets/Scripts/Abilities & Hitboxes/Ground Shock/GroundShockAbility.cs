using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundShockAbility : Ability {

    public static float RECOVERY_TIME = 0.15f;
    public static float CAST_TIME = 0.35f;
    public static float COOL_DOWN_TIME = 8f;
    public static float BASE_DAMAGE = 20f;
    public static float LIFE_TIME = 5f;
    public static string ABILITY_NAME = "Ground Shock";
    public static float REDUCED_COOLDOWN = 0.75f;

    private GameObject m_Weapon;

    public GroundShockAbility(CharacterStats character)
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
        if (m_Character.Grounded)
        {
            if (m_CoolDownTimer < Time.time)
            {
                if (m_Character.gameObject.tag == "Player")
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
                }
                Animator animator = m_Character.gameObject.GetComponentInChildren<Animator>();

                animator.SetBool("UseGroundShock", true);

                m_Character.usingGroundShock = true;

                base.Use();
            }
            else
            {
                GameManager.audioManager.PlaySound(AudioManager.Sounds.ABILITY_STILL_ON_CD);
            }
        }
    }

    protected override void ActivateEffect()
    {
        m_Character.usingGroundShock = false;

        if (m_Character.gameObject.tag == "Player")
        {
            m_Weapon.SetActive(true);

            Animator animator = m_Character.gameObject.GetComponentInChildren<Animator>();
            animator.SetBool("UseGroundShock", false);
        }

        Vector3 pos = m_Character.transform.position;
        Quaternion rot = m_Character.transform.rotation;

        // If I am an enemy move the position of my hitbox away from my pivot point, 
        // so that the pivot point of the hitbox is at the ground
        if (m_Character.gameObject.tag == "Enemy")
        {
            pos.y -= 1;
        }

        Hitbox = (GameObject)Object.Instantiate(Resources.Load("DamageHitboxes/GroundShockAbilityHitbox"), pos, rot);
        Hitbox.GetComponent<Hitbox>().Initialize(m_Character, m_Type, (int)Damage, m_Lifetime);
        //Hitbox.GetComponentInChildren<Renderer>().enabled = false;

        if (m_Character.CompareTag("Player"))
        {
            ((PlayerStats)m_Character).Rumble(0.1f, 0.1f, 0.35f);
        }

        #region Play a use Sound
        // play an stomping(?) sound
        GameManager.audioManager.PlaySoundAtPosition(AudioManager.Sounds.GROUND_SHOCK, m_Character.transform, Hitbox.transform.position); 
        #endregion
    }
}
