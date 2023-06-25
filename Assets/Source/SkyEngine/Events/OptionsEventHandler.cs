using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using SkySoft.IO;

namespace SkySoft.Events
{
    public enum OptionType
    {
        Toggle,
        Slider,
        Number,
        Combo
    }

    public enum ToggleMode
    {
        OnOff,
        TrueFalse,
        YesNo,
        Custom
    }

    [AddComponentMenu("SkyEngine/Events/Options Event")]
    public class OptionsEventHandler : ObjectEventHandler
    {
        private int DefaultValue = -1;
        public bool ClearOnClose = true;

        [Combo(True: "External", False: "Awake", Label = "Call Via")]
        public bool AwaitExternalCall = false;
        public OptionType Mode;
        [Combo(True: "Current Language", False: "Default")]
        public bool UseTranslation = false;
        public ToggleMode ToggleType;
        [Tooltip("This is only used for Custom ToggleType")] public string CustomOn, CustomOff;
        [Space]
        public string OptionSection = "Default";
        public string OptionKey;
        public int StepSize = 1;
        public int Value;
        public int MinimumValue;
        public int MaxValue;
        public string[] Combos;
        [Space]
        public UnityEvent<int> RightEvent, LeftEvent, ValueChanged;

        public Text Label, Label2;
        public Image SliderKnob;
        public RectTransform SliderRoot;

        [Button("Generate Script")]
        public bool GenerateScript;


        protected override void OnValidate()
        {
            base.OnValidate();

#if UNITY_EDITOR
            if (GenerateScript)
            {
                DoScriptGen();
                GenerateScript = false;
            }
#endif
        }

#if UNITY_EDITOR
        public void DoScriptGen()
        {
            if (Mode == OptionType.Combo)
            {
                string Path = UnityEditor.EditorUtility.SaveFilePanel("Generate Script", Application.dataPath + "Assets\\Scripts\\", gameObject.name, "cs");
                try
                {
                    string Values = "";

                    for (int I = 0; I < Combos.Length; I++)
                    {
                        if (I != 0)
                        {
                            Values += "\n\t\t";
                        }

                        Values += Combos[I];

                        if (I < Combos.Length - 1)
                        {
                            Values += ",";
                        }
                    }

                    string Script =
                        "namespace SkyEngine.Generated\n{\n\tpublic enum E_" + gameObject.name.Replace(' ', '_') + "\n\t{\n\t\tVAL\n\t}\n}".Replace("VAL", Values);

                    System.IO.File.WriteAllText(Path, Script);
                }
                catch (UnityException E)
                {
                    UnityEditor.EditorUtility.DisplayDialog("Generate Script", $"Ran into error\n{E.Message}\n{E.StackTrace}", "Okay");
                }
            }
            else
                UnityEditor.EditorUtility.DisplayDialog("Generate Script", "Must be a Combo to save as script", "Okay");
        }
#endif

        private void OnDisable()
        {
            if (!string.IsNullOrEmpty(OptionKey))
                ConfigManager.SetOption(OptionKey, Value, OptionSection);
        }

        public void OnEnable()
        {
            if (DefaultValue == -1)
                DefaultValue = Value;

            if (!string.IsNullOrEmpty(OptionKey))
                Value = ConfigManager.GetOption(OptionKey, Value, OptionSection);
            else
                if (ClearOnClose)
                    Value = DefaultValue;

            if (!AwaitExternalCall)
            {
                Display();
            }
        }

        public void ResetValue()
        {
            Value = DefaultValue;
        }

        public virtual void Display()
        {
            switch (Mode)
            {
                case OptionType.Toggle:
                    string On = "";
                    string Off = "";

                    switch (ToggleType)
                    {
                        case ToggleMode.OnOff:
                            On = SkyEngine.CommonTexts.ContainsKey("options.toggle_on") && UseTranslation ? SkyEngine.CommonTexts["options.toggle_on"] : "On";
                            Off = SkyEngine.CommonTexts.ContainsKey("options.toggle_off") && UseTranslation ? SkyEngine.CommonTexts["options.toggle_off"] : "Off";
                            break;
                        case ToggleMode.TrueFalse:
                            On = SkyEngine.CommonTexts.ContainsKey("options.toggle_true") && UseTranslation ? SkyEngine.CommonTexts["options.toggle_true"] : "True";
                            Off = SkyEngine.CommonTexts.ContainsKey("options.toggle_false") && UseTranslation ? SkyEngine.CommonTexts["options.toggle_false"] : "False";
                            break;
                        case ToggleMode.YesNo:
                            On = SkyEngine.CommonTexts.ContainsKey("general.yes") && UseTranslation ? SkyEngine.CommonTexts["general.yes"] : "Yes";
                            Off = SkyEngine.CommonTexts.ContainsKey("general.no") && UseTranslation ? SkyEngine.CommonTexts["general.no"] : "No";
                            break;
                        case ToggleMode.Custom:
                            On = SkyEngine.CommonTexts.ContainsKey(CustomOn) && UseTranslation ? SkyEngine.CommonTexts[CustomOn] : CustomOn;
                            Off = SkyEngine.CommonTexts.ContainsKey(CustomOff) && UseTranslation ? SkyEngine.CommonTexts[CustomOff] : CustomOff;
                            break;
                    }

                    Label.text = Value == 1 ? On : Off;
                    break;
                case OptionType.Slider:
                    if (Label)
                        Label.text = Value.ToString();
                    if (Label2)
                        Label2.text = $"[{Value + 1}/{MaxValue + 1}]";

                    SliderKnob.rectTransform.anchoredPosition = new Vector2(Mathf.Lerp(-(SliderRoot.rect.width / 2), SliderRoot.rect.width / 2, Value / (float)MaxValue), 0);
                    break;
                case OptionType.Number:
                    Label.text = Value.ToString();
                    break;
                case OptionType.Combo:
                    Label.text = SkyEngine.CommonTexts.ContainsKey(Combos[Value]) && UseTranslation ? SkyEngine.CommonTexts[Combos[Value]] : Combos[Value];
                    if (Label2)
                        Label2.text = $"[{Value + 1}/{Combos.Length}]";
                    break;
            }
        }

        public virtual void OnRight()
        {
            switch (Mode)
            {
                case OptionType.Toggle:
                    Value = 1;
                    break;
                case OptionType.Slider:
                    Value++;
                    Value = Mathf.Clamp(Value, 0, MaxValue);
                    break;
                case OptionType.Number:
                    Value += StepSize;
                    Value = Mathf.Clamp(Value, MinimumValue, MaxValue);
                    break;
                case OptionType.Combo:
                    Value++;
                    Value = Mathf.Clamp(Value, 0, Combos.Length - 1);
                    break;
            }

            if (!string.IsNullOrEmpty(OptionKey))
                ConfigManager.SetOption(OptionKey, Value, OptionSection);

            Display();
            RightEvent.Invoke(Value);
            ValueChanged.Invoke(Value);
        }

        public virtual void OnLeft()
        {
            switch (Mode)
            {
                case OptionType.Toggle:
                    Value = 0;
                    break;
                case OptionType.Slider:
                    Value--;
                    Value = Mathf.Clamp(Value, 0, MaxValue);
                    break;
                case OptionType.Number:
                    Value -= StepSize;
                    Value = Mathf.Clamp(Value, MinimumValue, MaxValue);
                    break;
                case OptionType.Combo:
                    Value--;
                    Value = Mathf.Clamp(Value, 0, Combos.Length - 1);
                    break;
            }

            if (!string.IsNullOrEmpty(OptionKey))
                ConfigManager.SetOption(OptionKey, Value, OptionSection);

            Display();
            LeftEvent.Invoke(Value);
            ValueChanged.Invoke(Value);
        }

        public override void Event()
        {
            if (Mode == OptionType.Toggle)
            {
                Value = Value == 1 ? 0 : 1;
                ValueChanged.Invoke(Value);
            }

            if (!string.IsNullOrEmpty(OptionKey))
                ConfigManager.SetOption(OptionKey, Value, OptionSection);

            Display();
            base.Event();
        }
    }
}