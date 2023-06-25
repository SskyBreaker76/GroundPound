using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[Obsolete("SkyEngine no longer supports the ExposeScript Attribute!")]
[CustomPropertyDrawer(typeof(ExposeScriptableObjectAttribute))]
public class ExposeScriptableObjectAttributeDrawer : PropertyDrawer
{
    private bool Foldout;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        GUI.Label(position, "This attribute is no longer supported!");
    }
}
