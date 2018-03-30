using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitAIState : AIState {

    public float WaitTime;

    public WaitAIState(EnemyAI _Owner, float time)
        : base(_Owner)
    {
        Type = StateType.Wait;
        WaitTime = time + Time.time;
    }

    public override void Update()
    {
        if (WaitTime <= Time.time)
        {
            Owner.SwitchState(new WanderAIState(Owner));
        }
    }

    public override void Deactivate()
    {

    }

    public override void Activate()
    {
        //Debug.Log("In Wait");
        if (Agent.enabled)
        {

            Agent.isStopped = true;
        }
    }
}
