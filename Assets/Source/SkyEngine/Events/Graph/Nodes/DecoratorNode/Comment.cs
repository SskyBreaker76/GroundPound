using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SkySoft.Events.Graph
{
    public class Comment : DecoratorNode
    {
        public override Color NodeTint => CommentColour;
        public override bool IsPure => true;
        public string Text;
        public Color CommentColour = Color.green;

        public override string DecorativeName => "Comment";
        public override string Description => $"<color=#{ColorUtility.ToHtmlStringRGB(CommentColour)}>/*\n{Text}\n*/</color>";

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
            GUILayout.Label("Comment Text");
            Text = EditorGUILayout.TextArea(Text);
            CommentColour = EditorGUILayout.ColorField(new GUIContent("Colour"), CommentColour, false, true, false);
#endif
        }
    }
}