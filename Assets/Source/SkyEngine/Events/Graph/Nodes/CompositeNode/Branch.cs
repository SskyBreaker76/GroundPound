using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SkySoft.Events.Graph
{
    public class Branch : CompositeNode
    {
        public override Color NodeTint => Color.cyan;
        public override bool UseDefaultOutput => false;
        public override string DecorativeName => "User Choice";
        public string Options = "Yes/;/No";
        public string[] Branches => Options.Split("/;/");
        public int BranchCount => Branches.Length;

        public override void SetupNode()
        {
            for (int I = 0; I < BranchCount; I++) 
            {
                Returns.Add(new NodeVariable { Key = Branches[I], Port = GetPort(I), VariableType = typeof(Node) });
            }
        }

        public override void Run(Action OnDone)
        {

        }

        protected override void OnNodeWasModified()
        {

        }

        protected override void OnStart()
        {

        }

        protected override void OnStop()
        {

        }

        protected override NodeState OnUpdate()
        {
            return NodeState.Success;
        }

        private Vector2 GUIScroll = new Vector2();

        private static bool ShowLocalisations;

        private LocalisationFile[] AllLocalisations = { };

        public override void OnStartInspector()
        {
            AllLocalisations = Resources.FindObjectsOfTypeAll<LocalisationFile>();
        }

        public override void DrawInspector()
        {
#if UNITY_EDITOR
            List<string> Split = Options.Split("/;/").ToList(); // We don't want to modify the actual branches string or array

            string O = "";

            GUIScroll = EditorGUILayout.BeginScrollView(GUIScroll);
            {
                for (int I = 0; I < Split.Count; I++)
                {
                    GUILayout.BeginHorizontal();
                    Split[I] = EditorGUILayout.TextField($"Option {I}", Split[I]);

                    if (GUILayout.Button("Remove"))
                    {
                        Split.RemoveAt(I);
                        break;
                    }

                    GUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("Add Option"))
            {
                Split.Add("");
            }

            for (int I = 0; I < Split.Count; I++)
            {
                if (I > 0)
                    O += "/;/";

                O += Split[I];
            }

            if (O != Options)
                Options = O;

            if (AllLocalisations.Length > 0)
            {
                if (ShowLocalisations = EditorGUILayout.Foldout(ShowLocalisations, "Localisations"))
                {
                    foreach (LocalisationFile File in AllLocalisations)
                    {
                        EditorGUI.BeginChangeCheck();
                        GUI.enabled = File.Language != Localisation.English; // English is default language, so don't edit through here

                        LocalisedGraph G = null;
                        bool HadGraph = false;
                        foreach (LocalisedGraph Graph in File.Graphs)
                        {
                            if (Graph.GraphID == ParentTree.name)
                            {
                                G = Graph;
                                HadGraph = true;
                                break;
                            }
                        }

                        if (HadGraph)
                        {
                            foreach (LocalisedNode N in G.Nodes)
                            {
                                if (N.GUID == GUID)
                                {
                                    GUILayout.BeginVertical(EditorStyles.helpBox);

                                    GUILayout.Label($"{File.Language} - {File.DisplayName}");
                                    string[] LSplit = N.Text.Split("/;/");

                                    for (int I = 0; I < LSplit.Length; I++)
                                    {
                                        LSplit[I] = EditorGUILayout.TextField($"Option {I}", LSplit[I]);
                                    }
                                    string LStitched = "";
                                    for (int I = 0; I < LSplit.Length;I++)
                                    {
                                        if (I != 0)
                                            LStitched += "/;/";
                                        LStitched += LSplit[I];
                                    }
                                    N.Text = LStitched;

                                    GUILayout.EndVertical();
                                }
                            }
                        }

                        GUI.enabled = true;
                        if (EditorGUI.EndChangeCheck())
                        {
                            EditorUtility.SetDirty(File);
                        }
                    }
                }
            }
            else
            {
                EditorGUILayout.HelpBox("No localisations were found for this node!\nMake sure to hit the \"Update Localisation\" button to generate localisations for English (UK)!", MessageType.Info);
            }
#endif
        }
    }
}