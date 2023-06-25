using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using SerializedObject = SkySoft.Objects.SerializedObject;
using SkySoft.Inventory;
using SkySoft.Events.Graph;

public class InspectorView : VisualElement
{
    public new class UxmlFactory : UxmlFactory<InspectorView, VisualElement.UxmlTraits> { }

    public InspectorView()
    {

    }

    public GUIStyle Heading
    {
        get
        {
            GUIStyle Output = new GUIStyle(GUI.skin.label);
            Output.alignment = TextAnchor.MiddleCenter;
            Output.fontSize = 14;
            Output.fontStyle = FontStyle.Bold;
            return Output;
        }
    }

    public GUIStyle WarningLabel
    {
        get
        {
            GUIStyle Output = new GUIStyle(GUI.skin.label);
            Output.normal.textColor = Color.yellow;
            return Output;
        }
    }

    bool DebugFoldout = false;

    internal void UpdateSelection(NodeView NodeView)
    {
        Clear();
        Label L = new Label($"<b>{NodeView.Node.DecorativeName}</b>");

        L.style.fontSize = 14;
        L.style.backgroundColor = new StyleColor(NodeView.BaseColour * NodeView.Node.NodeTint);
        L.style.paddingBottom = 4;
        L.style.paddingLeft = 4;
        L.style.paddingRight = 4;
        L.style.paddingTop = 4;
        L.style.unityTextAlign = TextAnchor.MiddleCenter;
        Add(L);

        IMGUIContainer Container = new IMGUIContainer(() => 
        {
            EditorGUILayout.Space();

            StartNode Start = NodeView.Node as StartNode;
            ShowText Text = NodeView.Node as ShowText;
            EndDialogue End = NodeView.Node as EndDialogue;

            if (EventTreeEditor.ActiveEditor.DebugMode.value == true)
            {
                if (DebugFoldout = EditorGUILayout.Foldout(DebugFoldout, "Debug"))
                {
                    Editor E = Editor.CreateEditor(NodeView.Node);
                    E.DrawDefaultInspector();
                }
            }

            EditorGUI.BeginChangeCheck();
            NodeView.Node.DrawInspector();

            if (EditorGUI.EndChangeCheck())
            {
                EventTreeEditor.ActiveEditor.MarkDirty();
                EventTreeEditor.ActiveEditor.TreeView.Populate(EventTreeEditor.ActiveEditor.TreeView.Tree);
            }

            SubEvent SubEventNode = NodeView.Node as SubEvent;
            if (SubEventNode)
            {
                if (GUILayout.Button("Open"))
                {
                    if (EditorUtility.DisplayDialog("Open SubEvent Graph", "This will exit your current graph, are you sure you want to open it?", "Yes", "No"))
                    {
                        EventTreeEditor.ActiveEditor.SaveChanges();
                        EventTreeEditor.OpenAsset(SubEventNode.Target.GetInstanceID(), 0);
                    }
                }
            }
        });
        Add(Container);
    }

    private Vector2 Scroll;
    private Vector2 Scroll2;

    internal void UpdateSelection(EventTree Tree)
    {
        Clear();
        IMGUIContainer Container = new IMGUIContainer(() =>
        {
            EditorGUI.BeginChangeCheck();

            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label("Switches", Heading);
            Scroll = EditorGUILayout.BeginScrollView(Scroll, GUILayout.Height(contentRect.height / 3));
            {
                for (int I = 0; I < Tree.Switches.Count; I++)
                {
                    EditorGUILayout.BeginVertical(GUI.skin.box);
                    {
                        // Tree.Switches[I].Exposed = EditorGUILayout.Toggle("Exposed", Tree.Switches[I].Exposed);
                        EditorGUILayout.BeginHorizontal();
                        if (GUILayout.Button("Remove"))
                        {
                            Tree.Switches.RemoveAt(I);
                            break;
                        }
                        Tree.Switches[I].Key = EditorGUILayout.TextField(Tree.Switches[I].Key);
                        EditorGUILayout.EndHorizontal();
                        Tree.Switches[I].Value = EditorGUILayout.Popup("Default Value", Tree.Switches[I].Value ? 1 : 0, new string[] { "Off", "On" }, EditorStyles.toolbarPopup) == 1;
                    }
                    EditorGUILayout.EndVertical();
                }
            }
            EditorGUILayout.EndScrollView();
            if (GUILayout.Button("Add"))
            {
                Tree.Switches.Add(new DialogueSwitch());
            }
            GUILayout.EndVertical();
            EditorGUILayout.Space();

            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label("Variables", Heading);
            Scroll2 = EditorGUILayout.BeginScrollView(Scroll2, GUILayout.Height(contentRect.height / 3));
            {
                for (int I = 0; I < Tree.Variables.Count; I++)
                {
                    EditorGUILayout.BeginVertical(GUI.skin.box);
                    {
                        Tree.Variables[I].Exposed = EditorGUILayout.Toggle("Exposed", Tree.Variables[I].Exposed);
                        EditorGUILayout.BeginHorizontal();
                        if (GUILayout.Button("Remove"))
                        {
                            Tree.Variables.RemoveAt(I);
                            break;
                        }
                        Tree.Variables[I].Key = EditorGUILayout.TextField(Tree.Variables[I].Key);
                        EditorGUILayout.EndHorizontal();
                        Tree.Variables[I].Value = EditorGUILayout.TextField("Default Value", Tree.Variables[I].Value);
                    }
                    EditorGUILayout.EndVertical();
                }
            }
            EditorGUILayout.EndScrollView();
            if (GUILayout.Button("Add"))
            {
                Tree.Variables.Add(new DialogueText());
            }
            GUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                EventTreeEditor.ActiveEditor.MarkDirty();
            }
        });
        Add(Container);
    }
}