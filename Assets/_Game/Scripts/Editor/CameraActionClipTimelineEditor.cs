using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[CustomTimelineEditor(typeof(CameraActionClip))]
public class CameraActionClipTimelineEditor : ClipEditor
{
    private static readonly Color StartColor = new Color(0.3f, 1.0f, 0.8f, 1.0f);
    private static readonly Color EndColor = new Color(1.0f, 0.8f, 0.2f, 1.0f);
    private static readonly Color MidColor = new Color(0.9f, 0.95f, 1.0f, 1.0f);

    [InitializeOnLoadMethod]
    private static void RegisterSceneGui()
    {
        SceneView.duringSceneGui -= DrawSelectedClipHandles;
        SceneView.duringSceneGui += DrawSelectedClipHandles;
    }

    private static void DrawSelectedClipHandles(SceneView sceneView)
    {
        TimelineClip selectedClip = TimelineEditor.selectedClip;
        if (selectedClip == null) return;
        if (!(selectedClip.asset is CameraActionClip clipAsset)) return;
        if (clipAsset.actionType != CameraTimelineActionType.CamMoveTo) return;

        PlayableDirector director = TimelineEditor.inspectedDirector;
        if (director == null) return;

        DrawMoveHandles(clipAsset, director);
    }

    private static void DrawMoveHandles(CameraActionClip clipAsset, PlayableDirector director)
    {
        Vector3 startPosition = ResolvePosition(clipAsset.useStartTransform, clipAsset.startTarget.Resolve(director), clipAsset.startWorldPosition);
        Vector3 endPosition = ResolvePosition(clipAsset.useEndTransform, clipAsset.endTarget.Resolve(director), clipAsset.endWorldPosition);

        bool showStartHandle = clipAsset.useStartPosition;
        bool canEditStart = showStartHandle && !clipAsset.useStartTransform;
        bool canEditEnd = !clipAsset.useEndTransform;
        bool canMoveBoth = showStartHandle && canEditStart && canEditEnd;

        if (showStartHandle)
        {
            DrawCube(startPosition, StartColor, "Start");
            DrawCube(endPosition, EndColor, "End");
            Handles.color = Color.white;
            Handles.DrawLine(startPosition, endPosition);

            if (canMoveBoth)
            {
                Vector3 mid = (startPosition + endPosition) * 0.5f;
                DrawCube(mid, MidColor, "Move Both");
            }
        }
        else
        {
            DrawCube(endPosition, EndColor, "CamMoveTo");
        }

        EditorGUI.BeginChangeCheck();

        Vector3 newStart = startPosition;
        Vector3 newEnd = endPosition;

        if (canMoveBoth)
        {
            Vector3 mid = (newStart + newEnd) * 0.5f;
            Vector3 newMid = Handles.Slider2D(
                mid,
                Vector3.forward,
                Vector3.right,
                Vector3.up,
                HandleUtility.GetHandleSize(mid) * 0.16f,
                Handles.RectangleHandleCap,
                Vector2.zero
            );

            Vector3 delta = newMid - mid;
            if (delta.sqrMagnitude > 0.0000001f)
            {
                newStart += delta;
                newEnd += delta;
            }
        }

        if (canEditStart)
        {
            Vector3 movedStart = Handles.Slider2D(
                newStart,
                Vector3.forward,
                Vector3.right,
                Vector3.up,
                HandleUtility.GetHandleSize(newStart) * 0.16f,
                Handles.RectangleHandleCap,
                Vector2.zero
            );

            newStart = movedStart;
        }

        if (canEditEnd)
        {
            Vector3 movedEnd = Handles.Slider2D(
                newEnd,
                Vector3.forward,
                Vector3.right,
                Vector3.up,
                HandleUtility.GetHandleSize(newEnd) * 0.16f,
                Handles.RectangleHandleCap,
                Vector2.zero
            );

            newEnd = movedEnd;
        }

        if (!EditorGUI.EndChangeCheck())
            return;

        Undo.RecordObject(clipAsset, "Move Camera Action Path Points");
        if (canEditStart) clipAsset.startWorldPosition = newStart;
        if (canEditEnd) clipAsset.endWorldPosition = newEnd;
        EditorUtility.SetDirty(clipAsset);
        TimelineEditor.Refresh(RefreshReason.ContentsModified);
    }

    private static Vector3 ResolvePosition(bool useTransform, Transform target, Vector3 worldPosition)
    {
        if (useTransform && target != null)
            return target.position;

        return worldPosition;
    }

    private static void DrawCube(Vector3 position, Color color, string label)
    {
        float size = HandleUtility.GetHandleSize(position) * 0.18f;
        Handles.color = color;
        Handles.CubeHandleCap(0, position, Quaternion.identity, size * 0.72f, EventType.Repaint);
        Handles.color = Color.white;
        Handles.DrawWireCube(position, Vector3.one * size * 1.35f);
        Handles.Label(position + Vector3.up * (size * 1.5f), label);
    }
}