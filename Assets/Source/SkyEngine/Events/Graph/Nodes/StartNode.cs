using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SkySoft.Events.Graph
{
    public class StartNode : Node
    {
        public override Color NodeTint => Color.green;
        public EventTree TargetTree;
        public override string DecorativeName => $"Event Start";
        public Node Child;
        [Obsolete]
        public string Title;
        [Obsolete]
        public string Identifier;
        public string Name { get; private set; }
        private string m_Description;
        public override string Description { get => m_Description; }

        public override void SetupNode()
        {
            Variables.Add(new NodeVariable { Key = Name, ValueType = null });
        }

        public override async void Run(Action OnDone)
        {
            EventGraphManager.IsProcessingEvent = true;
            bool LoadedVariables = false;
            ParentTree.UpdateFlow(); // This initializes all values

            // Since the EventGraphManager now handles the execution of events we don't need to worry about the Data files EventName not being unique as it
            // can now read from the Entity. I'll probably need to add some extra things for SubEvents but for now I'm not doing SubEvents inside SubEvents
            // and will probably just block the user from executing SubEvents with SubEvent nodes in them just to be on the safe side
            // ParentTree.Data.EventName = GUID; // Rather than using easy-to-read event names, a GUID provides a string that is always unique
            ParentTree.Data.ReadFile(() => LoadedVariables = true);

            while (!LoadedVariables)
                await Task.Delay(10);

            if (Returns == null)
                Debug.Log("Returns is null");
            if (Returns[0].Value == null)
                Debug.Log("Returns[0] is null");
            if (OnDone == null)
                Debug.Log("OnDone is null");

            Connections[0].Target.Run(OnDone);
        }

        protected override NodeState OnUpdate()
        {
            return Child.Update();
        }

        public override void DrawInspector()
        {
#if UNITY_EDITOR
            Name = EditorGUILayout.TextField("Event Name", Name);
            GUILayout.Label("Description");
            m_Description = GUILayout.TextArea(m_Description);
#endif
        }
    }
}