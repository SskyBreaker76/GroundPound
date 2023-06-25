using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SkySoft.Events.Graph
{
    public class SetVariable : CompositeNode
    {
        public override Color NodeTint => Color.Lerp(Color.red, Color.yellow, 0.5f);
        public int VariableID;
        public string Value;

        public override string DecorativeName => "Set Variable";
        public override string Description => $"{ParentTree.Variables[VariableID].Key} = \"{Value}\"";

        public override void SetupNode()
        {
            Variables.Add(new NodeVariable { Key = "Value", Value = "", VariableType = typeof(string) });
        }

        public override void Run(Action OnDone)
        {
            if (DebugMode)
                Debug.Log($"Run: {name}");

            ParentTree.Variables[VariableID].Value = Value;
            ConnectionDict[0].Target.Run(OnDone);
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

                int NewSwitchID = EditorGUILayout.Popup("Variable", VariableID, SwitchKeys.ToArray());
                if (NewSwitchID != VariableID)
                {
                    Value = ParentTree.Variables[NewSwitchID].Value;
                    VariableID = NewSwitchID;
                }
                Value = EditorGUILayout.TextField("Value", Value);
            }
            else
            {
                EditorGUILayout.HelpBox("ParentTree is null! Press the Compile button to fix this", MessageType.Error);
            }
#endif
        }
    }
}