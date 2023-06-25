using SkySoft;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(DisplayOnlyAttribute))]
public class DisplayOnlyAttributeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        GUI.enabled = false;
        EditorGUI.PropertyField(position, property, label);
        GUI.enabled = true;
    }
}
