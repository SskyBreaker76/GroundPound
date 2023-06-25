using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using SkySoft;
using System;

[CustomPropertyDrawer(typeof(ButtonAttribute))]
public class ButtonAttributeDrawer : PropertyDrawer
{
    string Label => ((ButtonAttribute)attribute).Label;
    string Tooltip => ((ButtonAttribute)attribute).Tooltip;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (GUI.Button(position, string.IsNullOrEmpty(Label) ? label : new GUIContent(Label, string.IsNullOrEmpty(Tooltip) ? label.tooltip : Tooltip)))
        {
            property.boolValue = true;
            property.serializedObject.ApplyModifiedProperties();
        }
    }
}
