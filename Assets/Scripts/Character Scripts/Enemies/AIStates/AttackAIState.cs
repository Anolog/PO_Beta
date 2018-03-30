using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackAIState : AIState {

    public AttackAIState(EnemyAI _Owner)
        : base(_Owner)
    {
        Type = StateType.Attack;
    }

    public override void Update()
    {
        Owner.LookAtTarget();

        if (Owner.attackTimer <= Time.time)
        {
            Owner.Animator.SetTrigger("UseAttack");
            Owner.Stats.Abilities[0].Use();
            Owner.attackTimer = Time.time + Owner.timeBetweenAttacks;
        }
    }

    public override void Deactivate()
    {

    }

    public override void Activate()
    {
        //Debug.Log("In Attack");
        Agent.isStopped = false;
        //Owner.attackTimer = Time.time + Owner.timeBeforeFirstAttack;
    }
}
