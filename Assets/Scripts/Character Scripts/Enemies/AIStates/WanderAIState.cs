using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WanderAIState : AIState
{
    private GameObject WanderLocation = null;
    //private GameObject PreviousWanderLocation = null;
    float closeEnough = 0.5f;

    private float m_FindBetterWanderLocationEvery = 0.5f;
    private float m_UpdateLocationTimer = 0;

    public WanderAIState(EnemyAI _Owner)
        : base(_Owner)
    {
        Type = StateType.Wander;
    }

    public override void Update()
    {
        if (Agent.enabled == false)
        {
            return;
        }

        // see if I am going to the best position
        if (m_UpdateLocationTimer <= Time.time)
        {
            m_UpdateLocationTimer = Time.time + m_FindBetterWanderLocationEvery;
            GameObject tempNewLoc = FindWanderLocation();
            // if there is a better location go there
            if (tempNewLoc != null)
            {
                WanderLocation = tempNewLoc;
                //PreviousWanderLocation = WanderLocation;
            }
        }

        Agent.destination = WanderLocation.transform.position;
        if (Agent.velocity.normalized != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(Agent.velocity.normalized); // look into why updateRotation isnt working

        if (Vector3.Distance(WanderLocation.transform.position, Owner.gameObject.transform.position) <= closeEnough)
        {
            Owner.SwitchState(new WaitAIState(Owner, 0.25f));
        }

    }

    public override void Deactivate()
    {

    }

    public override void Activate()
    {
        if (Agent.enabled)
        {
            // to make this enemy smarter they will be frequently looking for a better location
            m_UpdateLocationTimer = Time.time + m_FindBetterWanderLocationEvery;

            //Debug.Log("In Wander");
            Agent.isStopped = false;
            WanderLocation = FindWanderLocation();

            // if I couldn't find a wander location with a nearby wounded enemy just find a random one
            if (WanderLocation == null)
            {
                WanderLocation = Owner.WanderLocations[Random.Range(0, Owner.WanderLocations.Length - 1)];
            }
        }
        //PreviousWanderLocation = WanderLocation;
        //Debug.Log(WanderLocation.name);
    }

    public GameObject FindWanderLocation()
    {
        int checkRadius = 10;

        int maxEnemies = 0;

        GameObject tempWanderLocaiton = null;
        // check all of my wander locations
        foreach (GameObject wanderLocation in Owner.WanderLocations)
        {
            int counter = 0;
            // do a sphere check in each wander location
            Collider[] colliders = Physics.OverlapSphere(wanderLocation.transform.position, checkRadius);
            foreach (Collider collider in colliders)
            {
                // check if I am colliding with an enemy
                if ((collider.gameObject.tag == "Enemy" || collider.gameObject.tag == "Boss") && collider.gameObject.name != "SupportEnemy(Clone)")
                {
                    // if the height difference between us is more than a meter then don't count that enemy as they are probably on a different level
                    if (Mathf.Abs(collider.gameObject.transform.position.y - Owner.gameObject.transform.position.y) < 1)
                    {
                        // if they are wounded increment my counter
                        CharacterStats stats = collider.gameObject.GetComponent<CharacterStats>();
                        if (stats.Health < stats.MaxHealth)
                        {
                            counter++;
                        }
                    }
                }
            }
            // if this spot has more wounded enemies than any other I have checked, make it my wander location and update my max wounded enemies
            if (counter > maxEnemies)
            {
                maxEnemies = counter;
                tempWanderLocaiton = wanderLocation;
            }
        }

        return tempWanderLocaiton;
    }
}
