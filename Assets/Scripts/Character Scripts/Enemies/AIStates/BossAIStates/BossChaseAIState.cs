using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossChaseAIState : AIState {

    private float m_UpdateEvery = 0.1f;
    private float m_UpdateTimer = 0f;

    public new Boss1AI Owner;

    public BossChaseAIState(Boss1AI _Owner)
        : base(_Owner)
    {
        Owner = _Owner;
        Type = StateType.Chase;
    }

    public override void Update()
    {
        if (Owner.GetCurrentTargetAbilityType() == Ability.AbilityType.Ranged &&
            Vector3.Distance(Target.gameObject.transform.position, Owner.gameObject.transform.position) >= Owner.TeleportDistance &&
            Owner.Stats.Abilities[3].GetCoolDownTime() <= 0)
        {
            Owner.SwitchState(new BossTeleportAIState(Owner, Target));
            return;
        }
        if (Vector3.Distance(Target.gameObject.transform.position, Owner.gameObject.transform.position) <= Owner.AttackDistance)
        {
            Owner.SwitchState(new BossAttackAIState(Owner));
            return;
        }
        if (m_UpdateTimer <= Time.time)
        {
            m_UpdateTimer = Time.time + m_UpdateEvery;
            Agent.destination = Target.transform.position;
            //if (Agent.velocity.normalized != Vector3.zero)
            //    transform.rotation = Quaternion.LookRotation(Agent.velocity.normalized); // look into why updateRotation isnt working
        }
    }

    public override void Deactivate()
    {
        Agent.velocity = Vector3.zero;
    }

    public override void Activate()
    {
        Agent.enabled = true;
        Agent.isStopped = false;
        Agent.updateRotation = true;
        //Debug.Log("In Chase State");
    }
}
