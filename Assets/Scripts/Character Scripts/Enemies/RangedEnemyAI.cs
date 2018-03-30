using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI; // Use this for the NavMeshAgent

public class RangedEnemyAI : EnemyAI
{

    public GameObject Projectile;

    override protected void Awake()
    {
        base.Awake();
        // Set our enemies attack ability
        Stats.Abilities[0] = new RockThrowAbility(Stats);
        AttackDistance = 10f;

        Agent.stoppingDistance = 10;
        Agent.avoidancePriority = Random.Range(25, 49);

        // Find our target
        Players = GameManager.playerManager.PlayerList(); //GameObject.FindGameObjectsWithTag("Player");
        FindTarget();
        if (Target != null)
        {
            Agent.destination = Target.transform.position;
            SwitchState(new ChaseAIState(this));
        }

    }

    protected override void CheckStateChange()
    {
        if (Vector3.Distance(transform.position, Target.transform.position) < AttackDistance / 2 && m_State.Type == StateType.Attack)
        {
            SwitchState(new KiteAIState(this));
            return;
        }
        if (Vector3.Distance(transform.position, Target.transform.position) <= AttackDistance && m_State.Type != StateType.Kite)
        {
            SwitchState(new AttackAIState(this));
            return;
        }
        if (Vector3.Distance(transform.position, Target.transform.position) > AttackDistance || CheckHealth())
        {
            SwitchState(new ChaseAIState(this));
            return;
        }
    }

    public void Reload()
    {
        Projectile.SetActive(true);
        Projectile.transform.localScale = Vector3.zero;
        Stats.StartCoroutine(ReloadCoroutine(0.5f));
    }

    IEnumerator ReloadCoroutine(float speed)
    {
        while(Projectile.transform.localScale != Vector3.one)
        {
            Projectile.transform.localScale += Vector3.one * Time.deltaTime * speed;

            if (Projectile.transform.localScale.x >= 1)
            {
                Projectile.transform.localScale = Vector3.one;
            }

            yield return null;
        }

        yield return null;
    }
}
