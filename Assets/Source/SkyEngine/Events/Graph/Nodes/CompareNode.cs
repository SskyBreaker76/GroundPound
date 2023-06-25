using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SkySoft.Entities;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SkySoft.Events.Graph
{
    public class CompareNode : Node
    {
        public override bool IsPure => true;
        public override Color NodeTint => Color.blue;
        public override string DecorativeName => "Compare";
        public override string Description => $"Value == {ComparedValue}";
        public Type InputType;
        public string InputValue;
        public string ComparedValue = "<DEFAULT_VALUE>";

        public bool Value => InputValue == ComparedValue;

        public override void SetupNode()
        {
            Variables.Add(new NodeVariable { Key = "Value", VariableType = typeof(AnyType) });
            Returns.Add(new NodeVariable { Key = "", VariableType = typeof(bool), Port = GetPort(1), Value = InputValue == ComparedValue });
        }

        public override void DrawInspector()
        {
#if UNITY_EDITOR
            if (InputType != null)
            {
                bool Init = false;

                if (ComparedValue == "<DEFAULT_VALUE>")
                    Init = true;

                try
                {
                    if (InputType == typeof(string))
                    {
                        if (Init)
                            ComparedValue = "";

                        ComparedValue = EditorGUILayout.TextField("Value", ComparedValue);
                    }
                    else if (InputType == typeof(Vector3))
                    {
                        if (Init)
                            ComparedValue = JsonUtility.ToJson(new Vector3());

                        ComparedValue = JsonUtility.ToJson(EditorGUILayout.Vector3Field("Value", JsonUtility.FromJson<Vector3>(ComparedValue)));
                    }
                    else if (InputType == typeof(Entity))
                    {
                        EditorGUILayout.HelpBox("Entity comparisons have not yet been implemented", MessageType.Warning);
                    }
                    else if (InputType == typeof(bool))
                    {
                        if (Init)
                            ComparedValue = "0";

                        ComparedValue = EditorGUILayout.Toggle("Value", ComparedValue == "1") ? "1" : "0";
                    }
                    else if (InputType == typeof(int))
                    {
                        if (Init)
                            ComparedValue = "0";

                        ComparedValue = EditorGUILayout.IntField("Value", int.Parse(ComparedValue)).ToString();
                    }
                    else if (InputType == typeof(float))
                    {
                        if (Init)
                            ComparedValue = "0";

                        ComparedValue = EditorGUILayout.FloatField("Value", float.Parse(ComparedValue)).ToString();
                    }
                }
                catch { ComparedValue = "<DEFAULT_VALUE>"; }
            }
            else
            {
                EditorGUILayout.HelpBox("You must connect an input first!", MessageType.Info);
            }
#endif
        }
    }
}