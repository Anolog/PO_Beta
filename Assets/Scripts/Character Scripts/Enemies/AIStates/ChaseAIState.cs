using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaseAIState : AIState
{
    List<Vector3> directions = new List<Vector3>();

    float closeEnough = 1.1f;

    Vector3 randDir;

    public ChaseAIState(EnemyAI _Owner)
        : base(_Owner)
    {
        Type = StateType.Chase;

        // 8 different directions going out in a circle
        directions.Add(new Vector3(0, 0, 1));
        directions.Add(new Vector3(0.5f, 0, 0.5f));
        directions.Add(new Vector3(1, 0, 0));
        directions.Add(new Vector3(0.5f, 0, -0.5f));
        directions.Add(new Vector3(0, 0, -1));
        directions.Add(new Vector3(-0.5f, 0, -0.5f));
        directions.Add(new Vector3(-1, 0, 1));
        directions.Add(new Vector3(-0.5f, 0, 0.5f));

        List<Vector3> Directions = new List<Vector3>();

        // randomize the order of these direcitons for when I use them later
        while (directions.Count > 0)
        {
            int rand = Random.Range(0, directions.Count - 1);
            Directions.Add(directions[rand]);
            directions.RemoveAt(rand);
        }

        directions = new List<Vector3>(Directions);

        //PICK RANDOM DIRECTION AROUND PLAYER
        randDir = new Vector3(Random.Range(-100, 100), 0, Random.Range(-100, 100));
        randDir.Normalize();
    }

    public override void Update()
    {
        UnityEngine.AI.NavMeshHit hit;

        CapsuleCollider col = Target.GetComponent<CapsuleCollider>();
        float height = col.height;
        if (UnityEngine.AI.NavMesh.SamplePosition(Target.transform.position + height * 0.5f * Vector3.up, out hit, 2 * height, 0))
        {
            Agent.SetDestination(hit.position);
        }

        if (Agent.velocity.normalized != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(Agent.velocity.normalized); // look into why updateRotation isnt working
            if (Owner is MeleeEnemyAI)
            {
                Owner.Animator.SetBool("Walk", true);
            }
        }
        else
        {
            if (Owner is MeleeEnemyAI)
            {
                Owner.Animator.SetBool("Walk", false);
            }
        }

        //GO THERE AROUND PLAYER INSTEAD OF PLAYER POS
        // only do this if I am the fodder enemy
        if (Owner is MeleeEnemyAI)
        {
            //MULTIPLY RAND DIR BY 0.75 ATTACK DISTANCE
            Agent.SetDestination(Target.transform.position + randDir * Owner.AttackDistance * 0.75f);

            if (Vector3.Distance(Target.transform.position, Owner.transform.position - randDir * Agent.stoppingDistance) <= closeEnough)
            {
                //Debug.Log("In Here");
                Owner.Animator.SetBool("Walk", false);
                transform.rotation = Quaternion.LookRotation(Target.gameObject.transform.position - Owner.gameObject.transform.position);
            }

            if (Vector3.Distance(Owner.gameObject.transform.position, Target.gameObject.transform.position) <= closeEnough * Agent.stoppingDistance)
            {
                Owner.Animator.SetBool("Walk", false);
                transform.rotation = Quaternion.LookRotation(Target.gameObject.transform.position - Owner.gameObject.transform.position);
                return;
            }
        }

        // check if I have been injured
        Owner.CheckHealth();
    }

    protected bool CheckIfPositionOpen(Ray ray)
    {
        RaycastHit rayHit;

        if (Physics.Raycast(ray, out rayHit, Owner.AttackDistance))
        {
            if (rayHit.collider.gameObject.tag == "Enemy" || 
                rayHit.collider.gameObject.tag == "Decoration" ||
                rayHit.collider.gameObject.tag == "Platform" ||
                rayHit.collider.gameObject.tag == "Wall"
                )
            {
                return false;
            }
        }

        return true;
    }

    public override void Deactivate()
    {

    }

    public override void Activate()
    {
        if (Owner is MeleeEnemyAI)
        {
            Owner.Animator.SetBool("Walk", true);
            //Debug.Log("Activate being hit");
        }
        Agent.isStopped = false;
    }
}
