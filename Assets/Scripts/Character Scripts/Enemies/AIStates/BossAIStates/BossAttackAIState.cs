using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossAttackAIState : AIState {

    public BossAttackAIState(Boss1AI _Owner)
        : base(_Owner)
    {
        Type = StateType.Attack;
    }

    public override void Update()
    {
        Owner.LookAtTarget();

        if (Owner.attackTimer <= Time.time)
        {
            Owner.Stats.Abilities[0].Use();
            Owner.attackTimer = Time.time + Owner.timeBetweenAttacks;
        }
        if (Vector3.Distance(Target.gameObject.transform.position, Owner.gameObject.transform.position) > Owner.AttackDistance)
        {
            Owner.SwitchState(new BossChaseAIState((Boss1AI)Owner));
        }
    }

    public override void Deactivate()
    {

    }

    public override void Activate()
    {
        Agent.enabled = true;
        //Agent.isStopped = true;
        //Debug.Log("In Attack State");
    }
}
