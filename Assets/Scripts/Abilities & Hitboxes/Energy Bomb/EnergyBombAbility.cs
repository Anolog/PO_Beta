using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyBombAbility : Ability {

    public static float RECOVERY_TIME = 0.15f;
    public static float CAST_TIME = 0.1f;
    public static float COOL_DOWN_TIME = 8f;
    public static float LIFE_TIME = 5f;
    public static string ABILITY_NAME = "Energy Bomb";
    public static float REDUCED_COOLDOWN = 0.9f;

    public GameObject Bomb;

    private GameObject m_Weapon;

    private Transform HelperTransform;

    private static float MIN_DISTANCE = 0.25f;
    private static float MAX_DISTANCE = 12f;
    const float LAUNCH_ANGLE = 15.0f;
    public float SPEED_MULTIPLIER = 1f;

    public EnergyBombAbility(CharacterStats character)
        : base(character)
    {
        m_Type = AbilityType.Ranged;
        m_CastTime = CAST_TIME;
        m_RecoveryTime = RECOVERY_TIME;
        m_CoolDownTime = COOL_DOWN_TIME;
        m_BaseDamage = 0;
        AbilityName = ABILITY_NAME;
        m_ReducedCooldown = REDUCED_COOLDOWN;
        m_DefaultCooldown = COOL_DOWN_TIME;
        Damage = m_BaseDamage + m_Character.Damage;

        m_Lifetime = LIFE_TIME;
    }

    protected override void ActivateEffect()
    {
        #region Play throwing sound
        // play a throwing sound
        GameManager.audioManager.PlaySoundAtPosition(AudioManager.Sounds.ENERGY_BOMB, m_Character.transform, m_Character.transform.position);
        //goat
        #endregion

        IEnumerator hideWeapons = HideWeaponsCoroutine(0.6f);
        m_Character.StartCoroutine(hideWeapons);

        Animator animator = m_Character.gameObject.GetComponentInChildren<Animator>();

        animator.SetLayerWeight(animator.GetLayerIndex("UpperBodyLayer"), 1f);

        animator.SetTrigger("UseEnergyBomb");

        HelperTransform = m_Character.gameObject.transform.Find("Mesh_Player");
        HelperTransform = HelperTransform.Find("Marine001Pelvis");
        HelperTransform = HelperTransform.Find("Marine001Spine1");
        HelperTransform = HelperTransform.Find("Marine001Spine2");
        HelperTransform = HelperTransform.Find("Marine001Spine3");
        HelperTransform = HelperTransform.Find("Marine001Ribcage");
        HelperTransform = HelperTransform.Find("Marine001RCollarbone");
        HelperTransform = HelperTransform.Find("Marine001RUpperarm");
        HelperTransform = HelperTransform.Find("Marine001RForearm1");
        HelperTransform = HelperTransform.Find("Marine001RForearm2");
        HelperTransform = HelperTransform.Find("Marine001RForearm3");
        HelperTransform = HelperTransform.Find("Marine001RPalm");
        HelperTransform = HelperTransform.Find("SwordHelper");
      
        // determine a position over my shoulder to start the throw from
        Vector3 pos = HelperTransform.position;
        //pos.y += 1.6f;
        //pos.x += m_Character.transform.forward.x * 0.5f;
        //pos.z += m_Character.transform.forward.z * 0.5f;
        Quaternion rot = m_Character.transform.rotation;

        Bomb = (GameObject)Object.Instantiate(Resources.Load("EnergyBomb/EnergyBomb"), pos, rot);
        Bomb.GetComponent<EnergyBombProjectile>().Initialize(m_Character, m_Type, (int)Damage, m_Lifetime);

        IEnumerator throwBomb = ThrowBombCoroutine(0.34f);
        m_Character.StartCoroutine(throwBomb);
    }

    IEnumerator ThrowBombCoroutine(float duration)
    {
        while (duration > 0)
        {
            Bomb.transform.position = HelperTransform.position;
            duration -= Time.deltaTime;
            yield return null;
        }

        Bomb.GetComponentInChildren<Collider>().enabled = true;

        // get vertical rotation from third person camera (-1 to 1 value)
        float verticalRotation = m_Character.gameObject.GetComponentInChildren<Third_Person_Camera>().VerticalRotation;
        // normalize this value (0 to 1 value)
        verticalRotation += 1f; // makes it from 0 to 2
        verticalRotation *= 0.5f; // makes it from 0 to 1
        if (verticalRotation < 0.1f)
        {
            verticalRotation = 0f;
        }
        // use this value to lerp between my min throw distance, and my max throw distance (also determine these)
        float horizontalDist = Mathf.Lerp(MIN_DISTANCE, MAX_DISTANCE, verticalRotation);
        float verticalDist = 0f;
        // copy rock thrower code to determine the force I need to throw at
        float g = -Physics.gravity.y;
        float xSqr = Mathf.Pow(horizontalDist, 2);
        float sin2Angle = Mathf.Sin(LAUNCH_ANGLE * 2 * Mathf.Deg2Rad);
        float cosSqrAngle = Mathf.Cos(LAUNCH_ANGLE * Mathf.Deg2Rad);

        float speed = Mathf.Sqrt((xSqr * g) / (horizontalDist * sin2Angle - 2 * verticalDist * cosSqrAngle)) * SPEED_MULTIPLIER;

        Vector2 direction2D = new Vector2(m_Character.transform.forward.x, m_Character.transform.forward.z);
        Vector3 xForce = Mathf.Cos(LAUNCH_ANGLE * Mathf.Deg2Rad) * speed * Bomb.GetComponent<Rigidbody>().mass * Vector3.Normalize(new Vector3(direction2D.x, 0, direction2D.y));
        Vector3 yForce = Mathf.Sin(LAUNCH_ANGLE * Mathf.Deg2Rad) * speed * Bomb.GetComponent<Rigidbody>().mass * Vector3.up;

        // use this:
        Bomb.GetComponent<Rigidbody>().AddForce(xForce + yForce, ForceMode.Impulse);

        yield return null;
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

        Animator animator = m_Character.gameObject.GetComponentInChildren<Animator>();

        animator.SetLayerWeight(animator.GetLayerIndex("UpperBodyLayer"), 0f);

        yield return null;
    }
}
