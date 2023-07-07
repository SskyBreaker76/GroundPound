using SkySoft;
using SkySoft.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Sky.GroundPound
{
    [AddComponentMenu("Ground Pound/Pointer Handler"), RequireComponent(typeof(Image))]
    public class PointerHandler : MonoBehaviour
    {
        public static PointerHandler Instance { get; private set; }

        private Image m_Image;
        public Image Image
        {
            get
            {
                if (!m_Image)
                    m_Image = GetComponent<Image>();

                return m_Image;
            }
        }

        [Tooltip ("Leave this blank if there's only one camera in the scene ( IE: In the menu )")]
        public Camera ReferenceCamera;
        private Texture2D ActiveCursorGraphic;
        public Texture2D[] DarkPointers;
        public Texture2D[] LightPointers;
        public float PointerDistance = 1.5f;

        [SerializeField, DisplayOnly, Combo(True: "Controller", False: "Mouse")] private bool LastUsedInput = false;

        private void Awake()
        {
            Instance = this;
        }

        private void FixedUpdate()
        {
            if (Instance != this)
            {
                Instance = this;
            }

            if (!ReferenceCamera)
                ReferenceCamera = Camera.main;

            Texture2D Target = ConfigManager.GetOption("PointerColour", 0, "Personalisation") == 0 ? DarkPointers[ConfigManager.GetOption("PointerTint", 0, "Personalisation")] : LightPointers[ConfigManager.GetOption("PointerTint", 0, "Personalisation")];

            if (ActiveCursorGraphic != Target)
                Cursor.SetCursor(Target, Vector2.zero, CursorMode.Auto);

            bool SoftwarePointerVisiblity = false;

            if (Player.LocalPlayer)
            {
                if (ReferenceCamera)
                {                    
                    SoftwarePointerVisiblity = true;
                    Vector2 Position;

                    LastUsedInput = Game.LastUsedDevice;

                    if (!LastUsedInput) // If the player is using a mouse, we want the "cursor" to always try and snap to that
                    {
                        Cursor.lockState = CursorLockMode.Confined;
                        Cursor.visible = true;
                        Position = Player.LocalPlayer.transform.InverseTransformPoint(ReferenceCamera.ScreenToWorldPoint(Mouse.current.position.value));
                    }
                    else // Otherwise, lock and hide the pointer so it doesn't get in the way
                    {
                        Cursor.lockState = CursorLockMode.Locked;
                        Cursor.visible = false;
                        Position = Gamepad.current.rightStick.ReadValue() * PointerDistance;
                    }

                    Position = Vector2.ClampMagnitude(Position, PointerDistance);
                    transform.position = Player.LocalPlayer.transform.TransformPoint(Position);
                }
            }

            Image.color = SoftwarePointerVisiblity ? Color.white : Color.clear;
        }
    }
}