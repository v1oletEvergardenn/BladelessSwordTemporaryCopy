using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[CustomTimelineEditor(typeof(BossActionClip))]
public class BossActionClipTimelineEditor : ClipEditor
{
    [InitializeOnLoadMethod]
    private static void RegisterSceneGui()
    {
        SceneView.duringSceneGui -= DrawSelectedClipTarget;
        SceneView.duringSceneGui += DrawSelectedClipTarget;
    }

    public override void OnClipChanged(TimelineClip clip)
    {
        SceneView.RepaintAll();
    }

    private static void DrawSelectedClipTarget(SceneView sceneView)
    {
        TimelineClip selectedClip = TimelineEditor.selectedClip;
        if (selectedClip == null) return;
        if (!(selectedClip.asset is BossActionClip clipAsset)) return;
        if (clipAsset.targetPlayer) return;
        if (!clipAsset.showTargetGizmo) return;

        PlayableDirector director = TimelineEditor.inspectedDirector;
        if (director == null) return;

        Vector3 targetPosition = clipAsset.targetWorldPosition;
        string label = clipAsset.targetWorldGizmoLabel;

        if (clipAsset.useTargetTransform)
        {
            Transform target = clipAsset.target.Resolve(director);
            if (target == null) return;

            targetPosition = target.position;
            label = target.gameObject.name;
        }

        Handles.color = clipAsset.targetGizmoColor;
        float size = HandleUtility.GetHandleSize(targetPosition) * 0.2f;

        Handles.SphereHandleCap(0, targetPosition, Quaternion.identity, size, EventType.Repaint);
        Handles.DrawWireDisc(targetPosition, Vector3.up, size * 2f);
        Handles.Label(targetPosition + Vector3.up * (size * 1.5f), label);

        if (clipAsset.useTargetTransform) return;

        EditorGUI.BeginChangeCheck();
        Vector3 newPosition = Handles.PositionHandle(targetPosition, Quaternion.identity);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(clipAsset, "Move Boss Action Target");
            clipAsset.targetWorldPosition = newPosition;
            EditorUtility.SetDirty(clipAsset);
            TimelineEditor.Refresh(RefreshReason.ContentsModified);
        }
    }
}