using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SkySoft.Events.Graph
{
    public class SpawnObject : CompositeNode
    {
        public override Color NodeTint => Color.magenta * 1.5f;
        public override string DecorativeName => "Spawn Object";
        public override string Description => (Object ? Object.name : "<color=red><b>No Object Set</b></color>");

        public bool UseCurrentEventLocation;
        public Vector3 Location;
        public GameObject Object;

        public override void SetupNode()
        {
            if (!UseCurrentEventLocation)
                Variables.Add(new NodeVariable { Key = "Location", VariableType = typeof(Vector3) });
        }

        public override void Run(Action OnDone)
        {
            if (Object)
            {
                Instantiate(Object, UseCurrentEventLocation ? EventGraphManager.CurrentEvent.transform.position : Location, Object.transform.rotation);
            }

            ConnectionDict[0].Target.Run(OnDone);
        }

        public override void DrawInspector()
        {
#if UNITY_EDITOR
            Object = (GameObject)EditorGUILayout.ObjectField("Object", Object, typeof(GameObject), false);
            GUI.enabled = !(UseCurrentEventLocation = EditorGUILayout.Toggle("Get Location from current Event", UseCurrentEventLocation));
            Location = EditorGUILayout.Vector3Field("Location", Location);
            GUI.enabled = true;
#endif
        }
    }
}