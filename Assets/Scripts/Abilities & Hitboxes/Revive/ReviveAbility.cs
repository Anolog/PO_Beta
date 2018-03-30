using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReviveAbility : Ability {

    public const int REVIVE_HIT_AMOUNT = 10;
    public static float RECOVERY_TIME = 0.15f;
    public static float CAST_TIME = 0.1f;
    public static float COOL_DOWN_TIME = 0f;

    private int m_ButtonPressCounter = 0;
    private float m_MaximumTimeBetweenPresses = 0.5f;
    private float m_TimeBetweenButtonPressTimer = -1f;
    private bool m_FirstTime = true;
    private GameObject m_Target = null;
    private float m_FillAmount = 0.0f;

    public GameObject GetTarget() { return m_Target; }
    public void SetTarget(GameObject target) { m_Target = target; }

    public ReviveAbility(PlayerStats character)
        : base(character)
    {
        m_CastTime = CAST_TIME;
        m_RecoveryTime = RECOVERY_TIME;
        m_CoolDownTime = COOL_DOWN_TIME;
        m_DefaultCooldown = COOL_DOWN_TIME;
    }

    public override void Use()
    {
        if (m_Target == null)
        {
            m_ButtonPressCounter = 0;
            return;
        }

        if (m_TimeBetweenButtonPressTimer > Time.time || m_FirstTime)
        {
            m_FirstTime = false;
            m_ButtonPressCounter++;
            m_TimeBetweenButtonPressTimer = Time.time + m_MaximumTimeBetweenPresses;
        }

        else
        {
            m_ButtonPressCounter = 0;
            m_TimeBetweenButtonPressTimer = Time.time + m_MaximumTimeBetweenPresses;
        }

        //Debug.Log(m_ButtonPressCounter);

        switch (m_ButtonPressCounter)
        {
            case 0:
            case 1:
            case 2:
            case 3:
                GameManager.audioManager.PlaySound(AudioManager.Sounds.REVIVING_LOW);
                break;
            case 4:
            case 5:
            case 6:
            case 7:
                GameManager.audioManager.PlaySound(AudioManager.Sounds.REVIVING_MED);
                break;
            case 8:
            case 9:
            case 10:
                GameManager.audioManager.PlaySound(AudioManager.Sounds.REVIVING_HIGH);
                break;
            default:
                break;
        }


        if (m_ButtonPressCounter >= REVIVE_HIT_AMOUNT)
        {
            base.Use();
            m_ButtonPressCounter = 0;
        }


    }

    protected override void ActivateEffect()
    {
        PlayerStats stats = (PlayerStats)m_Character;
		Command.RevivePlayer(m_Target, stats);
        stats.gameObject.GetComponentInChildren<PlayerUI>().RevivePrompt.SetActive(false);
        m_Target = null;
    }

    public override void Update()
    {
        base.Update();

        if (m_TimeBetweenButtonPressTimer < Time.time || m_FirstTime)
        {
            m_ButtonPressCounter = 0;
            m_TimeBetweenButtonPressTimer = Time.time + m_MaximumTimeBetweenPresses;
        }

        m_FillAmount = ((float)m_ButtonPressCounter / (float)REVIVE_HIT_AMOUNT);
        m_Character.gameObject.transform.Find("Player_UI_Prefab").Find("RevivePrompt").Find("ReviveBar").GetComponent<UnityEngine.UI.Image>().fillAmount = m_FillAmount;

    }
}
