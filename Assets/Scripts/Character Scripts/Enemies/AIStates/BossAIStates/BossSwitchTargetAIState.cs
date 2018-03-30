using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossSwitchTargetAIState : AIState {

    public BossSwitchTargetAIState(Boss1AI owner)
        : base(owner)
    {
        Type = StateType.SwitchTarget;
    }
	
	// Update is called once per frame
	public override void Update ()
    {
        Owner.FindNewTarget(Target);
        Owner.SwitchState(new BossChaseAIState((Boss1AI)Owner));
	}

    public override void Deactivate()
    {
        
    }

    public override void Activate()
    {
        Agent.enabled = true;
        Agent.isStopped = true;
        //Debug.Log("In Switch Target State");
    }
}
