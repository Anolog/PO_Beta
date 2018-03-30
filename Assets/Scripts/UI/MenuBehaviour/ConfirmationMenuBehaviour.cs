using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConfirmationMenuBehaviour : UIBehaviour {

    [SerializeField]
    protected GameObject m_MainMenu;

    public Button callingButton;

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

    public override void RemoveControls()
    {
        m_PlayerInput.HandleDPadDown = null;
        m_PlayerInput.HandleDPadUp = null;
        m_PlayerInput.HandleLeftStick = null;
        m_PlayerInput.HandleStart = null;
        m_PlayerInput.HandleAButton = null;
    }

    public void Confirm()
    {
        if (callingButton.gameObject.name == "ExitButton")
        {
            EndGame();
        }
        else
        {
            ExitToDesktop();
        }
    }

    public void GoBack()
    {
        gameObject.SetActive(false);
        m_MainMenu.gameObject.SetActive(true);
    }

    public void EndGame()
    {
        for (int i = 0; i < GameManager.playerManager.PlayerList().Count; i++)
        {
            GameManager.playerManager.PlayerList()[i].SetActive(false);
        }
        Time.timeScale = 1;
        GameManager.audioManager.PlayMusic(AudioManager.Sounds.MUSIC_POST_GAME, 2);
        GameManager.sceneManager.SwitchScenes("PostGame_Scene");
    }

    public void ExitToDesktop()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }
}
