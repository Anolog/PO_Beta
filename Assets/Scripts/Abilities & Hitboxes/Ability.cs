
using UnityEngine;

/// <summary>
/// Parent to abilities
/// Drag and drop into stats class
/// </summary>
public class Ability{

    // Variables
    protected GameObject Hitbox;
    protected float m_BaseDamage = 0;
    public float Damage;
    public float m_Lifetime;
    public string AbilityName = "NoName";
    protected float m_ReducedCooldown = 0.5f;
    protected float m_DefaultCooldown;
    public float AnimationDuration;
    public float GetReducedCooldown() { return m_ReducedCooldown; }

    //All the abilities in the game
    public enum Abilities { BasicMelee, LastAbility }

    public enum AbilityType { Melee, Ranged }

    [SerializeField]
    protected float m_CastTime = 1.0f;
    public float CastTime { get { return m_CastTime; } set { m_CastTime = value; } }
    [SerializeField]
    protected float m_RecoveryTime = 1.0f;
    [SerializeField]
    protected float m_CoolDownTime = 5.0f;
    [SerializeField]
    public AbilityType m_Type;
    
    //Used for timing
    protected float m_CoolDownTimer = -1.0f;
    protected float m_CastTimer = -1.0f;

    protected CharacterStats m_Character;

    //Are we cating the ability?
    protected bool m_Casting = false;

    public Ability(CharacterStats character)
    {
        m_Character = character;
    }

    public float GetCoolDown() { return m_CoolDownTime; }

    /// <summary>
    /// The stats block needs to update the abilities so they know when to go off
    /// </summary>
    public virtual void Update()
    {
        //If we are casting and the cast timer is done cast
        if(m_Casting && m_CastTimer < Time.time)
        {
            m_Casting = false;
            ActivateEffect(); 
        }
    }

    /// <summary>
    /// Activate the ability
    /// </summary>
	public virtual void Use()
    {
        if (m_CoolDownTimer <= Time.time)
        {
            m_Casting = true;
            m_CoolDownTimer = Time.time + m_CoolDownTime;
            m_CastTimer = Time.time + m_CastTime;
            m_Character.CastLockCharacter(m_ReducedCooldown);
            if (m_Character.CompareTag("Player"))
            {
                if (AbilityName != "Sword and Shield" &&
                    AbilityName != "Bow" &&
                    AbilityName != "Knockback")
                {
                    m_Character.gameObject.GetComponent<PlayerStats>().AddToAbilitiesUsed();
                }
                else if (AbilityName == "Sword and Shield" ||
                    AbilityName == "Bow")
                {
                    m_Character.gameObject.GetComponent<PlayerStats>().AddToBasicAttacksUsed();
                }
                else if (AbilityName == "Knockback")
                {
                    m_Character.gameObject.GetComponent<PlayerStats>().AddToKnockbacksUsed();
                }
            }

        }
        else
        {
            GameManager.audioManager.PlaySound(AudioManager.Sounds.ABILITY_STILL_ON_CD);
        }
    }

    public virtual void Use(GameObject target)
    {
    }


    public void RemoveCooldown()
    {
        m_CoolDownTime = m_ReducedCooldown;
        m_CoolDownTimer = Time.time;
    }

    // each ability should override this to reset their cooldowns properly
    public void ResetCooldown()
    {
        m_CoolDownTime = m_DefaultCooldown;
    }

    /// <summary>
    /// The actual effect, override this to keep activates functionality
    /// </summary>
    protected virtual void ActivateEffect()
    {

    }

    /// <summary>
    /// Get the time before the cool down is done
    /// </summary>
    public virtual float GetCoolDownTime()
    {
        return Mathf.Clamp(m_CoolDownTimer - Time.time, 0, 100000);
    }

    /// <summary>
    /// Get the time until we cast
    /// </summary>
    public virtual float GetTimeUntilCast()
    {
        return Mathf.Clamp(m_CastTimer - Time.time, 0, 100000);
    }

    /// <summary>
    /// Get the percent before the cool down is done
    /// </summary>
    public virtual float GetCoolDownTimePercent()
    {
        return Mathf.Clamp((m_CoolDownTimer - Time.time) / m_CoolDownTime, 0, 1);
    }

    /// <summary>
    /// Get the percent until we cast
    /// </summary>
    public virtual float GetTimeUntilCastPercent()
    {
        return Mathf.Clamp((m_CastTimer - Time.time) / m_CastTime, 0, 1);
    }

    //We new ability is made add it here too!
    /// <summary>
    /// Given a enum list return an array of the abilities 
    /// </summary>
    /// <param name="values"></param>
    /// <returns></returns>
    public static Ability[] CreateAbilityList(Abilities[] values, CharacterStats character)
    {
        Ability[] abilities = new Ability[values.Length];

        for(int i = 0; i < abilities.Length; i++)
        {
            switch(values[i])
            {
                case Abilities.LastAbility:
                    abilities[i] = new Ability(character);
                    break;
            }
        }

        return abilities;
    }



    public void UpdateAttackSpeed(float attackSpeed)
    {
        m_CoolDownTime = attackSpeed;

        if (m_CoolDownTimer > Time.time + m_CoolDownTime)
        {
            m_CoolDownTimer = Time.time + m_CoolDownTime;
        }
    }

    public void UpdateCastTime(float castTime)
    {
        m_CastTime = castTime;
        if (m_CastTimer > Time.time + m_CastTime)
        {
            m_CastTimer = Time.time + m_CastTime;
        }
    }

    public void UpdateReducedCooldown(float reducedCoolDown)
    {
        m_ReducedCooldown = reducedCoolDown;
    }
}
