using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerInput : MonoBehaviour
{

    // Delegates
    public delegate void HandleButtonPress(Controllers gamePad);
    public delegate void HandleJoyStick(Controllers gamePad, Vector2 joystick);
    // Delegate Functions
    public HandleButtonPress HandleAButton,
                             HandleXButton,
                             HandleYButton,
                             HandleBButton,
                             HandleLeftBumper,
                             HandleRightBumper,
                             HandleStart,
                             HandleBack,
                             HandleDPadUp,
                             HandleDPadDown,
                             HandleDPadLeft,
                             HandleDPadRight,
                             HandleLeftThumbClick,
                             HandleRightThumbClick,
                             HandleLeftMouseClick,
                             HandleRightMouseClick,
                             HandleLeftTrigger,
                             HandleRightTrigger;

    public HandleJoyStick HandleRightStick,
                          HandleLeftStick,
                          HandleMouseMove;

    private TF_XINPUT m_ControllerInput;

    //[SerializeField]
    //private float m_XRotationSensitivity = 120.0f;
    //[SerializeField]
    //private float m_YRotationSensitivity = 1.8f;

    public bool paused = false;

    int numControllers = 0;

    private bool m_CommandConsoleOpen = false;

#if UNITY_EDITOR
    //bool cursorVisible = false;
#endif
    //public delegate void XButtonPressEvent(System.EventArgs args);

    //XButtonPressEvent XButtonPress = new XButtonPressEvent(Command.UseAbility1);

    void Start()
    {
        m_ControllerInput = GameManager.controllerInput;
        if (m_ControllerInput != null)
        {
            for (int i = 0; i < 4; i++)
            {
                if (m_ControllerInput.IsControllerConnected((Controllers)i))
                {
                    numControllers++;
                }
            }
        }
    }

    void Awake()
    {
        /*
        m_ControllerInput = GameManager.controllerInput;
        if (m_ControllerInput != null)
        {
            for (int i = 0; i < 4; i++)
            {
                if (m_ControllerInput.IsControllerConnected((Controllers)i))
                {
                    numControllers++;
                }
            }
        }
        */
    }

    void Update()
    {
        HandleKeyboardInput(Controllers.KEYBOARD_MOUSE);
        if (numControllers > 0)
        {
            m_ControllerInput.HandleControllerInput();
        }
    }

    private void HandleKeyboardInput(Controllers controller)
    {
        Vector2 direction = Vector2.zero;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (HandleAButton != null)
            {
                HandleAButton(controller);
            }
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (HandleXButton != null)
            {
                HandleXButton(controller);
            }
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (HandleYButton != null)
            {
                HandleYButton(controller);
            }
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (HandleBButton != null)
            {
                HandleBButton(controller);
            }
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (HandleLeftBumper != null)
            {
                HandleLeftBumper(controller);
            }
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (HandleStart != null)
            {
                HandleStart(controller);
            }
        }
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (HandleBack != null)
            {
                HandleBack(controller);
            }
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            if (HandleDPadUp != null)
            {
                HandleDPadUp(controller);
            }
        }
        if (Input.GetKey(KeyCode.W))
        {
            if (HandleDPadUp == null)
            {
                direction.y += 1;
            }
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            if (HandleDPadDown != null)
            {
                HandleDPadDown(controller);
            }
        }
        if (Input.GetKey(KeyCode.S))
        {
            if (HandleDPadDown == null)
            {
                direction.y -= 1;
            }
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            if (HandleDPadLeft != null)
            {
                HandleDPadLeft(controller);
            }
        }
        if (Input.GetKey(KeyCode.A))
        {
            if (HandleDPadLeft == null)
            {
                direction.x -= 1;
            }
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            if (HandleDPadRight != null)
            {
                HandleDPadRight(controller);
            }
        }
        if (Input.GetKey(KeyCode.D))
        {
            if (HandleDPadRight == null)
            {
                direction.x += 1;
            }
        }

        if (Input.GetKeyDown(KeyCode.BackQuote))
        {
            if (GameObject.Find("Game") != null)
            {
                m_CommandConsoleOpen = !m_CommandConsoleOpen;
                Transform pauseUI = GameObject.Find("Game").GetComponent<Game>().PauseMenuUI.gameObject.transform;
                if (pauseUI.gameObject.activeInHierarchy)
                {
                    // If I am paused and I close the command console Give me my controls back
                    if (!m_CommandConsoleOpen)
                    {
                        if (pauseUI.Find("PauseMenu").gameObject.activeInHierarchy) { pauseUI.Find("PauseMenu").gameObject.GetComponent<PauseMenuBehaviour>().SetUpControls(); }
                        else if (pauseUI.Find("OptionsMenu").gameObject.activeInHierarchy) { pauseUI.Find("OptionsMenu").gameObject.GetComponent<OptionsMenuBehaviour>().SetUpControls(); }
                        else if (pauseUI.Find("GraphicsMenu").gameObject.activeInHierarchy) { pauseUI.Find("GraphicsMenu").gameObject.GetComponent<GraphicsMenuBehaviour>().SetUpControls(); }
                        else if (pauseUI.Find("AudioMenu").gameObject.activeInHierarchy) { pauseUI.Find("AudioMenu").gameObject.GetComponent<AudioMenuBehaviour>().SetUpControls(); }
                        else if (pauseUI.Find("ControlsMenu").gameObject.activeInHierarchy) { pauseUI.Find("ControlsMenu").gameObject.GetComponent<ControlsBehaviour>().SetUpControls(); }
                    }
                    // if I am paused and open the command console Remove my controls
                    else
                    {
                        if (pauseUI.Find("PauseMenu").gameObject.activeInHierarchy) { pauseUI.Find("PauseMenu").gameObject.GetComponent<PauseMenuBehaviour>().RemoveControls(); }
                        else if (pauseUI.Find("OptionsMenu").gameObject.activeInHierarchy) { pauseUI.Find("OptionsMenu").gameObject.GetComponent<OptionsMenuBehaviour>().RemoveControls(); }
                        else if (pauseUI.Find("GraphicsMenu").gameObject.activeInHierarchy) { pauseUI.Find("GraphicsMenu").gameObject.GetComponent<GraphicsMenuBehaviour>().RemoveControls(); }
                        else if (pauseUI.Find("AudioMenu").gameObject.activeInHierarchy) { pauseUI.Find("AudioMenu").gameObject.GetComponent<AudioMenuBehaviour>().RemoveControls(); }
                        else if (pauseUI.Find("ControlsMenu").gameObject.activeInHierarchy) { pauseUI.Find("ControlsMenu").gameObject.GetComponent<ControlsBehaviour>().RemoveControls(); }
                    }
                }
                else
                {
                    // if I am not paused and I close the command console Give me my controls back
                    if (!m_CommandConsoleOpen)
                    {
                        // if I used the command prompt to start the cutscene I need to make sure I give myself
                        // the cutscene controls back, not the in game controls
                        if (GameObject.Find("Game").GetComponent<Game>().inCutscene)
                        {
                            GameObject.Find("Game").GetComponent<Game>().SetUpCutSceneInput();
                        }
                        else
                        {
                            GameObject.Find("Game").GetComponent<Game>().SetUpInput();
                        }
                    }
                    // if I am not paused and I open the command console Remove my controls
                    else
                    {
                        GameObject.Find("Game").GetComponent<Game>().RemoveInput();
                    }
                }
            }
            else if (GameObject.Find("Loadout") != null)
            {
                m_CommandConsoleOpen = !m_CommandConsoleOpen;
                //GameObject.Find("EventSystem").GetComponent<StandaloneInputModule>().enabled = m_CommandConsoleOpen;

                if (GameObject.Find("Loadout").transform.Find("LoadoutCanvas") != null)
                {
                    // if I am in the loadout and I close the command console Give me my controls back
                    if (!m_CommandConsoleOpen)
                    {
                        GameObject.Find("Loadout").transform.Find("LoadoutCanvas").GetComponent<LoadoutBehaviour>().SetUpControls();
                    }
                    // if I am in the loadout and I open the command console Remove my controls
                    else
                    {
                        GameObject.Find("Loadout").transform.Find("LoadoutCanvas").GetComponent<LoadoutBehaviour>().RemoveControls();
                    }
                }
            }
        }

        if (HandleLeftStick != null)
        {
            if (direction.magnitude > Mathf.Epsilon)
            {
                HandleLeftStick(controller, direction);
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (HandleLeftMouseClick != null)
            {
                HandleLeftMouseClick(controller);
                if (Cursor.visible == true || Cursor.lockState != CursorLockMode.Locked)
                {
                    Cursor.visible = false;
                    Cursor.lockState = CursorLockMode.Locked;
                }
            }
        }
        if (Input.GetMouseButtonDown(1))
        {
            if (HandleRightMouseClick != null)
            {
                HandleRightMouseClick(controller);
            }
        }
        Vector2 mouseMove = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        if (mouseMove.magnitude > Mathf.Epsilon)
        {
            if (HandleMouseMove != null)
            {
                HandleMouseMove(controller, mouseMove);
            }
        }

#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.End))
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            //cursorVisible = true;
        }
#endif

        if ((Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) && Input.GetKey(KeyCode.F4))
        {
            Application.Quit();
        }

        //if (!player.GetComponent<PlayerStats>().isDowned)
        //{
        //    if (Input.GetKeyDown(KeyCode.Space))
        //    {
        //        Command.Jump(player);
        //    }
        //    if (!paused)
        //    {
        //        if (Input.GetKeyDown(KeyCode.Q))
        //        {
        //            Command.UseAbility1(player);
        //        }
        //        if (Input.GetKeyDown(KeyCode.E))
        //        {
        //            Command.UseAbility2(player);
        //        }
        //        if (Input.GetKeyDown(KeyCode.R))
        //        {
        //            Command.UseAbility3(player);
        //        }
        //        if (Input.GetKeyDown(KeyCode.F))
        //        {
        //            Command.UseRevive(player);
        //        }
        //    }
        //    if (Input.GetKeyDown(KeyCode.Escape))
        //    {
        //        if (paused == false)
        //        {
        //            Cursor.visible = true;
        //            Cursor.lockState = CursorLockMode.None;
        //            Command.Pause(GameObject.Find("Game").GetComponent<Game>().PauseMenuUI);
        //            paused = true;
        //        }
        //        else
        //        {
        //            Cursor.visible = false;
        //            Cursor.lockState = CursorLockMode.Locked;
        //            Command.UnPause(GameObject.Find("Game").GetComponent<Game>().PauseMenuUI);
        //            paused = false;
        //        }
        //    }
        //    #if UNITY_EDITOR
        //    if (Input.GetKeyDown(KeyCode.End))
        //    {
        //        Cursor.visible = true;
        //        Cursor.lockState = CursorLockMode.None;
        //        cursorVisible = true;
        //    }
        //    #endif
        //        if (!paused)
        //    {
        //        if (Input.GetMouseButtonDown(1))
        //        {
        //            Command.UseAbility4(player); // This is defensive player ability
        //        }
        //        if (Input.GetMouseButtonDown(0))
        //        {
        //            Command.UseAttackAbility(player);
        //            #if UNITY_EDITOR
        //            if (cursorVisible == true)
        //            {
        //                Cursor.visible = false;
        //                Cursor.lockState = CursorLockMode.Locked;
        //            }
        //            #endif
        //        }
        //    }
        //    // Find the Direction to move
        //    if (Input.GetKey(KeyCode.W))
        //    {
        //        direction.y += 1;
        //    }
        //    if (Input.GetKey(KeyCode.S))
        //    {
        //        direction.y -= 1;
        //    }
        //    if (Input.GetKey(KeyCode.A))
        //    {
        //        direction.x -= 1;
        //    }
        //    if (Input.GetKey(KeyCode.D))
        //    {
        //        direction.x += 1;
        //    }

        //    if (direction.magnitude > Mathf.Epsilon)
        //    {
        //        Command.Move(player, direction);
        //    }
        //}
        //if (Mathf.Abs(Input.GetAxis("Mouse X")) > 0)
        //{
        //    Command.RotateHorizontal(player, -Input.GetAxis("Mouse X") * m_XRotationSensitivity);
        //}
        //if (Mathf.Abs(Input.GetAxis("Mouse Y")) > 0)
        //{
        //    Command.RotateCameraVertical(player, -Input.GetAxis("Mouse Y") * m_YRotationSensitivity);
        //}
    }

    private void HandleControllerInput(GameObject player, GamePad aGamePad)
    {
        //if (!player.GetComponent<PlayerStats>().isDowned)
        //{
        //    if (aGamePad.GetButton(GAMEPAD_BUTTONS.A_BUTTON, BUTTON_STATES.PRESSED))
        //    {
        //        Command.Jump(player);
        //    }
        //    if (!paused)
        //    {
        //        if (aGamePad.GetButton(GAMEPAD_BUTTONS.X_BUTTON, BUTTON_STATES.PRESSED))
        //        {
        //            Command.UseAbility1(player);
        //        }
        //        if (aGamePad.GetButton(GAMEPAD_BUTTONS.Y_BUTTON, BUTTON_STATES.PRESSED))
        //        {
        //            Command.UseAbility2(player);
        //        }
        //        if (aGamePad.GetButton(GAMEPAD_BUTTONS.B_BUTTON, BUTTON_STATES.PRESSED))
        //        {
        //            Command.UseAbility3(player);
        //        }
        //        if (aGamePad.GetButton(GAMEPAD_BUTTONS.LEFT_BUMPER, BUTTON_STATES.PRESSED) || aGamePad.GetButton(GAMEPAD_BUTTONS.RIGHT_BUMPER, BUTTON_STATES.PRESSED))
        //        {
        //            Command.UseRevive(player);
        //        }
        //    }
        //    if (aGamePad.GetButton(GAMEPAD_BUTTONS.START, BUTTON_STATES.PRESSED))
        //    {
        //        if (!paused)
        //        {
        //            Command.Pause(GetComponent<Game>().PauseMenuUI);
        //            paused = true;
        //        }
        //        else
        //        {
        //            Command.UnPause(GetComponent<Game>().PauseMenuUI);
        //            paused = false;
        //        }
        //    }
        //    if (!paused)
        //    {
        //        if (aGamePad.LeftTrigger() > Mathf.Epsilon)
        //        {
        //            Command.UseAbility4(player); // This is defensive player ability
        //        }
        //        if (aGamePad.RightTrigger() > Mathf.Epsilon)
        //        {
        //            Command.UseAttackAbility(player);
        //        }
        //    }
        //    if (aGamePad.LeftStick().magnitude > Mathf.Epsilon)
        //    {
        //        Command.Move(player, aGamePad.LeftStick());
        //    }

            
        //}
        //if (Mathf.Abs(aGamePad.RightStick().x) > Mathf.Epsilon)
        //{
        //    Command.RotateConstSpeedHorizontal(player, aGamePad.RightStick().x * aGamePad.RightStickXSensitivity);
        //}
        //if (Mathf.Abs(aGamePad.RightStick().y) > Mathf.Epsilon)
        //{
        //    Command.RotatePlayerCameraVertical(player, aGamePad.RightStick().y * aGamePad.RightStickYSensitivity);
        //}
    }
}