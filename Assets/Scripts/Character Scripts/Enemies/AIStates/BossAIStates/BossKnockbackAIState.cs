using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossKnockbackAIState : AIState {

    public BossKnockbackAIState(Boss1AI _Owner)
        : base(_Owner)
    {
        Type = StateType.Knockback;
    }

    public override void Update()
    {

        Owner.Stats.Abilities[4].Use();

        if (Target == null)
        {
            return;
        }

        if (Vector3.Distance(Target.gameObject.transform.position, Owner.gameObject.transform.position) > Owner.AttackDistance)
        {
            Owner.SwitchState(new BossChaseAIState((Boss1AI)Owner));
        }
        else
        {
            Owner.SwitchState(new BossAttackAIState((Boss1AI)Owner));
        }
    }

    public override void Deactivate()
    {
        Agent.enabled = true;
    }

    public override void Activate()
    {
        Agent.isStopped = true;
        Agent.enabled = false;
        //Debug.Log("In Knockback State");
    }
}
