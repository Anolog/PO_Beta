using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionsMenuBehaviour : UIBehaviour
{
    //[SerializeField]
    //private Canvas m_GraphicsScreen;
    //[SerializeField]
    //private Canvas m_AudioScreen;
    //[SerializeField]
    //private Canvas m_MainMenuScreen;

    public GameObject m_GraphicsScreen;
    public GameObject m_AudioScreen;
    public GameObject m_MainMenuScreen;

    protected override void Start()
    {
        base.Start();
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

    public void SetupGraphicsMenu()
    {
        gameObject.SetActive(false);
        m_GraphicsScreen.SetActive(true);
    }

    public void SetupAudioMenu()
    {
        //Debug.Log("SetupAudioMenu");
        gameObject.SetActive(false);
        m_AudioScreen.SetActive(true);
    }

    protected override void OnClick(Controllers controller)
    {
        base.OnClick(controller);
    }

    public void SetupMainMenu()
    {
        gameObject.SetActive(false);
        m_MainMenuScreen.SetActive(true);  
    }
}
