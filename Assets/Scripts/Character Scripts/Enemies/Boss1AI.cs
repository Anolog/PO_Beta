using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Boss1AI : EnemyAI
{

    //private float m_MostHealth;
    private bool m_NeedNewTarget;
    public float TeleportDistance = 10;
    private Dictionary<PlayerStats, int> m_PlayerDamages = new Dictionary<PlayerStats, int>();
    private Ability.AbilityType m_CurrentTargetAbilityType;
    public Ability.AbilityType GetCurrentTargetAbilityType() { return m_CurrentTargetAbilityType; }

    private float m_EnergyMineCoolDownTime;
    private float m_KnockbackCoolDownTime;

    private float m_EnergyMineTimer;
    private float m_KnockbackTimer;

    private GameObject[] TeleportLocations;

    private float m_TimeForRecentDamage = 5;
    private float m_RecentDamageTimer;
    private float m_RecentDamage = 0;
    private float m_AmountOfDamageAllowed = 60;


    public GameObject spawnLocation;

    public bool needToMovePos = true;

    public GameObject CutsceneMesh;
    public GameObject InGameMesh;

    //private float StateChangeLockOutTimer = 0;
    //private float StateChangeLockOutTime = 1f;
    protected void Start()
    {
        // setup out stats and navigation
        Agent = GetComponent<NavMeshAgent>();
        Stats = GetComponent<CharacterStats>();
        Animator = GetComponentInChildren<Animator>();
        Agent.speed = Stats.MovementSpeed;
        Agent.acceleration = Stats.Acceleration;
        Agent.angularSpeed = Stats.AngularSpeed;
        timeBetweenAttacks = Stats.AttSpd;
        Body = GetComponent<Rigidbody>();
        Stats.MaxHealth = (int)Stats.Health;
        AttackDistance = 1f;

        // setup our abilities
        Stats.Abilities[0] = new BossMeleeAbility(Stats);

        Stats.Abilities[1] = new EnergyMineAbility(Stats);
        m_EnergyMineCoolDownTime = Stats.Abilities[1].GetCoolDown() * 2;
        m_EnergyMineTimer = m_EnergyMineCoolDownTime + Time.time;

        Stats.Abilities[3] = new TeleportAbility(Stats);

        Stats.Abilities[4] = new BasicKnockbackAbility(Stats);
        m_KnockbackCoolDownTime = Stats.Abilities[4].GetCoolDown();
        m_KnockbackTimer = m_KnockbackCoolDownTime + Time.time;

        // Find our target
        Players = GameManager.playerManager.PlayerList();
        if (Agent.enabled)
        {
            FindInitialTarget();
            Agent.destination = Target.transform.position;
            SwitchState(new BossChaseAIState(this));
        }
        TeleportLocations = GameObject.FindGameObjectsWithTag("BossTeleportLocation");


        spawnLocation = GameObject.FindGameObjectWithTag("BossSpawn");
    }

    protected override void Awake()
    {

    }

    public void SwitchToInGame()
    {
        transform.position = spawnLocation.transform.position;
        transform.rotation = spawnLocation.transform.rotation;

        CutsceneMesh.SetActive(false);
        InGameMesh.SetActive(true);

        Animator = GetComponentInChildren<Animator>();
        Animator.SetLayerWeight(Animator.GetLayerIndex("InGameLayer"), 1);

        GetComponent<Collider>().enabled = true;

        Body.useGravity = true;
        Agent.enabled = true;
        FindInitialTarget();
        Agent.destination = Target.transform.position;
        SwitchState(new BossChaseAIState(this));
    }

    protected override void FixedUpdate()
    {
        if (CutsceneMesh.activeInHierarchy)
        {
            return;
        }
        if (Target != null)
        {
            if (Target.gameObject.tag != "Player")
            {
                FindTarget();
                return;
            }
        }

        if (Stats.isControllable)
        {
            if (m_State != null)
            {
                CheckStateChange();
                m_State.Update();
            }
        }

        if (m_RecentDamageTimer >= Time.time)
        {
            m_RecentDamage = 0;
        }

    }

    public void AddDamage(PlayerStats attacker, int damage, Ability.AbilityType type)
    {
        //Debug.Log(Stats.Health);

        // reset the damage counter timer everytime I take damage
        m_RecentDamageTimer = m_TimeForRecentDamage + Time.time;
        m_RecentDamage += damage;

        // check if my recent damage is above the threshold
        if (m_RecentDamage >= m_AmountOfDamageAllowed)
        {
            // find a random teleport location and switch my state to teleport there
            SwitchState(new BossTeleportAIState(this, TeleportLocations[Random.Range(0, 4)]));
            m_RecentDamage = 0;
        }

        m_PlayerDamages[attacker] += damage;
        m_NeedNewTarget = CheckIfNeedNewTarget();

        if (m_NeedNewTarget)
        {
            m_CurrentTargetAbilityType = type;
        }

        float tenPercentOfMaxHealth = Stats.MaxHealth * 0.1f;
        float maxHealthMinusTenPercent = Stats.MaxHealth - tenPercentOfMaxHealth;
        float currentHealthMinusTenPercent = Stats.Health - tenPercentOfMaxHealth;

        float AbilityTimerCoolDownMultiplier = ((currentHealthMinusTenPercent / maxHealthMinusTenPercent) + 1) / 2f;
        m_EnergyMineCoolDownTime *= AbilityTimerCoolDownMultiplier;
        m_KnockbackCoolDownTime *= AbilityTimerCoolDownMultiplier;

        m_EnergyMineCoolDownTime = Mathf.Clamp(m_EnergyMineCoolDownTime, 1, 10);
        m_KnockbackCoolDownTime = Mathf.Clamp(m_KnockbackCoolDownTime, 1, 10);
    }

    private bool CheckIfNeedNewTarget()
    {
        int mostDamage = 0;
        PlayerStats tempTarget = null;
        foreach (PlayerStats player in m_PlayerDamages.Keys)
        {
            if (mostDamage < m_PlayerDamages[player])
            {
                mostDamage = m_PlayerDamages[player];
                tempTarget = player;
            }
        }

        if (tempTarget != null)
        {
            Target = tempTarget.gameObject;
            return true;
        }
        return false;
    }

    protected override void CheckStateChange()
    {
        if (m_NeedNewTarget)
        {
            SwitchState(new BossSwitchTargetAIState(this));
            m_NeedNewTarget = false;
            return;
        }

        //if (StateChangeLockOutTimer <= Time.time)
        //{
        //StateChangeLockOutTimer = Time.time + StateChangeLockOutTime;
        if (m_EnergyMineTimer <= Time.time)
        {
            SwitchState(new BossEnergyMineAIState(this));
            m_EnergyMineTimer = m_EnergyMineCoolDownTime + Time.time;
        }

        if (m_KnockbackTimer <= Time.time)
        {
            foreach (PlayerStats player in m_PlayerDamages.Keys)
            {
                if (Vector3.Distance(player.gameObject.transform.position, gameObject.transform.position) <= AttackDistance)
                {
                    SwitchState(new BossKnockbackAIState(this));
                    m_KnockbackTimer = m_KnockbackCoolDownTime + Time.time;
                    return;
                }
            }
        }
        //}
    }

    protected virtual void FindInitialTarget()
    {
        GameObject tempTarget = null;
        float mostHealth = 0;
        int i = 0;
        foreach (GameObject player in Players)
        {
            PlayerStats stats = player.GetComponent<PlayerStats>();
            m_PlayerDamages.Add(stats, 0);
            if (player.GetComponent<CharacterStats>().Health > mostHealth)
            {
                mostHealth = player.GetComponent<CharacterStats>().Health;
                tempTarget = player;
            }
            i++;
        }

        //m_MostHealth = mostHealth;
        FindNewTarget(tempTarget);
    }

    private void OnBecameInvisible()
    {
        Debug.Log("Hello");
    }
}
