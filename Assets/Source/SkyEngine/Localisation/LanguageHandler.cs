using SkySoft.Events;
using SkySoft.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SkySoft
{
    public class LanguageHandler : MonoBehaviour
    {
        public static LanguageHandler Instance;

        protected void Awake()
        {
            if (Instance != null && Instance != this)
                Destroy(gameObject);

            Instance = this;
            DontDestroyOnLoad(gameObject);
            SkyEngine.Initialize();
            SkyEngine.SetLanguage(ConfigManager.GetOption("LanguageInt", 0, "System"));
        }

        LocalisationFile PreviousFile;

        public virtual void OnLanguageUpdated() { }

        private void Update()
        {
            if (PreviousFile != SkyEngine.ActiveLocalisation)
            {
                foreach (LocalisedText Text in FindObjectsOfType<LocalisedText>(true))
                {
                    Text.UpdateText();
                }
                foreach (OptionsEventHandler EV in FindObjectsOfType<OptionsEventHandler>(true))
                {
                    EV.Display();
                }

                OnLanguageUpdated();
            }

            PreviousFile = SkyEngine.ActiveLocalisation;
        }
    }
}
