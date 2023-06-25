using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using SkySoft;

[CustomPropertyDrawer(typeof(ComboAttribute))]
public class ComboAttributeDrawer : PropertyDrawer
{
    string[] Values => ((ComboAttribute)attribute).Options;
    string Label => ((ComboAttribute)attribute).Label;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType == SerializedPropertyType.Integer ||  
            property.propertyType == SerializedPropertyType.Boolean || 
            property.propertyType == SerializedPropertyType.Float)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                    property.intValue = EditorGUI.Popup(position, string.IsNullOrEmpty(Label) ? label.text : Label, property.intValue, Values);
                    break;
                case SerializedPropertyType.Float:
                    property.floatValue = EditorGUI.Popup(position, string.IsNullOrEmpty(Label) ? label.text : Label, (int)property.floatValue, Values);
                    break;
                case SerializedPropertyType.Boolean:
                    property.boolValue = EditorGUI.Popup(position, string.IsNullOrEmpty(Label) ? label.text : Label, property.boolValue ? 1 : 0, Values) == 1;
                    break;
            }
        }
        else
        {
            GUI.Label(position, $"{label.text} must be either an integer, boolean or float to use this attribute!");
        }

        property.serializedObject.ApplyModifiedProperties();
    }
}