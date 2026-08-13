using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ObjectiveKeyActionDropdownAttribute))]
public class ObjectiveKeyActionDropdownDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var attr = (ObjectiveKeyActionDropdownAttribute)attribute;
        SerializedProperty keyProp = GetSiblingProperty(property, attr.KeyFieldName);

        if (keyProp == null)
        {
            EditorGUI.PropertyField(position, property, label);
            return;
        }

        QuestActionKey key = keyProp.objectReferenceValue as QuestActionKey;
        if (key == null)
        {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUI.Popup(position, label.text, 0, new[] { "(Assign objectiveKey first)" });
            EditorGUI.EndDisabledGroup();
            property.stringValue = string.Empty;
            return;
        }

        List<(string name, string value)> entries = GetStringEntries(key);
        if (entries.Count == 0)
        {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUI.Popup(position, label.text, 0, new[] { "(No actions found)" });
            EditorGUI.EndDisabledGroup();
            property.stringValue = string.Empty;
            return;
        }

        string[] display = entries.Select(e => e.name).ToArray();
        string[] values = entries.Select(e => e.value).ToArray();

        int current = Array.IndexOf(values, property.stringValue);
        if (current < 0) current = 0;

        EditorGUI.BeginProperty(position, label, property);
        int next = EditorGUI.Popup(position, label.text, current, display);
        property.stringValue = values[next];
        EditorGUI.EndProperty();
    }

    private static List<(string name, string value)> GetStringEntries(QuestActionKey key)
    {
        const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
        Type type = key.GetType();
        var list = new List<(string name, string value)>();

        // Fields
        FieldInfo[] fields = type.GetFields(flags)
            .Where(f => f.FieldType == typeof(string))
            .ToArray();

        for (int i = 0; i < fields.Length; i++)
        {
            object raw = fields[i].IsStatic ? fields[i].GetValue(null) : fields[i].GetValue(key);
            string value = raw as string;
            if (string.IsNullOrEmpty(value)) continue;
            list.Add((fields[i].Name, value));
        }

        // Properties
        PropertyInfo[] props = type.GetProperties(flags)
            .Where(p => p.PropertyType == typeof(string) && p.CanRead && p.GetIndexParameters().Length == 0)
            .ToArray();

        for (int i = 0; i < props.Length; i++)
        {
            MethodInfo getter = props[i].GetGetMethod();
            if (getter == null) continue;

            object raw = getter.IsStatic ? props[i].GetValue(null) : props[i].GetValue(key);
            string value = raw as string;
            if (string.IsNullOrEmpty(value)) continue;
            list.Add((props[i].Name, value));
        }

        return list
            .GroupBy(e => e.value)
            .Select(g => g.First())
            .ToList();
    }

    private static SerializedProperty GetSiblingProperty(SerializedProperty property, string siblingName)
    {
        string path = property.propertyPath;
        int lastDot = path.LastIndexOf('.');
        if (lastDot < 0) return null;

        string parentPath = path.Substring(0, lastDot);
        return property.serializedObject.FindProperty(parentPath + "." + siblingName);
    }
}