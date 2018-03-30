using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RockThrowAbility : Ability
{

    public static float RECOVERY_TIME = 0.15f;
    public static float CAST_TIME = 0.1f;
    public static float COOL_DOWN_TIME = 0f;
    public static float BASE_DAMAGE = 0;
    public static float LIFE_TIME = 7f;

    const float LAUNCHANGLE = 30.0f;

    private Transform HelperTransform;

    Vector3 XForce;
    Vector3 YForce;

    public RockThrowAbility(CharacterStats character)
        : base(character)
    {
        m_Type = AbilityType.Ranged;
        m_CastTime = CAST_TIME;
        m_RecoveryTime = RECOVERY_TIME;
        m_CoolDownTime = COOL_DOWN_TIME;
        m_BaseDamage = BASE_DAMAGE;
        m_DefaultCooldown = COOL_DOWN_TIME;
        Damage = m_BaseDamage + m_Character.Damage;

        m_Lifetime = 7f;
    }


    protected override void ActivateEffect()
    {
        Animator animator = m_Character.gameObject.GetComponentInChildren<Animator>();
        animator.SetLayerWeight(animator.GetLayerIndex("UpperBody"), 1f);

        Vector3 pos = m_Character.transform.position;
        Quaternion rot = Quaternion.identity;
        //Vector3 euler = rot.eulerAngles;
        //euler.x = -90;
        //euler.y = -280;
        //euler.z = -180;
        //rot = Quaternion.Euler(rot.x, rot.y, rot.z);

        //GameObject target = m_Character.gameObject.GetComponent<EnemyAI>().Target;

        //Vector3 handPos = m_Character.gameObject.transform.GetChild(0).Find("Helper_Rock").position;// + m_Character.transform.forward;

        HelperTransform = m_Character.gameObject.transform.GetChild(0).Find("Helper_Rock");

        pos = HelperTransform.position;

        //Vector2 my2DPosition = new Vector2(pos.x, pos.z);
        //Vector2 target2DPosition = new Vector2(target.transform.position.x, target.transform.position.z);
        //float horizontalDist = Vector2.Distance(my2DPosition, target2DPosition);

        //float myYPosition = pos.y;
        //float targetYPosition = target.transform.position.y;
        //float verticalDist = (targetYPosition + target.GetComponent<CapsuleCollider>().height * 0.5f) - (myYPosition);

        //float g = -Physics.gravity.y;

        //float xSqr = Mathf.Pow(horizontalDist, 2);
        //float sin2Angle = Mathf.Sin(LAUNCHANGLE * 2 * Mathf.Deg2Rad);
        //float cosSqrAngle = Mathf.Cos(LAUNCHANGLE * Mathf.Deg2Rad);

        //float speed = Mathf.Sqrt((xSqr * g) / (horizontalDist * sin2Angle - 2 * verticalDist * cosSqrAngle));

        //if (float.IsNaN(speed))
        //{
        //    return;
        //}

        m_Character.gameObject.GetComponent<RangedEnemyAI>().Projectile.SetActive(false);

        Hitbox = (GameObject)Object.Instantiate(Resources.Load("DamageHitboxes/RockThrowAbilityHitbox"), pos, rot);
        Hitbox.GetComponent<Hitbox>().Initialize(m_Character, m_Type, (int)Damage, m_Lifetime);

        // find which player is the listener
        GameObject tempListener = GameManager.playerManager.PlayerList()[0];
        float dist = float.MaxValue;
        foreach (GameObject player in GameManager.playerManager.PlayerList())
        {
            if (Vector3.Distance(player.transform.position, m_Character.transform.position) < dist)
            {
                dist = Vector3.Distance(player.transform.position, m_Character.transform.position);
                tempListener = player;
            }
        }
        GameManager.audioManager.PlaySoundAtPosition(AudioManager.Sounds.BOULDER_THROW, tempListener.transform, m_Character.transform.position);

        IEnumerator throwRock = ThrowRockCoroutine(0.3f);
        m_Character.StartCoroutine(throwRock);

        //Hitbox.GetComponent<Rigidbody>().AddForce(xForce + yForce, ForceMode.Impulse);
    }

    IEnumerator ThrowRockCoroutine(float duration)
    {
        while (duration > 0 && Hitbox != null)
        {
            Hitbox.transform.position = HelperTransform.position;
            duration -= Time.deltaTime;
            yield return null;
        }

        GameObject target = m_Character.gameObject.GetComponent<EnemyAI>().Target;

        Vector3 pos = HelperTransform.position;

        Vector2 my2DPosition = new Vector2(pos.x, pos.z);

        if (target == null)
        {
            yield break;
        }

        Vector2 target2DPosition = new Vector2(target.transform.position.x, target.transform.position.z);
        float horizontalDist = Vector2.Distance(my2DPosition, target2DPosition);

        float myYPosition = pos.y;
        float targetYPosition = target.transform.position.y;
        float verticalDist = (targetYPosition + target.GetComponent<CapsuleCollider>().height * 0.5f) - (myYPosition);

        float g = -Physics.gravity.y;

        float xSqr = Mathf.Pow(horizontalDist, 2);
        float sin2Angle = Mathf.Sin(LAUNCHANGLE * 2 * Mathf.Deg2Rad);
        float cosSqrAngle = Mathf.Cos(LAUNCHANGLE * Mathf.Deg2Rad);

        float speed = Mathf.Sqrt((xSqr * g) / (horizontalDist * sin2Angle - 2 * verticalDist * cosSqrAngle));

        if (float.IsNaN(speed))
        {
            yield break;
        }

        Vector2 direction2D = target2DPosition - my2DPosition;

        if (Hitbox == null)
        {
            yield break;
        }

        Vector3 xForce = Mathf.Cos(LAUNCHANGLE * Mathf.Deg2Rad) * speed * Hitbox.GetComponent<Rigidbody>().mass * Vector3.Normalize(new Vector3(direction2D.x, 0, direction2D.y));
        Vector3 yForce = Mathf.Sin(LAUNCHANGLE * Mathf.Deg2Rad) * speed * Hitbox.GetComponent<Rigidbody>().mass * Vector3.up;

        XForce = xForce * 1.35f; // magic number i needed when I put in the animation. This it has to do with me making the projectile follow the hand a bit before it is thrown
        YForce = yForce * 1.35f;

        Hitbox.GetComponent<Rigidbody>().AddForce(XForce + YForce, ForceMode.Impulse);

        m_Character.gameObject.GetComponent<RangedEnemyAI>().Reload();

        yield return null;

    }
}
