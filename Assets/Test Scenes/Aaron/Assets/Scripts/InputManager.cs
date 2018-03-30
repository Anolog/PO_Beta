
using System;
using System.Collections.Generic;
using UnityEngine;

#region Public Enumerators

public enum ControllerType
{
    GamePad,
    Mouse,
    Keyboard,
    None,
}

public enum InputState
{
    //First Pressed
    Pressed,
    //If continued to be pressed
    Held,
    //Frame when it is released
    Released,
    //Not touched
    Up,
    //If we have disable/enable feature
    Disabled,
    //Nothing at all, for inputs only reliant on Value
    None,
}

public enum ControllerInputType
{
    LeftJoyStick,
    RightJoyStick,
    LeftTrigger,
    RightTrigger,
    Button,
}

public enum InputAxis
{
    X,
    Y,
    None,
}

#endregion

public class InputManager : MonoBehaviour
{
    TF_XINPUT m_WrapperInput;

    private Dictionary<string, CustomInput> m_Inputs = new Dictionary<string, CustomInput>();
    private KeyCode[] m_ModifierKeys =
        {
            KeyCode.None,
            KeyCode.LeftAlt,
            KeyCode.RightAlt,
            KeyCode.LeftShift,
            KeyCode.RightShift,
            KeyCode.LeftControl,
            KeyCode.RightControl,
            KeyCode.LeftCommand,
            KeyCode.RightCommand
        };

    #region Setters and Getters
    public TF_XINPUT Wrapper { get { return m_WrapperInput; } }
    #endregion  

    private void Start()
    {
        m_WrapperInput = gameObject.AddComponent<TF_XINPUT>();

        TestSetup();
    }

    private void TestSetup()
    {
        {
            InputSet setOne = CreateKeyboardSet(KeyCode.Space, KeyCode.None);
            InputSet setTwo = CreateEmptySet();

            //this is generating an unused local variable warning
            //bool test = 
            AddAndCreateInput(setOne, setTwo, ControllerType.Keyboard, "Fire");
        }

        {
            InputSet setOne = CreateControllerButtonSet(Players.PLAYER_ONE, GAMEPAD_BUTTONS.A_BUTTON);
            InputSet setTwo = CreateEmptySet();

            AddAndCreateInput(setOne, setTwo, ControllerType.GamePad, "Controller_Fire");
        }
    }

    private void Update()
    {
        foreach (KeyValuePair<string, CustomInput> input in m_Inputs)
        {
            input.Value.Update();
        }

        Debug.Log("Fire State" + GetState("Fire").ToString());
        Debug.Log("Controller_Fire State" + GetState("Controller_Fire").ToString());
    }

    #region Input Creation Methods

    public bool AddInput(CustomInput aInput)
    {
        Debug.Log(m_Inputs.ContainsKey(aInput.Name));

        if (m_Inputs.ContainsKey(aInput.Name))
            return false;

        m_Inputs.Add(aInput.Name, aInput);

        return true;
    }

    public CustomInput CreateInput(InputSet positiveInput, InputSet negativeInput, ControllerType aType, string aName)
    {
        CustomInput newInput = new CustomInput(positiveInput, negativeInput, aType, aName, this);
        return newInput;
    }

    public bool AddAndCreateInput(InputSet positiveInput, InputSet negativeInput, ControllerType aType, string aName)
    {
        return AddInput(CreateInput(positiveInput, negativeInput, aType, aName));
    }
    #endregion

    #region Input and Set Get Methods
    public CustomInput GetInput(string aName)
    {
        if (!m_Inputs.ContainsKey(aName))
            return null;

        return m_Inputs[aName];
    }
    public InputSet GetInputSet(string aName, bool isPositiveSet)
    {
        if (!m_Inputs.ContainsKey(aName))
            return null;

        if (isPositiveSet)
        {
            return m_Inputs[aName].PositiveSet;
        }

        return m_Inputs[aName].NegativeSet;
    }
    #endregion 

    #region Set Creation Methods

    public InputSet CreateKeyboardSet(KeyCode aKey, KeyCode aMod)
    {
        if (!CheckModifierKey(aMod))
        {
            return null;
        }
        InputSet keyboardSet = new KeyboardInput(aKey, aMod);
        return keyboardSet;
    }

    public InputSet CreateEmptySet()
    {
        InputSet emptySet = new EmptyInput();
        return emptySet;
    }

    public InputSet CreateMouseSet(InputAxis aAxis)
    {
        InputSet mouseSet = new MouseInput(aAxis);
        return mouseSet;
    }

    public InputSet CreateControllerLeftTriggerSet(Players aPlayer)
    {
        InputSet controllerSet = new ControllerInput(aPlayer, ControllerInputType.LeftTrigger, GAMEPAD_BUTTONS.A_BUTTON, this, InputAxis.None);
        return controllerSet;
    }

    public InputSet CreateControllerRightTriggerSet(Players aPlayer)
    {
        InputSet controllerSet = new ControllerInput(aPlayer, ControllerInputType.RightTrigger, GAMEPAD_BUTTONS.A_BUTTON, this, InputAxis.None);
        return controllerSet;
    }

    public InputSet CreateControllerRightStickSet(Players aPlayer, InputAxis aAxis)
    {
        InputSet controllerSet = new ControllerInput(aPlayer, ControllerInputType.RightJoyStick, GAMEPAD_BUTTONS.A_BUTTON, this, aAxis);
        return controllerSet;
    }

    public InputSet CreateControllerLeftStickSet(Players aPlayer, InputAxis aAxis)
    {
        InputSet controllerSet = new ControllerInput(aPlayer, ControllerInputType.LeftJoyStick, GAMEPAD_BUTTONS.A_BUTTON, this, aAxis);
        return controllerSet;
    }

    public InputSet CreateControllerButtonSet(Players aPlayer, GAMEPAD_BUTTONS aButton)
    {
        InputSet controllerSet = new ControllerInput(aPlayer, ControllerInputType.Button, aButton, this, InputAxis.None);
        return controllerSet;
    }

    #endregion

    #region Get Input Info
    public InputState GetState(string aName)
    {
        return m_Inputs[aName].currentState;
    }
    public float GetValue(string aName)
    {
        return m_Inputs[aName].Value;
    }
    public ControllerType GetControllerType(string aName)
    {
        return m_Inputs[aName].Controller;
    }
    #endregion

    private bool CheckModifierKey(KeyCode aKey)
    {
        for (int i = 0; i < m_ModifierKeys.Length; i++)
        {
            if (aKey == m_ModifierKeys[i])
                return true;
        }

        return false;
    }
}

//Any state will be gotten from the positive
public class CustomInput
{
    private float m_Value = 0.0f;
    private string m_Name;
    private InputSet m_PositiveSet;
    private InputSet m_NegativeSet;
    private ControllerType m_Controller;
    //unused variable warning
    //private InputManager m_InputManager;

    #region Setters and Getters
    public float Value { get { return m_Value; } private set { m_Value = value; } }
    public string Name { get { return m_Name; } private set { m_Name = value; } }
    public InputSet PositiveSet { get { return m_PositiveSet; } }
    public InputSet NegativeSet { get { return m_NegativeSet; } }
    public ControllerType Controller { get { return m_Controller; } }
    public InputState currentState { get { return m_PositiveSet.CurrentState; } }
    #endregion

    public CustomInput(InputSet aPositive, InputSet aNegative, ControllerType aType, string aName, InputManager aIM)
    {
        //m_InputManager = aIM;
        m_PositiveSet = aPositive;
        m_NegativeSet = aNegative;
        Name = aName;
        //m_InputManager = aIM;
        m_Controller = aType;
    }

    public void Update()
    {
        m_PositiveSet.Update();
        m_NegativeSet.Update();

        if (m_Controller == ControllerType.Keyboard)
        {
            m_Value = m_PositiveSet.Value + -m_NegativeSet.Value;
        }
        else
        {
            m_Value = m_PositiveSet.Value;
        }
    }
}

#region InputSets

public abstract class InputSet
{
    protected InputState m_CurrentState = InputState.None;
    protected float m_Value = 0.0f;
    protected InputManager m_InputManager;
    protected InputAxis m_Axis = InputAxis.None;

    #region Setters and Getters
    public InputState CurrentState { get { return m_CurrentState; } protected set { m_CurrentState = value; } }
    public float Value { get { return m_Value; } protected set { m_Value = value; } }
    public InputAxis Axis { get { return m_Axis; } set { m_Axis = value; } }
    #endregion

    public abstract void Update();

    public void SetInputManager(InputManager aIM)
    {
        m_InputManager = aIM;
    }
}

public class KeyboardInput : InputSet
{
    private KeyCode m_Key;
    private KeyCode m_ModifierKey;

    #region Setters and Getters
    public KeyCode Key { get { return m_Key; } set { m_Key = value; } }
    public KeyCode ModifierKey { get { return m_ModifierKey; } set { m_ModifierKey = value; } }
    #endregion

    public override void Update()
    {
        if (CheckModifierKey())
        {
            if (Input.GetKeyUp(m_Key))
            {
                Value = 1.0f;
                CurrentState = InputState.Released;
            }
            else if (Input.GetKeyDown(m_Key))
            {
                Value = 1.0f;
                CurrentState = InputState.Pressed;
            }
            else if (Input.GetKey(m_Key))
            {
                Value = 1.0f;
                CurrentState = InputState.Held;
            }
            else
            {
                Value = 0.0f;
                CurrentState = InputState.Up;
            }
        }
    }

    private bool CheckModifierKey()
    {
        if (m_ModifierKey == KeyCode.None)
            return true;

        if (Input.GetKey(m_ModifierKey) || Input.GetKeyDown(m_ModifierKey) || Input.GetKeyUp(m_ModifierKey))
            return true;

        return false;
    }

    public KeyboardInput(KeyCode aKey, KeyCode aModKey)
    {
        m_Key = aKey;
        m_ModifierKey = aModKey;
    }
}

public class ControllerInput : InputSet
{
    private GamePad m_GamePad;
    private Players m_Player;
    private ControllerInputType m_InputType;
    private GAMEPAD_BUTTONS m_Button;

    #region Setters and Getters
    public GamePad gamePad { get { return m_GamePad; } set { m_GamePad = value; } }
    public Players player { get { return m_Player; } set { m_Player = value; } }
    public ControllerInputType inputType { get { return m_InputType; } set { m_InputType = value; } }
    public GAMEPAD_BUTTONS gamepadButton { get { return m_Button; } set { m_Button = value; } }
    #endregion

    public ControllerInput(Players aPlayer, ControllerInputType aType, GAMEPAD_BUTTONS aButton, InputManager aIM, InputAxis aAxis)
    {
        m_Player = aPlayer;
        m_InputType = aType;
        m_Button = aButton;
        SetInputManager(aIM);
        Axis = aAxis;
        m_GamePad = aIM.Wrapper.GetController(aPlayer);
    }

    public override void Update()
    {
        if (m_InputType == ControllerInputType.Button)
        {
            if (m_GamePad == null)
                Debug.Log("Gamepad Null");

            if (m_GamePad.GetButtonState(m_Button) == BUTTON_STATES.DOWN)
            {
                m_CurrentState = InputState.Held;
                m_Value = 1;
            }
            else if (m_GamePad.GetButtonState(m_Button) == BUTTON_STATES.PRESSED)
            {
                m_CurrentState = InputState.Pressed;
                m_Value = 1;
            }
            else if (m_GamePad.GetButtonState(m_Button) == BUTTON_STATES.RELEASED)
            {
                m_CurrentState = InputState.Released;
                m_Value = 1;
            }
            else if (m_GamePad.GetButtonState(m_Button) == BUTTON_STATES.UP)
            {
                m_CurrentState = InputState.Up;
                m_Value = 0;
            }
        }
        else if (m_InputType == ControllerInputType.LeftJoyStick)
        {
            if (Axis == InputAxis.X)
            {
                m_Value = m_GamePad.LeftStick().x;
            }
            else if (Axis == InputAxis.Y)
            {
                m_Value = m_GamePad.LeftStick().y;
            }
        }
        else if (m_InputType == ControllerInputType.RightJoyStick)
        {
            if (Axis == InputAxis.X)
            {
                m_Value = m_GamePad.RightStick().x;
            }
            else if (Axis == InputAxis.Y)
            {
                m_Value = m_GamePad.RightStick().y;
            }
        }
        else if (m_InputType == ControllerInputType.RightTrigger)
        {
            m_Value = m_GamePad.RightTrigger();
        }
        else if (m_InputType == ControllerInputType.LeftTrigger)
        {
            m_Value = m_GamePad.LeftTrigger();
        }
    }
}

public class MouseInput : InputSet
{
    public MouseInput(InputAxis aAxis)
    {
        m_Axis = aAxis;
    }

    public override void Update()
    {
        if (Axis == InputAxis.X)
        {
            m_Value = Input.GetAxis("Mouse X");
        }
        else if (Axis == InputAxis.Y)
        {
            m_Value = Input.GetAxis("Mouse Y");
        }
    }
}

public class EmptyInput : InputSet
{
    public override void Update()
    {
        
    }
}

#endregion 