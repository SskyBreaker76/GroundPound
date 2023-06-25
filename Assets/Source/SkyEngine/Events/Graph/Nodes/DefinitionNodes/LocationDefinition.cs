using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using SkySoft.Entities;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SkySoft.Events.Graph
{
    public class LocationDefinition : DefinitionNode
    {
        public override Type NodeType => typeof(Vector3);
        public override string Description => $"[{Location.x}, {Location.y}, {Location.z}]";
        public Vector3 Location;
        public override string Value { get => JsonUtility.ToJson(Location); set => Location = JsonUtility.FromJson<Vector3>(value); }
        
        public override void SetupNode()
        {
            Variables.Add(new NodeVariable { Key = "TargetEntity", VariableType = typeof(Entity) });
            Returns.Add(new NodeVariable { Key = "Value", Value = Location, Port = GetPort(1), VariableType = typeof(Vector3) });
        }

        public override Vector3 GetLocation => Location;

        public override void DrawInspector()
        {
#if UNITY_EDITOR
            Label = GUILayout.TextField(Label, EditorStyles.label);
            Location = EditorGUILayout.Vector3Field("Location", Location);
#endif
        }
    }
}