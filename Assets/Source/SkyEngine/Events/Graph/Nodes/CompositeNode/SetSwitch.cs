using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SkySoft.Events.Graph
{
    public class SetSwitch : CompositeNode
    {
        public override Color NodeTint => Color.Lerp(Color.red, Color.yellow, 0.5f);
        public int SwitchID;
        public bool Value;

        public override string DecorativeName => "Set Switch";
        public override string Description => $"{ParentTree.Switches[SwitchID].Key} = {(Value ? "On" : "Off")}";

        public override void Run(Action OnDone)
        {
            if (DebugMode)
                Debug.Log($"Run: {name}");

            ParentTree.Switches[SwitchID].Value = Value;
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
                foreach (DialogueSwitch Switch in ParentTree.Switches)
                {
                    SwitchKeys.Add(Switch.Key);
                }

                int NewSwitchID = EditorGUILayout.Popup("Switch", SwitchID, SwitchKeys.ToArray());
                if (NewSwitchID != SwitchID)
                {
                    Value = ParentTree.Switches[NewSwitchID].Value;
                    SwitchID = NewSwitchID;
                }
                Value = EditorGUILayout.Popup("Value", Value ? 1 : 0, new string[] { "Off", "On" }) == 1;
            }
            else
            {
                EditorGUILayout.HelpBox("ParentTree is null! Press the Compile button to fix this", MessageType.Error);
            }
#endif
        }
    }
}