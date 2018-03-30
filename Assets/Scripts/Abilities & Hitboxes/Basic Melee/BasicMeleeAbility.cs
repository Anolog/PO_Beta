using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicMeleeAbility : Ability {

    public static float RECOVERY_TIME = 0.15f;
    public static float CAST_TIME = 0.2f;
    public static float COOL_DOWN_TIME = 0.0f;
    public static float BASE_DAMAGE = 12;
    public static float LIFE_TIME = 0.3f;
    public static string ABILITY_NAME = "Sword and Shield";
    public static float REDUCED_COOLDOWN = 1.0f;
    public static float ANIMATION_DURATION = 1.0f;

    public BasicMeleeAbility(CharacterStats character)
        : base(character)
    {
        m_Type = AbilityType.Melee;
        m_CastTime = CAST_TIME;
        m_BaseDamage = BASE_DAMAGE;
        m_RecoveryTime = RECOVERY_TIME;
        AbilityName = ABILITY_NAME;
        m_ReducedCooldown = REDUCED_COOLDOWN;
        AnimationDuration = ANIMATION_DURATION;
        float attackTime = m_Character.AttSpd;
        m_DefaultCooldown = attackTime + COOL_DOWN_TIME;
        m_CoolDownTime = attackTime + COOL_DOWN_TIME;
        Damage = m_BaseDamage + m_Character.Damage;

        m_Lifetime = LIFE_TIME;
    }

    public override void Use()
    {
        if (m_CoolDownTimer < Time.time)
        {
            base.Use();


            IEnumerator PlayAnimation = PlayAnimationCoroutine(AnimationDuration);
            m_Character.StartCoroutine(PlayAnimation);
            GameManager.audioManager.PlaySound(GameManager.audioManager.GetSoundFromEffect("Sword Swing", false));
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

        Hitbox = (GameObject)Object.Instantiate(Resources.Load("DamageHitboxes/BasicMeleeAbilityHitbox"), pos, rot);
        Hitbox.GetComponent<Hitbox>().Initialize(m_Character, m_Type, (int)Damage, m_Lifetime);
    }

    IEnumerator PlayAnimationCoroutine(float duration)
    {
        Animator animator = m_Character.gameObject.GetComponentInChildren<Animator>();
        animator.SetLayerWeight(animator.GetLayerIndex("UpperBodyLayer"), 1f);

        animator.SetTrigger("UseBasicMelee");

        yield return new WaitForSeconds(duration);

        animator.SetLayerWeight(animator.GetLayerIndex("UpperBodyLayer"), 0f);

        yield return null;
    }
}
