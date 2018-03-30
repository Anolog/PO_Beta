using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossTeleportAIState : AIState {

    private GameObject TeleportLocation;

    public BossTeleportAIState(Boss1AI _Owner, GameObject teleportLocation)
        : base(_Owner)
    {
        Type = StateType.Teleport;

        TeleportLocation = teleportLocation;
    }

    public override void Update()
    {
        Owner.LookAtTarget();

        Owner.Stats.Abilities[3].Use(TeleportLocation); 

        if (Vector3.Distance(Target.gameObject.transform.position, Owner.gameObject.transform.position) > Owner.AttackDistance)
        {
            Owner.SwitchState(new BossChaseAIState((Boss1AI)Owner));
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
        //Debug.Log("In Teleport State");
    }
}
