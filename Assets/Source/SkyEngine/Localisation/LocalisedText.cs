using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SkySoft
{
    public class LocalisedText : MonoBehaviour
    {
        private Text Display => GetComponent<Text>();

        [Tooltip("Only use this for dynamic texts, as it makes the LocalisedText component more expensive to update")]
        public bool SmartMode;
        [CommonTextKey]
        public string Key;
        private string DefaultText;

        private void Awake()
        {
            SkyEngine.Initialize(); // Just in-case it isn't initialized already. SkyEngine does have a check to make sure you can't double-intialise it
            
            if (string.IsNullOrEmpty(DefaultText)) // Fixes the issue where if another language is loaded first, the Text forgets what English is
                DefaultText = Display.text;

            UpdateText();
        }

        public void UpdateText()
        {
            if (string.IsNullOrEmpty(DefaultText))
                DefaultText = Display.text;

            if (!SmartMode)
            {
                if (SkyEngine.CommonTexts.ContainsKey(Key))
                {
                    Display.text = SkyEngine.CommonTexts[Key];
                }
                else
                {
                    Display.text = DefaultText;
                }
            }
            else
            {
                string FinalisedText = "";

                foreach (string Key in SkyEngine.CommonTexts.Keys)
                {
                    FinalisedText = Display.text.Replace($"<{Key}>", SkyEngine.CommonTexts[Key]);
                }
            }
        }
    }
}
