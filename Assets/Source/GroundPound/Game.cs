/*
    Developed by Sky MacLennan
 */

using SkySoft;
using SkySoft.Audio;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace Sky.GroundPound
{
    public class Game : MonoBehaviour
    {
        public static int BGM_Tempo = 120;
        public static float BeatLength => 60f / BGM_Tempo;

        public static Action AnimationTick = () => { };

        private static Game m_Instance;
        protected static Game Instance
        {
            get
            {
                if (!Application.isPlaying)
                    return null;

                if (!m_Instance)
                {
                    Initialize();
                }

                return m_Instance;
            }
        }

        public static void Initialize(uint AvatarSize = 512, Action OnComplete = null)
        {
            GameObject InstanceObj = new GameObject("[ GROUND POUND ]");
            m_Instance = InstanceObj.AddComponent<Game>();
            DontDestroyOnLoad(InstanceObj);

            m_User = new UserInformation(E_AccountType.Discord, AvatarSize, User => { if (OnComplete != null) OnComplete(); });
            m_SetupUser = true;
        }

        private static UserInformation m_User;
        private static bool m_SetupUser = false;
        public static UserInformation User
        {
            get
            {
                if (!m_SetupUser)
                {
                    m_User = new UserInformation(E_AccountType.Discord);
                    m_SetupUser = true;
                }

                return m_User;
            }
        }

        public static bool LastUsedDevice { get; private set; }

        private void Update()
        {
            if (Keyboard.current != null)
            {
                if (Keyboard.current.anyKey.wasPressedThisFrame)
                {
                    LastUsedDevice = false;
                }
                if (Mouse.current != null)
                {
                    foreach (InputControl Control in Mouse.current.allControls)
                    {
                        if (Control is ButtonControl Button && Button.wasPressedThisFrame)
                        {
                            LastUsedDevice = false;
                            break;
                        }
                    }

                    if (Mouse.current.delta.ReadValue().normalized.magnitude > 0.5f)
                    {
                        LastUsedDevice = false;
                    }
                }
                if (Gamepad.current != null)
                {
                    foreach (InputControl Control in Gamepad.current.allControls)
                    {
                        if (Control is ButtonControl Button && Button.wasPressedThisFrame)
                        {
                            LastUsedDevice = true;
                            break;
                        }
                    }
                }
            }
        }
    }
}