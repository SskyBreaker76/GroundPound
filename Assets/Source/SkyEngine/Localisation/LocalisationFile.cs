using IniParser;
using IniParser.Model;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SkySoft
{
    [Serializable]
    public class StringCategory
    {
        [NonSerialized] public bool IsFlagOpen;
        [NonSerialized] public bool IsListOpen;

        public string Name;
        public List<LocalisedString> Strings = new List<LocalisedString>();

        public bool ContainsString(string Key)
        {
            foreach (LocalisedString Str in Strings)
                if (Str.Key == Key)
                    return true;

            return false;
        }
    }

    [CreateAssetMenu(fileName = "en-uk", menuName = "SkyEngine/Localisation")]
    public class LocalisationFile : ScriptableObject
    {
        [Button("Generate JSON File")] public bool GenerateJson;

        public string Language = Localisation.English;
        public string DisplayName = "English ( UK )";
        public string GameName;
        public List<LocalisedGraph> Graphs;
        public List<LocalisedString> Strings;
        public List<StringCategory> Categories;

        private Dictionary<string, string> m_CommonTexts = new Dictionary<string, string>();
        public Dictionary<string, string> CommonTexts
        {
            get
            {
                if (m_CommonTexts == null || m_CommonTexts.Count == 0)
                {
                    m_CommonTexts = new Dictionary<string, string>();
                    foreach (StringCategory Category in Categories)
                    {
                        foreach (LocalisedString Str in Category.Strings)
                        {
                            if (!m_CommonTexts.ContainsKey($"{Category.Name.ToLower()}.{Str.Key.ToLower()}"))
                                m_CommonTexts.Add($"{Category.Name.ToLower()}.{Str.Key.ToLower()}", Str.Text);
                            else
                                m_CommonTexts[$"{Category.Name.ToLower()}.{Str.Key.ToLower()}"] = Str.Text;
                        }
                    }
                }

                return m_CommonTexts;
            }
        }

        private void OnValidate()
        {
            if (GenerateJson)
            {
#if UNITY_EDITOR
                string Path = EditorUtility.SaveFilePanel("Save Language", "", "Language", "json");
                if (!string.IsNullOrEmpty(Path))
                {
                    System.IO.File.WriteAllText(Path, JsonUtility.ToJson(this));
                }
#endif
                GenerateJson = false;
            }
        }

        public bool ContainsCategory(string Category)
        {
            foreach (StringCategory Cat in Categories)
                if (Cat.Name == Category) return true;

            return false;
        }

        public void UpdateClass()
        {
            if (Strings.Count > 0)
            {
                foreach (LocalisedString String in Strings)
                {
                    string[] Parts = String.Key.Split('.');
                    bool Success = false;

                    if (Categories != null)
                    {
                        foreach (StringCategory Category in Categories)
                        {
                            if (Category.Name == Parts[0])
                            {
                                Category.Strings.Add(new LocalisedString { Key = Parts[1], Text = String.Text });
                                Success = true;
                                break;
                            }
                        }
                    }
                    else
                    {
                        Categories = new List<StringCategory>();
                    }

                    if (!Success)
                    {
                        Categories.Add(new StringCategory { Name = Parts[0], Strings = new List<LocalisedString> { new LocalisedString { Key = Parts[1], Text = String.Text } } });
                    }
                }

                Strings.Clear();
            }
        }

        public string GetNodeText(string NodeGUID, string DefaultText)
        {
            foreach (LocalisedGraph Graph in Graphs)
            {
                foreach (LocalisedNode Node in Graph.Nodes)
                {
                    if (Node.GUID == NodeGUID)
                    {
                        return Node.Text;
                    }
                }
            }

            return DefaultText;
        }

        /// <summary>
        /// This property uses the disc, and is therefor an expensive property. Only use when required!
        /// </summary>
        public static string[] GetLanguages
        {
            get
            {
                List<string> Value = new List<string>();

                if (!Directory.Exists($"{Application.dataPath}/Languages/"))
                    Directory.CreateDirectory($"{Application.dataPath}/Languages/");

                foreach (FileInfo File in new DirectoryInfo($"{Application.dataPath}/Languages/").GetFiles())
                {
                    Value.Add(File.Name.Replace($".{File.Extension}", "").Replace(File.Extension, ""));
                }

                return Value.ToArray();
            }
        }

        public static LocalisationFile ReadFromDisc(string LanguageName)
        {
            return JsonUtility.FromJson<LocalisationFile>(File.ReadAllText($"{Application.dataPath}/Languages/{LanguageName}.json"));
        }
    }
}
