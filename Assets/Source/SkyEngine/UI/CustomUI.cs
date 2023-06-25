using UnityEngine;
using SkySoft.Objects;
using System.Collections.Generic;
using System.IO;

namespace SkySoft.UI
{
    [System.Serializable]
    public class StoredUIElement
    {
        public string Label;
        public string InstanceID;
        public Vector3 LocalRotation;
        public Vector3 LocalPosition;
        public Vector3 LocalScale;
        public Vector2 AnchorMin;
        public Vector2 AnchorMax;
        public Vector2 AnchoredPosition;
        public Vector2 SizeDelta;
        public Vector2 Pivot;

        public StoredUIElement(RectTransform T)
        {
            if (T.GetComponent<SerializedObject>())
            {
                string Label = "";

                Transform Target = T;
                List<string> Names = new List<string>();

            ScanParent:
                if (Target.parent != null)
                {
                    Names.Add(Target.name);
                    Target = Target.parent;
                    goto ScanParent;
                }
                else
                {
                    try
                    {
                        for (int I = Names.Count; I > 0; I--)
                        {
                            string Post = I == 0 ? "" : ".";
                            Label += $"{Names[I]}{Post}";
                        }
                    } catch { }
                }

                this.Label = Label;
                LocalRotation = T.localEulerAngles;
                LocalPosition = T.localPosition;
                LocalScale = T.localScale;
                AnchorMin = T.anchorMin;
                AnchorMax = T.anchorMax;
                AnchoredPosition = T.anchoredPosition;
                SizeDelta = T.sizeDelta;
                Pivot = T.pivot;
                InstanceID = T.GetComponent<SerializedObject>().InstanceID;
            }
        }

        public void SetTransform(RectTransform T)
        {
            if (!T.GetComponent<SerializedObject>() || T.GetComponent<SerializedObject>().InstanceID == InstanceID) // We don't want to apply transforms to non-matching Objects
            {
                T.localEulerAngles = LocalRotation;
                T.localPosition = LocalPosition;
                T.localScale = LocalScale;
                T.anchorMin = AnchorMin;
                T.anchorMax = AnchorMax;
                T.anchoredPosition = AnchoredPosition;
                T.sizeDelta = SizeDelta;
                T.pivot = Pivot;
            }
        }
    }

    [System.Serializable]
    public class StoredUI
    {
        public List<StoredUIElement> m_Elements = new List<StoredUIElement>();
        private Dictionary<string, StoredUIElement> m_DElements;
        public Dictionary<string, StoredUIElement> Elements
        {
            get
            {
                if (m_DElements == null || m_DElements.Count < m_Elements.Count)
                {
                    m_DElements = new Dictionary<string, StoredUIElement>();

                    foreach (StoredUIElement Element in m_Elements)
                    {
                        if (!m_DElements.ContainsKey(Element.InstanceID))
                        {
                            m_DElements.Add(Element.InstanceID, Element);
                        }
                    }
                }

                return m_DElements;
            }
        }
    }

    [AddComponentMenu("SkyEngine/UI/Serialized UI Layout")]
    public class CustomUI : MonoBehaviour
    {
        private void OnDrawGizmos()
        {
            foreach (Transform T in transform)
            {
                if (T.tag == "StoredUIElement" && !T.GetComponent<SerializedObject>())
                {
                    T.gameObject.AddComponent<SerializedObject>();
                    T.gameObject.GetComponent<SerializedObject>().OnValidate();
                }
            }
        }

        private string FolderPath => $"{Application.persistentDataPath}\\UserInterfaceLayouts";
        private string AssetPath => $"{FolderPath}\\{gameObject.name}.layout";
        private string DefaultAssetPath => $"{FolderPath}\\{gameObject.name}.default_layout";

        [SerializeField] private bool LoadDefault;
        [SerializeField] private bool LoadCustom;
        [SerializeField] private bool SaveCustom;

        private void Awake()
        {
            if (!Directory.Exists(FolderPath))
                Directory.CreateDirectory(FolderPath);

            Save(true); // Always save the default layout so that the client can reset their element if they want

            if (!File.Exists(AssetPath)) // If this UI hasn't been made yet, Save it for future use
            {
                Save();
            }
            else // Otherwise, load the users layout
            {
                Load();
            }
        }

        private void Update()
        {
            if (LoadDefault)
            {
                Load(true);
                LoadDefault = false;
            }
            if (LoadCustom)
            {
                Load();
                LoadCustom = false;
            }
            if (SaveCustom)
            {
                Save();
                SaveCustom = false;
            }
        }

        public void Save(bool Default = false)
        {
            StoredUI Result = new StoredUI();

            foreach (SerializedObject Obj in GetComponentsInChildren<SerializedObject>())
            {
                Result.m_Elements.Add(new StoredUIElement(Obj.GetComponent<RectTransform>()));
            }

            File.WriteAllText(Default ? DefaultAssetPath : AssetPath, JsonUtility.ToJson(Result, true));
        }

        public void Load(bool Default = false)
        {
            StoredUI Result = JsonUtility.FromJson<StoredUI>(File.ReadAllText(Default ? DefaultAssetPath : AssetPath));

            foreach (SerializedObject Obj in GetComponentsInChildren<SerializedObject>())
            {
                if (Result.Elements.ContainsKey(Obj.InstanceID))
                {
                    Result.Elements[Obj.InstanceID].SetTransform(Obj.GetComponent<RectTransform>());
                }
            }
        }
    }
}