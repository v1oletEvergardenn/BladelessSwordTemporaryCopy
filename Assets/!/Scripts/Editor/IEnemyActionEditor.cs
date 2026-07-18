using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEditor.Timeline;

[CustomEditor(typeof(IEnemyAction), true)]
public class IEnemyActionEditor : OdinEditor
{
    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();
        DrawDefaultInspector();
        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            TimelineEditor.Refresh(RefreshReason.ContentsModified);
        }
    }
}