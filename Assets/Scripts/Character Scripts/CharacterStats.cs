using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class CharacterStats : MonoBehaviour
{

    public static float DEFAULT_MOVEMENT_SPEED = 3f;
    public static float JUMP_IMPULSE = 8000f;
    // Added for new movement formula
    public static float ACCELERATION_SPEED = 1f;

    public static float VERTICAL_THRESHHOLD = 0.8f;

    [HideInInspector]
    public int MaxShield = 100,
               MaxHealth = 100,
               DamageDealt = 0,
               DamageTaken = 0,
               HealingDone = 0,
               HealingRecieved = 0,
               ScorePointsEarned = 0;

    public int Shield = 100,
               Damage = 50,
               ScorePointValue = 0;


    public float Acceleration = 1f,
                 AngularSpeed = 90f,
                 AngularAcceleration = 1f,
                 MovementSpeed = 1f,
                 Health = 100,
                 AttSpd = 1;

    [HideInInspector]
    public float CurrentMovementSpeed = 0f,
                 DamageReduction = 0f,
                 LifeSteal = 0f;

    public bool isControllable = true;
    public bool isCastLocked = false;
    public bool isDowned = false;
    private bool isInvincible = false;

    public void SetInvincible(bool invincible) { isInvincible = invincible; }

    float m_LockoutTimer = 0.0f;
    float m_CastLockTimer = 0.0f;

    //why do we need this?
    public GameObject Game;

    public Ability[] Abilities = new Ability[5];

    // for shield regain
    private float m_TimeBeforeShieldRegen = 10f;
    private float m_ShieldRegenTimer;
    private float m_ShieldRegenSpeed = 20;

    protected bool m_Grounded = true;
    public bool Grounded { get { return m_Grounded; } set { m_Grounded = value; } }
    public bool usingCharge = false;
    public bool usingWeaponSmash = false;
    public bool usingGroundShock = false;
    public bool usingChainLightning = false;
    public bool usingHeal = false;
    public bool usingProtectMe = false;

    private int m_HealthDamageTaken = 0;
    public int GetHealthDamageTaken() { return m_HealthDamageTaken; }
    private int m_ShieldDamageTaken = 0;
    public int GetShieldDamageTaken() { return m_ShieldDamageTaken; }
    private int m_KnockbacksDone = 0;
    public int GetKnockBacksDone() { return m_KnockbacksDone; }

    float pointsFlashTime = 0.25f;
    float pointsFlashTimer;
    bool updatePointsFlash = false;
    public Color PointsColour;
    public Color PointsFlashColor;

    protected virtual void Start()
    {
        MaxHealth = (int)Health;
        MaxShield = Shield;

        Game = GameObject.Find("Game");
    }

    public void LockoutCharacter(float time)
    {
        if (m_LockoutTimer < time + Time.time)
            m_LockoutTimer = time + Time.time;

        isControllable = false;
    }

    public void CastLockCharacter(float time)
    {
        if (m_CastLockTimer < time + Time.time)
            m_CastLockTimer = time + Time.time;

        isCastLocked = true;
    }

    /// <summary>
    /// Swap abilities from the list
    /// </summary>
    /// <param name="index">Which ability in the array to swap with</param>
    /// <param name="abilityID">Which ability in the enum</param>
    public void SwapAbility(int index, int abilityID)
    {
        ConsoleBehvaiour.SubmitToBuffer("print \"Ability: " + Abilities[index].ToString() + "\"");

        switch ((Ability.Abilities)abilityID)
        {
            case Ability.Abilities.BasicMelee:
                Abilities[index] = new BasicMeleeAbility(this);
                break;
            default:
                ConsoleBehvaiour.SubmitToBuffer("print \"Error: No ability with that ID\"");
                break;
        }
    }

    protected virtual void Update()
    {

        if (isControllable == false)
            if (m_LockoutTimer < Time.time)
                isControllable = true;

        if (isCastLocked == true)
            if (m_CastLockTimer < Time.time)
                isCastLocked = false;

        for (int i = 0; i < 5; i++)
        {
            if (Abilities[i] != null)
                Abilities[i].Update();
        }

        if (m_ShieldRegenTimer <= Time.time && Shield < MaxShield && !isDowned)
        {
            Shield += 1 + (int)(Time.deltaTime * m_ShieldRegenSpeed);
            if (Shield > MaxShield)
                Shield = MaxShield;
        }

        if (pointsFlashTimer <= Time.time && updatePointsFlash)
        {
            this.gameObject.transform.Find("Player_UI_Prefab").GetComponent<PlayerUI>().ScoreText.GetComponent<UnityEngine.UI.Text>().color = PointsColour;

            updatePointsFlash = false;
        }
    }

    public virtual void TakeHealing(CharacterStats healer, int healingAmount)
    {
        //Return if max health
        if (Health >= MaxHealth)
        {
            return;
        }

        GameManager.audioManager.PlaySound(AudioManager.Sounds.RECIEVE_HEALING);

        //Keep track of actual amount healed
        int amountHealed = 0;

        int amountAbleToHeal = MaxHealth - (int)Health;

        if (amountAbleToHeal >= healingAmount)
        {
            amountHealed = healingAmount;
        }

        else if (Health + healingAmount >= MaxHealth)
        {
            amountHealed = MaxHealth - (int)Health;
        }

        if (healer != null)
        {
            //Give healer score based on amount healed
            healer.ScorePointsEarned += amountHealed;

            //Set stats
            healer.HealingDone += amountHealed;
        }

        HealingRecieved += amountHealed;

        //Set health back up
        Health += amountHealed;

    }

    public EnemyList GetCharacterType(CharacterStats character)
    {
        GameObject characterObject = character.gameObject;
        if (characterObject.GetComponent<MeleeEnemyAI>() != null)
        {
            return EnemyList.FodderEnemy;
        }
        else if (characterObject.GetComponent<RangedEnemyAI>() != null)
        {
            return EnemyList.RockThrowEnemy;
        }
        else if (characterObject.GetComponent<SupportEnemyAI>() != null)
        {
            return EnemyList.SupportEnemy;
        }
        else if (characterObject.GetComponent<Boss1AI>() != null)
        {
            return EnemyList.Boss;
        }
        else
        {
            //Debug.Log("Invalid Character Type");
            return EnemyList.NumEnemies;
        }
    }

    public AudioManager.Sounds GetHitSound(GameObject character)
    {
        switch (GetCharacterType(character.GetComponent<CharacterStats>()))
        {
            case EnemyList.FodderEnemy:
                return GameManager.audioManager.GetSoundFromEffect("Plant Hit", true);
            case EnemyList.RockThrowEnemy:
                return GameManager.audioManager.GetSoundFromEffect("Rock Thrower Hit", true);
            case EnemyList.SupportEnemy:
                return GameManager.audioManager.GetSoundFromEffect("Support Hit", true);
            case EnemyList.Boss:
                return GameManager.audioManager.GetSoundFromEffect("Boss Hit", true);
            case EnemyList.NumEnemies:
                //this happens when enemies use player abilities, such as the boss using basic knockback
                return GameManager.audioManager.GetSoundFromEffect("Player Hit", true);
            default:
                break;
        }

        return AudioManager.Sounds.ERROR_SOUND;
    }

    public virtual void TakeDamage(CharacterStats attacker, Ability.AbilityType type, int abilityDamage)
    {
        if (isInvincible)
        {
            return;
        }
        int damageInflicted = 0;
        abilityDamage = (int)(abilityDamage * (1 - DamageReduction) + 0.5f); // add 0.5 for rounding when ints truncate
        //Debug.Log("Damage Taken: " + abilityDamage);

        if (gameObject.tag == "Enemy" || gameObject.tag == "Boss")
        {
            gameObject.GetComponent<EnemyAI>().LatestHitType = type;
            gameObject.GetComponent<EnemyAI>().LatestAttacker = attacker.gameObject;
            if (gameObject.tag == "Boss")
            {
                gameObject.GetComponent<Boss1AI>().AddDamage((PlayerStats)(attacker), (int)abilityDamage, type);
            }
        }

        m_ShieldRegenTimer = m_TimeBeforeShieldRegen + Time.time;

        //you could do a try cast on "this" but that seems more complicated
        PlayerStats player = gameObject.GetComponent<PlayerStats>();

        if (Shield > abilityDamage)
        {
            Shield -= abilityDamage;
            attacker.InflictedDamage(abilityDamage);
            DamageTaken += abilityDamage;
            m_ShieldDamageTaken += abilityDamage;

            #region Play Shield Hit Sound
            GameManager.audioManager.PlaySoundAtPosition(GameManager.audioManager.GetSoundFromEffect("Shield Hit", true), gameObject.transform, gameObject.transform.position);
            #endregion

            //A/B Build
            if (player != null)
            {
                player.Rumble(0, 0.2f + abilityDamage * 0.02f, 0.3f + abilityDamage * 0.015f);
                player.GetComponentInChildren<PlayerUI>().FlashHUDBars(abilityDamage, false);
            }

            return;
        }
        else if (Shield > 0)
        {
            abilityDamage -= Shield;
            damageInflicted = Shield;
            Shield = 0;
            m_ShieldDamageTaken += Shield;

        }

        if (Health > abilityDamage)
        {
            Health -= abilityDamage;
            m_HealthDamageTaken += abilityDamage;
            damageInflicted += abilityDamage;
            attacker.InflictedDamage(damageInflicted);
            DamageTaken += abilityDamage;

            if (player != null)
            {
                player.Rumble(0.1f + abilityDamage * 0.01f, 0.2f + abilityDamage * 0.02f, 0.3f + abilityDamage * 0.015f);
                player.GetComponentInChildren<PlayerUI>().FlashHUDBars(abilityDamage, true);
            }

            return;
        }
        else
        {
            m_HealthDamageTaken += (int)Health;
            damageInflicted += (int)Health;
            attacker.InflictedDamage(damageInflicted);
            DamageTaken += abilityDamage;
            if (attacker.tag == "Player")
            {
                attacker.GivePoints(ScorePointValue);
            }

            if (player != null)
            {
                player.Rumble(0.4f, 0.4f, 1.5f);
                player.GetComponentInChildren<PlayerUI>().FlashHUDBars((int)Health, true);
            }

            Health = 0;
            Die(attacker);
        }
    }

    public virtual void InflictedDamage(int damage)
    {
        //PlayerStats player = gameObject.GetComponent<PlayerStats>();

        //A/B build
        //if (player != null)
        //player.Rumble(0.1f + damage * 0.01f, 0.3f + damage * 0.03f, 0.2f);

        DamageDealt += damage;
        Health += (int)(damage * LifeSteal);

        if (Health > MaxHealth)
        {
            Health = MaxHealth;
        }

        if (gameObject.CompareTag("Player"))
        {
            GetComponentInChildren<Third_Person_Camera>().AddReticleDamage(damage);
        }
    }

    public virtual void GivePoints(int points)
    {
        ScorePointsEarned += points;

        this.gameObject.transform.Find("Player_UI_Prefab").GetComponent<PlayerUI>().ScoreText.GetComponent<UnityEngine.UI.Text>().color = PointsFlashColor;

        pointsFlashTimer = Time.time + pointsFlashTime;

        updatePointsFlash = true;

    }

    public virtual void Die(CharacterStats attacker)
    {
        // When an enemy dies we need to let the game know so it can keep track of how many points we have left
        if (gameObject.tag == "Enemy")
        {
            if (gameObject.GetComponent<MeleeEnemyAI>() != null)
            {
                // play fodder death sound
            }
            else if (gameObject.GetComponent<RangedEnemyAI>() != null)
            {
                // play ranged death sound
            }
            else
            {
                // play support death sound
            }
            // find which player is the listener
            GameObject tempListener = GameManager.playerManager.PlayerList()[0];
            float dist = float.MaxValue;
            foreach (GameObject player in GameManager.playerManager.PlayerList())
            {
                if (Vector3.Distance(player.transform.position, gameObject.transform.position) < dist)
                {
                    dist = Vector3.Distance(player.transform.position, gameObject.transform.position);
                    tempListener = player;
                }
            }
            // play enemy death sound
            //GameManager.audioManager.PlaySoundAtPosition(AudioManager.Sounds.MONO_TEST, tempListener.transform, gameObject.transform.position);
            Command.KillEnemy(gameObject);
            Game.GetComponent<Game>().SetPointsLeft(Game.GetComponent<Game>().GetPointsLeft() - ScorePointValue);
        }
        else if (gameObject.tag == "Boss")
        {
            // find which player is the listener
            GameObject tempListener = GameManager.playerManager.PlayerList()[0];
            float dist = float.MaxValue;
            foreach (GameObject player in GameManager.playerManager.PlayerList())
            {
                if (Vector3.Distance(player.transform.position, gameObject.transform.position) < dist)
                {
                    dist = Vector3.Distance(player.transform.position, gameObject.transform.position);
                    tempListener = player;
                }
            }
            // play enemy death sound
            //GameManager.audioManager.PlaySoundAtPosition(AudioManager.Sounds.MONO_TEST, tempListener.transform, gameObject.transform.position);
            attacker.gameObject.GetComponent<PlayerStats>().SetKilledBoss(true);

            Command.KillBoss(gameObject);
        }
        else
        {
            //GameManager.audioManager.PlaySoundAtPosition(AudioManager.Sounds.MONO_TEST, gameObject.transform, gameObject.transform.position);
            Command.DownPlayer(gameObject);
        }
    }

    public void KnockbackCharacter(Vector3 force, float duration, CharacterStats attacker)
    {
        if (force != Vector3.zero)
        {
            ++attacker.m_KnockbacksDone;
        }

        IEnumerator knockback = KnockbackCharacterCoroutine(force, duration);
        StartCoroutine(knockback);
    }

    public void AddMass(float mass, float duration)
    {
        IEnumerator addMass = AddMassCoroutine(mass, duration);
        StartCoroutine(addMass);
    }

    public void AddDamageReduction(float amount, float duration)
    {
        IEnumerator reduction = AddDamageReductionCoroutine(amount, duration);
        StartCoroutine(reduction);
    }

    public void AddLifesteal(float amount, float duration)
    {
        if (!EnrageInEffect)
        {
            IEnumerator lifesteal = AddLifeStealCoroutine(amount, duration);
            StartCoroutine(lifesteal);
        }
    }

    public bool EnrageInEffect = false;

    public void IncreaseAttackSpeed(float amount, float duration)
    {
        if (!EnrageInEffect)
        {
            IEnumerator attackSpeed = IncreaseAttackSpeedCoroutine(amount, duration);
            StartCoroutine(attackSpeed);
        }
    }

    public void IncreaseMovementSpeed(float amount, float duration)
    {
            IEnumerator movementSpeed = IncreaseMovementSpeedCoroutine(amount, duration);
            StartCoroutine(movementSpeed);
    }

    public void IncreaseAcceleration(float amount, float duration)
    {
        IEnumerator accel = IncreaseAccelerationCoroutine(amount, duration);
        StartCoroutine(accel);
    }

    public void ReduceCooldowns(float duration)
    {
        this.gameObject.transform.Find("Player_UI_Prefab").GetComponent<PlayerUI>().NoCDsGlow.SetActive(true);

        IEnumerator reducCDs = ReduceCooldownsCoroutine(duration);
        StartCoroutine(reducCDs);
    }

    public void InvulnDamageItemPickup(float duration)
    {
        this.gameObject.transform.Find("Player_UI_Prefab").GetComponent<PlayerUI>().InvincibilityGlow.SetActive(true);

        IEnumerator invulnDam = InvulnerableCoroutine(duration);
        StartCoroutine(invulnDam);
    }

    public void DamageItemPickup(float duration)
    {
        IEnumerator damageBoost = DamageBoostCoroutine(duration);
        StartCoroutine(damageBoost);
        IEnumerator fieryWeapon = FieryWeaponCoroutine(duration);
        StartCoroutine(fieryWeapon);
    }

    IEnumerator KnockbackCharacterCoroutine(Vector3 force, float duration)
    {
        LockoutCharacter(duration);

        UnityEngine.AI.NavMeshAgent agent = gameObject.GetComponent<UnityEngine.AI.NavMeshAgent>();
        EnemyAI enemy = gameObject.GetComponent<EnemyAI>();
        Rigidbody body = gameObject.GetComponent<Rigidbody>();

        Vector3 startingPos = body.transform.position;

        float mass = body.mass;

        if (enemy != null && Health > 0)
        {
            agent.enabled = false;
            body.isKinematic = false;
            if (agent.isOnNavMesh)
            {
                agent.enabled = false;
                body.isKinematic = false;

                gameObject.GetComponent<EnemyAI>().Animator.SetBool("Stunned", true);

            }
            if (enemy is Boss1AI)
            {
                agent.enabled = false;
                body.isKinematic = false;

                gameObject.GetComponent<EnemyAI>().Animator.SetBool("Stunned", true);
            }
        }
        //necessary since enemies could have 0 health and miss the previous block
        else if (gameObject.CompareTag("Player"))// they are probably a player if they don't have an enemyAI - look into this if there are bugs
        {
            body.mass = 100;
            agent.enabled = false;
            Animator animator = gameObject.GetComponentInChildren<Animator>();
            animator.SetBool("Stunned", true);
            animator.SetLayerWeight(animator.GetLayerIndex("UpperBodyLayer"), 0);
            animator.SetLayerWeight(animator.GetLayerIndex("UpperBodyWithPelvisLayer"), 0);
        }

        body.velocity = Vector3.zero;
        body.AddForce(force, ForceMode.Impulse);

        yield return new WaitForSeconds(duration);

        if (enemy != null && Health > 0)
        {
            agent.enabled = true;

            if (agent.enabled == false)
            {
                Command.KillEnemy(gameObject);
            }

            UnityEngine.AI.NavMeshHit hit;
            if (!agent.isOnNavMesh)
            {
                if (UnityEngine.AI.NavMesh.SamplePosition(gameObject.transform.position + agent.height * 0.5f * Vector3.up, out hit, agent.height * 2, 0))
                {
                    agent.Warp(hit.position);
                    Debug.Log("Warped");
                }

            }

            body.isKinematic = true;


            gameObject.GetComponent<EnemyAI>().Animator.SetBool("Stunned", false);

        }
        //necessary since enemies could have 0 health and miss the previous block
        else if (gameObject.CompareTag("Player"))// they are probably a player if they don't have a navmesh - look into this if there are bugs
        {
            body.mass = mass;
            agent.enabled = true;
            Animator animator = gameObject.GetComponentInChildren<Animator>();
            animator.SetBool("Stunned", false);
        }

        yield return null;
    }

    IEnumerator AddMassCoroutine(float mass, float duration)
    {
        gameObject.GetComponent<Rigidbody>().mass += mass;

        yield return new WaitForSeconds(duration);

        gameObject.GetComponent<Rigidbody>().mass -= mass;

        yield return null;
    }

    IEnumerator AddDamageReductionCoroutine(float amount, float duration)
    {
        DamageReduction += amount;

        yield return new WaitForSeconds(duration);

        DamageReduction -= amount;

        yield return null;
    }

    IEnumerator AddLifeStealCoroutine(float amount, float duration)
    {
        LifeSteal += amount;

        yield return new WaitForSeconds(duration);

        LifeSteal -= amount;

        yield return null;
    }

    IEnumerator IncreaseAttackSpeedCoroutine(float amount, float duration)
    {
        float regularAttackSpeed = AttSpd;
        float regularCastTime = Abilities[0].CastTime;

        float multiplier = (1 / (1 + amount));

        float regularReducedCooldown = Abilities[0].GetReducedCooldown();

        AttSpd = regularAttackSpeed * multiplier;
        float CastTime = regularCastTime * multiplier;
        float ReducedCooldown = regularReducedCooldown * multiplier;

        float RegularAnimationDuration = 0;
        //float regularLifeTime = Abilities[0].m_Lifetime;
        Abilities[0].UpdateAttackSpeed(AttSpd);
        Abilities[0].UpdateCastTime(CastTime);
        Abilities[0].UpdateReducedCooldown(ReducedCooldown);


        RegularAnimationDuration = Abilities[0].AnimationDuration;
        Abilities[0].AnimationDuration = (RegularAnimationDuration * 0.5f);


        Animator animator = gameObject.GetComponentInChildren<Animator>();
        float speedMultiplier = 1 + amount;

        animator.SetFloat("BasicAttackSpeedMultiplier", speedMultiplier);
        gameObject.transform.Find("Bow").GetComponentInChildren<Animator>().SetFloat("SpeedMultiplier", speedMultiplier);

        yield return new WaitForSeconds(duration);

        AttSpd = regularAttackSpeed;

        Abilities[0].UpdateAttackSpeed(AttSpd);
        Abilities[0].UpdateCastTime(regularCastTime);
        Abilities[0].UpdateReducedCooldown(regularReducedCooldown);



        Abilities[0].AnimationDuration = RegularAnimationDuration;


        animator.SetFloat("BasicAttackSpeedMultiplier", 1);
        gameObject.transform.Find("Bow").GetComponentInChildren<Animator>().SetFloat("SpeedMultiplier", 1);
        EnrageInEffect = false;
        yield return null;
    }

    IEnumerator IncreaseMovementSpeedCoroutine(float amount, float duration)
    {
        float regularMovementSpeed = MovementSpeed;

        MovementSpeed += 6 * amount;

        yield return new WaitForSeconds(duration);

        MovementSpeed -= 6 * amount;

        yield return null;
    }

    IEnumerator IncreaseAccelerationCoroutine(float amount, float duration)
    {
        float regularAcceleration = Acceleration;
        float regularAngularAccel = AngularAcceleration;

        Acceleration += 20 * amount;
        AngularAcceleration += 1 * amount;

        yield return new WaitForSeconds(duration);

        Acceleration -= 20 * amount;
        AngularAcceleration -= 1 * amount;

        yield return null;
    }

    IEnumerator InvulnerableCoroutine(float duration)
    {
        isInvincible = true;
        /*
        float[] baseDamages = new float[5];

        for(int i = 0; i < 5; i++)
        {
            if (Abilities[i] != null)
            {
                baseDamages[i] = Abilities[i].Damage;
                Abilities[i].Damage = (int)(Abilities[i].Damage * 1.5f);
            }
        }
        */
        yield return new WaitForSeconds(duration);

        isInvincible = false;

        /*
        for (int i = 0; i < 5; i++)
        {
            if (Abilities[i] != null)
            {
                Abilities[i].Damage = baseDamages[i];
            }
        }
        */
        this.gameObject.transform.Find("Player_UI_Prefab").GetComponent<PlayerUI>().InvincibilityGlow.SetActive(false);

        yield return null;
    }

    IEnumerator DamageBoostCoroutine(float duration)
    {

        float[] baseDamages = new float[5];

        for (int i = 0; i < 5; i++)
        {
            if (Abilities[i] != null)
            {
                baseDamages[i] = Abilities[i].Damage;
                Abilities[i].Damage = (int)(Abilities[i].Damage * 1.5f);
            }
        }

        yield return new WaitForSeconds(duration);

        for (int i = 0; i < 5; i++)
        {
            if (Abilities[i] != null)
            {
                Abilities[i].Damage = baseDamages[i];
            }
        }

        yield return null;
    }

    IEnumerator FieryWeaponCoroutine(float duration)
    {
        GameObject SwordAndShield = transform.Find("SwordAndShield").gameObject;
        ParticleSystem sys;

        if (SwordAndShield.activeInHierarchy)
            sys = SwordAndShield.GetComponentInChildren<ParticleSystem>(true);
        else
            sys = transform.Find("Bow").gameObject.GetComponentInChildren<ParticleSystem>(true);

        sys.gameObject.SetActive(true);

        yield return new WaitForSeconds(duration);

        sys.gameObject.SetActive(false);

        yield return null;
    }

    IEnumerator ReduceCooldownsCoroutine(float duration)
    {
        for (int i = 0; i < 5; i++)
        {
            if (Abilities[i] != null)
            {
                Abilities[i].RemoveCooldown();
            }
        }

        yield return new WaitForSeconds(duration);

        for (int i = 0; i < 5; i++)
        {
            if (Abilities[i] != null)
            {
                Abilities[i].ResetCooldown();
            }
        }

        this.gameObject.transform.Find("Player_UI_Prefab").GetComponent<PlayerUI>().NoCDsGlow.SetActive(false);

        yield return null;
    }
}
