using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnrageAbility : Ability {

    public static float RECOVERY_TIME = 0.15f;
    public static float CAST_TIME = 0.8f;
    public static float COOL_DOWN_TIME = 13f;
    public static float BASE_DAMAGE = 0;
    public static float LIFE_TIME = 8f;
    public static string ABILITY_NAME = "Enrage";
    public static float REDUCED_COOLDOWN = 0.8f;

    private GameObject m_Weapon;

    public EnrageAbility(CharacterStats character)
        : base(character)
    {
        m_Type = AbilityType.Melee;
        m_CastTime = CAST_TIME;
        m_BaseDamage = BASE_DAMAGE;
        m_RecoveryTime = RECOVERY_TIME;
        AbilityName = ABILITY_NAME;
        m_ReducedCooldown = REDUCED_COOLDOWN;
        m_DefaultCooldown = COOL_DOWN_TIME;
        m_CoolDownTime = COOL_DOWN_TIME;
        Damage = m_BaseDamage + m_Character.Damage;

        m_Lifetime = LIFE_TIME;
    }

    public override void Use()
    {
        if (m_CoolDownTimer < Time.time)
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

            Animator animator = m_Character.gameObject.GetComponentInChildren<Animator>();

            animator.SetLayerWeight(animator.GetLayerIndex("UpperBodyLayer"), 1f);

            animator.SetBool("UseEnrage", true);

            base.Use();

            m_Character.StartCoroutine(ParticleEffectCoroutine(m_Lifetime + m_CastTime));
        }
        else
        {
            GameManager.audioManager.PlaySound(AudioManager.Sounds.ABILITY_STILL_ON_CD);
        }
    }

    IEnumerator ParticleEffectCoroutine(float time)
    {
        ParticleSystem[] particles = m_Character.transform.Find("Mesh_Player").GetComponentsInChildren<ParticleSystem>(true);

        foreach (ParticleSystem sys in particles)
        {
            sys.gameObject.SetActive(true);
        }

        yield return new WaitForSeconds(time);

        foreach (ParticleSystem sys in particles)
        {
            sys.gameObject.SetActive(false);
        }
    }

    protected override void ActivateEffect()
    {
        Animator animator = m_Character.gameObject.GetComponentInChildren<Animator>();

        animator.SetLayerWeight(animator.GetLayerIndex("UpperBodyLayer"), 0f);

        animator.SetBool("UseEnrage", false);

        m_Character.AddLifesteal(0.5f, m_Lifetime);
        m_Character.IncreaseAttackSpeed(1f, m_Lifetime);
        m_Character.IncreaseMovementSpeed(0.5f, m_Lifetime);
        m_Character.EnrageInEffect = true;

        ((PlayerStats)m_Character).Rumble(0.2f, 0.5f, 0.5f);

        m_Weapon.SetActive(true);

        #region Play Yelling Sound
        // play a yelling sound
        GameManager.audioManager.PlaySoundAtPosition(AudioManager.Sounds.ENRAGE, m_Character.transform, m_Character.transform.position); 
        #endregion
    }
}
