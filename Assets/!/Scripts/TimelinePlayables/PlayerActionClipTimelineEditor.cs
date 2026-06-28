using System;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[CustomTimelineEditor(typeof(PlayerActionClip))]
public class PlayerActionClipTimelineEditor : ClipEditor
{
    private static readonly Color StartColor = new Color(0.3f, 1.0f, 0.8f, 1.0f);
    private static readonly Color EndColor = new Color(1.0f, 0.8f, 0.2f, 1.0f);

    [InitializeOnLoadMethod]
    private static void RegisterSceneGui()
    {
        SceneView.duringSceneGui -= DrawSelectedClipHandles;
        SceneView.duringSceneGui += DrawSelectedClipHandles;
    }

    public override void OnCreate(TimelineClip clip, TrackAsset track, TimelineClip clonedFrom)
    {
        base.OnCreate(clip, track, clonedFrom);
        TryAutoUpdateClipDuration(clip);
        SceneView.RepaintAll();
    }

    public override void OnClipChanged(TimelineClip clip)
    {
        TryAutoUpdateClipDuration(clip);
        SceneView.RepaintAll();
    }

    private static void DrawSelectedClipHandles(SceneView sceneView)
    {
        TimelineClip selectedClip = TimelineEditor.selectedClip;
        if (selectedClip == null) return;
        if (!(selectedClip.asset is PlayerActionClip clipAsset)) return;

        PlayableDirector director = TimelineEditor.inspectedDirector;
        if (director == null) return;

        bool isMove = clipAsset.actionType == PlayerTimelineActionType.MoveTo;
        bool isTimelineMove = isMove && clipAsset.moveMode != PlayerMoveExecutionMode.LegacyRunToPosition;

        Vector3 startPosition = ResolveStartPosition(clipAsset, director);
        Vector3 endPosition = ResolveEndPosition(clipAsset, director);

        if (isTimelineMove)
        {
            float startSize = HandleUtility.GetHandleSize(startPosition) * 0.18f;
            float endSize = HandleUtility.GetHandleSize(endPosition) * 0.18f;

            Handles.color = StartColor;
            Handles.SphereHandleCap(0, startPosition, Quaternion.identity, startSize, EventType.Repaint);
            Handles.Label(startPosition + Vector3.up * (startSize * 1.5f), "Start");

            Handles.color = EndColor;
            Handles.SphereHandleCap(0, endPosition, Quaternion.identity, endSize, EventType.Repaint);
            Handles.Label(endPosition + Vector3.up * (endSize * 1.5f), "End");

            Handles.DrawLine(startPosition, endPosition);
        }
        else
        {
            float endSize = HandleUtility.GetHandleSize(endPosition) * 0.2f;
            Handles.color = EndColor;
            Handles.SphereHandleCap(0, endPosition, Quaternion.identity, endSize, EventType.Repaint);
            Handles.Label(endPosition + Vector3.up * (endSize * 1.5f), selectedClip.displayName);
        }

        EditorGUI.BeginChangeCheck();

        Vector3 newStart = startPosition;
        Vector3 newEnd = endPosition;

        if (isTimelineMove && !clipAsset.useStartTransform)
            newStart = Handles.PositionHandle(startPosition, Quaternion.identity);

        if (!clipAsset.useEndTransform)
            newEnd = Handles.PositionHandle(endPosition, Quaternion.identity);

        if (!EditorGUI.EndChangeCheck())
            return;

        Undo.RecordObject(clipAsset, "Move Player Action Path Points");

        if (isTimelineMove && !clipAsset.useStartTransform)
            clipAsset.startWorldPosition = newStart;

        if (!clipAsset.useEndTransform)
            clipAsset.endWorldPosition = newEnd;

        EditorUtility.SetDirty(clipAsset);
        TryAutoUpdateClipDuration(selectedClip);
        TimelineEditor.Refresh(RefreshReason.ContentsModified);
    }

    private static void TryAutoUpdateClipDuration(TimelineClip clip)
    {
        if (clip == null) return;
        if (!(clip.asset is PlayerActionClip clipAsset)) return;
        if (clipAsset.actionType != PlayerTimelineActionType.MoveTo) return;
        if (clipAsset.moveMode != PlayerMoveExecutionMode.TimelineFixedSpeed) return;

        PlayableDirector director = TimelineEditor.inspectedDirector;
        if (director == null) return;

        Vector3 startPosition = ResolveStartPosition(clipAsset, director);
        Vector3 endPosition = ResolveEndPosition(clipAsset, director);

        float distance = Vector3.Distance(startPosition, endPosition);
        float speed = Mathf.Max(0.01f, clipAsset.moveSpeed);

        double newDuration = Math.Max(0.05d, distance / speed);
        if (Math.Abs(clip.duration - newDuration) < 0.0001d)
            return;

        clip.duration = newDuration;
        TimelineEditor.Refresh(RefreshReason.ContentsModified);
    }

    private static Vector3 ResolveStartPosition(PlayerActionClip clipAsset, PlayableDirector director)
    {
        if (clipAsset.useStartTransform && clipAsset.moveMode != PlayerMoveExecutionMode.LegacyRunToPosition)
        {
            Transform startTransform = clipAsset.startTarget.Resolve(director);
            if (startTransform != null)
                return startTransform.position;
        }

        return clipAsset.startWorldPosition;
    }

    private static Vector3 ResolveEndPosition(PlayerActionClip clipAsset, PlayableDirector director)
    {
        if (clipAsset.useEndTransform)
        {
            Transform endTransform = clipAsset.endTarget.Resolve(director);
            if (endTransform != null)
                return endTransform.position;
        }

        return clipAsset.endWorldPosition;
    }
}