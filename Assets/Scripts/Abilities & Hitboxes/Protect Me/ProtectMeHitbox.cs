using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProtectMeHitbox : Hitbox {

    public static float PLAYER_DAMAGE_REDUCTION_PERCENT = 0.5f,
                        ENEMY_SLOW_PERCENT = 0.5f,
                        ENEMY_ATTACK_SLOW_PERCENT = 0.5f;

    private List<GameObject> m_ObjectsInShield = new List<GameObject>();

    protected override void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            m_ObjectsInShield.Add(other.gameObject);
            PlayerStats stats = other.gameObject.GetComponent<PlayerStats>();
            stats.DamageReduction = PLAYER_DAMAGE_REDUCTION_PERCENT;

        }
        if (other.gameObject.tag == "Enemy" || other.gameObject.tag == "Boss")
        {
            m_ObjectsInShield.Add(other.gameObject);
            EnemyAI AI = other.gameObject.GetComponent<EnemyAI>();
            AI.Agent.speed -= AI.Stats.MovementSpeed * ENEMY_SLOW_PERCENT;
            AI.timeBetweenAttacks += AI.Stats.AttSpd * ENEMY_ATTACK_SLOW_PERCENT;
        }
    }

    protected void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            m_ObjectsInShield.Remove(other.gameObject);
            PlayerStats stats = other.gameObject.GetComponent<PlayerStats>();
            stats.DamageReduction = 0;
        }
        if (other.gameObject.tag == "Enemy" || other.gameObject.tag == "Boss")
        {
            m_ObjectsInShield.Remove(other.gameObject);
            EnemyAI AI = other.gameObject.GetComponent<EnemyAI>();
            AI.Agent.speed = AI.Stats.MovementSpeed;
            AI.timeBetweenAttacks = AI.Stats.AttSpd;
        }
    }

    protected void OnDestroy()
    {
        foreach (GameObject obj in m_ObjectsInShield)
        {
            if (obj == null)
            {
                continue;
            }
            if (obj.tag == "Player")
            {
                PlayerStats stats = obj.GetComponent<PlayerStats>();
                stats.DamageReduction = 0;
            }
            if (obj.tag == "Enemy" || obj.tag == "Boss")
            {
                EnemyAI AI = obj.GetComponent<EnemyAI>();
                AI.Agent.speed = AI.Stats.MovementSpeed;
                AI.timeBetweenAttacks = AI.Stats.AttSpd;
            }
        }

        m_ObjectsInShield.Clear();
    }
}
