using System;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace KabreetGames.ParallaxSystem
{
    [CustomEditor(typeof(ParallaxManager))]
    public class ParallaxManagerEditor : Editor
    {
        private SerializedProperty propOffset;
        private SerializedProperty propCamera;
        private SerializedProperty propPlanes;
        private SerializedProperty propParent;

        private SerializedProperty backgroundLayerProperty;
        private SerializedProperty midgroundLayerProperty;
        private SerializedProperty foregroundLayerProperty;
        private SerializedProperty backgroundStartSortLayer;
        private SerializedProperty midgroundStartSortLayer;
        private SerializedProperty foregroundStartSortLayer;

        private bool clearOldPlanes;
        private string[] layerNames;
        private int[] layerIDs;
        private GUIStyle myFoldoutStyle;
        private bool hasChanges = false;


        private int activeWindow;
        private int mainNum;
        private Plane mainPlane;
        private Texture2D icon;

        private ParallaxManager Manager => target as ParallaxManager;

        private void OnEnable()
        {
            Load();
            LoadProperties();
            Manager.cameraObject = Camera.main;
            layerNames = GetSortingLayerNames();
            layerIDs = GetSortingLayerUniqueIDs();
        }

        private void LoadProperties()
        {
            propCamera = serializedObject.FindProperty("cameraObject");
            propOffset = serializedObject.FindProperty("xyOffset");

            propPlanes = serializedObject.FindProperty("planes");
            propParent = serializedObject.FindProperty("planesParent");

            backgroundLayerProperty = serializedObject.FindProperty("backgroundLayer");
            backgroundStartSortLayer = serializedObject.FindProperty("backgroundSortLayer");

            midgroundLayerProperty = serializedObject.FindProperty("midgroundLayer");
            midgroundStartSortLayer = serializedObject.FindProperty("midgroundSortLayer");

            foregroundLayerProperty = serializedObject.FindProperty("foregroundLayer");
            foregroundStartSortLayer = serializedObject.FindProperty("foregroundSortLayer");
        }

        private void OnDisable()
        {
            Save();
        }

        private void Load()
        {
            activeWindow = EditorPrefs.GetInt("activeWindowKey", 0);
            clearOldPlanes = EditorPrefs.GetBool("overwriteKey", false);
            icon = Resources.Load<Texture2D>("Ultimate 2D Parallax System Icon");
        }


        private void Save()
        {
            EditorPrefs.SetInt("activeWindowKey", activeWindow);
            EditorPrefs.SetBool("overwriteKey", clearOldPlanes);
        }

        public override void OnInspectorGUI()
        {
            myFoldoutStyle = new GUIStyle(EditorStyles.label)
            {
                fontStyle = FontStyle.Bold,
                richText = true,
                fontSize = 14,
                margin = new RectOffset(15, 0, 0, 0),
                normal =
                {
                    textColor = Color.white
                },
            };
            serializedObject.Update();


            DrawLabelHeader();
            EditorGUILayout.BeginHorizontal();

            GUI.enabled = activeWindow != 0;

            if (GUILayout.Button("Render Settings"))
            {
                activeWindow = 0;
            }

            GUI.enabled = activeWindow != 1;

            if (GUILayout.Button("Planes List"))
            {
                activeWindow = 1;
            }

            GUI.enabled = activeWindow != 2;
            if (GUILayout.Button("Extract Planes"))
            {
                activeWindow = 2;
            }

            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();

            EditorGUI.BeginChangeCheck();

            switch (activeWindow)
            {
                case 0:
                    DrawRenderSettings();
                    break;
                case 1:
                    DrawAllPlanes();
                    break;
                case 2:
                    ExtractPlanes();
                    break;
            }

            if (EditorGUI.EndChangeCheck())
            {
                hasChanges = true;
            }

            EditorGUILayout.Space(30);
            GUI.enabled = Manager.planes.Count != 0;
            if (GUILayout.Button(hasChanges ? "Sort *" : "Sort"))
            {
                Sort();
            }

            GUI.enabled = true;


            using (new EditorGUI.DisabledGroupScope(!Manager.sorted))
            {
                if (GUILayout.Button("Clean inspector"))
                {
                    CleanInspector();
                }
            }


            serializedObject.ApplyModifiedProperties();
        }

        private void ExtractPlanes()
        {
            var content = new GUIContent("Extract Planes",
                "Extract the Manager.planes from the parent and add them to the list");
            EditorGUILayout.LabelField(content, myFoldoutStyle);

            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.BeginHorizontal();
            clearOldPlanes = EditorGUILayout.Toggle(clearOldPlanes, GUILayout.Width(15));
            EditorGUILayout.LabelField("Clear old Manager.planes", EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.LabelField(
                clearOldPlanes
                    ? "It will delete the old Manager.planes to add the new ones"
                    : "It will add the new Manager.planes on top of the old ones",
                EditorStyles.miniBoldLabel);

            EditorGUILayout.PropertyField(propParent);
            EditorGUILayout.EndVertical();
            var hasValidParent = Manager.planesParent != null && Manager.planesParent.transform.childCount != 0;
            GUI.enabled = hasValidParent;
            var extractContent = new GUIContent("Extract",
                hasValidParent
                    ? "Extract the Manager.planes from the parent to the list"
                    : "Select a parent with at least one child first");
            if (GUILayout.Button(extractContent))
            {
                Extract();
            }

            GUI.enabled = true;
        }

        private void DrawAllPlanes()
        {
            var content = new GUIContent("Planes List", "But in this Array all your Manager.planes game objects");
            var controlRect = EditorGUILayout.GetControlRect(true, EditorGUI.GetPropertyHeight(propPlanes, true));
            EditorGUI.PropertyField(controlRect, propPlanes, GUIContent.none, true);
            EditorGUI.LabelField(new Rect(controlRect.x, controlRect.y, 100, 18), content, myFoldoutStyle);

            GUI.enabled = Manager.planes.Count != 0;
            if (GUILayout.Button("Clear List"))
            {
                Clear();
            }

            GUI.enabled = true;
        }

        private void DrawRenderSettings()
        {
            var content = new GUIContent("Render Layers Settings",
                "Define your camera and render layers and sort orders");
            EditorGUILayout.LabelField(content, myFoldoutStyle);
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.PropertyField(propCamera);
            DrawRenderLayer(backgroundLayerProperty, backgroundStartSortLayer, "Background Layer");
            DrawRenderLayer(midgroundLayerProperty, midgroundStartSortLayer, "Midground Layer");
            DrawRenderLayer(foregroundLayerProperty, foregroundStartSortLayer, "Foreground Layer");
            EditorGUILayout.PropertyField(propOffset);
            EditorGUILayout.EndVertical();
        }


        private void DrawRenderLayer(SerializedProperty layerID, SerializedProperty startSortLayer, string label)
        {
            EditorGUILayout.BeginHorizontal();
            var backgroundLayerID = GetLayerIDForLayerInt(layerID);
            backgroundLayerID = EditorGUILayout.Popup(label, backgroundLayerID, layerNames);
            startSortLayer.intValue = EditorGUILayout.IntField(startSortLayer.intValue, GUILayout.Width(70));
            layerID.intValue = layerIDs[backgroundLayerID];
            EditorGUILayout.EndHorizontal();
        }

        private int GetLayerIDForLayerInt(SerializedProperty layer)
        {
            var sID = layer.intValue;
            var selectedLayer = Array.IndexOf(layerIDs, sID);

            // If not found, find the default layer (ID = 0)
            if (selectedLayer == -1)
            {
                selectedLayer = Array.IndexOf(layerIDs, 0);
            }

            return selectedLayer;
        }

        private void DrawLabelHeader()
        {
            var centeredStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.UpperCenter,
                richText = true,
                fontSize = 27,
                fontStyle = FontStyle.Bold,
                normal = new GUIStyleState()
                {
                    textColor = new Color(0.92f, 0.92f, 0.92f)
                }
            };
            const float labelWidth = 500f;

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(new GUIContent("Ultimate 2D Parallax Manager", icon), centeredStyle,
                    GUILayout.Width(labelWidth),
                    GUILayout.Height(35)))
            {
                Application.OpenURL("https://assetstore.unity.com/packages/slug/214502");
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            centeredStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.UpperCenter,
                richText = true,
                fontStyle = FontStyle.Bold,
            };

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField($"Planes Count: {propPlanes.arraySize}", centeredStyle,
                GUILayout.Width(labelWidth));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private string[] GetSortingLayerNames()
        {
            var internalEditorUtilityType = typeof(InternalEditorUtility);
            var sortingLayersProperty =
                internalEditorUtilityType.GetProperty("sortingLayerNames",
                    BindingFlags.Static | BindingFlags.NonPublic);
            return sortingLayersProperty is null
                ? Array.Empty<string>()
                : (string[])sortingLayersProperty.GetValue(null, Array.Empty<object>());
        }

        private int[] GetSortingLayerUniqueIDs()
        {
            var internalEditorUtilityType = typeof(InternalEditorUtility);
            var sortingLayerUniqueIDsProperty = internalEditorUtilityType.GetProperty("sortingLayerUniqueIDs",
                BindingFlags.Static | BindingFlags.NonPublic);
            return sortingLayerUniqueIDsProperty is null
                ? Array.Empty<int>()
                : (int[])sortingLayerUniqueIDsProperty.GetValue(null, Array.Empty<object>());
        }

        private void Extract()
        {
            Undo.RecordObject(Manager, "Extract Manager.planes form parent");
            if (clearOldPlanes)
            {
                CleanInspector();
            }

            for (var i = 0; i < Manager.planesParent.transform.childCount; i++)
            {
                var plane = new Plane { originalPlane = Manager.planesParent.transform.GetChild(i).gameObject };
                Manager.planes.Add(plane);
            }
        }

        private void Clear()
        {
            if (Manager.planes.Count == 0) return;
            Undo.RecordObject(Manager, "Clear Planes List");
            ClearPlanList();
        }

        private void CleanInspector()
        {
            Undo.RecordObject(Manager, "Clean Planes From Inspector and List");
            RemoveAllObjectsInTheScene();
        }


        #region Sorting

        private void Sort()
        {
            RemoveAllObjectsInTheScene();
            mainPlane = GetMainPlane();
            if (Manager.cameraObject == null) Manager.cameraObject = Camera.main;
            StartSorting();
        }

        private void StartSorting()
        {
            if (!mainPlane.originalPlane)
            {
                Debug.LogWarning("Main Plane Is null : please set one");
                Failed();
                return;
            }

            Manager.planesHolder = new GameObject("parallax Planes Parent Holder");
            var holder = Manager.planesHolder.AddComponent<PlanesHolder>();
            holder.hideFlags = HideFlags.HideInInspector;

            for (var i = 0; i < Manager.planes.Count; i++)
            {
                if (HandelPlaneCreation(i)) continue;
                Clear();
                return;
            }

            SuccessSorting();
        }

        private void SuccessSorting()
        {
            Manager.sorted = true;
            if (Manager.planesParent == null)
            {
                foreach (var managerPlane in Manager.planes)
                {
                    managerPlane.originalPlane.SetActive(false);
                }
            }
            else
            {
                Manager.planesParent.SetActive(false);
            }

            Debug.Log("Sort Successfully");
            hasChanges = false;
        }

        private bool HandelPlaneCreation(int i)
        {
            if (!Manager.planes[i].originalPlane)
            {
                Debug.LogWarning($"The Plane at slot {i} is null");
                Failed();
                return false;
            }

            var planeParent = GetPlaneParent(i, Manager.planesHolder);

            GetFirstSupPlane(i, planeParent);
            GetPlaneLength(i);


            if (Manager.cameraObject is not null)
            {
                if (mainPlane == null)
                {
                    Debug.LogError("You did not attach main Plane (main Plane that player moves in layer)");
                    return false;
                }

                SetStartPositionAndRotation(i);
                var clipPlane = GetClipPlane(i);
                var parallaxFactor = CalcParallaxFactor(clipPlane, i);
                SetFarPlaneToStatic(parallaxFactor, i);
                Manager.cameraStartPos =
                    Manager.cameraObject.transform.TransformPoint(0, 0, Manager.cameraObject.farClipPlane);
                Manager.cameraStartRotation = Manager.cameraObject.transform.rotation;
            }

            if (!Manager.planes[i].doNotRepeat)
            {
                AddExtraSupPlanes(i, planeParent);
            }

            return true;
        }

        private void GetPlaneLength(int i)
        {
            if (Manager.planes[i].supPlane0.TryGetComponent<Renderer>(out var rend))
            {
                Manager.planes[i].length = (Manager.planes[i].useBackgroundLengthToRepeat &&
                                            Manager.planes[0].length != 0 && i != 0)
                    ? Manager.planes[0].length
                    : Manager.planes[i].repeatY
                        ? rend.bounds.size.y
                        : rend.bounds.size.x;
                SetSortLayer(i, rend);
            }
            else
            {
                Manager.planes[i].length = Manager.planes[i].repeatY
                    ? Manager.planes[i].supPlane0.transform.lossyScale.y
                    : Manager.planes[i].supPlane0.transform.lossyScale.x;
            }
        }

        private void GetFirstSupPlane(int i, Transform planeParent)
        {
            var originalPlane = Manager.planes[i].originalPlane;
            Manager.planes[i].supPlane0 = Instantiate(originalPlane, originalPlane.transform.position,
                originalPlane.transform.rotation,
                planeParent);
            Manager.planes[i].supPlane0.name = $"{originalPlane.name} sup 0";
        }

        private Transform GetPlaneParent(int i, GameObject parent)
        {
            Transform planeParent;
            if (Manager.planes[i].parent == null ||
                Manager.planes[i].originalPlane.transform.parent != Manager.planes[i].parent)
            {
                var str = Manager.planes[i].isMain
                    ? $"{Manager.planes[i].originalPlane.name} Holder ( Main )"
                    : $"{Manager.planes[i].originalPlane.name} Holder";
                planeParent = new GameObject(str).transform;
                if (parent) planeParent.parent = parent.transform;
                Manager.planes[i].parent = planeParent;
            }
            else
            {
                planeParent = Manager.planes[i].parent;
            }

            return planeParent;
        }

        private void SetSortLayer(int i, Renderer rend)
        {
            if (i < mainNum)
            {
                rend.sortingLayerID = Manager.backgroundLayer;
                rend.sortingOrder = i + Manager.backgroundSortLayer;
            }
            else if (i == mainNum)
            {
                rend.sortingLayerID = Manager.midgroundLayer;
                rend.sortingOrder = i + Manager.midgroundSortLayer;
            }
            else
            {
                rend.sortingLayerID = Manager.foregroundLayer;
                rend.sortingOrder = i + Manager.foregroundSortLayer;
            }
        }

        private void SetStartPositionAndRotation(int i)
        {
            var savePos = Manager.planes[i].saveXyPos;

            var distanceFromCamera = Manager.planes[i].distanceFromCamera;
            var cameraCenter =
                Manager.cameraObject.transform.position + Manager.cameraObject.transform.forward * distanceFromCamera;
            if (Mathf.Approximately(cameraCenter.z, Manager.cameraObject.farClipPlane)) cameraCenter.z -= 1f;
            Vector3 offsetPosition;
            if (savePos)
            {
                offsetPosition = new Vector3(Manager.planes[i].originalPlane.transform.position.x,
                    Manager.planes[i].originalPlane.transform.position.y,
                    cameraCenter.z);
                Debug.Log(offsetPosition);
            }
            else
            {
                var xyOffsetWorld =
                    Manager.cameraObject.transform.right * Manager.xyOffset.x +
                    Manager.cameraObject.transform.up * Manager.xyOffset.y;
                offsetPosition = cameraCenter + xyOffsetWorld;
            }

            Manager.planes[i].startPos = offsetPosition;
            Manager.planes[i].parent.transform.position = offsetPosition;
            Manager.planes[i].supPlane0.transform.position = offsetPosition;
            Manager.planes[i].supPlane0.transform.LookAt(Manager.cameraObject.transform.position);
            Manager.planes[i].supPlane0.transform.rotation =
                Quaternion.LookRotation(Manager.cameraObject.transform.forward);
            if (Mathf.Approximately(distanceFromCamera, Manager.cameraObject.farClipPlane))
            {
                Manager.planes[i].supPlane0.transform.position -= Manager.planes[i].supPlane0.transform.forward;
            }
        }


        private float GetClipPlane(int i)
        {
            Manager.planes[i].distanceFromMainPlane =
                Manager.planes[i].distanceFromCamera - mainPlane.distanceFromCamera;

            var mainDistanceFromCamera = mainPlane.distanceFromCamera;
            var clipPlane = Manager.planes[i].distanceFromMainPlane > 0
                ? Manager.cameraObject.farClipPlane - mainDistanceFromCamera
                : mainDistanceFromCamera - Manager.cameraObject.nearClipPlane;
            return clipPlane;
        }

        private float CalcParallaxFactor(float clipPlane, int i)
        {
            var parallaxFactor = clipPlane != 0 ? Manager.planes[i].distanceFromMainPlane / clipPlane : 0;
            Manager.planes[i].parallaxFactor = parallaxFactor;
            return parallaxFactor;
        }

        private void SetFarPlaneToStatic(float parallaxFactor, int i)
        {
            if (Math.Abs(parallaxFactor - 1) < 0.0001) Manager.planes[i].doNotRepeat = true;
        }


        private void AddExtraSupPlanes(int i, Transform planeParent)
        {
            var dir = Manager.planes[i].repeatY ? Vector3.up : Vector3.right;
            Manager.planes[i].supPlane1 = Instantiate(Manager.planes[i].supPlane0,
                Manager.planes[i].supPlane0.transform.position + dir * Manager.planes[i].length,
                Manager.planes[i].supPlane0.transform.rotation, planeParent);
            Manager.planes[i].supPlane2 = Instantiate(Manager.planes[i].supPlane1,
                Manager.planes[i].supPlane0.transform.position - dir * Manager.planes[i].length,
                Manager.planes[i].supPlane0.transform.rotation, planeParent);
            Manager.planes[i].supPlane1.name = $"{Manager.planes[i].originalPlane.name} sup 1";
            Manager.planes[i].supPlane2.name = $"{Manager.planes[i].originalPlane.name} sup 2";
        }

        private Plane GetMainPlane()
        {
            mainNum = -1;
            for (var j = 0; j < Manager.planes.Count; j++)
            {
                if (!Manager.planes[j].isMain) continue;
                if (mainNum != -1)
                    Debug.LogWarning(
                        $"You Have main Plane at slot {mainNum} and slot {j} we will Chose originalPlane at slot {j} as main originalPlane ");
                mainNum = j;
                return Manager.planes[j];
            }

            mainNum = Manager.planes[^1].originalPlane is null ? 0 : Manager.planes.Count - 1;
            return Manager.planes[mainNum];
        }

        private void RemoveAllObjectsInTheScene()
        {
            Manager.planesHolder = FindFirstObjectByType<PlanesHolder>()?.gameObject;
            if (!Manager.planesHolder)
            {
                Manager.sorted = false;
                return;
            }

            DestroyImmediate(Manager.planesHolder);
        }

        private void ClearPlanList()
        {
            Manager.planes.Clear();
        }

        private static void Failed()
        {
            Debug.LogError("Sort Failed");
        }

        #endregion


        [MenuItem("GameObject/Parallax/Create Parallax Manager", false, 1)]
        public static void AddManagerToScene()
        {
            var manager = FindAnyObjectByType<ParallaxManager>();
            if (manager != null)
            {
                Debug.Log("You have Parallax Manager In the scene");
                return;
            }

            var game = new GameObject { name = "Parallax Manager" };
            game.AddComponent<ParallaxManager>();
            Undo.RegisterCreatedObjectUndo(game, "Create Parallax Manager");
        }

        [MenuItem("GameObject/Parallax/Create Parallax Manager", true, 1)]
        public static bool AddManagerToSceneValidate()
        {
            return FindAnyObjectByType<ParallaxManager>() == null;
        }
    }
}