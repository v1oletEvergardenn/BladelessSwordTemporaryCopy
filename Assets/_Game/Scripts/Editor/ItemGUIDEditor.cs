using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class ItemGUIDEditor
{
    private const string LastItemIDKey = "ItemSystem_LastItemID";

    static ItemGUIDEditor()
    {
        EditorApplication.projectChanged += AssignIDsToItems;
    }

    private static void AssignIDsToItems()
    {
        // Add all concrete Item types you use here
        string[] guids = AssetDatabase.FindAssets("t:Item");

        int lastID = EditorPrefs.GetInt(LastItemIDKey, 1);

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var item = AssetDatabase.LoadAssetAtPath<Item>(path);
            if (item != null && item.itemID == 0)
            {
                item.itemID = lastID++;
                EditorUtility.SetDirty(item);
            }
        }

        EditorPrefs.SetInt(LastItemIDKey, lastID);
        AssetDatabase.SaveAssets();
    }
}