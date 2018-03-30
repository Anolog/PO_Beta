using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class GraphicsMenuBehaviour : UIBehaviour
{

    [SerializeField]
    private GameObject m_OptionsScreen;

    public GameObject m_BrightnessSlider;
    public GameObject m_QualitySlider;
    public GameObject m_ResolutionSlider;

    public Text m_QualityText;
    public Text m_ResolutionText;

    // Use this for initialization
    protected override void Start()
    {
        base.Start();
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

    protected void SliderMovementDown(Controllers controller)
    {
        if (m_CurrentSelectedButton == transform.Find("BrightnessButton").GetComponent<Button>())
        {
            SliderMovementDown(controller, m_BrightnessSlider);
        }
        else if (m_CurrentSelectedButton == transform.Find("GraphicsQualityButton").GetComponent<Button>())
        {
            SliderMovementDown(controller, m_QualitySlider);
        }
        else if (m_CurrentSelectedButton == transform.Find("ResolutionButton").GetComponent<Button>())
        {
            SliderMovementDown(controller, m_ResolutionSlider);
        }
    }

    protected void SliderMovementUp(Controllers controller)
    {
        if (m_CurrentSelectedButton == transform.Find("BrightnessButton").GetComponent<Button>())
        {
            SliderMovementUp(controller, m_BrightnessSlider);
        }
        else if (m_CurrentSelectedButton == transform.Find("GraphicsQualityButton").GetComponent<Button>())
        {
            SliderMovementUp(controller, m_QualitySlider);
        }
        else if (m_CurrentSelectedButton == transform.Find("ResolutionButton").GetComponent<Button>())
        {
            SliderMovementUp(controller, m_ResolutionSlider);
        }
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
                if (m_CurrentSelectedButton == transform.Find("BrightnessButton").GetComponent<Button>())
                {
                    SliderMovementUp(controller, m_BrightnessSlider);
                }
                else if (m_CurrentSelectedButton == transform.Find("GraphicsQualityButton").GetComponent<Button>())
                {
                    SliderMovementUp(controller, m_QualitySlider);
                }
                else if (m_CurrentSelectedButton == transform.Find("ResolutionButton").GetComponent<Button>())
                {
                    SliderMovementUp(controller, m_ResolutionSlider);
                }
            }
            else if (joyStick.x < -0.1f)
            {
                if (m_CurrentSelectedButton == transform.Find("BrightnessButton").GetComponent<Button>())
                {
                    SliderMovementDown(controller, m_BrightnessSlider);
                }
                else if (m_CurrentSelectedButton == transform.Find("GraphicsQualityButton").GetComponent<Button>())
                {
                    SliderMovementDown(controller, m_QualitySlider);
                }
                else if (m_CurrentSelectedButton == transform.Find("ResolutionButton").GetComponent<Button>())
                {
                    SliderMovementDown(controller, m_ResolutionSlider);
                }
            }
        }
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

    public void SetupOptionsMenu()
    {
        gameObject.SetActive(false);
        m_OptionsScreen.gameObject.SetActive(true);
    }

    public override void MoveToNextButton(Controllers controller)
    {
        if (m_LockedButton == false)
        {
            GameManager.audioManager.PlaySound(AudioManager.Sounds.MENU_SELECT);

            //Reset the button to the other image
            m_CurrentSelectedButton.image.sprite = m_UnSelectedButtonImage;
            UnityEngine.UI.Button nextButton = m_CurrentSelectedButton.GetComponent<ButtonNode>().NextButton;
            m_CurrentSelectedButton = nextButton;
            m_CurrentSelectedButton.Select();
        }
    }

    public override void MoveToPreviousButton(Controllers controller)
    {
        if (m_LockedButton == false)
        {
            GameManager.audioManager.PlaySound(AudioManager.Sounds.MENU_SELECT);

            //Reset the button to the other image
            m_CurrentSelectedButton.image.sprite = m_UnSelectedButtonImage;
            UnityEngine.UI.Button previousButton = m_CurrentSelectedButton.GetComponent<ButtonNode>().PreviousButton;
            m_CurrentSelectedButton = previousButton;
            m_CurrentSelectedButton.Select();
        }
    }
    public void SliderMovementUp(Controllers controller, GameObject slider)
    {
        GameManager.audioManager.PlaySound(AudioManager.Sounds.MENU_SLIDER);
        if (slider.GetComponent<Slider>().value <= slider.GetComponent<Slider>().maxValue)
            slider.GetComponent<Slider>().value += 0.1f;
    }

    public void SliderMovementDown(Controllers controller, GameObject slider)
    {
        GameManager.audioManager.PlaySound(AudioManager.Sounds.MENU_SLIDER);
        if (slider.GetComponent<Slider>().value >= 0.0f)
            slider.GetComponent<Slider>().value -= 0.1f;

    }

    public void ChangeLockFromBButton(Controllers controller)
    {
        GameManager.audioManager.PlaySound(AudioManager.Sounds.MENU_CONFIRM);

        SetLockedButton(false);
        RemoveSliderControls();
        SetUpControls();
    }
}
