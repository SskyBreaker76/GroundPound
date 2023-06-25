using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SkySoft.Events.Graph
{
    public class RandomBranch : Branch
    {
        public override Color NodeTint => Color.magenta;
        public override string DecorativeName => "Random";
        public int TotalBranches = 3;

        public override void SetupNode()
        {
            for (int I = 0; I < TotalBranches; I++)
            {
                Returns.Add(new NodeVariable { Key = I.ToString(), Port = GetPort(I + 1), VariableType = typeof(Node) });
            }
        }

        public override void DrawInspector()
        {
#if UNITY_EDITOR
            TotalBranches = EditorGUILayout.IntField("Branches", TotalBranches);
#endif
        }

        public override void Run(Action OnDone)
        {
            if (DebugMode)
                Debug.Log($"Run: {name}");

            RegenNumber:
            bool HasValue = false;

            foreach (ConnectionInfo Connection in ConnectionDict.Values)
            {
                if (Connection.Target != null)
                    HasValue = true;
            }

            if (!HasValue)
            {
                EventGraphManager.IsProcessingEvent = false;
                OnDone();
                return;
            }
            
            int Target = UnityEngine.Random.Range(0, TotalBranches);
            if (ConnectionDict[Target].Target != null)
            {
                ConnectionDict[Target].Target.Run(OnDone);
            }
            else
            {
                goto RegenNumber;
            }
        }
    }
}