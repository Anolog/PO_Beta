using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PauseMenuBehaviour : UIBehaviour {

    [SerializeField]
    public GameObject m_SettingsMenu;

    [SerializeField]
    protected GameObject m_ControlsMenu;

    [SerializeField]
    protected GameObject m_ConfirmationMenu;

    protected override void OnEnable()
    {
        if (m_GameManager == null)
        {
            m_GameManager = GameObject.FindGameObjectWithTag("GameManager");
        }
        base.OnEnable();
    }

    protected override void Start()
    {
        base.Start();
        JoyStickUpdateTimer = Time.time + JoyStickUpdateTime;
    }

    public override void SetUpControls()
    {
        if (m_PlayerInput == null)
        {
            return;
        }

        m_PlayerInput.HandleDPadDown = MoveToNextButton;
        m_PlayerInput.HandleDPadUp = MoveToPreviousButton;
        m_PlayerInput.HandleLeftStick = MoveToButtonStick;
        m_PlayerInput.HandleStart = Command.HandlePause;
        m_PlayerInput.HandleAButton = OnClick;
    }

    protected override void OnClick(Controllers controller)
    {
        if (m_CurrentSelectedButton == transform.Find("Controls").GetComponent<Button>())
        {
            GameManager.audioManager.PlaySound(AudioManager.Sounds.MENU_CONFIRM);
            GoToControls(controller);
        }
        else if (m_CurrentSelectedButton == transform.Find("ResumeButton").GetComponent<Button>())
        {
            GameManager.audioManager.PlaySound(AudioManager.Sounds.MENU_BACK);
            ReturnToGame(controller);
        }
        base.OnClick(controller);
    }

    public override void RemoveControls()
    {
        m_PlayerInput.HandleDPadDown = null;
        m_PlayerInput.HandleDPadUp = null;
        m_PlayerInput.HandleLeftStick = null;
        m_PlayerInput.HandleStart = null;
        m_PlayerInput.HandleAButton = null;
    }

    public void ReturnToGame(Controllers controller)
    {
        Command.HandlePause(controller);
    }

    public void GoToSettings()
    {
        gameObject.SetActive(false);
        m_SettingsMenu.gameObject.SetActive(true);
    }

    public void GoToControls(Controllers controller)
    {
        gameObject.SetActive(false);
        m_ControlsMenu.gameObject.SetActive(true);
        m_ControlsMenu.gameObject.GetComponent<ControlsBehaviour>().AllowController(controller);
    }

    public void GoToConfirmation()
    {
        Button callingButton = m_CurrentSelectedButton;
        gameObject.SetActive(false);
        m_ConfirmationMenu.gameObject.SetActive(true);
        m_ConfirmationMenu.gameObject.GetComponent<ConfirmationMenuBehaviour>().callingButton = callingButton;
    }

    //public void EndGame()
    //{
    //    for (int i = 0; i < GameManager.playerManager.PlayerList().Count; i++)
    //    {
    //        GameManager.playerManager.PlayerList()[i].SetActive(false);
    //    }

    //    //GameManager.audioManager.PlayMusic(AudioManager.Sounds.MUSIC_POST_GAME, 2);
    //    GameManager.sceneManager.SwitchScenes("PostGame_Scene");
    //}

    public void ExitToDesktop()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }
}
