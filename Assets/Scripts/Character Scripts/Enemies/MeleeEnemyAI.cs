using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI; // Use this for the NavMeshAgent

public class MeleeEnemyAI : EnemyAI {

    protected override void Awake()
    {
        base.Awake();
        // Set our enemies attack ability
        Stats.Abilities[0] = new EnemyMeleeAbility(Stats);
        AttackDistance = 1.75f;
        Agent.stoppingDistance = AttackDistance;
        Agent.avoidancePriority = 50;

        // Find our target
        Players = GameManager.playerManager.PlayerList(); //GameObject.FindGameObjectsWithTag("Player");
        FindTarget();
        if (Target != null)
        {
            Agent.destination = Target.transform.position;
            SwitchState(new ChaseAIState(this));
        }
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        
        //if (Agent.remainingDistance <= Agent.stoppingDistance)
        //{
        //    Debug.Log("In here");
        //    Animator.SetBool("Walk", false);
        //}
    }

    protected override void CheckStateChange()
    {
        if (Vector3.Distance(transform.position, Target.transform.position) <= AttackDistance)
        {
            Animator.SetBool("Walk", false);
            SwitchState(new AttackAIState(this));
            return;
        }
        if (Vector3.Distance(transform.position, Target.transform.position) > AttackDistance || CheckHealth())
        {
            //Animator.SetBool("Walk", true);
            SwitchState(new ChaseAIState(this));
            return;
        }
    }
}
