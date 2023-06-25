using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SkySoft.Events.Graph
{
    public class ReadVariable : DefinitionNode
    {
        public override Color NodeTint => Color.red;
        public override string DecorativeName => "Read Variable";
        public override string Description => Key;
        public override string Value 
        { 
            get
            {
                if (!string.IsNullOrEmpty(Key))
                    return ParentTree.Data.GetVariable(Key);

                return "";
            } 
        }

        public string Key;
        public override Type NodeType => typeof(string);

        public override void DrawInspector()
        {
#if UNITY_EDITOR
            Key = EditorGUILayout.DelayedTextField("Key", Key);
#endif
        }
    }
}