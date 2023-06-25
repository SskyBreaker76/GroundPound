using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace SkySoft.Input
{
    [AddComponentMenu("SkyEngine/Input/Input Debugger")]
    public class InputDebugger : MonoBehaviour
    {
        public Image CurrentPress;

        private void Update()
        {
            Sprite T = null;

            if (Gamepad.current != null)
            {
                if (Gamepad.current.buttonNorth.isPressed)
                {
                    T = SkyEngine.Properties.North;
                }
                if (Gamepad.current.buttonEast.isPressed)
                {
                    T = SkyEngine.Properties.East;
                }
                if (Gamepad.current.buttonSouth.isPressed)
                {
                    T = SkyEngine.Properties.South;
                }
                if (Gamepad.current.buttonWest.isPressed)
                {
                    T = SkyEngine.Properties.West;
                }

                if (Gamepad.current.dpad.up.isPressed)
                {
                    T = SkyEngine.Properties.DPadUp;
                }
                if (Gamepad.current.dpad.right.isPressed)
                {
                    T = SkyEngine.Properties.DPadRight;
                }
                if (Gamepad.current.dpad.down.isPressed)
                {
                    T = SkyEngine.Properties.DPadDown;
                }
                if (Gamepad.current.dpad.left.isPressed)
                {
                    T = SkyEngine.Properties.DPadLeft;
                }

                if (Gamepad.current.leftShoulder.isPressed)
                {
                    T = SkyEngine.Properties.LeftBumper;
                }
                if (Gamepad.current.rightShoulder.isPressed)
                {
                    T = SkyEngine.Properties.RightBumper;
                }
                if (Gamepad.current.leftTrigger.isPressed)
                {
                    T = SkyEngine.Properties.LeftTrigger;
                }
                if (Gamepad.current.rightTrigger.isPressed)
                {
                    T = SkyEngine.Properties.RightTrigger;
                }
            }

            CurrentPress.color = T != null ? Color.white : Color.clear;
            CurrentPress.sprite = T;
        }
    }
}