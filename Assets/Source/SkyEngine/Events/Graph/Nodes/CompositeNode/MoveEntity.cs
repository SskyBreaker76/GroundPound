using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SkySoft.Entities;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SkySoft.Events.Graph
{
    public class MoveEntity : CompositeNode
    {
        public string DestinationGUID;
        public Vector3 Location;
        public float StoppingDistance;

        public override string DecorativeName => "Move Entity";
        public override string Description => "<color=yellow><b>This node is still being developed!</b></color>";

        public override void Run(Action OnDone)
        {
            if (DebugMode)
                Debug.Log($"Run: {name}");

            ConnectionDict[0].Target.Run(OnDone);
        }

        public override void SetupNode()
        {
            Variables.Add(new NodeVariable { Key = "Destination", Value = Vector3.zero, VariableType = typeof(Vector3) });
            Variables.Add(new NodeVariable { Key = "Entity", Value = null, VariableType = typeof(Entity) });
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

        public override void DrawInspector()
        {
#if UNITY_EDITOR
            GUI.enabled = string.IsNullOrEmpty(DestinationGUID);
            Location = EditorGUILayout.Vector3Field($"Destination", Location);
            GUI.enabled = true;
            StoppingDistance = EditorGUILayout.Slider(StoppingDistance, 0.1f, 2f);
#endif
        }
    }
}