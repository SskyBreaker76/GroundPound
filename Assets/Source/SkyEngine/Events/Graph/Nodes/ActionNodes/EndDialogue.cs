using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SkySoft.Events.Graph
{
    public class EndDialogue : ActionNode
    {
        public override Color NodeTint => Color.red;
        public override string DecorativeName => "End Event";

        public override void Run(Action OnDone)
        {
            EventGraphManager.IsProcessingEvent = false;
            OnDone();
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
            EditorGUILayout.HelpBox("End Node has no properties to edit!", MessageType.Info);
#endif
        }
    }
}