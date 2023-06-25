using SkySoft.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SkySoft
{
    public class LocalisationSetter : MonoBehaviour
    {
        private void OnEnable()
        {
            OptionsEventHandler EV;

            // This allows the extra functionality of scanning through the available languages
            if (EV = GetComponent<OptionsEventHandler>())
            {
                List<string> Languages = new List<string>();
                foreach (LocalisationFile Language in SkyEngine.Languages)
                {
                    Languages.Add(Language.DisplayName);
                }
                EV.Combos = Languages.ToArray();
                EV.OnEnable();
            }
        }

        public void UpdateLocalisation(int Value)
        {
            Debug.Log("Set Language to " + Value.ToString());
            SkyEngine.SetLanguage(Value);
        }
    }
}
