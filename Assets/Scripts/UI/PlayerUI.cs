using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    //The abilities 2 and 3 are commented out because they have nothing attached to them yet.
    //Need to make a null check for them
    //Or something like an empty ability. As an ability.

    //Abilitiy tracking
    private float m_Ability1CooldownPercent = 0;
    private float m_Ability2CooldownPercent = 0;
    private float m_Ability3CooldownPercent = 0;
    private float m_AbilityDefensiveCooldownPercent = 0;


    //Gameobjects for the UI
    public GameObject HealthBarUI;
    public GameObject ShieldBarUI;
    public GameObject Ability1UI;
    public GameObject Ability2UI;
    public GameObject Ability3UI;
    public GameObject AbilityDefensiveUI;
    public GameObject Reticle;
    public GameObject Ability1UIBackground;
    public GameObject Ability2UIBackground;
    public GameObject Ability3UIBackground;
    public GameObject RevivePrompt;
    public GameObject ScoreText;
    public GameObject HealthDamageBarUI;
    public GameObject ShieldDamageBarUI;
    public GameObject LeftBorder;
    public GameObject RightBorder;
    public GameObject CurrentWaveText;
    public GameObject BossHealth;
    public GameObject BossHealthBarUI;
    public GameObject BossHealthDamageBarUI;
    public GameObject InvincibilityGlow;
    public GameObject NoCDsGlow;


    //Stat tracking, to get health and shield from
    private PlayerStats m_PlayerStats;
    private int m_CurrentHealth;
    private int m_MaxHealth;
    private int m_CurrentShield;
    private int m_MaxShield;
    private float m_CurrentHealthFill;
    private float m_CurrentHealthDamageFill;
    private float m_CurrentShieldFill;
    private float m_CurrentShieldDamageFill;

    private CharacterStats m_BossStats;
    private int m_BossCurrentHealth;
    private int m_BossMaxHealth;
    private float m_BossCurrentHealthFill;
    private float m_BossCurrentHealthDamageFill;

    //For toggling decrease for damage bars
    private bool m_ShieldDecrease = false;
    private bool m_HealthDecrease = false;

    private bool m_BossHealthDecrease = false;

    //Shield bar
    //private bool m_HasFlashed = false;

    //Speed for decreasing, float is % 
    private float m_DamageDecreaseSpeed = 0.03f;

    //Speed for flashing borders
    //private float m_BorderFlashSpeed = 5.0f;

    //Speed for the invincibility pulse
    private float m_InvincibilityPulseSpeed = 4f;
    private float m_NoCDsPulseSpeed = 4f;

    //Abilities
    public List<Sprite> m_AbilityImageList;
    public List<Sprite> m_AbilityCDImageList;
    public Sprite m_KnockbackImage;

    //Revive sprites
    public Sprite[] m_LeftBumperImage = new Sprite[2];
    public Sprite[] m_RightBumperImage = new Sprite[2];
    public Sprite m_ReviveAmountImage;
    public const float BUMPER_TIMER = 0.3f;
    //private float m_LeftBumperTimer = BUMPER_TIMER;
    //private float m_RightBumperTimer = BUMPER_TIMER;
    //private bool m_LeftBumperStart = false;
    //private bool m_RightBumperStart = false;

    //Enum for ability list
    //Abilities must be put in the list in this order
    enum AbilityOrder
    {
        Charge,
        Energy_Bomb,
        Ground_Shock,
        Weapon_Smash,
        Heal,
        Enrage,
        Chain_Lightning,
        Vortex,
        Protect_Me

    };

    //private Vector3 reticlePos;
    //private bool hasResized = false;

    // Use this for initialization
    void Start()
    {
        //Set start values for player
        m_PlayerStats = GetComponentInParent<PlayerStats>();
        UpdateUIInfo();

        //reticlePos = Reticle.transform.position;

        //Grab the ability name or something to represent it from PlayerStats.Abilitites
        //Use this to determine the color of the UI Icon
        for (int i = 1; i < 4; i++)
        {
            if (m_PlayerStats.Abilities[i].AbilityName == "Charge")
            {
                //UpdateAbilityIcon(i, m_AbilityImageList[(int)AbilityOrder.Charge]);

                UpdateAbilityIcon(i, AbilityOrder.Charge);
            }

            else if (m_PlayerStats.Abilities[i].AbilityName == "Energy Bomb")
            {
                UpdateAbilityIcon(i, AbilityOrder.Energy_Bomb);
            }

            else if (m_PlayerStats.Abilities[i].AbilityName == "Ground Shock")
            {
                UpdateAbilityIcon(i, AbilityOrder.Ground_Shock);
            }

            else if (m_PlayerStats.Abilities[i].AbilityName == "Weapon Smash")
            {
                UpdateAbilityIcon(i, AbilityOrder.Weapon_Smash);
            }

            else if (m_PlayerStats.Abilities[i].AbilityName == "Heal")
            {
                UpdateAbilityIcon(i, AbilityOrder.Heal);
            }

            else if (m_PlayerStats.Abilities[i].AbilityName == "Enrage")
            {
                UpdateAbilityIcon(i, AbilityOrder.Enrage);
            }

            else if (m_PlayerStats.Abilities[i].AbilityName == "Chain Lightning")
            {
                UpdateAbilityIcon(i, AbilityOrder.Chain_Lightning);
            }

            else if (m_PlayerStats.Abilities[i].AbilityName == "Vortex")
            {
                UpdateAbilityIcon(i, AbilityOrder.Vortex);
            }
            else if (m_PlayerStats.Abilities[i].AbilityName == "Protect Me")
            {
                UpdateAbilityIcon(i, AbilityOrder.Protect_Me);
            }
        }
     
        //PlayerStats.Abilities[0].

    }

    // Update is called once per frame
    void LateUpdate()
    {     
        UpdateUIInfo();

        //Could also make this a function as well.
        UpdateFillAmount(HealthBarUI, (float)m_CurrentHealth / (float)m_MaxHealth);
        UpdateFillAmount(ShieldBarUI, (float)m_CurrentShield / (float)m_MaxShield);
        UpdateFillAmount(ShieldDamageBarUI, m_CurrentShieldDamageFill);
        UpdateFillAmount(HealthDamageBarUI, m_CurrentHealthDamageFill);

        //Abilities Update
        UpdateFillAmount(Ability1UI, m_Ability1CooldownPercent);
        UpdateFillAmount(Ability2UI, m_Ability2CooldownPercent);
        UpdateFillAmount(Ability3UI, m_Ability3CooldownPercent);
        UpdateFillAmount(AbilityDefensiveUI, m_AbilityDefensiveCooldownPercent);

        //Do stuff for damage meter
        m_CurrentHealthFill = HealthBarUI.GetComponent<Image>().fillAmount;
        m_CurrentShieldFill = ShieldBarUI.GetComponent<Image>().fillAmount;

        //Could make a getter/setter for the stuff and call it from PlayerStats but since are here, why not contain it all?
        if (m_CurrentShieldFill < m_CurrentShieldDamageFill)
        {
            m_ShieldDecrease = true;
        }

        if (m_CurrentHealthFill < m_CurrentHealthDamageFill)
        {
            m_HealthDecrease = true;
        }

        //Decrease the damage meter and show it 
        if (m_ShieldDecrease == true)
        {
            m_CurrentShieldDamageFill -= m_DamageDecreaseSpeed;

            if (m_CurrentShieldDamageFill <= m_CurrentShieldFill)
            {
                m_ShieldDecrease = false;
            }
        }

        //If its false then just make it == to the current amount incase it does stuff like healing and whatnot
        else
        {
            m_CurrentShieldDamageFill = m_CurrentShieldFill;
        }

        //Do the same for health
        if (m_HealthDecrease == true)
        {
            m_CurrentHealthDamageFill -= m_DamageDecreaseSpeed;

            if (m_CurrentHealthDamageFill <= m_CurrentHealthFill)
            {
                m_HealthDecrease = false;
            }
        }

        else
        {
            m_CurrentHealthDamageFill = m_CurrentHealthFill;
        }

        //Update Boss Health if it is active
        if (BossHealth.activeInHierarchy)
        {
            UpdateFillAmount(BossHealthBarUI, (float)m_BossCurrentHealth / (float)m_BossMaxHealth);
            UpdateFillAmount(BossHealthDamageBarUI, m_BossCurrentHealthDamageFill);

            m_BossCurrentHealthFill = BossHealthBarUI.GetComponent<Image>().fillAmount;

            if (m_BossCurrentHealthFill < m_BossCurrentHealthDamageFill)
            {
                m_BossHealthDecrease = true;
            }


            if (m_BossHealthDecrease == true)
            {
                m_BossCurrentHealthDamageFill -= m_DamageDecreaseSpeed;

                if (m_BossCurrentHealthDamageFill <= m_BossCurrentHealthFill)
                {
                    m_BossHealthDecrease = false;
                }
            }

            else
            {
                m_BossCurrentHealthDamageFill = m_BossCurrentHealthFill;
            }
        }

        if (InvincibilityGlow.activeInHierarchy)
        {
            FlashGlow(InvincibilityGlow, m_InvincibilityPulseSpeed);
        }

        if (NoCDsGlow.activeInHierarchy)
        {
            FlashGlow(NoCDsGlow, m_NoCDsPulseSpeed);
        }

        //Update the side borders of the screen
        UpdateBorders();
    }

    private void FlashGlow(GameObject glowObject, float speed)
    {
        for (int i = 0; i < glowObject.transform.childCount; ++i)
        {
            Image glow = glowObject.transform.GetChild(i).gameObject.GetComponent<Image>();
            float a = glow.color.a;

            a = Mathf.Sin(speed * Time.time) * 0.5f + 0.5f;

            glow.color = new Color(glow.color.r, glow.color.g, glow.color.b, a);

            //Debug.Log(a);
        }
    }

    //Created overrides to make it easier
    //Takes gameobject (UI Object), and a new text for it
    public void UpdateText(GameObject Object, string newText)
    {
        Object.GetComponentsInChildren<Text>()[0].text = newText;
    }

    public void UpdateText(GameObject Object, uint newText)
    {
        Object.GetComponentsInChildren<Text>()[0].text = newText.ToString();
    }

    public void UpdateText(GameObject Object, int newText)
    {
        Object.GetComponentsInChildren<Text>()[0].text = newText.ToString();
    }

    public void UpdateText(GameObject Object, float newText)
    {
        Object.GetComponentsInChildren<Text>()[0].text = newText.ToString();
    }


    //Updates/Refreshes the current stats, do this every frame to get accurate reading, helps reduce line clutter
    void UpdateUIInfo()
    {
        m_CurrentHealth = (int)m_PlayerStats.Health;
        m_MaxHealth = m_PlayerStats.MaxHealth;

        m_CurrentShield = m_PlayerStats.Shield;
        m_MaxShield = m_PlayerStats.MaxShield;

        ScoreText.GetComponent<Text>().text = m_PlayerStats.ScorePointsEarned.ToString();

        //Basic attack is index 0, Index 4 is knockback
        //m_Ability1Cooldown = m_PlayerStats.Abilities[1].GetCoolDownTime();
        //m_Ability2Cooldown = m_PlayerStats.Abilities[2].GetCoolDownTime();
        //m_Ability3Cooldown = m_PlayerStats.Abilities[3].GetCoolDownTime();
        //m_AbilityDefensiveCooldown = m_PlayerStats.Abilities[4].GetCoolDownTime();

        //Update the cooldown status for fill
        //Debug.Log(PlayerStats.Abilities[1].GetCoolDownTime());
        m_Ability1CooldownPercent = m_PlayerStats.Abilities[1].GetCoolDownTimePercent();
        m_Ability2CooldownPercent = m_PlayerStats.Abilities[2].GetCoolDownTimePercent();
        m_Ability3CooldownPercent = m_PlayerStats.Abilities[3].GetCoolDownTimePercent();
        m_AbilityDefensiveCooldownPercent = m_PlayerStats.Abilities[4].GetCoolDownTimePercent();

        if (BossHealth.activeInHierarchy)
        {
            if (m_BossStats == null)
            {
                m_BossStats = GameObject.FindGameObjectWithTag("Boss").GetComponent<CharacterStats>();
            }

            m_BossCurrentHealth = (int)m_BossStats.Health;
            m_BossMaxHealth = m_BossStats.MaxHealth;
        }

    }

    public void FlashHUDBars(int damageTaken, bool healthDamage)
    {
        Image bar = LeftBorder.GetComponent<Image>();
        Color newCol = bar.color;

        if (healthDamage)
        { 
            newCol.b = Mathf.Clamp01(bar.color.b - damageTaken * 0.1f);
            newCol.g = newCol.b;
        }
        else
        {
            newCol.a = Mathf.Clamp01(bar.color.a - damageTaken * 0.1f);
        }

        bar.color = newCol;
        RightBorder.GetComponent<Image>().color = newCol;
    }

    void UpdateBorders()
    {
        Image bar = LeftBorder.GetComponent<Image>();
        Color newCol = bar.color;

        newCol.g = Mathf.Clamp01(newCol.g + Time.deltaTime);
        newCol.b = newCol.g;
        newCol.a = Mathf.Clamp01(newCol.a + Time.deltaTime);

        bar.color = newCol;
        RightBorder.GetComponent<Image>().color = newCol;
    }

    //Takes gameobject to access, and a percentage to fill the image/button
    void UpdateFillAmount(GameObject Object, float fillPercent)
    {
        Object.GetComponentInChildren<Image>().fillAmount = fillPercent;
    }

    //void UpdateBackgroundColor(int abilityNum, Color color)
    //{
    //    if (abilityNum == 1)
    //    {
    //        Ability1UIBackground.GetComponent<Image>().color = color;
    //    }

    //    else if (abilityNum == 2)
    //    {
    //        Ability2UIBackground.GetComponent<Image>().color = color;
    //    }

    //    else if (abilityNum == 3)
    //    {
    //        Ability3UIBackground.GetComponent<Image>().color = color;
    //    }
    //}

    void UpdateAbilityIcon(int abilityNum, AbilityOrder sprite)
    {
        if (abilityNum == 1)
        {
            Ability1UI.GetComponent<Image>().sprite = m_AbilityCDImageList[(int)sprite];
            Ability1UIBackground.GetComponent<Image>().sprite = m_AbilityImageList[(int)sprite];
        }

        if (abilityNum == 2)
        {
            Ability2UI.GetComponent<Image>().sprite = m_AbilityCDImageList[(int)sprite];
            Ability2UIBackground.GetComponent<Image>().sprite = m_AbilityImageList[(int)sprite];
        }

        if (abilityNum == 3)
        {
            Ability3UI.GetComponent<Image>().sprite = m_AbilityCDImageList[(int)sprite];
            Ability3UIBackground.GetComponent<Image>().sprite = m_AbilityImageList[(int)sprite];
        }
    }

}
