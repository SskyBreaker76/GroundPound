using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR

#endif

namespace SkySoft.Events.Graph
{
    public class GetSwitch : DefinitionNode
    {
        public int VariableIndex;
        public override string DecorativeName => "Get Switch";
        public override string Description => "";
        public override string Value { get => ParentTree.Switches[VariableIndex].Value ? "1" : "0"; }
        public override Type NodeType => typeof(bool);

        public override void SetupNode()
        {
            Returns.Add(new NodeVariable { Key = ParentTree.Switches[VariableIndex].Key, Port = GetPort(1), Value = Value, VariableType = typeof(bool) });
        }

        protected override void OnNodeWasModified()
        {

        }

        public override void DrawInspector()
        {
#if UNITY_EDITOR
            if (ParentTree)
            {
                List<string> SwitchKeys = new List<string>();
                foreach (DialogueSwitch Switch in ParentTree.Switches)
                {
                    SwitchKeys.Add(Switch.Key);
                }

                int NewSwitchID = EditorGUILayout.Popup("Switch", VariableIndex, SwitchKeys.ToArray());
                if (NewSwitchID != VariableIndex)
                {
                    VariableIndex = NewSwitchID;
                }
            }
            else
            {
                EditorGUILayout.HelpBox("ParentTree is null! Press the Compile button to fix this", MessageType.Error);
            }
#endif
        }
    }
}