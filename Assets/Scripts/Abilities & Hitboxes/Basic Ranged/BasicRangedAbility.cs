using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicRangedAbility : Ability
{

    public static float RECOVERY_TIME = 0.15f;
    public static float CAST_TIME = 0.25f;
    public static float COOL_DOWN_TIME = 0.0f;
    public static float BASE_DAMAGE = 15;
    public static float LIFE_TIME = 5f;
    public static string ABILITY_NAME = "Bow";
    public static float REDUCED_COOLDOWN = 1f;
    public static float ANIMATION_DURATION = 0.75f;

    public BasicRangedAbility(CharacterStats character)
        : base(character)
    {
        m_Type = AbilityType.Ranged;
        m_CastTime = CAST_TIME;
        m_RecoveryTime = RECOVERY_TIME;
        m_BaseDamage = BASE_DAMAGE;
        AbilityName = ABILITY_NAME;
        m_ReducedCooldown = REDUCED_COOLDOWN;
        AnimationDuration = ANIMATION_DURATION;
        float attackTime = m_Character.AttSpd;
        m_CoolDownTime = attackTime + COOL_DOWN_TIME;
        m_DefaultCooldown = attackTime + COOL_DOWN_TIME;

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
            GameManager.audioManager.PlaySound(GameManager.audioManager.GetSoundFromEffect("Bow Fire", false));
        }
        else
        {
            GameManager.audioManager.PlaySound(AudioManager.Sounds.ABILITY_STILL_ON_CD);
        }
    }

    protected override void ActivateEffect()
    {
        Vector3 pos = m_Character.transform.position;
        Quaternion rot = Quaternion.identity;

        rot.eulerAngles = new Vector3(m_Character.gameObject.GetComponentInChildren<Third_Person_Camera>().transform.rotation.eulerAngles.x, m_Character.transform.rotation.eulerAngles.y, m_Character.transform.rotation.eulerAngles.z);

        //Debug.Log(rot.eulerAngles);

        float yForce = m_Character.gameObject.GetComponentInChildren<Third_Person_Camera>().transform.rotation.eulerAngles.x;

        //Debug.Log(yForce);

        pos.y += 1.3f;
        pos.x += Mathf.Sin(rot.eulerAngles.y * (Mathf.PI / 180));
        pos.z += Mathf.Cos(rot.eulerAngles.y * (Mathf.PI / 180));

        if (yForce > 300f)
            yForce -= 360f;

        Vector3 force = new Vector3(90, 1.5f, 90);
        force.x *= m_Character.gameObject.transform.forward.x;
        force.y *= -yForce;
        force.z *= m_Character.gameObject.transform.forward.z;

        Hitbox = (GameObject)Object.Instantiate(Resources.Load("DamageHitboxes/BasicRangedAbilityHitbox"), pos, rot);
        Hitbox.GetComponent<Hitbox>().Initialize(m_Character, m_Type, (int)Damage, m_Lifetime);
        Hitbox.GetComponent<Rigidbody>().AddForce(force, ForceMode.Impulse);

        // play a shooting sound
        AudioManager.Sounds sound = GameManager.audioManager.GetSoundFromEffect("Bow Fire", true);
        GameManager.audioManager.PlaySoundAtPosition(sound, m_Character.transform, m_Character.transform.position + m_Character.transform.forward);

        //Debug.Log(force);
    }

    IEnumerator PlayAnimationCoroutine(float duration)
    {
        Animator animator = m_Character.gameObject.GetComponentInChildren<Animator>();

        animator.SetLayerWeight(animator.GetLayerIndex("UpperBodyWithPelvisLayer"), 1f);

        animator.SetTrigger("UseBasicRanged");

        Animator bowAnimator = m_Character.gameObject.transform.Find("Bow").GetComponentInChildren<Animator>();
        bowAnimator.SetTrigger("Fire");

        //GameObject bow = m_Character.gameObject.transform.Find("Bow").gameObject;

        //bow.GetComponent<WeaponPosition>().shouldUpdate = false;

        yield return new WaitForSeconds(duration);

        animator.SetLayerWeight(animator.GetLayerIndex("UpperBodyWithPelvisLayer"), 0f);
        //bow.GetComponent<WeaponPosition>().shouldUpdate = true;

        yield return null;
    }
}
