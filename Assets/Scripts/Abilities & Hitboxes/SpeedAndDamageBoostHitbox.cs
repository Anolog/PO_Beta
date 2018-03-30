using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedAndDamageBoostHitbox : Hitbox {

    public static float ENEMY_SPEED_INCREASE_PERCENT = 0.5f,
                        ENEMY_ATTACK_DAMAGE_PERCENT = 0.5f;

    private int RegularDamage;

    private List<GameObject> m_ObjectsInShield = new List<GameObject>();

    private SupportEnemyAI m_EnemeyAI;

    protected override void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Enemy" || other.gameObject.tag == "Boss")
        {
            m_ObjectsInShield.Add(other.gameObject);
            EnemyAI AI = other.gameObject.GetComponent<EnemyAI>();

            RegularDamage = AI.Stats.Damage;

            AI.Agent.speed += AI.Stats.MovementSpeed * ENEMY_SPEED_INCREASE_PERCENT;
            AI.Stats.Damage += (int)(RegularDamage * ENEMY_ATTACK_DAMAGE_PERCENT);
        }
    }

    protected void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Enemy" || other.gameObject.tag == "Boss")
        {
            m_ObjectsInShield.Remove(other.gameObject);
            EnemyAI AI = other.gameObject.GetComponent<EnemyAI>();
            AI.Agent.speed = AI.Stats.MovementSpeed;
            AI.Stats.Damage = RegularDamage;
        }
    }

    protected void OnDestroy()
    {
        foreach (GameObject obj in m_ObjectsInShield)
        {
            if (obj == null)
            {
                return;
            }
            if (obj.tag == "Enemy" || obj.tag == "Boss")
            {
                EnemyAI AI = obj.GetComponent<EnemyAI>();
                AI.Agent.speed = AI.Stats.MovementSpeed;
                AI.Stats.Damage = RegularDamage;
            }
        }

        m_ObjectsInShield.Clear();
    }
}
