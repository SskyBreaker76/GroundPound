using Codice.CM.SEIDInfo;
using SkySoft;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[CustomPropertyDrawer(typeof(CommonTextKeyAttribute))]
public class CommonTextKeyAttributeDrawer : PropertyDrawer
{
    private bool HasGotAllKeys = false;
    private List<string> Keys = new List<string>();

    PopupField<string> KeySelector;

    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        VisualElement Root = new VisualElement();

        if (!HasGotAllKeys)
        {
            Keys.Clear();
            foreach (string Key in SkyEngine.CommonTexts.Keys)
            {
                Keys.Add(Key);
            }
            HasGotAllKeys = true;
        }

        if (attribute is CommonTextKeyAttribute Attribute && property.propertyType == SerializedPropertyType.String)
        {            
            KeySelector = new PopupField<string>("Target", Keys, Keys.Contains(property.stringValue) ? Keys.IndexOf(property.stringValue) : 0);
            
            KeySelector.RegisterValueChangedCallback(Event =>
            {
                property.stringValue = Event.newValue;
                EditorUtility.SetDirty(property.serializedObject.targetObject);
                Undo.RecordObject(property.serializedObject.targetObject, "ModifyKey");
            });
            Root.Add(KeySelector);
        }

        return Root;
    }
}
