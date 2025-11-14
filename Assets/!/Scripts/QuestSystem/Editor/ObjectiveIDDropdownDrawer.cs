using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

[CustomPropertyDrawer(typeof(ObjectiveIDDropdownAttribute))]
public class ObjectiveIDDropdownDrawer : PropertyDrawer
{
    // Cache for type-to-class mapping and field values
    private static readonly Dictionary<ObjectiveType, Type> typeToClass = new();

    private static readonly Dictionary<Type, string[]> classToOptions = new();
    private static bool initialized = false;

    private static void InitializeCache()
    {
        if (initialized) return;
        initialized = true;

        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        var idTypes = assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => t.IsSubclassOf(typeof(ObjectiveIDs)) && !t.IsAbstract);

        foreach (var t in idTypes)
        {
            var instance = Activator.CreateInstance(t) as ObjectiveIDs;
            if (instance != null)
            {
                var objType = instance.ObjectiveType;
                typeToClass[objType] = t;

                var fields = t.GetFields(BindingFlags.Public | BindingFlags.Static)
                    .Where(f => f.FieldType == typeof(string))
                    .ToArray();
                classToOptions[t] = fields.Select(f => (string)f.GetValue(null)).ToArray();
            }
        }
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        InitializeCache();

        // Get the parent object (QuestObjectives)
        var parent = property.serializedObject.targetObject;
        var path = property.propertyPath;
        object questObjective = GetParentObjectOfProperty(path, parent);

        // Get the type field value
        ObjectiveType type = ObjectiveType.Custom;
        var typeField = questObjective.GetType().GetField("type");
        if (typeField != null)
        {
            type = (ObjectiveType)typeField.GetValue(questObjective);
        }

        string[] options;
        if (typeToClass.TryGetValue(type, out var staticClassType) && classToOptions.TryGetValue(staticClassType, out options))
        {
            if (options.Length == 0)
                options = new[] { "(None)" };
        }
        else
        {
            options = new[] { "(None)" };
        }

        int index = Array.IndexOf(options, property.stringValue);
        if (index < 0) index = 0;

        EditorGUI.BeginProperty(position, label, property);
        int newIndex = EditorGUI.Popup(position, label.text, index, options);
        property.stringValue = options[newIndex] == "(None)" ? "" : options[newIndex];
        EditorGUI.EndProperty();
    }

    // Helper to get the parent object of a property
    private object GetParentObjectOfProperty(string path, object obj)
    {
        var elements = path.Replace(".Array.data[", "[").Split('.');
        foreach (var element in elements.Take(elements.Length - 1))
        {
            if (element.Contains("["))
            {
                var elementName = element.Substring(0, element.IndexOf("["));
                var index = Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
                var field = obj.GetType().GetField(elementName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                var list = field.GetValue(obj) as System.Collections.IList;
                obj = list[index];
            }
            else
            {
                var field = obj.GetType().GetField(element, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                obj = field.GetValue(obj);
            }
        }
        return obj;
    }
}