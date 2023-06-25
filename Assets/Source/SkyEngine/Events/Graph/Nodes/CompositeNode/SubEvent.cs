using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SkySoft.Events.Graph
{
    public class SubEvent : CompositeNode
    {
        public override Color NodeTint => Color.green;
        public EventTree Target;

        public override string DecorativeName => $"{(Target == null ? "<color=red><b>No Event</b></color>" : Target.name.Replace("_", " "))}";
        public override string Description => $"{(Target != null ? (Target.CanEnd ? "" : "<color=red><b>This event can't end!\nMake sure <i>every</b> branch has an End Event node!</b></color>") : "<color=red><b>No Event</b></color>")}";

        public Dictionary<string, ConnectionInfo> SetSwitches = new Dictionary<string, ConnectionInfo>();
        public Dictionary<string, ConnectionInfo> SetVariables = new Dictionary<string, ConnectionInfo>();

        public void SetSwitchConnection(int InputIndex, Node Connector)
        {
            ConnectionInfo Connection = new ConnectionInfo { Target = Connector };
            string Key = Variables[InputIndex].Key;
            if (SetSwitches.ContainsKey(Key))
            {
                SetSwitches[Key] = Connection;
            }
            else
            {
                SetSwitches.Add(Key, Connection);
            }
        }

        public void SetVariableConnection(int InputIndex, Node Connector)
        {
            ConnectionInfo Connection = new ConnectionInfo { Target = Connector };
            string Key = Variables[InputIndex].Key;
            if (SetVariables.ContainsKey(Key))
            {
                SetVariables[Key] = Connection;
            }
            else
            {
                SetVariables.Add(Key, Connection);
            }
        }

        public override void Run(Action OnDone)
        {
            if (DebugMode)
                Debug.Log($"Run: {name}");

            foreach (string Key in SetSwitches.Keys)
            {
                if (SetSwitches[Key].Target && (SetSwitches[Key].Target is CompareNode || SetSwitches[Key].Target is DefinitionNode))
                {
                    CompareNode AsCompare = SetSwitches[Key].Target as CompareNode;
                    DefinitionNode AsDef = SetSwitches[Key].Target as DefinitionNode;

                    if (AsCompare)
                    {
                        Target.SetSwitchValue(Key, AsCompare.Value);
                    }
                    else if (AsDef)
                    {
                        Target.SetSwitchValue(Key, AsDef.Value == "1");
                    }
                }
            }
            foreach (string Key in SetVariables.Keys)
            {
                if (SetVariables[Key].Target && (SetVariables[Key].Target is DefinitionNode))
                {
                    DefinitionNode AsDef = SetVariables[Key].Target as DefinitionNode;

                    if (AsDef)
                    {
                        Target.SetVariableValue(Key, AsDef.Value);
                    }
                }
            }

            Target.RootNode.Run(() => { ConnectionDict[0].Target.Run(OnDone); });
        }

        public override void SetupNode()
        {
            if (Target)
            {
                foreach (DialogueSwitch SwitchVar in Target.Switches)
                {
                    if (SwitchVar.Exposed)
                        Variables.Add(new NodeVariable { Key = SwitchVar.Key, Value = SwitchVar.Value, VariableType = typeof(bool) });
                }

                foreach (DialogueText TextVar in Target.Variables)
                {
                    if (TextVar.Exposed)
                        Variables.Add(new NodeVariable { Key = TextVar.Key, Value = TextVar.Value, VariableType = typeof(string) });
                }
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
            Target = (EventTree)EditorGUILayout.ObjectField("Event", Target, typeof(EventTree), false);
#endif
        }
    }
}