using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProtectMeAbility : Ability {

    public static float RECOVERY_TIME = 0.15f;
    public static float CAST_TIME = 0.1f;
    public static float COOL_DOWN_TIME = 18f;
    public static float BASE_DAMAGE = 0;
    public static float LIFE_TIME = 6f;
    public static string ABILITY_NAME = "Protect Me";
    public static float RAYCAST_DOWN_AMOUNT = 6.0f;
    public static float REDUCED_COOLDOWN = 0.8f;

    private GameObject m_Weapon;

    public ProtectMeAbility(CharacterStats character)
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

    protected override void ActivateEffect()
    {
        Vector3 pos = m_Character.transform.position;
        Quaternion rot = m_Character.transform.rotation;

        //Raycast to make sure it spawns on the ground when used
        Ray raycast = new Ray(pos + Vector3.up, Vector3.down * RAYCAST_DOWN_AMOUNT);
        RaycastHit hitInfo = new RaycastHit();

        if (Physics.Raycast(raycast, out hitInfo))
        {
            if (hitInfo.transform.tag == "Platform" || hitInfo.transform.tag == "Wall")
            {
                pos -= new Vector3(0, hitInfo.distance - 1, 0);
            }
        }

        Hitbox = (GameObject)Object.Instantiate(Resources.Load("HealingHitboxes/ProtectMeAbilityHitbox"), pos, rot);
        Hitbox.GetComponent<Hitbox>().Initialize(m_Character, m_Type, (int)Damage, m_Lifetime);

        IEnumerator hideWeapons = HideWeaponsCoroutine(0.5f);
        m_Character.StartCoroutine(hideWeapons);

        Animator animator = m_Character.gameObject.GetComponentInChildren<Animator>();

        animator.SetTrigger("UseProtectMe");

        m_Character.usingProtectMe = true;


        #region Play Sound
        // play a yelling sound or something like that
        GameManager.audioManager.PlaySoundAtPosition(AudioManager.Sounds.PROTECT_ME, m_Character.transform, m_Character.transform.position); 
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
        m_Character.usingProtectMe = false;

        yield return null;
    }
}
