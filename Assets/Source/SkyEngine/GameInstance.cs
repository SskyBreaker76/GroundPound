using SkySoft.IO;
using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SkySoft
{
    /// <summary>
    /// This class always exists, so use it whenever you need a physical GameObject to do something
    /// </summary>
    [AddComponentMenu("SkyEngine/Game Instance")]
    public class GameInstance : MonoBehaviour
    {
        private static GameInstance m_Instance;

        public static GameInstance Main
        {
            get
            {
                if (!m_Instance)
                {
                    GameObject NewInstance = new GameObject("[ SkyEngine ]");
                    DontDestroyOnLoad(NewInstance);
                    m_Instance = NewInstance.AddComponent<GameInstance>();
                }

                return m_Instance;
            }
        }

        protected void Update()
        {
            RunUpdate();
        }

        int FrameCounter = 0;

        protected void FixedUpdate()
        {
            FrameCounter++;

            if (FrameCounter >= 5)
                RunTick();
        }

        public Action OnUpdate = new Action(() => { });
        public Action OnTick = new Action(() => { });

        /// <summary>
        /// This function is called once per frame
        /// </summary>
        /// <remarks>
        /// Base method contains default implentation of the Master Volume control.<br/>
        /// <b>Please note that MusicVolume is handled by the BGM class, and is therefor not modifiable without writing your own BGM and LevelManager system or hacking ours.</b>
        /// </remarks>
        public virtual void RunUpdate()
        {
            AudioListener.volume = ConfigManager.GetOption("Volume", 10, "Audio") / 10f;
            OnUpdate();
        }
        
        /// <summary>
        /// This function is called 10 times a second (once every 0.1 seconds)
        /// </summary>
        public virtual void RunTick()
        {
            OnTick();
        }

        public bool RandomTick => GetRandomTick();

        /// <summary>
        /// Gets whether or not the game should run a random tick based off Input Chance
        /// </summary>
        /// <param name="Chance">How likeley is this to return true (Can either be a decimal value OR a percentage. If you want to use less than 1% you MUST use decimal value)</param>
        /// <returns></returns>
        public bool GetRandomTick(float Chance = 0.05f)
        {
            // This accounts for if a percentage value is passed rather than decimal
            if (Chance > 1 && Chance < 100)
                Chance /= 100;

            Chance = Mathf.Clamp01(Chance);

            if (Random.value < Chance)
                return true;

            return false;
        }
    }
}
