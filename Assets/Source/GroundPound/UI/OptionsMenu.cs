/*
    Developed by Sky MacLennan
 */

using SkySoft;
using SkySoft.IO;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

namespace Sky.GroundPound
{
    [Serializable]
    public class Tab
    {
        public string TabID;
        public string TabName;
        public Control<Slider>[] SliderOptions;
        public Control<Toggle>[] ToggleOptions;
        public Control<Dropdown>[] DropdownOptions;
        [Tooltip ("Only set this if the generated tab isn't suitable")] public GameObject CustomTabObject;
    }

    [Serializable]
    public class Control<T> where T: MonoBehaviour
    {
        public string TabID;
        public string ControlID;
        public string ControlName;
        [Space]
        [Tooltip("This value is only used for Sliders")] public int MaxValue;
        [Tooltip("This value is only used for Dropdowns")] public string[] Options;
        public int DefaultValue;
        [SerializeField, DisplayOnly] private int m_Value;
        public UnityEvent<int> OnValueChanged;
        public int Value 
        { 
            get
            {
                Read();
                return m_Value;
            }
            set
            {
                m_Value = value;
                Write();
                OnValueChanged?.Invoke(m_Value);
            }
        }

        public void Write()
        {
            ConfigManager.SetOption(ControlID, m_Value, TabID);
        }

        public void Read()
        {
            m_Value = ConfigManager.GetOption(ControlID, DefaultValue, TabID);
        }
    }

    [AddComponentMenu("Ground Pound/Options Menu")]
    public class OptionsMenu : MonoBehaviour
    {
        [Tooltip("This is NOT the name of the options menu, so please do NOT put any spaces as it can break the options menu")] public string MenuID = "Options";
        public static bool IsDirty = false;

#if UNITY_EDITOR
        public bool EditorRefresh = false;
#endif

        [SerializeField] private GameObject m_TabButtonPrefab, m_SliderPrefab, m_TogglePrefab, m_DropdownPrefab;
        [SerializeField] private RectTransform m_TabLocation, m_TabButtonLocation;

        public bool PersistTab;
        public bool AddLanguageTab;
        public Tab[] Tabs;
        [SerializeField] private int PreviewTab = 0;
        [DisplayOnly]
        public int ActiveTab = 0;
                
        private void OnEnable()
        {
            SetTab(0);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (EditorRefresh)
            {
                ActiveTab = PreviewTab;
                UpdateTabs(true);
            }
        }
#endif

        public void SetTab(int Tab)
        {
            ActiveTab = Tab;
            UpdateTabs();
        }

        private List<GameObject> SpawnedLangauges = new List<GameObject>();

        private void UpdateTabs(bool EditorCall = false)
        {
            foreach (Transform T in m_TabLocation)
                if (!EditorCall)
                    Destroy(T.gameObject);
                else
                {
#if UNITY_EDITOR
                    UnityEditor.EditorApplication.delayCall += () => DestroyImmediate(T.gameObject);
                    UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
#endif
                }

            foreach (Transform T in m_TabButtonLocation)
                if (!EditorCall)
                    Destroy(T.gameObject);
                else
                {
#if UNITY_EDITOR
                    UnityEditor.EditorApplication.delayCall += () => DestroyImmediate(T.gameObject);
                    UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
#endif
                }

            int TabIndex = 0;

            foreach (Tab Tab in Tabs)
            {
                GameObject TabButton = Instantiate(m_TabButtonPrefab, m_TabButtonLocation);
                TabButton.GetComponentInChildren<Text>().text = SkyEngine.CommonTexts.ContainsKey(Tab.TabName) ? SkyEngine.CommonTexts[Tab.TabName] : Tab.TabName;
                TabButton.name = TabIndex.ToString();

                TabButton.GetComponent<Button>().onClick.AddListener(() => SetTab(int.Parse(TabButton.name)));

                if (ActiveTab == TabIndex)
                {
                    TabButton.GetComponent<Button>().interactable = false;

                    foreach (Control<Slider> Slider in Tab.SliderOptions)
                    {
                        GameObject Base;
                        Slider S = (Base = Instantiate(m_SliderPrefab, m_TabLocation)).GetComponentInChildren<Slider>();
                        Base.transform.GetChild(0).GetComponent<Text>().text = $"{(SkyEngine.CommonTexts.ContainsKey(Slider.ControlName) ? SkyEngine.CommonTexts[Slider.ControlName] : Slider.ControlName).PadRight(20)}{Slider.Value.ToString("00")}";
                        S.maxValue = Slider.MaxValue;
                        S.value = Slider.Value;
                        S.onValueChanged.AddListener(Value =>
                        {
                            Slider.Value = (int)Value;
                            Base.transform.GetChild(0).GetComponent<Text>().text = $"{(SkyEngine.CommonTexts.ContainsKey(Slider.ControlName) ? SkyEngine.CommonTexts[Slider.ControlName] : Slider.ControlName).PadRight(20)}{Slider.Value.ToString("00")}";
                        });
                    }

                    foreach (Control<Toggle> Toggle in Tab.ToggleOptions)
                    {
                        GameObject Base;
                        Toggle T = (Base = Instantiate(m_TogglePrefab, m_TabLocation)).GetComponentInChildren<Toggle>();
                        Base.transform.GetChild(0).GetComponent<Text>().text = $"{(SkyEngine.CommonTexts.ContainsKey(Toggle.ControlName) ? SkyEngine.CommonTexts[Toggle.ControlName] : Toggle.ControlName).PadRight(22)}";
                        T.isOn = Toggle.Value == 1;
                        T.onValueChanged.AddListener(Value =>
                        {
                            Toggle.Value = Value ? 1 : 0;
                            Base.transform.GetChild(0).GetComponent<Text>().text = $"{(SkyEngine.CommonTexts.ContainsKey(Toggle.ControlName) ? SkyEngine.CommonTexts[Toggle.ControlName] : Toggle.ControlName).PadRight(22)}";
                        });
                    }

                    foreach (Control<Dropdown> Dropdown in Tab.DropdownOptions)
                    {
                        GameObject Base;
                        Dropdown D = (Base = Instantiate(m_DropdownPrefab, m_TabLocation)).GetComponentInChildren<Dropdown>();
                        Base.transform.GetChild(0).GetComponent<Text>().text = $"{(SkyEngine.CommonTexts.ContainsKey(Dropdown.ControlName) ? SkyEngine.CommonTexts[Dropdown.ControlName] : Dropdown.ControlName).PadRight(22)}";
                        D.options.Clear();
                        foreach (string S in Dropdown.Options)
                        {
                            D.options.Add(new UnityEngine.UI.Dropdown.OptionData(S));
                        }
                        D.value = Dropdown.Value;
                        D.onValueChanged.AddListener(Value =>
                        {
                            Dropdown.Value = Value;
                            Base.transform.GetChild(0).GetComponent<Text>().text = $"{(SkyEngine.CommonTexts.ContainsKey(Dropdown.ControlName) ? SkyEngine.CommonTexts[Dropdown.ControlName] : Dropdown.ControlName).PadRight(22)}";
                        });
                    }
                }

                TabIndex++;
            }

            if (AddLanguageTab)
            {
                GameObject TabButton = Instantiate(m_TabButtonPrefab, m_TabButtonLocation);
                TabButton.GetComponentInChildren<Text>().text = SkyEngine.CommonTexts["options.language"];
                TabButton.GetComponent<Button>().onClick.AddListener(() => SetTab(-1));

                if (ActiveTab == -1)
                {
                    TabButton.GetComponent<Button>().interactable = false;

                    for (int I = 0; I < Localisation.Languages.Length; I++)
                    {
                        LocalisationFile Language = SkyEngine.GetLocalisation(Localisation.Languages[I]);

                        GameObject Base;
                        Toggle T = (Base = Instantiate(m_TogglePrefab, m_TabLocation)).GetComponentInChildren<Toggle>();
                        Base.name = I.ToString();
                        Base.transform.GetChild(0).GetComponent<Text>().text = Language.DisplayName.PadRight(22);
                        T.isOn = I == ConfigManager.GetOption("Language", 0, "System");
                        T.onValueChanged.AddListener(Value =>
                        {
                            if (!Value)
                                T.isOn = true;
                            else
                            {
                                int V;

                                if (int.TryParse(Base.name, out V))
                                {
                                    SkyEngine.SetLanguage(V);
                                    ConfigManager.SetOption("Language", V, "System");
                                }

                                UpdateTabs();
                            }
                        });
                    }
                }
            }
        }
    }
}