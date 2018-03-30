using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : CharacterStats
{
    public static float PUSH_CONSTANT = 50000;

    public GameObject Camera;

    public ReviveAbility Revive;

    public List<CharacterStats> Followers = new List<CharacterStats>();

    private GameObject m_DownedHitbox;
    private Rigidbody m_RigidBody;

    private Animator m_Animator;

    public void SetDownedHitbox(GameObject hitbox) { m_DownedHitbox = hitbox; }
    public GameObject GetDownedHitbox() { return m_DownedHitbox; }

    private int m_NumDowns = 0;
    private int m_NumRevives = 0;

    private bool m_WinOrLoseGame;
    public bool WinOrLoseGame { get { return m_WinOrLoseGame; } set { m_WinOrLoseGame = value; } }
    public void AddToDowns(int numDownsToAdd) { m_NumDowns += numDownsToAdd; }
    public int GetDowns() { return m_NumDowns; }
    public void AddToRevives(int numRevivesToAdd) { m_NumRevives += numRevivesToAdd; }

    public bool Landing = false;

    [ReflectionAttribute]
    public int GetRevives() { return m_NumRevives; }

    [ColorUsageAttribute(false, true, 0.0f, 4.0f, 0.0f, 4.0f)]
    public Vector4 PlayerColour;

    private Controllers m_Controller;
    public Controllers Controller {
        get
        {
            return m_Controller;
        }
        set
        {
            m_Controller = value;
        }
    }

    float GroundFriction = 0.6f;

    // only check collision every 0.15s so we have enough time to get off the ground before we check if we arte touching it agian
    public float CollisionUpdateTime = 0.15f;
    public float m_CollisionUpdateTimer;

    float m_HiFiRumble = 0;
    float m_LowFiRumble = 0;

    // accolade hooks
    private bool m_KilledBoss = false;
    public void SetKilledBoss(bool killedBoss) { m_KilledBoss = killedBoss; }
    [ReflectionAttribute]
    public bool GetKilledBoss() { return m_KilledBoss; }
    private int m_AbilitiesUsed = 0;
    public void AddToAbilitiesUsed() { ++m_AbilitiesUsed; }
    
    [ReflectionAttribute]
    public int GetAbilitiesUsed() { return m_AbilitiesUsed; }
    private int m_BasicAttacksUsed = 0;
    public void AddToBasicAttacksUsed() { ++m_BasicAttacksUsed; }

    [ReflectionAttribute]
    public int GetBasicAttacksUsed() { return m_BasicAttacksUsed; }
    private int m_Jumps = 0;
    public void AddToJumps() { ++m_Jumps; }

    [ReflectionAttribute]
    public int GetJumps() { return m_Jumps; }
    private float m_DistanceTravelled = 0;
    [ReflectionAttribute]
    public float GetDistanceTravelled() { return m_DistanceTravelled; }
    private Vector3 m_OldPosition = Vector3.zero;
    private Vector3 m_CurrentPosition = Vector3.zero;
    private int m_KnockbacksUsed;
    public void AddToKnockbacksUsed() { ++m_KnockbacksUsed; }
    [ReflectionAttribute]
    public int GetKnockbacksUsed() { return m_KnockbacksUsed; }

    protected override void Start()
    {
        base.Start();

        //if (Abilities[1] == null)
        //{
        //    Abilities[0] = new BasicMeleeAbility(this);
        //    //Abilities[0] = new BasicRangedAbility(this);
        //    Abilities[4] = new BasicKnockbackAbility(this);
        //    Abilities[1] = new ChargeAbility(this);
        //    Abilities[2] = new GroundShockAbility(this);
        //    Abilities[3] = new EnergyBombAbility(this);
        //    Controller = Controllers.KEYBOARD_MOUSE;
        //}
        Revive = new ReviveAbility(this);

        m_CollisionUpdateTimer = CollisionUpdateTime + Time.time;

        m_Animator = gameObject.GetComponentInChildren<Animator>();
        m_RigidBody = gameObject.GetComponent<Rigidbody>();

        m_OldPosition = transform.position;
        m_OldPosition.y = 0;

        //gameObject.GetComponent<UnityEngine.AI.NavMeshAgent>().isStopped = true;
    }

    protected override void Update()
    {
        base.Update();

        if (Revive.GetTarget() != null)
            Revive.Update();

        Vector3 localVel = m_RigidBody.velocity;
        localVel = transform.InverseTransformDirection(localVel);

        
        //Debug.Log(localVel);

        m_Animator.SetFloat("ForwardSpeed", localVel.z);// / MovementSpeed);
        m_Animator.SetFloat("RightSpeed", localVel.x);// / MovementSpeed);

        m_CurrentPosition = transform.position;
        m_CurrentPosition.y = 0;

        float distanceTravelledThisFrame = Vector3.Distance(m_OldPosition, m_CurrentPosition);

        m_DistanceTravelled += distanceTravelledThisFrame;

        m_OldPosition = m_CurrentPosition;
    }

    public override void Die(CharacterStats attacker)
    {
        Command.DownPlayer(gameObject);
    }

    //Deprecated
    public virtual void Heal(int amountToHeal)
    {
        if (!isDowned)
        {
            Health += amountToHeal;
            if (Health > MaxHealth)
            {
                Health = MaxHealth;
            }
        }
    }

    public virtual void AddToFollowers(CharacterStats follower)
    {
        if (Followers.Contains(follower))
        {
            //Debug.Log("Follower already in list");
            return;
        }

        Followers.Add(follower);
    }

    public virtual void RemoveFromFollowers(CharacterStats followerToRemove)
    {
        Followers.Remove(followerToRemove);

    }

    public virtual void RemoveAllFollowers()
    {
        //foreach (CharacterStats follower in Followers)
        //{
        //    //Object.Destroy(follower.gameObject);
        //    follower.gameObject.GetComponent<EnemyAI>().FindTarget();
        //}
        Followers.Clear();
    }

    public void Rumble(float lowFi, float hiFi, float duration)
    {
        if (Controller == Controllers.KEYBOARD_MOUSE)
            return;

        IEnumerator rumble = RumbleCoroutine(lowFi, hiFi, duration);
        StartCoroutine(rumble);
    }

    IEnumerator RumbleCoroutine(float lowFi, float hiFi, float duration)
    {
        float endTime = Time.unscaledTime + duration;

        while(Time.unscaledTime < endTime)
        {
            m_LowFiRumble = Mathf.Max(lowFi, m_LowFiRumble);
            m_HiFiRumble = Mathf.Max(hiFi, m_HiFiRumble);

            GameManager.controllerInput.Rumble(Controller, m_LowFiRumble, m_HiFiRumble);

            yield return null;
        }

        m_HiFiRumble = 0;
        m_LowFiRumble = 0;

        GameManager.controllerInput.Rumble(Controller, 0, 0);

        yield return null;
    }

    private void OnCollisionStay(Collision collision)
    {
        if (usingCharge)
        {
            return;
        }
        if (m_CollisionUpdateTimer <= Time.time)
        {
            foreach (ContactPoint contact in collision.contacts)
            {
                float dot = Vector3.Dot(contact.normal, Vector3.up);
                if (dot <= 1 && dot >= VERTICAL_THRESHHOLD)
                {
                    if (collision.gameObject.tag == "Decoration" ||
                    collision.gameObject.tag == "Platform" ||
                    collision.gameObject.tag == "Wall")
                    {
                        if (collision.gameObject.tag != "Decoration")
                        {
                            PhysicMaterial material = gameObject.GetComponent<CapsuleCollider>().material;
                            material.dynamicFriction = GroundFriction;
                            material.staticFriction = GroundFriction;
                            material.frictionCombine = PhysicMaterialCombine.Average;
                        }
                        m_Grounded = true;
                    }

                    if (Landing)
                    {
                        Landing = false;
                        GameManager.audioManager.PlaySound(AudioManager.Sounds.PLAYER_LAND);
                        //gameObject.GetComponent<UnityEngine.AI.NavMeshAgent>().enabled = true;
                    }

                    if (collision.gameObject.tag == "Enemy" || 
                        collision.gameObject.tag == "Player" || 
                        collision.gameObject.tag == "Boss" || 
                        collision.gameObject.tag == "Decoration")
                    {
                        Vector3 otherPosition = contact.point;
                        Vector3 direction = transform.position - otherPosition;

                        direction.Normalize();

                        Vector3 force = direction;
                        force.y = 0;

                        force *= PUSH_CONSTANT;
                        force *= dot * dot;

                        gameObject.GetComponent<Rigidbody>().AddForce(force, ForceMode.Force);
                        gameObject.GetComponent<UnityEngine.AI.NavMeshAgent>().enabled = false;

                    }
                    else
                    {
                        UnityEngine.AI.NavMeshAgent agent = gameObject.GetComponent<UnityEngine.AI.NavMeshAgent>();
                        agent.enabled = true;
                        agent.updatePosition = false;
                        if (agent.isOnNavMesh == false)
                        {
                            agent.enabled = false;
                        }
                    }

                    m_Animator.SetBool("Jump", false);
                }
                
            }
        }
    }
}
