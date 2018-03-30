using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SupportEnemyAI : EnemyAI
{
    public GameObject HealTarget;
    public float HealSpeed = 5;

	protected override void Awake ()
    {
        base.Awake();

        Stats.Abilities[0] = new TargetHealAbility(Stats, HealSpeed);

        WanderLocations = GameObject.FindGameObjectsWithTag("WanderPoint");
        SwitchState(new WanderAIState(this));

        Animator = gameObject.transform.Find("test 2").GetComponent<Animator>();
	}

    protected override void FixedUpdate()
    {
        if (Stats.isControllable)
        {
            CheckStateChange();
            m_State.Update();
        }
    }

    protected override void CheckStateChange()
    {
        // if I can see an enemy, start to heal them
        float checkRadius = 10f;
        // find all colliders in the checkradius
        Collider[] colliders = Physics.OverlapSphere(gameObject.transform.position, checkRadius);
        foreach (Collider collider in colliders)
        {
            // make sure the collider belongs to an enemy or boss character
            if ((collider.gameObject.tag == "Enemy" || collider.gameObject.tag == "Boss") && collider.gameObject.name != "SupportEnemy(Clone)") // fix this better later perhaps
            {
                // make sure I have line of sight
                RaycastHit hit;
                Vector3 dir = collider.gameObject.transform.position - gameObject.transform.position;
                Ray ray = new Ray(gameObject.transform.position, dir.normalized);
                if (Physics.Raycast(ray, out hit, checkRadius))
                {
                    if (hit.transform == collider.gameObject.transform)
                    {
                        // double check the enemy is wounded
                        if ((int)(collider.gameObject.GetComponent<CharacterStats>().Health + 0.5f) < collider.gameObject.GetComponent<CharacterStats>().MaxHealth)
                        {
                            HealTarget = collider.gameObject;
                            SwitchState(new TargetHealAIState(this, HealTarget));
                            break;
                        }
                    }
                }
            }
        }

        //if (HealTarget != null)
        //{
        //    if (Vector3.Distance(gameObject.transform.position, HealTarget.transform.position) > checkRadius)
        //    {
        //        SwitchState(new WaitAIState(this, 0.25f));
        //    }
        //}

    }
}
