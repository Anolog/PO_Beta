using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetHealAIState : AIState {

    private GameObject m_HealTarget;

    public TargetHealAIState(EnemyAI _Owner, GameObject healTarget)
        : base(_Owner)
    {
        Type = StateType.Heal;
        m_HealTarget = healTarget;
    }

    public override void Update()
    {
        if (m_HealTarget == null)
        {
            Owner.SwitchState(new WaitAIState(Owner, 0.25f));
            Owner.gameObject.GetComponent<SupportEnemyAI>().HealTarget = null;
            return;
        }

        Vector3 direction = m_HealTarget.transform.position - Owner.transform.position;
        direction.Normalize();
        direction.y = 0;
        Owner.transform.rotation = Quaternion.LookRotation(direction);

        if (m_HealTarget.GetComponent<CharacterStats>().Health >= m_HealTarget.GetComponent<CharacterStats>().MaxHealth)
        {
            Owner.SwitchState(new WaitAIState(Owner, 0.25f));
            Owner.gameObject.GetComponent<SupportEnemyAI>().HealTarget = null;
            m_HealTarget = null;
            return;
        }

        float distance = Vector3.Distance(m_HealTarget.transform.position, Owner.transform.position);
        if (distance >= 10)
        {
            Owner.SwitchState(new WaitAIState(Owner, 0.25f));
            Owner.gameObject.GetComponent<SupportEnemyAI>().HealTarget = null;
            m_HealTarget = null;
            return;
        }

        // see where there are the most enemies around me
        float checkRadius = 7.5f; // this is 1.5* the radius of my banner carrier hitbox

        float avgX = 0;
        float avgY = 0;
        float avgZ = 0;
        int numEnemies = 0;

        Collider[] colliders = Physics.OverlapSphere(Owner.gameObject.transform.position, checkRadius);
        foreach (Collider collider in colliders)
        {
            // check if I am colliding with an enemy
            if ((collider.gameObject.tag == "Enemy" || collider.gameObject.tag == "Boss") && collider.gameObject.name != "SupportEnemy(Clone)")
            {
                ++numEnemies;
                avgX += collider.gameObject.transform.position.x;
                avgY += collider.gameObject.transform.position.y;
                avgZ += collider.gameObject.transform.position.z;
            }
        }

        if (numEnemies != 0)
        {
            avgX /= numEnemies;
            avgY /= numEnemies;
            avgZ /= numEnemies;

            if (Agent.enabled == true)
            {
                Agent.SetDestination(new Vector3(avgX, avgY, avgZ));
            }
        }
    }

    public override void Deactivate()
    {
        Owner.Animator.SetBool("TargetHeal", false);

    }

    public override void Activate()
    {
        if (Agent.enabled)
        {
            //Debug.Log("In Target Heal");
            Agent.isStopped = false;

            Owner.Stats.Abilities[0].Use(m_HealTarget);

            Owner.Animator.SetBool("TargetHeal", true);
        }
    }
}
