using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace KabreetGames.ParallaxSystem
{
    [InitializeOnLoad]
    public class ParallaxManagerHierarchyWindow
    {
        #region Fields - State
 
        private static Color? backgroundColor;
 
        #endregion
 
        #region Properties

        private static Color BackgroundColor
        {
            get
            {
                if (backgroundColor.HasValue) return backgroundColor.Value;
                var method = typeof(EditorGUIUtility).GetMethod("GetDefaultBackgroundColor", BindingFlags.NonPublic | BindingFlags.Static);
                if (method != null) backgroundColor = (Color)method.Invoke(null, null);

                return backgroundColor ?? Color.black;
            }
        }
 
        #endregion
        static ParallaxManagerHierarchyWindow()
        {
            EditorApplication.hierarchyWindowItemOnGUI -= HierarchyWindowItemOnGUI;
            EditorApplication.hierarchyWindowItemOnGUI += HierarchyWindowItemOnGUI;
        }

        private static void HierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
        {
            var gameObject = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
            if (!gameObject || !gameObject.TryGetComponent(out ParallaxManager _)) return;

            var iconRect = new Rect(selectionRect);
            iconRect.width = iconRect.height;
            EditorGUI.DrawRect(iconRect, BackgroundColor);
            var icon = Resources.Load<Texture2D>("Ultimate 2D Parallax System Icon");
            GUI.DrawTexture(iconRect, icon);
        }
    }
}