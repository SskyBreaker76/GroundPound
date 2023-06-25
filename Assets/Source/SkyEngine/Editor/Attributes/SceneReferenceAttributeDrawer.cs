using SkySoft;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(SceneReferenceAttribute))]
public class SceneReferenceAttributeDrawer : PropertyDrawer
{
    private bool HasGotAllScenes;
    private List<string> Scenes = new List<string>();
    private List<string> SceneKeys = new List<string>();
    
    public override void OnGUI(Rect Position, SerializedProperty Property, GUIContent Label)
    {
        if (Property.propertyType == SerializedPropertyType.String)
        {
            if (Scenes.Count != SkyEngine.Levels.Scenes.Count)
            {
                Scenes.Clear();

                foreach (string Key in SkyEngine.Levels.Scenes.Keys)
                {
                    Scenes.Add($"{Key} ({SkyEngine.Levels.GetDisplayName(Key)})");
                    SceneKeys.Add(Key);
                }
            }

            if (SceneKeys.Contains(Property.stringValue))
            {
                Property.stringValue = SceneKeys[EditorGUI.Popup(Position, Label.text, SceneKeys.IndexOf(Property.stringValue), Scenes.ToArray())];
            }
            else
            {
                Property.stringValue = SceneKeys[0];
            }
        }
    }
}
