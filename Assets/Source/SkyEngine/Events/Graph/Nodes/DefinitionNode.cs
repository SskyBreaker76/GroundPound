using SkySoft.Objects;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SkySoft.Entities;
using System;

#if UNITY_EDITOR
using UnityEditor;
using SerializedObject = SkySoft.Objects.SerializedObject;
#endif

namespace SkySoft.Events.Graph
{
    public class DefinitionNode : Node
    {
        public override Color NodeTint => Color.yellow;
        public override bool IsPure => true;
        public override string DecorativeName => $"Define {NodeType.Name} as {Label}";
        public override string Description => $"{(SoftTarget ? Value : (TargetPlayer ? "Player" : (string.IsNullOrEmpty(TargetID) ? "NULL" : TargetID)))}";
        public virtual Type NodeType { get => SoftTarget ? typeof(string) : typeof(Entity); }
        public bool SoftTarget = false;
        public string Label = $"New Entity";
        public string TargetID;
        private string m_Value = "John Doe";
        public virtual string Value 
        { 
            get 
            {
                if (!SoftTarget)
                    if (Target && Target is Entity)
                        return (Target as Entity).Properties.Name;

                return m_Value;
            } 
            set 
            {
                if (SoftTarget)
                    m_Value = value;
            } 
        }

        public bool TargetPlayer;

        public override void SetupNode()
        {
            Returns.Add(new NodeVariable { Key = "Value", Value = SoftTarget ? Value : (TargetPlayer ? "Player" : (string.IsNullOrEmpty(TargetID) ? "NULL" : TargetID)), Port = GetPort(1), VariableType = NodeType });
        }

        private SerializedObject Target
        {
            get
            {
                SerializedObject V = null;

                foreach (SerializedObject Object in FindObjectsOfType<SerializedObject>())
                {
                    if (Object.InstanceID == TargetID)
                        V = Object;
                }

                if (TargetPlayer)
                    V = SkyEngine.PlayerEntity;

                return V;
            }
        }

        public virtual Vector3 GetLocation
        {
            get
            {
                if (!SoftTarget)
                    if (Target)
                        return Target.transform.position;

                return Vector3.negativeInfinity;
            }
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
            Label = EditorGUILayout.TextField(Label, EditorStyles.label);

            SoftTarget = EditorGUILayout.Popup("Definition Source", SoftTarget ? 1 : 0, new string[] { "Physical Entity", "Imaginary Entity" }) == 1;

            if (!SoftTarget)
            {
                SerializedObject Target = null;

                foreach (SerializedObject Object in FindObjectsOfType<SerializedObject>())
                {
                    if (Object.InstanceID == TargetID)
                    {
                        Target = Object;
                    }
                }

                if (TargetPlayer)
                {
                    if (FindObjectOfType<SerializedObject>())
                    {
                        Target = FindObjectOfType<SerializedObject>();
                    }
                }

                TargetPlayer = EditorGUILayout.Toggle($"Target Player Entity", TargetPlayer);

                if (string.IsNullOrEmpty(TargetID) || Target != null)
                {
                    GUI.enabled = !TargetPlayer;
                    Target = EditorGUILayout.ObjectField("Target", Target, typeof(Entity), true) as Entity;
                    GUI.enabled = true;
                }
                else
                {
                    EditorGUILayout.HelpBox("TargetID is invalid!\nThis could be because Target is in a different scene!", MessageType.Warning);
                }

                if (Target)
                {
                    TargetID = Target.InstanceID;
                    if (Target is Entity)
                        Value = TargetPlayer ? (SkyEngine.PlayerEntity ? SkyEngine.PlayerEntity.Properties.Name : "Player") : (Target as Entity).Properties.Name;
                }

                GUILayout.Label($"Target ID: {TargetID}");
            }
            else
            {
                Value = EditorGUILayout.TextField("Name", Value);
            }
#endif
        }
    }
}