using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ChanceAttribute))]
public class ChanceAttributeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType == SerializedPropertyType.Integer)
        {
            GUI.Label(position, label);
            GUI.Label(new Rect(position.x + 64, position.y, position.width, position.height), "1 in ");
            property.intValue = EditorGUI.IntField(new Rect(position.x + 96, position.y, 64, position.height), property.intValue);
            GUI.Label(new Rect(position.x + (96 + 64), position.y, position.width, position.height), $"({(1f / property.intValue) * 100}%)");
        }
    }
}
