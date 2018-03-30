using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI; // Use this for the NavMeshAgent

public class EnemyAI : MonoBehaviour
{

    // variables for attacking
    public float timeBetweenAttacks,
                 timeBeforeFirstAttack = 0.5f,
                 attackTimer = 0f;

    // re target timers
    private float m_ReTargetTimer = 0f;
    private float m_RetargetTime = 0.5f;

    public GameObject Target;
    public List<GameObject> Players;
    public NavMeshAgent Agent;
    public CharacterStats Stats;
    public Rigidbody Body;
    public Animator Animator;
    public GameObject[] WanderLocations;

    public Ability.AbilityType LatestHitType;
    public GameObject LatestAttacker;

    public float Health;
    public float AttackDistance;

    protected AIState m_State;

    public AIState State { get { return m_State; } }

    public bool isDead = false;
    // Use this for initialization
    virtual protected void Awake()
    {
        // setup out stats and navigation
        Agent = GetComponent<NavMeshAgent>();
        Stats = GetComponent<CharacterStats>();
        Animator = GetComponentInChildren<Animator>();

        Agent.speed = Stats.MovementSpeed;
        Agent.acceleration = Stats.Acceleration;
        Agent.angularSpeed = Stats.AngularSpeed;

        Health = Stats.Health;
        timeBetweenAttacks = Stats.AttSpd;
        Body = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    protected virtual void FixedUpdate()
    {
        if (isDead)
        {
            Body.velocity = Vector3.zero;
            return;
        }
        if (!Stats.isControllable)
        {
            return;
        }
        if (Target == null)
        {
            FindTarget();
        }
        else if (m_ReTargetTimer <= Time.time)
        {
            m_ReTargetTimer = m_RetargetTime + Time.time;
            FindTarget();
        }
        else
        {
            CheckStateChange();
            m_State.Update();
        }

        if (gameObject.transform.position.y < -5)
        {
            Command.KillEnemy(gameObject);
            //Debug.Log*
        }
    }

    public bool CheckHealth()
    {
        if (Stats.Health < Health)
        {
            Health = Stats.Health;
            if (LatestHitType == Ability.AbilityType.Melee)
            {
                FindNewTarget(LatestAttacker);
            }
            else
            {
                float distanceToCurrent = Vector3.Distance(transform.position, Target.transform.position);
                float distanceToAttacker = Vector3.Distance(transform.position, LatestAttacker.transform.position);
                if (distanceToCurrent > distanceToAttacker)
                {
                    FindNewTarget(LatestAttacker);
                }
            }
            return (true);
        }
        return (false);
    }

    protected virtual void CheckStateChange()
    {
    }

    public void SwitchState(AIState newState)
    {
        if (m_State != null)
        {
            if (m_State.Type == newState.Type)
                return;

            m_State.Deactivate();
        }

        m_State = newState;
        m_State.Activate();
    }

    // function to look at your target but not change your pitch
    public void LookAtTarget()
    {
        Vector3 direction = Target.transform.position - transform.position;
        direction.Normalize();
        direction.y = 0;

        transform.rotation = Quaternion.LookRotation(direction);
    }

    public void FindTarget()
    {
        Vector3 myPos = transform.position;

        float distanceToPlayer = float.MaxValue;

        if (Target != null)
        {
            distanceToPlayer = Vector3.Distance(myPos, Target.transform.position);
        }

        GameObject tempTarget = null;
        // loop through each player in the game
        foreach (GameObject player in Players)
        {
            if (player != null)
            {
                if (player.tag == "Player") // downed players will not have this tag
                {
                    // if they are closer to me than the previous player set the target to that player
                    if (distanceToPlayer > Vector3.Distance(myPos, player.transform.position))
                    {
                        tempTarget = player;
                        distanceToPlayer = Vector3.Distance(myPos, player.transform.position);
                    }
                }
            }
        }

        if (tempTarget != Target)
        {
            FindNewTarget(tempTarget);
        }
    }

    public void FindNewTarget(GameObject target)
    {
        // temp fix to stop getting errors when all players die
        if (target == null)
        {
            Target = null;
            return;
        }

        if (Target != null)
        {
            Target.GetComponent<PlayerStats>().RemoveFromFollowers(Stats);
        }
        Target = target;
        Target.GetComponent<PlayerStats>().AddToFollowers(Stats);
        if (Agent.enabled)
        {
            Agent.destination = Target.transform.position;
            Agent.isStopped = false;
        }
        if (m_State != null)
            m_State.Target = Target;

        //Debug.Log(Agent.destination);
    }
}
