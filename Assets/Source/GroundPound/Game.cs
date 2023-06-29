/*
    Developed by Sky MacLennan
 */

using SkySoft;
using SkySoft.Audio;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sky.GroundPound
{
    public class Game : MonoBehaviour
    {
        public static Action AnimationTick = () => { };

        private static Game m_Instance;
        protected static Game Instance
        {
            get
            {
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

        public static int CurrentTempo = 120;
        public static float BeatLength = 60f / CurrentTempo;

        private void Update()
        {
            if (BGM.ActiveAudioSource)
            {
                if (BGM.ActiveAudioSource.time % BeatLength == 0)
                {
                    AnimationTick();
                }
            }
        }
    }
}