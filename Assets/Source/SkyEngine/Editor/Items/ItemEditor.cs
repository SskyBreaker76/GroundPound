using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using SkySoft.Inventory;

[CustomEditor(typeof(Item))]
public class ItemEditor : Editor
{
    public override void OnInspectorGUI()
    {
        Item Obj = (Item)target;

        GUILayout.Label($"Item ID: {Obj.ID}");
        
        GUILayout.BeginHorizontal();
        {
            Obj.Icon = (Sprite)EditorGUILayout.ObjectField(Obj.Icon, typeof(Sprite), false, GUILayout.Width(64), GUILayout.Height(64));
            
            GUILayout.BeginVertical();
            {
                Obj.Name = EditorGUILayout.TextField("Name", Obj.Name);
                Obj.Value = EditorGUILayout.IntField("Value", Obj.Value);
            }
            GUILayout.EndVertical();
        }
        GUILayout.EndHorizontal();

        Obj.Description = GUILayout.TextArea(Obj.Description);
        
    }
}
