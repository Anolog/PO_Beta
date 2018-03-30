using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AudioMenuBehaviour : UIBehaviour
{

    [SerializeField]
    private GameObject m_OptionsScreen;

    public GameObject m_SFXSlider;
    public GameObject m_MusicSlider;

    public Text m_SFXText;
    public Text m_MUsicText;

    public void Awake()
    {
        GameManager.audioManager.m_SoundFXSlider = m_SFXSlider.GetComponent<Slider>();
        //m_SFXSlider.GetComponent<Slider>().value = GameManager.audioManager.m_Volume;
        //m_MusicSlider.GetComponent<Slider>().value = GameManager.audioManager.m_Volume;
        m_SFXSlider.GetComponent<Slider>().value = GameManager.audioManager.m_Volume;

        m_MusicSlider.GetComponent<Slider>().value = GameManager.audioManager.m_MusicVolume;

    }

    // Use this for initialization
    protected override void Start()
    {
        base.Start();

        m_SFXSlider.GetComponent<Slider>().value = GameManager.audioManager.m_Volume;

        m_MusicSlider.GetComponent<Slider>().value = GameManager.audioManager.m_MusicVolume;
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
        m_PlayerInput.HandleLeftStick = SliderMovementStick;
        m_PlayerInput.HandleBButton = ChangeLockFromBButton;
    }

    protected void SliderMovementStick(Controllers controller, Vector2 joyStick)
    {
        if (joyStick.magnitude <= Mathf.Epsilon)
        {
            JoyStickUpdateTimer = 0;
        }
        if (JoyStickUpdateTimer <= Time.unscaledTime)
        {
            //Debug.Log(joyStick);
            JoyStickUpdateTimer = Time.unscaledTime + JoyStickUpdateTime;

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

    protected void SliderMovementDown(Controllers controller)
    {
        if (m_CurrentSelectedButton == transform.Find("SFXVolumeButton").GetComponent<Button>())
        {
            SliderMovementDown(controller, m_SFXSlider);
        }
        else if (m_CurrentSelectedButton == transform.Find("MusicVolButton").GetComponent<Button>())
        {
            SliderMovementDown(controller, m_MusicSlider);
        }
    }

    protected void SliderMovementUp(Controllers controller)
    {
        if (m_CurrentSelectedButton == transform.Find("SFXVolumeButton").GetComponent<Button>())
        {
            SliderMovementUp(controller, m_SFXSlider);
        }
        else if (m_CurrentSelectedButton == transform.Find("MusicVolButton").GetComponent<Button>())
        {
            SliderMovementUp(controller, m_MusicSlider);
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
        m_PlayerInput.HandleBButton = null;
        m_PlayerInput.HandleDPadLeft = null;
        m_PlayerInput.HandleDPadRight = null;
        m_PlayerInput.HandleLeftStick = null;
    }

    public void SetupOptionsMenu()
    {
        //Debug.Log("SetupOptionsMenu");
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
        if (slider.GetComponent<Slider>().value <= 1.0f)
            slider.GetComponent<Slider>().value += 0.1f;

        GameManager.audioManager.PlaySound(AudioManager.Sounds.MENU_SLIDER);

        if (slider == m_SFXSlider)
        {
            GameManager.audioManager.m_Volume = slider.GetComponent<Slider>().value;
        }
        else
        {
            GameManager.audioManager.SetMusicVolume(slider.GetComponent<Slider>().value);
        }
    }

    public void SliderMovementDown(Controllers controller, GameObject slider)
    {
        if (slider.GetComponent<Slider>().value >= 0.0f)
            slider.GetComponent<Slider>().value -= 0.1f;

        GameManager.audioManager.PlaySound(AudioManager.Sounds.MENU_SLIDER);

        if (slider == m_SFXSlider)
        {
            GameManager.audioManager.m_Volume = slider.GetComponent<Slider>().value;
        }
        else
        {
            GameManager.audioManager.SetMusicVolume(slider.GetComponent<Slider>().value);
        }
    }

    protected override void OnClick(Controllers controller)
    {
        base.OnClick(controller);
    }

    public void ChangeLockFromBButton(Controllers controller)
    {
        GameManager.audioManager.PlaySound(AudioManager.Sounds.MENU_CONFIRM);
        SetLockedButton(false);
    }
}
