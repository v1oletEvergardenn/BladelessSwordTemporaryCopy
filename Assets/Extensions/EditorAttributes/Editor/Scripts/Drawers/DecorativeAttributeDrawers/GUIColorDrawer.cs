using EditorAttributes.Editor.Utility;
using UnityEngine.UIElements;

namespace EditorAttributes.Editor
{
    public class GUIColorDrawer
    {
        public static void ColorField(VisualElement root, IColorAttribute colorAttribute)
        {
            var errorBox = new HelpBox();

            EditorExtension.GLOBAL_COLOR = ColorUtility.GetColorFromAttribute(colorAttribute, errorBox);
            ColorUtility.ApplyColor(root, colorAttribute, errorBox);
            PropertyDrawerBase.DisplayErrorBox(root, errorBox);
        }
    }
}