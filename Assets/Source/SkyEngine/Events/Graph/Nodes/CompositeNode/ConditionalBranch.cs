using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SkySoft.Events.Graph
{
    public class ConditionalBranch : Branch
    {
        public Node InputNode;
        public override Color NodeTint => Color.blue;
        public override string DecorativeName => $"Conditional Branch";
        public override string Description => "";
        public int SwitchID;

        public override void Run(Action OnDone)
        {
            if (DebugMode)
                Debug.Log($"Run: {name}");

            ConnectionDict[IsValid() ? 1 : 0].Target.Run(OnDone);
        }

        public override void SetupNode()
        {
            Variables.Add(new NodeVariable { Key = "Condition", VariableType = typeof(bool) });
            Returns.Add(new NodeVariable { Key = "When Value Off", Port = GetPort(1), VariableType = typeof(Node) });
            Returns.Add(new NodeVariable { Key = "When Value On", Port = GetPort(2), VariableType = typeof(Node) });
        }

        public bool IsValid()
        {
            if (InputNode != null)
            {
                if (InputNode is DefinitionNode)
                {
                    DefinitionNode AsDefinition = InputNode as DefinitionNode;

                    foreach (ConnectionInfo C in InputNode.Connections)
                    {
                        if (C.Target == this && AsDefinition.NodeType == typeof(bool))
                        {
                            return AsDefinition.Value == "1" || AsDefinition.Value.ToLower() == "true";
                        }
                    }
                }
                else if (InputNode is CompareNode)
                {
                    return (InputNode as CompareNode).Value;
                }
            }

            return ParentTree.Switches[SwitchID].Value;
        }

        public override void DrawInspector()
        {
#if UNITY_EDITOR
            if (ParentTree)
            {
                bool ExternallyControlled = false;


                if (InputNode != null)
                {
                    foreach (ConnectionInfo C in InputNode.Connections)
                    {
                        if (C.Target == this && InputNode.Returns[C.OutputIndex].ValueType == typeof(bool))
                        {
                            ExternallyControlled = true;
                            break;
                        }
                    }
                }

                GUI.enabled = !ExternallyControlled;

                List<string> SwitchKeys = new List<string>();
                foreach (DialogueSwitch Switch in ParentTree.Switches)
                {
                    SwitchKeys.Add(Switch.Key);
                }

                int NewSwitchID = EditorGUILayout.Popup("Switch", SwitchID, SwitchKeys.ToArray());
                if (NewSwitchID != SwitchID)
                {
                    SwitchID = NewSwitchID;
                }

                GUI.enabled = true;
            }
            else
            {
                EditorGUILayout.HelpBox("ParentTree is null! Press the Compile button to fix this", MessageType.Error);
            }
#endif
        }
    }
}