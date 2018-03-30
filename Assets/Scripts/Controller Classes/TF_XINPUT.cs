using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System;

[FlagsAttribute]
public enum GAMEPAD_BUTTONS
{
    DPAD_UP = 0x0001,
    DPAD_DOWN = 0x0002,
    DPAD_LEFT = 0x0004,
    DPAD_RIGHT = 0x0008,
    START = 0x0010,
    BACK = 0x0020,
    LEFT_THUMB_CLICK = 0x0040,
    RIGHT_THUMB_CLICK = 0x0080,
    LEFT_BUMPER = 0x0100,
    RIGHT_BUMPER = 0x0200,
    A_BUTTON = 0x1000,
    B_BUTTON = 0x2000,
    X_BUTTON = 0x4000,
    Y_BUTTON = 0x8000,
};

enum THRESHOLDS
{
    XINPUT_GAMEPAD_LEFT_THUMB_DEADZONE = 7849,
    XINPUT_GAMEPAD_RIGHT_THUMB_DEADZONE = 8689,
    XINPUT_GAMEPAD_TRIGGER_THRESHOLD = 30,
}

public enum BUTTON_STATES
{
    UP,
    DOWN,
    PRESSED,
    RELEASED,
}

[StructLayout(LayoutKind.Sequential)]
public struct XINPUT_VIBRATION
{
    public ushort wLeftMotorSpeed;
    public ushort wRightMotorSpeed;
}


[StructLayout(LayoutKind.Sequential)]
public struct XINPUT_GAMEPAD
{
    public ushort wButtons;
    public byte bLeftTrigger;
    public byte bRightTrigger;
    public short sThumbLX;
    public short sThumbLY;
    public short sThumbRX;
    public short sThumbRY;
}

[StructLayout(LayoutKind.Sequential)]
public struct XINPUT_STATE
{
    public uint dwPacketNumber;
    public XINPUT_GAMEPAD Gamepad;
}

[StructLayout(LayoutKind.Sequential)]
public struct XINPUT_CAPABILITIES
{
    public byte Type;
    public byte SubType;
    ushort Flags;
    XINPUT_GAMEPAD Gamepad;
    XINPUT_VIBRATION Vibration;
}

public class GamePad
{
    public XINPUT_GAMEPAD Controls;
    public ushort JustPressedButtons;
    public ushort JustReleasedButtons;
    public uint PacketID;

    public float RightStickXSensitivity = 1.5f;
    public float RightStickYSensitivity = 0.02f;

    //will report multiple states if applicable
    public bool GetButton(GAMEPAD_BUTTONS button, BUTTON_STATES state)
    {
        if (state == BUTTON_STATES.DOWN)
            return (Controls.wButtons & (ushort)button) != 0;
        else if (state == BUTTON_STATES.PRESSED)
            return (JustPressedButtons & (ushort)button) != 0;
        else if (state == BUTTON_STATES.RELEASED)
            return (JustReleasedButtons & (ushort)button) != 0;
        else if (state == BUTTON_STATES.UP)
            return (~Controls.wButtons & (ushort)button) != 0;
        else
            return false; //condition can't be hit, but compiler doesn't know that

    }

    //can only return a single state
    public BUTTON_STATES GetButtonState(GAMEPAD_BUTTONS button)
    {
        if ((JustPressedButtons & (ushort)button) != 0)
            return BUTTON_STATES.PRESSED;
        else if ((JustReleasedButtons & (ushort)button) != 0)
            return BUTTON_STATES.RELEASED;
        else if ((Controls.wButtons & (ushort)button) != 0)
            return BUTTON_STATES.DOWN;
        else
            return BUTTON_STATES.UP;
    }

    public Vector2 LeftStick()
    {
        float xVal;
        float yVal;
        short xStick = Controls.sThumbLX;
        short yStick = Controls.sThumbLY;
        ushort Dead = (ushort)THRESHOLDS.XINPUT_GAMEPAD_LEFT_THUMB_DEADZONE;

        if (Mathf.Abs(xStick) > Dead)
            xVal = (float)(Mathf.Abs(xStick) - Dead) / (short.MaxValue - Dead) * Mathf.Sign(xStick);
        else
            xVal = 0.0f;

        if (Mathf.Abs(yStick) > Dead)
            yVal = (float)(Mathf.Abs(yStick) - Dead) / (short.MaxValue - Dead) * Mathf.Sign(yStick);
        else
            yVal = 0.0f;

        return new Vector2(xVal, yVal);
    }

    public Vector2 RightStick()
    {
        float xVal;
        float yVal;
        short xStick = Controls.sThumbRX;
        short yStick = Controls.sThumbRY;
        ushort Dead = (ushort)THRESHOLDS.XINPUT_GAMEPAD_RIGHT_THUMB_DEADZONE;

        if (Mathf.Abs(xStick) > Dead)
            xVal = (float)(Mathf.Abs(xStick) - Dead) / (short.MaxValue - Dead) * Mathf.Sign(xStick);
        else
            xVal = 0.0f;

        if (Mathf.Abs(yStick) > Dead)
            yVal = (float)(Mathf.Abs(yStick) - Dead) / (short.MaxValue - Dead) * Mathf.Sign(yStick);
        else
            yVal = 0.0f;

        return new Vector2(xVal, yVal);
    }
    public float LeftTrigger()
    {
        return (float)(Controls.bLeftTrigger - (byte)THRESHOLDS.XINPUT_GAMEPAD_TRIGGER_THRESHOLD) / (byte.MaxValue - (byte)THRESHOLDS.XINPUT_GAMEPAD_TRIGGER_THRESHOLD);
    }

    public float RightTrigger()
    {
        return (float)(Controls.bRightTrigger - (byte)THRESHOLDS.XINPUT_GAMEPAD_TRIGGER_THRESHOLD) / (byte.MaxValue - (byte)THRESHOLDS.XINPUT_GAMEPAD_TRIGGER_THRESHOLD);
    }
}

public class TF_XINPUT : MonoBehaviour
{
    [DllImport("XInputWrapper")]
    private static extern uint XInputGetStateWrapper(uint playerIndex, ref XINPUT_STATE controllerState);

    [DllImport("XInputWrapper")]
    private static extern uint XInputSetStateWrapper(uint index, ref XINPUT_VIBRATION vibration);

    GamePad[] controllers = new GamePad[4];
    GameManager m_GameManager;

    public void Awake()
    {
        XINPUT_STATE state = new XINPUT_STATE();

        //check all controllers
        for (byte i = 0; i < 4; i++)
        {
            //if that controller is connected
            if (XInputGetStateWrapper(i, ref state) == 0)
            {
                if (controllers[i] == null)
                {
                    controllers[i] = new GamePad();
                }
            }
        }

        GameObject gameManager = GameObject.Find("GameManager");
        if (gameManager != null)
        {
            m_GameManager = gameManager.GetComponent<GameManager>();
        }
        else
        {
            m_GameManager = GameObject.Find("GameManager(Clone)").GetComponent<GameManager>();
        }
    }

    // Update is called once per frame

    private void OnDestroy()
    {
        for (int i = 0; i < 4; i++)
        {
            if (controllers[i] != null)
            {
                Rumble((Controllers)i, 0, 0);
            }
        }
    }

    void Update()
    {
        XINPUT_STATE state = new XINPUT_STATE();

        //check all controllers
        for (byte i = 0; i < 4; i++)
        {
            //if that controller is connected
            if (XInputGetStateWrapper(i, ref state) == 0)
            {
                if (controllers[i] == null)
                {
                    controllers[i] = new GamePad();
                }

                ushort newButtons = state.Gamepad.wButtons;

                controllers[i].JustPressedButtons = (ushort)(~controllers[i].Controls.wButtons & newButtons);
                controllers[i].JustReleasedButtons = (ushort)(controllers[i].Controls.wButtons & ~newButtons);
                controllers[i].Controls = state.Gamepad;
                controllers[i].PacketID = state.dwPacketNumber;
            }
            else
            {
                controllers[i] = null;
            }
        }
    }

    //Player number and controller number do not neccesarily correspond, get controller by controller instead
    public GamePad GetController(Players player)
    {
        return controllers[(int)player];
    }

    public GamePad GetController(Controllers control)
    {
        if (control == Controllers.KEYBOARD_MOUSE)
        {
            Debug.Log("Can't get Keyboard from XInput");
            return null;
        }

        return controllers[(int)control];
    }

    public bool IsControllerConnected(Controllers control)
    {
        if (control == Controllers.KEYBOARD_MOUSE)
            return true;

        if (controllers[(int)control] != null)
            return true;

        return false;
    }

    // Handle Controller Input
    // Delegates
    //public delegate void HandleButtonPress(Controllers gamePad);
    //public delegate void HandleTriggerPress(Controllers gamePad, float trigger);
    //public delegate void HandleJoyStick(Controllers gamePad, Vector2 joystick);
    //// Delegate Functions
    //public HandleButtonPress HandleAButton, 
    //                         HandleXButton,
    //                         HandleYButton,
    //                         HandleBButton,
    //                         HandleLeftBumper,
    //                         HandleRightBumper,
    //                         HandleStart,
    //                         HandleBack,
    //                         HandleDPadUp,
    //                         HandleDPadDown,
    //                         HandleDPadLeft,
    //                         HandleDPadRight,
    //                         HandleLeftThumbClick,
    //                         HandleRightThumbClick;

    //public HandleTriggerPress HandleLeftTrigger,
    //                          HandleRightTrigger;

    //public HandleJoyStick HandleRightStick,
    //                      HandleLeftStick;

    public void HandleControllerInput()
    {   
        int i = 0;
        foreach (GamePad gamePad in controllers)
        {
            if (gamePad == null)
            {
                continue;
            }

            Controllers controller;
            i++;
            switch (i)
            {
                case 1:
                    controller = Controllers.GAMEPAD_1;
                    break;
                case 2:
                    controller = Controllers.GAMEPAD_2;
                    break;
                case 3:
                    controller = Controllers.GAMEPAD_3;
                    break;
                case 4:
                    controller = Controllers.GAMEPAD_4;
                    break;
                default:
                    Debug.Log("Trying to get input from non-existant controller");
                    controller = Controllers.GAMEPAD_1;
                    break;
            }

            #region HandleButtonPress
            if (gamePad.GetButton(GAMEPAD_BUTTONS.A_BUTTON, BUTTON_STATES.PRESSED))
            {
                if (m_GameManager.playerInput.HandleAButton != null)
                {
                    m_GameManager.playerInput.HandleAButton(controller);
                }
            }
            if (gamePad.GetButton(GAMEPAD_BUTTONS.X_BUTTON, BUTTON_STATES.PRESSED))
            {
                if (m_GameManager.playerInput.HandleXButton != null)
                {
                    m_GameManager.playerInput.HandleXButton(controller);
                }
            }
            if (gamePad.GetButton(GAMEPAD_BUTTONS.Y_BUTTON, BUTTON_STATES.PRESSED))
            {
                if (m_GameManager.playerInput.HandleYButton != null)
                {
                    m_GameManager.playerInput.HandleYButton(controller);
                }
            }
            if (gamePad.GetButton(GAMEPAD_BUTTONS.B_BUTTON, BUTTON_STATES.PRESSED))
            {
                if (m_GameManager.playerInput.HandleBButton != null)
                {
                    m_GameManager.playerInput.HandleBButton(controller);
                }
            }
            if (gamePad.GetButton(GAMEPAD_BUTTONS.LEFT_BUMPER, BUTTON_STATES.PRESSED))
            {
                if (m_GameManager.playerInput.HandleLeftBumper != null)
                {
                    m_GameManager.playerInput.HandleLeftBumper(controller);
                }
            }
            if (gamePad.GetButton(GAMEPAD_BUTTONS.RIGHT_BUMPER, BUTTON_STATES.PRESSED))
            {
                if (m_GameManager.playerInput.HandleRightBumper != null)
                {
                    m_GameManager.playerInput.HandleRightBumper(controller);
                }
            }
            if (gamePad.GetButton(GAMEPAD_BUTTONS.START, BUTTON_STATES.PRESSED))
            {
                if (m_GameManager.playerInput.HandleStart != null)
                {
                    m_GameManager.playerInput.HandleStart(controller);
                }
            }
            if (gamePad.GetButton(GAMEPAD_BUTTONS.BACK, BUTTON_STATES.PRESSED))
            {
                if (m_GameManager.playerInput.HandleBack != null)
                {
                    m_GameManager.playerInput.HandleBack(controller);
                }
            }
            if (gamePad.GetButton(GAMEPAD_BUTTONS.DPAD_UP, BUTTON_STATES.PRESSED))
            {
                if (m_GameManager.playerInput.HandleDPadUp != null)
                {
                    m_GameManager.playerInput.HandleDPadUp(controller);
                }
            }
            if (gamePad.GetButton(GAMEPAD_BUTTONS.DPAD_DOWN, BUTTON_STATES.PRESSED))
            {
                if (m_GameManager.playerInput.HandleDPadDown != null)
                {
                    m_GameManager.playerInput.HandleDPadDown(controller);
                }
            }
            if (gamePad.GetButton(GAMEPAD_BUTTONS.DPAD_LEFT, BUTTON_STATES.PRESSED))
            {
                if (m_GameManager.playerInput.HandleDPadLeft != null)
                {
                    m_GameManager.playerInput.HandleDPadLeft(controller);
                }
            }
            if (gamePad.GetButton(GAMEPAD_BUTTONS.DPAD_RIGHT, BUTTON_STATES.PRESSED))
            {
                if (m_GameManager.playerInput.HandleDPadRight != null)
                {
                    m_GameManager.playerInput.HandleDPadRight(controller);
                }
            }
            if (gamePad.GetButton(GAMEPAD_BUTTONS.LEFT_THUMB_CLICK, BUTTON_STATES.PRESSED))
            {
                if (m_GameManager.playerInput.HandleLeftThumbClick != null)
                {
                    m_GameManager.playerInput.HandleLeftThumbClick(controller);
                }
            }
            if (gamePad.GetButton(GAMEPAD_BUTTONS.RIGHT_THUMB_CLICK, BUTTON_STATES.PRESSED))
            {
                if (m_GameManager.playerInput.HandleRightThumbClick != null)
                {
                    m_GameManager.playerInput.HandleRightThumbClick(controller);
                }
            }
            #endregion

            #region HandleTriggers
            if (gamePad.LeftTrigger() > Mathf.Epsilon)
            {
                if (m_GameManager.playerInput.HandleLeftTrigger != null)
                {
                    m_GameManager.playerInput.HandleLeftTrigger(controller);
                }
            }
            if (gamePad.RightTrigger() > Mathf.Epsilon)
            {
                if (m_GameManager.playerInput.HandleRightTrigger != null)
                {
                    m_GameManager.playerInput.HandleRightTrigger(controller);
                }
            }
            #endregion

            #region HandleJoySticks
            if (gamePad.LeftStick().magnitude > Mathf.Epsilon)
            {
                if (m_GameManager.playerInput.HandleLeftStick != null)
                {
                    m_GameManager.playerInput.HandleLeftStick(controller, gamePad.LeftStick());
                }
            }
            if (gamePad.RightStick().magnitude > Mathf.Epsilon)
            {
                if (m_GameManager.playerInput.HandleRightStick != null)
                {
                    m_GameManager.playerInput.HandleRightStick(controller, gamePad.RightStick());
                }
            } 
            #endregion
        }
    }

    public void Rumble(Controllers control, float lowFreqMotor, float hiFreqMotor)
    {
        XINPUT_VIBRATION vibe = new XINPUT_VIBRATION();
        //enforce 0-1 range
        vibe.wLeftMotorSpeed = (ushort)(Mathf.Min(Mathf.Max(lowFreqMotor, 0.0f), 1.0f) * ushort.MaxValue);
        vibe.wRightMotorSpeed = (ushort)(Mathf.Min(Mathf.Max(hiFreqMotor, 0.0f), 1.0f) * ushort.MaxValue);

        XInputSetStateWrapper((uint)control, ref vibe);
    }
}
