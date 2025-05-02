using UnityEngine;
using UnityEditor;

namespace KabreetGames.ParallaxSystem
{
    [CustomPropertyDrawer(typeof(Plane))]
    public class PlaneEditor : PropertyDrawer
    {
        private Rect thisPosition;
        private const int LessLineHeight = 20;
        private const int MoreLineHeight = 64;
        private int marginBetweenFields;


        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var showMore = property.FindPropertyRelative("showMore");
            if (showMore.boolValue)
            {
                ShowMore(position, property, label, showMore);
                return;
            }

            ShowLess(position, property, showMore);
        }

        private static void ShowMore(Rect position, SerializedProperty property, GUIContent label,
            SerializedProperty showMore)
        {
            var horizontalRect =
                RectExtensions.SplitRectHorizontal(
                    new Rect(position.x + 20, position.y, position.width - 10, MoreLineHeight), 4, 10);

            var rect = RectExtensions.MergeRects(horizontalRect[..^1]);

            EditorGUI.PropertyField(rect, property, label, true);
            if (GUI.Button(horizontalRect[3], "less"))
            {
                showMore.boolValue = !showMore.boolValue;
            }
        }

        private static void ShowLess(Rect position, SerializedProperty property, SerializedProperty showMore)
        {
            var isMainPosition = new Rect(position.x, position.y, 10, LessLineHeight);
            AddIsMainToggle(property, isMainPosition);
            var horizontalRect =
                RectExtensions.SplitRectHorizontal(
                    new Rect(position.x + 20, position.y, position.width - 10, LessLineHeight), 4, 10);
            DisplayPropertyNoLabel(horizontalRect[0], property, "originalPlane");
            DisplaySliderProperty(horizontalRect[1], property, "distanceFromCamera");
            var splitRect = RectExtensions.SplitRectHorizontal(horizontalRect[2], 5, 10);
            DisplayPropertyNoLabel(splitRect[0], property, "saveXyPos");
            DisplayPropertyNoLabel(splitRect[1], property, "doNotRepeat");
            DisplayPropertyNoLabel(splitRect[2], property, "useBackgroundLengthToRepeat");
            DisplayPropertyNoLabel(splitRect[3], property, "isStaticY");
            DisplayPropertyNoLabel(splitRect[4], property, "repeatY");

            if (GUI.Button(horizontalRect[3], "more"))
            {
                showMore.boolValue = !showMore.boolValue;
            }
        }

        private static void AddIsMainToggle(SerializedProperty property, Rect isMainPosition)
        {
            EditorGUI.BeginChangeCheck();
            var propertyRelative = property.FindPropertyRelative("isMain");
            var newValue = EditorGUI.Toggle(isMainPosition, GUIContent.none, propertyRelative.boolValue);
            if (EditorGUI.EndChangeCheck())
            {
                if (newValue)
                {
                    var parentObject = property.serializedObject.targetObject as ParallaxManager;
                    if (parentObject != null)
                        foreach (var plane in parentObject.planes)
                            plane.isMain = false;
                }

                property.serializedObject.Update();
                propertyRelative.boolValue = true;
                property.serializedObject.ApplyModifiedProperties();
            }

            var tooltip = $"{propertyRelative.displayName}\n{propertyRelative.tooltip}";
            EditorGUI.LabelField(isMainPosition, new GUIContent("", tooltip));
        }

        private static void DisplayPropertyNoLabel(Rect position, SerializedProperty property, string propertyName)
        {
            var propertyRelative = property.FindPropertyRelative(propertyName);
            EditorGUI.PropertyField(position, propertyRelative, GUIContent.none);
            var tooltip = $"{propertyRelative.displayName}\n{propertyRelative.tooltip}";
            EditorGUI.LabelField(position, new GUIContent("", tooltip));
        }

        private static void DisplayProperty(Rect position, SerializedProperty property, string propertyName)
        {
            var isMainPlane = property.FindPropertyRelative(propertyName);
            EditorGUI.PropertyField(position, isMainPlane);
            EditorGUI.LabelField(position, new GUIContent("", isMainPlane.tooltip));
        }

        private static void DisplaySliderProperty(Rect position, SerializedProperty property, string propertyName)
        {
            if (Camera.main == null) return;
            var propertyRelative = property.FindPropertyRelative(propertyName);
            var min = Camera.main.nearClipPlane;
            var max = Camera.main.farClipPlane;
            EditorGUI.Slider(position, propertyRelative, min, max, GUIContent.none);
            var tooltip = $"{propertyRelative.displayName}\n{propertyRelative.tooltip}";
            EditorGUI.LabelField(position, new GUIContent("", tooltip));
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var showMore = property.FindPropertyRelative("showMore");
            return showMore.boolValue
                ? Mathf.Max(EditorGUI.GetPropertyHeight(property), MoreLineHeight)
                : base.GetPropertyHeight(property, label);
        }
    }
}