using SkySoft.Events.Graph;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class DialogueTester : EditorWindow
{
    private static EventTree CurrentDialogue;

    public static void CreateWindow(int AssetID)
    {
        Object Target = EditorUtility.InstanceIDToObject(AssetID);

        if (Target is EventTree)
        {
            CurrentDialogue = Target as EventTree;
            OpenWindow();
        }
    }

    public static DialogueTester ActiveWindow;

    private static void OpenWindow()
    {
        DialogueTester Wnd = GetWindow<DialogueTester>();
        ActiveWindow = Wnd;
    }

    private bool StartedTest;
    private EventTree.NextNodeInf Node;

    private void OnGUI()
    {
        titleContent = new GUIContent($"Dialogue Tester {(CurrentDialogue != null ? $" - {CurrentDialogue.name}" : "")}");

        if (!StartedTest)
        {
            GUILayout.Label("Click the button below to start testing...");
            if (GUILayout.Button("Begin"))
            {
                if (CurrentDialogue)
                {
                    CurrentDialogue.RestartDialogue();
                    Node = CurrentDialogue.NextNode();
                    StartedTest = true;
                }
                else
                {
                    if (EditorUtility.DisplayDialog("Dialogue Tester", "Unable to test dialogue: No dialogue found", "Close"))
                    {
                        Close();
                    }
                }
            }
        }
        else
        {
            if (!Node.IsChoice)
            {
                EditorGUILayout.BeginVertical(GUI.skin.box);
                EditorGUILayout.LabelField(new GUIContent(Node.SpeakerName));
                EditorGUILayout.Separator();
                GUI.enabled = false;
                GUILayout.TextArea(Node.Dialogue, EditorStyles.label);
                GUI.enabled = true;
                EditorGUILayout.EndVertical();
            }
            if (Node.NextIsChoices || Node.IsChoice)
            {
                EditorGUILayout.BeginHorizontal(GUI.skin.box);
                for (int I = 0; I < Node.Choices.Length; I++)
                {
                    if (GUILayout.Button(Node.Choices[I].Text))
                    {
                        Node = CurrentDialogue.NextNode(Node.Choices[I].ChoiceIndex);
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                if (GUILayout.Button("Next"))
                {
                    Node = CurrentDialogue.NextNode();
                }
            }
        }
    }
}
