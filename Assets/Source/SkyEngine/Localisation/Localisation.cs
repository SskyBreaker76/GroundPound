using SkySoft.Events.Graph;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SkySoft
{
    [System.Serializable]
    public class LocalisedNode
    {
        public string GUID;
        public string Text;
    }

    [System.Serializable]
    public class LocalisedGraph
    {
        public string GraphID;
        public LocalisedNode[] Nodes;
    }

    [System.Serializable]
    public class LocalisedString
    {
        public string Key;
        public string Text;
    }

    public class Localisation : MonoBehaviour
    {
        public const string English = "en-uk";
        public static bool ShouldUpdateLanguages = true;

        // TODO[Sky] Add more localisations and improve localisation generator
        private static string[] m_Languages;
        public static List<string> CustomLanguages { get; private set; } = new List<string>();

        public static string[] Languages
        {
            get
            {
                if (m_Languages == null || m_Languages.Length == 0 || ShouldUpdateLanguages)
                {
                    CustomLanguages = new List<string>();
                    List<string> Langs = new List<string>
                    {
                        English // English is always at index 0 as it's the most commonly read/spoken language
                    };

                    List<LocalisationFile> AllLocalisations = Resources.LoadAll<LocalisationFile>("SkyEngine/Localisations/").ToList();

                    foreach (LocalisationFile L in AllLocalisations)
                    {
                        if (L.name != English)
                        {
                            Langs.Add(L.name);
                        }
                    }

                    foreach (string Lang in LocalisationFile.GetLanguages)
                    {
                        try
                        {
                            if (!Langs.Contains(Lang))
                            {
                                CustomLanguages.Add(Lang);
                                Langs.Add(Lang);
                            }
                        }
                        catch { continue; }
                    }

                    m_Languages = Langs.ToArray();
                }

                return m_Languages;
            }
        }

#if UNITY_EDITOR
        private static Dictionary<string, string> m_LocalisationPaths = new Dictionary<string, string>();
        /// <summary>
        /// This is only available in the Editor!
        /// </summary>
        public static Dictionary<string, string> LocalisationPaths
        {
            get
            {
                LocalisationFile[] AllLocalisations = Resources.LoadAll<LocalisationFile>("SkyEngine/Localisations/");

                if (m_LocalisationPaths.Count < AllLocalisations.Length)
                {
                    m_LocalisationPaths.Clear();

                    foreach (LocalisationFile File in AllLocalisations)
                    {
                        if (!m_LocalisationPaths.ContainsKey(File.Language))
                        {
                            m_LocalisationPaths.Add(File.Language, AssetDatabase.GetAssetPath(File));
                        }
                    }
                }

                return m_LocalisationPaths;
            }
        }
#endif

        public static void GenerateLocalisation(EventTree Graph, string Language = English)
        {
            List<LocalisedNode> Nodes = new List<LocalisedNode>();

            foreach (Node N in Graph.Nodes)
            {
                LocalisedNode LN = new LocalisedNode { GUID = N.GUID };
                bool ShouldAddNode = false;

                if (N is ShowText)
                {
                    LN.Text = (N as ShowText).Properties.Dialogue;
                    ShouldAddNode = true;
                }
                else if (N is Branch && !(N is ConditionalBranch) && !(N is RandomBranch))
                {
                    LN.Text = (N as Branch).Options;
                    ShouldAddNode = true;
                }

                if (ShouldAddNode)
                    Nodes.Add(LN);
            }

            foreach (LocalisationFile File in Resources.LoadAll<LocalisationFile>("SkyEngine/Localisations/"))
            {
                if (File)
                {
                    bool HadGraph = false;

                    foreach (LocalisedGraph G in File.Graphs)
                    {
                        if (G.GraphID == Graph.name)
                        {
                            HadGraph = true;
                            G.Nodes = Nodes.ToArray();
                            break;
                        }
                    }

                    if (!HadGraph)
                    {
                        LocalisedGraph GraphFile = new LocalisedGraph { GraphID = Graph.name };
                        GraphFile.Nodes = Nodes.ToArray();
                        File.Graphs.Add(GraphFile);

#if UNITY_EDITOR
                        EditorUtility.SetDirty(File);
#endif
                    }
                }
            }
        }
    }
}
