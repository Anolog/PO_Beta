using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChainLightningAbility : Ability {

    public static float RECOVERY_TIME = 0.15f;
    public static float CAST_TIME = 0.17f;
    public static float COOL_DOWN_TIME = 8f;
    public static float BASE_DAMAGE = 17f;
    public static float LIFE_TIME = 0.5f;
    public static string ABILITY_NAME = "Chain Lightning";
    public static float REDUCED_COOLDOWN = 1.0f;

    private GameObject m_Weapon;

    public ChainLightningAbility(CharacterStats character)
        : base(character)
    {
        m_Type = AbilityType.Ranged;
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
            IEnumerator hideWeapons = HideWeaponsCoroutine(m_ReducedCooldown - 0.3f);
            m_Character.StartCoroutine(hideWeapons);

            Animator animator = m_Character.gameObject.GetComponentInChildren<Animator>();

            animator.SetTrigger("UseChainLightning");

            m_Character.usingChainLightning = true;

            base.Use();
        }
        else
        {
            GameManager.audioManager.PlaySound(AudioManager.Sounds.ABILITY_STILL_ON_CD);
        }
    }

    protected override void ActivateEffect()
    {
        // Find the position of the bow helper as it is in the left hand
        // it is found in the bones of the mesh - and it needs to be there so it can be parented to palm properly
        // if a better way of doing this is found I will switch

        Transform helperTransform = m_Character.gameObject.transform.Find("Mesh_Player");
        helperTransform = helperTransform.Find("Marine001Pelvis");
        helperTransform = helperTransform.Find("Marine001Spine1");
        helperTransform = helperTransform.Find("Marine001Spine2");
        helperTransform = helperTransform.Find("Marine001Spine3");
        helperTransform = helperTransform.Find("Marine001Ribcage");
        helperTransform = helperTransform.Find("Marine001LArmCollarbone");
        helperTransform = helperTransform.Find("Marine001LArmUpperarm");
        helperTransform = helperTransform.Find("Marine001LArmForearm1");
        helperTransform = helperTransform.Find("Marine001LArmForearm2");
        helperTransform = helperTransform.Find("Marine001LArmForearm3");
        helperTransform = helperTransform.Find("Marine001LArmPalm");
        helperTransform = helperTransform.Find("HelperBow");

        Vector3 pos = helperTransform.position;
        Quaternion rot = m_Character.gameObject.transform.rotation;

        float angleX = m_Character.gameObject.GetComponentInChildren<Third_Person_Camera>().transform.rotation.eulerAngles.x;
        if (angleX > 300f)
            angleX -= 360f;

        //Debug.Log(angleX);

        rot = Quaternion.Euler(angleX, rot.eulerAngles.y, rot.eulerAngles.z);

        //Debug.Log(rot.eulerAngles.x);

        //pos.y += 1.4f;
        //pos.x += m_Character.gameObject.transform.forward.x;
        //pos.z += m_Character.gameObject.transform.forward.z;

        Vector3 look = m_Character.gameObject.GetComponentInChildren<Third_Person_Camera>().m_LookTarget;

        Vector3 dir = (look - pos).normalized;

        rot = Quaternion.FromToRotation(Vector3.up, dir);

        Hitbox = (GameObject)Object.Instantiate(Resources.Load("DamageHitboxes/ChainLightningAbilityHitbox"), pos, rot);
        Hitbox.GetComponent<ChainLightningHitbox>().Initialize(m_Character, m_Type, (int)Damage, m_Lifetime, new List<GameObject>());

        m_Character.GetComponent<PlayerStats>().Rumble(0.2f, 0.4f, m_Lifetime);
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
        m_Character.usingChainLightning = false;

        yield return null;
    }
}
