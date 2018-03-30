using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuBehaviour : UIBehaviour {

    [SerializeField]
    private Canvas m_OptionsScreen;

    [SerializeField]
    private Canvas m_ControlsScreen;

    [SerializeField]
    private Canvas m_CreditsScreen;

    protected override void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        //Play Main Menu Music
        GameManager.audioManager.PlayMusic(AudioManager.Sounds.MUSIC_MAIN_MENU, 1);

        base.Start();
    }

    public override void SetUpControls()
    {
        //This stops the error from showing, so I guess we need it at every
        //SetUpControls()...

        if (m_PlayerInput == null)
        {
            return;
        }

        m_PlayerInput.HandleDPadUp = MoveToPreviousButton;
        m_PlayerInput.HandleDPadDown = MoveToNextButton;
        m_PlayerInput.HandleAButton = OnClick;
        m_PlayerInput.HandleLeftStick = MoveToButtonStick;
    }

    public override void RemoveControls()
    {
        m_PlayerInput.HandleDPadUp = null;
        m_PlayerInput.HandleDPadDown = null;
        m_PlayerInput.HandleAButton = null;
        m_PlayerInput.HandleLeftStick = null;
    }
    
    //The order here matters.
    //Disable the object you're on first so that it hits the OnDisable() function
    //Then enable the other function so it goes to it's OnEnable() function
    //This way the player input doesn't get confused
    public void SetUpOptionsMenu()
    {
        gameObject.SetActive(false);
        m_OptionsScreen.gameObject.SetActive(true);     
    }

    public void SetUpControlsMenu(Controllers controller)
    {
        gameObject.SetActive(false);
        m_ControlsScreen.gameObject.SetActive(true);
        m_ControlsScreen.gameObject.GetComponent<ControlsBehaviour>().AllowController(controller);
    }

    protected override void OnClick(Controllers controller)
    {
        if (m_CurrentSelectedButton == transform.Find("Controls").GetComponent<Button>())
        {
            GameManager.audioManager.PlaySound(AudioManager.Sounds.MENU_CONFIRM);
            SetUpControlsMenu(controller);
        }

        //Disable stuff when going into main scene
        if (m_CurrentSelectedButton == transform.Find("Play_Button").GetComponent<Button>())
        {
            GameManager.audioManager.PlaySound(AudioManager.Sounds.MENU_CONFIRM);
            
            //Disable buttons
            for (int i = 0; i < m_ButtonList.Count; i++)
            {
                m_ButtonList[i].gameObject.SetActive(false);
            }
        }

        base.OnClick(controller);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }

    public void SetUpCreditsScreen()
    {
        gameObject.SetActive(false);
        m_CreditsScreen.gameObject.SetActive(true);
    }
}
