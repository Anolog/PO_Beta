using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetHealAbility : Ability {

    public static float RECOVERY_TIME = 0.15f;
    public static float CAST_TIME = 0.67f;
    public static float COOL_DOWN_TIME = 0f;
    public static float BASE_DAMAGE = 0f;
    public static string ABILITY_NAME = "Target Heal";

    private GameObject m_Target;
    private float m_HealSpeed;

    public TargetHealAbility(CharacterStats character, float healSpeed)
        : base(character)
    {
        m_Type = AbilityType.Ranged;
        m_CastTime = CAST_TIME;
        m_RecoveryTime = RECOVERY_TIME;
        m_CoolDownTime = COOL_DOWN_TIME;
        m_BaseDamage = BASE_DAMAGE;
        AbilityName = ABILITY_NAME;
        m_DefaultCooldown = COOL_DOWN_TIME;
        m_HealSpeed = healSpeed;

        Damage = m_BaseDamage + m_Character.Damage;
    }

    public override void Use(GameObject target)
    {
        base.Use();
        m_Target = target;
    }

    protected override void ActivateEffect()
    {
        Vector3 pos = m_Character.gameObject.transform.position;

        if (m_Target == null)
        {
            return;
        }
        Vector3 dir = m_Target.transform.position - m_Character.gameObject.transform.position;

        Quaternion rot = Quaternion.LookRotation(Vector3.up, dir);

        pos.y += m_Character.gameObject.GetComponent<CapsuleCollider>().height * 0.5f;
        pos.x += m_Character.gameObject.transform.forward.x;
        pos.z += m_Character.gameObject.transform.forward.z;

        Hitbox = (GameObject)Object.Instantiate(Resources.Load("HealingHitboxes/TargetHealAbilityHitbox"), pos, rot);
        Hitbox.GetComponent<TargetHealHitbox>().Initialize(m_Character, m_Type, (int)Damage, m_Target, m_HealSpeed);

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
        GameManager.audioManager.PlaySoundAtPosition(AudioManager.Sounds.TARGET_HEAL, tempListener.transform, m_Character.transform.position);
    }
}
