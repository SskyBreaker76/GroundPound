using SkySoft.Events.Graph;
using SkySoft.Interaction;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[CustomEditor(typeof(SkyEngineEvent))]
public class SkyEngineEventEditor : Editor
{
    LevelView.EventCache[] AllEvents = { };
    string[] AllEventNames = { };

    SerializedProperty Prop;

    private void OnEnable()
    {
        Prop = serializedObject.FindProperty("OnFinishedEvent");

        AllEvents = SkyEngineDB.GetAllEvents();

        List<string> AllEvs = new List<string> { "None" };
        foreach (LevelView.EventCache Event in AllEvents)
        {
            AllEvs.Add(Event.Name);
        }
        AllEventNames = AllEvs.ToArray();
    }

    private void OnSceneGUI()
    {
        SkyEngineEvent EventCaller = (SkyEngineEvent)target;
        EditorGUI.BeginChangeCheck();
        {
            Vector3 Offset = Handles.PositionHandle(EventCaller.transform.position + EventCaller.IconOffset, EventCaller.IsIconLocal ? EventCaller.transform.rotation : Quaternion.identity);
            EventCaller.IconOffset = Offset - EventCaller.transform.position;
        }
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(EventCaller, "SkyEngineEvent_Changed");
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }
    }

    public override void OnInspectorGUI()
    {
        SkyEngineEvent EventCaller = (SkyEngineEvent)target;

        if (EventCaller)
        {
            EditorGUI.BeginChangeCheck();
            {
                EventCaller.Properties.Name = EditorGUILayout.TextField(new GUIContent("Event Name", "This is only used by Events when they want the name of this Entity"), EventCaller.Properties.Name);
                EditorGUILayout.Space();
                EventCaller.IsIconLocal = EditorGUILayout.Popup("Icon Positioning", EventCaller.IsIconLocal ? 1 : 0, new string[] { "World-Space", "Local-Space" }) == 1;
                EventCaller.IconTargetsPlayer = EditorGUILayout.Popup("Icon Target", EventCaller.IconTargetsPlayer ? 1 : 0, new string[] { "This", "Player" }) == 1;
                EventCaller.Icon = (InteractionType)EditorGUILayout.EnumPopup("Icon", EventCaller.Icon);
                EventCaller.IconOffset = EditorGUILayout.Vector3Field("Icon Offset", EventCaller.IconOffset);
                EditorGUILayout.Space();
                EditorGUILayout.BeginHorizontal();
                {
                    EventCaller.Event = (EventTree)EditorGUILayout.ObjectField("Event", EventCaller.Event, typeof(EventTree), false, GUILayout.ExpandWidth(true));
                    GUI.enabled = !EventCaller.Event;
                    if (GUILayout.Button("New"))
                    {
                        string Path = EditorUtility.SaveFilePanelInProject("Create EventTree", "New_Event", "asset", "Select where you want your new Event saved.");
                        if (!string.IsNullOrEmpty(Path))
                        {
                            EventTree EV = ScriptableObject.CreateInstance<EventTree>();
                            AssetDatabase.CreateAsset(EV, Path);
                            EventCaller.Event = EV;
                        }
                    }
                    GUI.enabled = EventCaller.Event;
                    if (GUILayout.Button("Edit"))
                    {
                        EventTreeEditor.OpenAsset(EventCaller.Event.GetInstanceID(), 0);
                    }
                    GUI.enabled = true;
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(Prop);
            }
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(EventCaller, "SkyEngineEvent_Changed");
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            }
        }
    }
}
