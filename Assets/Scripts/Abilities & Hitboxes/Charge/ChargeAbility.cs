using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChargeAbility : Ability {

    public static float RECOVERY_TIME = 0.15f;
    public static float CAST_TIME = 0.4f;
    public static float COOL_DOWN_TIME = 9f;
    public static float BASE_DAMAGE = 10f;
    public static float LIFE_TIME = 2f;
    public static string ABILITY_NAME = "Charge";
    public static float REDUCED_COOLDOWN = 1.8f;

    private GameObject m_Weapon;

    public ChargeAbility(CharacterStats character)
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

        if (m_Character.gameObject.transform.Find("Bow").gameObject.activeSelf)
        {
            m_Weapon = m_Character.gameObject.transform.Find("Bow").gameObject;
        }
        else
        {
            m_Weapon = m_Character.gameObject.transform.Find("SwordAndShield").gameObject;
        }
    }

    public override void Use()
    {
        if (m_CoolDownTimer < Time.time)
        {
            base.Use();

            Animator animator = m_Character.gameObject.GetComponentInChildren<Animator>();

            if (m_Weapon.name == "SwordAndShield")
            {
                //animator.SetLayerWeight(animator.GetLayerIndex("UpperBodyLayer"), 1f);
                animator.SetBool("UseChargeSword", true);
            }
            else
            {
                animator.SetBool("UseChargeBow", true);
            }
        }
        else
        {
            GameManager.audioManager.PlaySound(AudioManager.Sounds.ABILITY_STILL_ON_CD);
        }
    }

    protected override void ActivateEffect()
    {
        Vector3 velocity = new Vector3(10,0,10);
        //m_Character.LockoutCharacter(1.8f);
        m_Character.gameObject.GetComponent<Rigidbody>().velocity = velocity;

        Vector3 pos = m_Character.transform.position;
        pos.y += 0.9f;
        pos.z += 0.2f * m_Character.transform.forward.z;
        pos.x += 0.2f * m_Character.transform.forward.x;

        Quaternion rot = m_Character.transform.rotation;

        Hitbox = (GameObject)Object.Instantiate(Resources.Load("DamageHitboxes/ChargeAbilityHitbox"), pos, rot);
        Hitbox.GetComponent<Hitbox>().Initialize(m_Character, m_Type, (int)Damage, m_Lifetime);

        Hitbox.GetComponent<ChargeHitbox>().SetUpCharge(velocity, m_Character.transform.position);

        m_Character.usingCharge = true;
        m_Character.Grounded = false;


        #region Play sound
        // play a yelling sound
        GameManager.audioManager.PlaySoundAtPosition(AudioManager.Sounds.CHARGE, m_Character.transform, m_Character.transform.position); 
        #endregion
    }
}
