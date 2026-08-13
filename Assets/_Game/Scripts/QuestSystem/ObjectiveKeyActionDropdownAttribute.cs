using UnityEngine;

public class ObjectiveKeyActionDropdownAttribute : PropertyAttribute
{
    public string KeyFieldName { get; }

    public ObjectiveKeyActionDropdownAttribute(string keyFieldName)
    {
        KeyFieldName = keyFieldName;
    }
}