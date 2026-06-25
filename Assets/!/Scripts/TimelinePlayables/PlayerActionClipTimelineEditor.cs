using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[CustomTimelineEditor(typeof(PlayerActionClip))]
public class PlayerActionClipTimelineEditor : ClipEditor
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
        if (!(selectedClip.asset is PlayerActionClip clipAsset)) return;
        if (!clipAsset.showMoveTargetGizmo) return;

        bool isMove = clipAsset.actionType == PlayerTimelineActionType.MoveTo;
        bool isRepel = clipAsset.actionType == PlayerTimelineActionType.Repel;
        if (!isMove && !isRepel) return;

        PlayableDirector director = TimelineEditor.inspectedDirector;
        if (director == null) return;

        Vector3 targetPosition = clipAsset.moveWorldPosition;
        string label = selectedClip.displayName;

        bool useTransformTarget = isMove && clipAsset.useTargetTransform;
        if (useTransformTarget)
        {
            Transform target = clipAsset.moveTarget.Resolve(director);
            if (target == null) return;

            targetPosition = target.position;
            label = target.gameObject.name;
        }

        Handles.color = clipAsset.moveTargetGizmoColor;
        float size = HandleUtility.GetHandleSize(targetPosition) * 0.2f;

        Handles.SphereHandleCap(0, targetPosition, Quaternion.identity, size, EventType.Repaint);
        Handles.DrawWireDisc(targetPosition, Vector3.up, size * 2f);
        Handles.Label(targetPosition + Vector3.up * (size * 1.5f), label);

        // Move + transform target: visual only, not movable.
        // Repel: always world-position editable.
        if (useTransformTarget) return;

        EditorGUI.BeginChangeCheck();
        Vector3 newPosition = Handles.PositionHandle(targetPosition, Quaternion.identity);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(clipAsset, "Move Player Action Target");
            clipAsset.moveWorldPosition = newPosition;
            EditorUtility.SetDirty(clipAsset);
            TimelineEditor.Refresh(RefreshReason.ContentsModified);
        }
    }
}