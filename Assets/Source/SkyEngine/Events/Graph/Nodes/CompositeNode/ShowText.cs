using SkySoft.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SkySoft.Events.Graph
{
    public class ShowText : CompositeNode
    {
        public string InputNodeGUID;
        public override string DecorativeName => $"Show Text";
        public override string Description => $"<b><color=yellow>{Properties.Speaker}</b></color>\n{Properties.Dialogue}";
        public ShowTextProperties Properties = new ShowTextProperties();

        public override void Run(Action OnDone)
        {
            if (DebugMode)
                Debug.Log($"Run: {name}");

            List<DialoguePanel.DialogueChoice> Choices = new List<DialoguePanel.DialogueChoice>();

            if (ConnectionDict[0].Target is Branch && !(ConnectionDict[0].Target is ConditionalBranch) && !(ConnectionDict[0].Target is RandomBranch))
            {
                Branch AsBranch = ConnectionDict[0].Target as Branch;

                string OptionsText = SkyEngine.ActiveLocalisation.GetNodeText(AsBranch.GUID, AsBranch.Options);
                string[] Options = OptionsText.Split("/;/");

                for (int I = 0; I < Options.Length; I++)
                {
                    DialoguePanel.DialogueChoice Choice = new DialoguePanel.DialogueChoice();
                    Choice.Text = Options[I];
                    Choice.Interactable = true;
                    Choice.Index = I;
                    //                                                          Here we clear the decisions so that they aren't visible for if we have an event happen such as saving the game
                    Choice.Chosen = new UnityEngine.Events.UnityAction(() => { EventGraphManager.Instance.Dialogue.ClearDecisions(); AsBranch.ConnectionDict[Choice.Index].Target.Run(OnDone); });
                    Choices.Add(Choice);
                }
            }

            string Dialogue = SkyEngine.ActiveLocalisation.GetNodeText(GUID, Properties.Dialogue);

            EventGraphManager.Instance.ShowText(Properties.Speaker, Dialogue, Properties.Position, 300, Choices.ToArray(), () => ConnectionDict[0].Target.Run(OnDone));
        }

        public override void SetupNode()
        {
            Variables.Add(new NodeVariable { Key = "Speaker", Value = "", VariableType = typeof(string) });
        }

        protected override void OnVariableUpdate()
        {
            if (Variables[1].HasConnection)
            {
                if (Variables[1].ValueType == typeof(string))
                    Properties.m_Speaker = Variables[1].Value as string;
                else if (Variables[1].ValueType == typeof(Entity))
                    Properties.m_Speaker = (Variables[1].Value as Entity).Properties.Name;
            }
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
            return NodeState.Running; // TEMP
        }

        private static bool ShowLocalisations;

        private LocalisationFile[] AllLocalisations = { };

        public override void OnStartInspector()
        {
            AllLocalisations = Resources.FindObjectsOfTypeAll<LocalisationFile>();
        }

        public override void DrawInspector()
        {
            Properties.DrawInspector();

#if UNITY_EDITOR
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
                                    GUILayout.Label($"{File.Language} - {File.DisplayName}");
                                    N.Text = GUILayout.TextArea(N.Text);
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