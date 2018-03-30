using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XINPUT_TEST : MonoBehaviour {

    TF_XINPUT tf_XInput;

    [Range(0, 1)]
    public float hiF;

    [Range(0, 1)]
    public float lowF;

    // Use this for initialization
    void Start () {
       tf_XInput = gameObject.AddComponent<TF_XINPUT>();
	}
	
	// Update is called once per frame
	void Update () {
        GamePad pad = tf_XInput.GetController(Players.PLAYER_ONE);

        tf_XInput.Rumble(Controllers.GAMEPAD_1, lowF, hiF);

        if (pad == null)
            return;

        if (pad.GetButtonState(GAMEPAD_BUTTONS.A_BUTTON) == BUTTON_STATES.PRESSED)
            Debug.Log("A Button Pressed");

        if (pad.GetButtonState(GAMEPAD_BUTTONS.A_BUTTON) == BUTTON_STATES.RELEASED)
            Debug.Log("A Button Released");

        if (pad.GetButtonState(GAMEPAD_BUTTONS.A_BUTTON) == BUTTON_STATES.DOWN)
            Debug.Log("A Button Down");

        if (pad.GetButtonState(GAMEPAD_BUTTONS.A_BUTTON) == BUTTON_STATES.UP)
            Debug.Log("A Button Up");


        if (pad.GetButtonState(GAMEPAD_BUTTONS.B_BUTTON) == BUTTON_STATES.PRESSED)
            Debug.Log("B Button Pressed");

        if (pad.GetButtonState(GAMEPAD_BUTTONS.B_BUTTON) == BUTTON_STATES.RELEASED)
            Debug.Log("B Button Released");

        if (pad.GetButtonState(GAMEPAD_BUTTONS.B_BUTTON) == BUTTON_STATES.DOWN)
            Debug.Log("B Button Down");

        if (pad.GetButtonState(GAMEPAD_BUTTONS.B_BUTTON) == BUTTON_STATES.UP)
            Debug.Log("B Button Up");


        if (pad.LeftTrigger() > 0.001f)
            Debug.Log("Left Trigger " + pad.LeftTrigger().ToString());

        if (pad.RightTrigger() > 0.001f)
            Debug.Log("Right Trigger " + pad.RightTrigger().ToString());

        if (pad.LeftStick().SqrMagnitude() > 0.001f)
            Debug.Log("Left Stick " + pad.LeftStick().ToString());

        if (pad.RightStick().SqrMagnitude() > 0.001f)
            Debug.Log("Right Stick " + pad.RightStick().ToString());
    }
}
