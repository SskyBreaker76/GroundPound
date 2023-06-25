using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR

#endif

namespace SkySoft.Events.Graph
{
    public class GetVariable : DefinitionNode
    {
        public int VariableIndex;
        public override string DecorativeName => "Get Variable";
        public override string Description => "";
        public override string Value { get => ParentTree.Variables[VariableIndex].Value; }
        public override Type NodeType => typeof(string);

        public override void SetupNode()
        {
            Returns.Add(new NodeVariable { Key = ParentTree.Variables[VariableIndex].Key, Port = GetPort(1), Value = Value, VariableType = typeof(string) });
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
                foreach (DialogueText Switch in ParentTree.Variables)
                {
                    SwitchKeys.Add(Switch.Key);
                }

                int NewSwitchID = EditorGUILayout.Popup("Variable", VariableIndex, SwitchKeys.ToArray());
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