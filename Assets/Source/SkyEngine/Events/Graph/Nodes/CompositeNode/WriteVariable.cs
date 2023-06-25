using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SkySoft.Events.Graph
{
    public class WriteVariable : CompositeNode
    {
        public override Color NodeTint => Color.red;

        public override string DecorativeName => "Write Variable";
        public override string Description => $"{Key} = {Value}";
        public string Key;
        public bool DoesValueHaveInput = false;
        public string Value;

        public override void SetupNode()
        {
            Variables.Add(new NodeVariable { Key = "Value", VariableType = typeof(string) });
        }

        public override void Run(Action OnDone)
        {
            if (DebugMode)
                Debug.Log($"Run: {name}");

            ParentTree.Data.SetVariable(Key, Value);
            ParentTree.Data.WriteFile(() => { });

            ConnectionDict[0].Target.Run(OnDone);
        }

        public override void DrawInspector()
        {
#if UNITY_EDITOR
            Key = EditorGUILayout.TextField(Key);
            GUI.enabled = !DoesValueHaveInput;
            Value = EditorGUILayout.TextField(Value);
            GUI.enabled = true;
#endif
        }
    }
}