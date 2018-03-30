using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class ControlsBehaviour : UIBehaviour
{
    [SerializeField]
    private GameObject m_MainMenu;

    public Slider m_SenSlider;

    public Button m_ContSchemeButtonRef;
    public Image m_ContSchemeImageObject;
    public Sprite m_ControllerScheme;
    public Sprite m_KeyboardScheme;

    public Toggle m_HorizontalToggle;
    public Toggle m_VerticalToggle;

    private Controllers m_AllowedController;

    protected override void Start()
    {
        base.Start();

        m_ContSchemeImageObject.sprite = m_ControllerScheme;
        m_ContSchemeImageObject.enabled = false;
        m_ContSchemeImageObject.preserveAspect = true;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
    }

    public override void SetLockedButton(bool isButtonLocked)
    {
        m_LockedButton = isButtonLocked;

        if (m_LockedButton == true)
        {
            m_CurrentSelectedButton.image.sprite = m_PressedButtonImage;
            RemoveControls();
            SetUpSliderControls();
        }

        if (m_LockedButton == false)
        {
            m_CurrentSelectedButton.image.sprite = m_SelectedButtonImage;
            RemoveControls();
            SetUpControls();
        }
    }

    public void SetUpSliderControls()
    {
        m_PlayerInput.HandleDPadLeft = SliderMovementDown;
        m_PlayerInput.HandleDPadRight = SliderMovementUp;
        m_PlayerInput.HandleBButton = ChangeLockFromBButton;
        m_PlayerInput.HandleLeftStick = SliderMovement;
    }

    public void RemoveSliderControls()
    {
        m_PlayerInput.HandleDPadLeft = null;
        m_PlayerInput.HandleDPadRight = null;
        m_PlayerInput.HandleBButton = null;
        m_PlayerInput.HandleLeftStick = null;
    }

    public override void SetUpControls()
    {
        if (m_PlayerInput == null)
        {
            return;
        }

        m_PlayerInput.HandleDPadUp = MoveToPreviousButton;
        m_PlayerInput.HandleDPadDown = MoveToNextButton;
        m_PlayerInput.HandleLeftStick = MoveToButtonStick;
        m_PlayerInput.HandleAButton = OnClick;
    }

    public override void RemoveControls()
    {
        m_PlayerInput.HandleDPadUp = null;
        m_PlayerInput.HandleDPadDown = null;
        m_PlayerInput.HandleAButton = null;
        m_PlayerInput.HandleLeftStick = null;
    }

    public void AllowController(Controllers controller)
    {
        PlayerManager playerManager = GameManager.playerManager;
        m_AllowedController = controller;
        if (playerManager.SensitivityMultiplier.ContainsKey(m_AllowedController))
        {
            m_SenSlider.value = playerManager.SensitivityMultiplier[m_AllowedController];

            if (playerManager.InvertedAxis[m_AllowedController].x > 0)
            {
                m_HorizontalToggle.isOn = false;
            }
            else
            {
                m_HorizontalToggle.isOn = true;
            }

            if (playerManager.InvertedAxis[m_AllowedController].y > 0)
            {
                m_VerticalToggle.isOn = false;
            }
            else
            {
                m_VerticalToggle.isOn = true;
            }

        }
        else
        {
            m_SenSlider.value = 1.0f;
            playerManager.SensitivityMultiplier.Add(m_AllowedController, m_SenSlider.value);
            playerManager.InvertedAxis.Add(m_AllowedController, Vector2.one);
            m_HorizontalToggle.isOn = false;
            m_VerticalToggle.isOn = false;
        }
    }

    public void SetUpMainMenu()
    {
        gameObject.SetActive(false);
        m_MainMenu.gameObject.SetActive(true);
    }

    protected override void OnClick(Controllers controller)
    {
        if (controller != m_AllowedController)
        {
            return;
        }

        base.OnClick(controller);
    }

    protected override void Update()
    {
        if (m_CurrentSelectedButton == m_ContSchemeButtonRef)
        {
            if (m_AllowedController != Controllers.KEYBOARD_MOUSE)
            {
                m_ContSchemeImageObject.sprite = m_ControllerScheme;
            }
            
            else if (m_AllowedController == Controllers.KEYBOARD_MOUSE)
            {
                m_ContSchemeImageObject.sprite = m_KeyboardScheme;
            }

            
            m_ContSchemeImageObject.enabled = true;
        }

        else
        {
            if (m_ContSchemeImageObject.enabled == true)
            {
                m_ContSchemeImageObject.enabled = false;
            }
        }

        if (m_LockedButton == true)
        {
            //May need some changes depending which slider you're on
            ChangeSensitivity();
        }
        base.Update();
    }

    public void InvertVertical()
    {
        PlayerManager playerManager = GameManager.playerManager;
        if (playerManager.InvertedAxis[m_AllowedController].y > 0)
        {
            playerManager.SetInvertedAxis(m_AllowedController, new Vector2(playerManager.InvertedAxis[m_AllowedController].x, -1));
            m_VerticalToggle.isOn = true;
        }
        else
        {
            playerManager.SetInvertedAxis(m_AllowedController, new Vector2(playerManager.InvertedAxis[m_AllowedController].x, 1));
            m_VerticalToggle.isOn = false;
        }
    }

    public void InvertHorizontal()
    {
        PlayerManager playerManager = GameManager.playerManager;
        if (playerManager.InvertedAxis[m_AllowedController].x > 0)
        {
            playerManager.SetInvertedAxis(m_AllowedController, new Vector2(-1, playerManager.InvertedAxis[m_AllowedController].y));
            m_HorizontalToggle.isOn = true;
        }
        else
        {
            playerManager.SetInvertedAxis(m_AllowedController, new Vector2(1, playerManager.InvertedAxis[m_AllowedController].y));
            m_HorizontalToggle.isOn = false;
        }
    }

    public void ChangeSensitivity()
    {
        PlayerManager playerManager = GameManager.playerManager;
        playerManager.SetSensitivityMultiplier(m_AllowedController, m_SenSlider.value);
    }

    public override void MoveToNextButton(Controllers controller)
    {
        if (controller != m_AllowedController)
        {
            return;
        }

        GameManager.audioManager.PlaySound(AudioManager.Sounds.MENU_SELECT);

        // m_CurrentlySelectedButton.image.sprite = unhighlightedButton
        //Debug.Log("Controller Type: " + controller.ToString());
        Button nextButton = m_CurrentSelectedButton.GetComponent<ButtonNode>().NextButton;

        //Reset the button to the other image
        m_CurrentSelectedButton.image.sprite = m_UnSelectedButtonImage;

        m_CurrentSelectedButton = nextButton;
        m_CurrentSelectedButton.Select();

    }

    public override void MoveToPreviousButton(Controllers controller)
    {
        if (controller != m_AllowedController)
        {
            return;
        }

        GameManager.audioManager.PlaySound(AudioManager.Sounds.MENU_SELECT);

        Button previousButton = m_CurrentSelectedButton.GetComponent<ButtonNode>().PreviousButton;

        //Reset the button to the other image
        m_CurrentSelectedButton.image.sprite = m_UnSelectedButtonImage;

        m_CurrentSelectedButton = previousButton;
        m_CurrentSelectedButton.Select();
        //Debug.Log(m_CurrentSelectedButton.ToString());
    }

    public void SliderMovementUp(Controllers controller)
    {
        if (controller != m_AllowedController)
        {
            return;
        }

        GameManager.audioManager.PlaySound(AudioManager.Sounds.MENU_SLIDER);

        if (m_SenSlider.GetComponent<Slider>().value <= m_SenSlider.GetComponent<Slider>().maxValue)
            m_SenSlider.GetComponent<Slider>().value += 0.1f;
    }

    public void SliderMovementDown(Controllers controller)
    {
        if (controller != m_AllowedController)
        {
            return;
        }

        GameManager.audioManager.PlaySound(AudioManager.Sounds.MENU_SLIDER);

        if (m_SenSlider.GetComponent<Slider>().value >= m_SenSlider.GetComponent<Slider>().minValue)
            m_SenSlider.GetComponent<Slider>().value -= 0.1f;
    }

    protected void SliderMovement(Controllers controller, Vector2 joyStick)
    {
        if (joyStick.magnitude <= Mathf.Epsilon)
        {
            JoyStickUpdateTimer = 0;
        }
        if (JoyStickUpdateTimer <= Time.unscaledTime)
        {
            JoyStickUpdateTimer = Time.time + JoyStickUpdateTime;
            if (joyStick.x > 0.1f)
            {
                SliderMovementUp(controller);
            }
            else if (joyStick.x < -0.1f)
            {
                SliderMovementDown(controller);
            }
        }
    }

    public void ChangeLockFromBButton(Controllers controller)
    {
        GameManager.audioManager.PlaySound(AudioManager.Sounds.MENU_CONFIRM);
        SetLockedButton(false);
        RemoveSliderControls();
        SetUpControls();
    }

}
