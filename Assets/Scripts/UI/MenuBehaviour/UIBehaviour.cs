using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine;

public class UIBehaviour : MonoBehaviour
{
    //Holds all buttons in screen
    public List<Button> m_ButtonList = new List<Button>();

    protected Button m_CurrentSelectedButton;

    public Button FirstSelectedButton;

    protected bool m_LockedButton = false;

    [SerializeField]
    protected GameObject m_GameManager;

    protected PlayerInput m_PlayerInput;

    public virtual void SetLockedButton(bool isButtonLocked) { m_LockedButton = isButtonLocked; }

    protected float JoyStickUpdateTime = 0.15f;
    protected float JoyStickUpdateTimer;

    [SerializeField]
    protected Sprite m_SelectedButtonImage;
    [SerializeField]
    protected Sprite m_UnSelectedButtonImage;
    [SerializeField]
    protected Sprite m_PressedButtonImage;
    

    protected virtual void OnEnable()
    {
        FirstSelectedButton.Select();
        m_CurrentSelectedButton = FirstSelectedButton;
        SetUpControls();
    }

    protected virtual void OnDisable()
    {
        if (m_SelectedButtonImage != null && m_UnSelectedButtonImage != null)
        {
            m_CurrentSelectedButton.image.sprite = m_UnSelectedButtonImage;
        }
        RemoveControls();
    }

    // Use this for initialization
    protected virtual void Start()
    {
        for (int i = 0; i < m_ButtonList.Count; i++)
        {
            m_ButtonList[i].image.sprite = m_UnSelectedButtonImage;
        }

        if (m_GameManager == null)
        {
            m_GameManager = GameObject.FindGameObjectWithTag("GameManager");
        }
        //Debug.Log(m_CurrentSelectedButton);

        m_PlayerInput = m_GameManager.GetComponent<GameManager>().playerInput;

        JoyStickUpdateTimer = Time.unscaledTime + JoyStickUpdateTime;

        SetUpControls();
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (m_SelectedButtonImage != null && m_UnSelectedButtonImage != null)
        {
            if (m_CurrentSelectedButton.image.sprite != m_PressedButtonImage)
            {
                m_CurrentSelectedButton.image.sprite = m_SelectedButtonImage;
            }
        }
    }

    public virtual void MoveToButtonStick(Controllers controller, Vector2 joyStick)
    {
        if (joyStick.magnitude <= Mathf.Epsilon)
        {
            JoyStickUpdateTimer = 0;
        }
        if (JoyStickUpdateTimer <= Time.unscaledTime)
        {
            //Debug.Log(joyStick);
            JoyStickUpdateTimer = Time.unscaledTime + JoyStickUpdateTime;

            if (joyStick.y > 0.1f)
            {
                MoveToPreviousButton(controller);
            }
            else if (joyStick.y < -0.1f)
            {
                MoveToNextButton(controller);
            }

        }
    }

    public virtual void MoveToNextButton(Controllers controller)
    {
        GameManager.audioManager.PlaySound(AudioManager.Sounds.MENU_SELECT);

        // m_CurrentlySelectedButton.image.sprite = unhighlightedButton
        //Debug.Log("Controller Type: " + controller.ToString());
        Button nextButton = m_CurrentSelectedButton.GetComponent<ButtonNode>().NextButton;

        //Reset the button to the other image
        m_CurrentSelectedButton.image.sprite = m_UnSelectedButtonImage;

        m_CurrentSelectedButton = nextButton;
        m_CurrentSelectedButton.Select();

    }

    public virtual void MoveToPreviousButton(Controllers controller)
    {
        GameManager.audioManager.PlaySound(AudioManager.Sounds.MENU_SELECT);

        Button previousButton = m_CurrentSelectedButton.GetComponent<ButtonNode>().PreviousButton;

        //Reset the button to the other image
        m_CurrentSelectedButton.image.sprite = m_UnSelectedButtonImage;

        m_CurrentSelectedButton = previousButton;
        m_CurrentSelectedButton.Select();
        //Debug.Log(m_CurrentSelectedButton.ToString());
    }

    public void MoveToSpecificButton(int buttonIndex)
    {
        //m_CurrentIndex = buttonIndex;
        //m_CurrentSelectedButton = m_ButtonList[m_CurrentIndex];
    }

    public void LockActiveButton()
    {
        SetLockedButton(true);
    }

    public void UnlockActiveButton()
    {
        SetLockedButton(false);
    }

    protected void SubscribeToButton(Button buttonToSubscribe, UnityAction call)
    {
        buttonToSubscribe.gameObject.SetActive(true);
        buttonToSubscribe.onClick.AddListener(call);
    }

    protected void UnsubscribeToButton(Button buttonToUnsubscribe, UnityAction call)
    {
        buttonToUnsubscribe.gameObject.SetActive(false);
        buttonToUnsubscribe.onClick.RemoveListener(call);
    }

    public virtual void SetUpControls()
    {
        
    }

    public virtual void RemoveControls()
    {

    }

    protected virtual void OnClick(Controllers controller)
    {
        if (m_CurrentSelectedButton.gameObject.name == "Back_Button")
        {
            GameManager.audioManager.PlaySound(AudioManager.Sounds.MENU_BACK);
        }
        else
        {
            GameManager.audioManager.PlaySound(AudioManager.Sounds.MENU_CONFIRM);
        }

        m_CurrentSelectedButton.onClick.Invoke();
    }
}
