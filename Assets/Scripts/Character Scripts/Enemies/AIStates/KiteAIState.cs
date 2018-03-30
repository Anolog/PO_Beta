using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KiteAIState : AIState {

    public KiteAIState(EnemyAI _Owner)
        : base(_Owner)
    {
        Owner.attackTimer = Time.time + Owner.timeBetweenAttacks;
        Type = StateType.Kite;
    }

    public override void Update()
    {
        Owner.LookAtTarget();

        Vector3 direction = new Vector3(Target.gameObject.transform.forward.x, 0, Target.gameObject.transform.forward.z);
        direction.Normalize();
        Agent.destination = 10 * direction;

        if (Owner.attackTimer <= Time.time || Vector3.Distance(Owner.transform.position, Target.transform.position) >= Owner.AttackDistance / 2)
        {
            Owner.SwitchState(new AttackAIState(Owner));
        }
    }

    public override void Deactivate()
    {

    }

    public override void Activate()
    {
        //Debug.Log("In Kite");
        Agent.isStopped = false;
    }
}
