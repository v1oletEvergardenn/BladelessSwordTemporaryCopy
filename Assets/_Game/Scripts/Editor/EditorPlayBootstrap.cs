using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR

using UnityEditor;
using UnityEditor.SceneManagement;

#endif

[InitializeOnLoad]
public static class EditorPlayBootstrap
{
#if UNITY_EDITOR
    private const string PersistentSceneName = "PersistentDataScene";

    static EditorPlayBootstrap()
    {
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.ExitingEditMode)
        {
            return;
        }

        Scene activeScene = SceneManager.GetActiveScene();
        if (activeScene.name == "MainMenu" ||
            activeScene.name == "PreLoad" ||
            activeScene.name == PersistentSceneName)
        {
            return;
        }

        Scene persistentScene = SceneManager.GetSceneByName(PersistentSceneName);
        if (!persistentScene.isLoaded)
        {
            string[] sceneGuids = AssetDatabase.FindAssets(PersistentSceneName + " t:Scene");
            if (sceneGuids.Length == 0)
            {
                Debug.LogWarning("[EditorPlayBootstrap] Persistent scene not found: " + PersistentSceneName);
                return;
            }

            string scenePath = AssetDatabase.GUIDToAssetPath(sceneGuids[0]);
            persistentScene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
        }

        if (persistentScene.IsValid() && persistentScene.isLoaded)
        {
            EditorSceneManager.SetActiveScene(persistentScene);
            Debug.Log("[EditorPlayBootstrap] Opened and set active: " + PersistentSceneName);
        }
    }

#endif
}