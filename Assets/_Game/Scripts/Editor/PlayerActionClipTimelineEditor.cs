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
    private static readonly Color MidColor = new Color(0.9f, 0.95f, 1.0f, 1.0f);

    private static GUIStyle _centeredLabelStyle;

    private static GUIStyle CenteredLabelStyle
    {
        get
        {
            if (_centeredLabelStyle != null)
                return _centeredLabelStyle;

            _centeredLabelStyle = new GUIStyle(EditorStyles.boldLabel);
            _centeredLabelStyle.alignment = TextAnchor.MiddleCenter;
            return _centeredLabelStyle;
        }
    }

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
        if (!(selectedClip.asset is PlayerActionClip clipAsset)) return;

        PlayableDirector director = TimelineEditor.inspectedDirector;
        if (director == null) return;

        if (clipAsset.actionType == PlayerTimelineActionType.MoveTo)
        {
            DrawMoveHandles(clipAsset, director, selectedClip);
            return;
        }

        if (clipAsset.actionType == PlayerTimelineActionType.TeleportTo && !clipAsset.useTeleportTargetTransform)
        {
            DrawCubePointHandle(
                clipAsset,
                clipAsset.teleportWorldPosition,
                value => clipAsset.teleportWorldPosition = value,
                "TeleportTo",
                EndColor
            );
            return;
        }

        if (clipAsset.actionType == PlayerTimelineActionType.Repel && !clipAsset.useRepelDistance && !clipAsset.useRepelTargetTransform)
        {
            DrawCubePointHandle(
                clipAsset,
                clipAsset.repelWorldPosition,
                value => clipAsset.repelWorldPosition = value,
                "Repel",
                EndColor
            );
        }
    }

    private static void DrawMoveHandles(PlayerActionClip clipAsset, PlayableDirector director, TimelineClip selectedClip)
    {
        Vector3 startPosition = ResolvePosition(clipAsset.useStartTransform, clipAsset.startTarget.Resolve(director), clipAsset.startWorldPosition);
        Vector3 endPosition = ResolvePosition(clipAsset.useEndTransform, clipAsset.endTarget.Resolve(director), clipAsset.endWorldPosition);

        bool showStartHandle = clipAsset.useStartPosition;
        bool canEditStart = showStartHandle && !clipAsset.useStartTransform;
        bool canEditEnd = !clipAsset.useEndTransform;
        bool canMoveBoth = showStartHandle && canEditStart && canEditEnd;

        float sharedY = (startPosition.y + endPosition.y) * 0.5f;
        Vector3 startAligned = new Vector3(startPosition.x, sharedY, startPosition.z);
        Vector3 endAligned = new Vector3(endPosition.x, sharedY, endPosition.z);

        if (showStartHandle)
        {
            float startSize = HandleUtility.GetHandleSize(startAligned) * 0.18f;
            float endSize = HandleUtility.GetHandleSize(endAligned) * 0.18f;

            DrawCubeHandleVisual(startAligned, startSize, StartColor, "Start");
            DrawCubeHandleVisual(endAligned, endSize, EndColor, "End");

            Handles.color = Color.white;
            Handles.DrawLine(startAligned, endAligned);

            if (canMoveBoth)
            {
                Vector3 mid = (startAligned + endAligned) * 0.5f;
                float midSize = HandleUtility.GetHandleSize(mid) * 0.14f;
                DrawCubeHandleVisual(mid, midSize, MidColor, "Move Both");
            }
        }
        else
        {
            float endSize = HandleUtility.GetHandleSize(endPosition) * 0.18f;
            DrawCubeHandleVisual(endPosition, endSize, EndColor, selectedClip.displayName);
        }

        EditorGUI.BeginChangeCheck();

        Vector3 newStart = startPosition;
        Vector3 newEnd = endPosition;
        float newSharedY = sharedY;

        if (canMoveBoth)
        {
            Vector3 mid = new Vector3((newStart.x + newEnd.x) * 0.5f, newSharedY, (newStart.z + newEnd.z) * 0.5f);
            float midSize = HandleUtility.GetHandleSize(mid) * 0.16f;

            Handles.color = MidColor;
            Vector3 newMid = Handles.Slider2D(
                mid,
                Vector3.forward,
                Vector3.right,
                Vector3.up,
                midSize,
                Handles.RectangleHandleCap,
                Vector2.zero
            );

            Vector3 delta = newMid - mid;
            if (delta.sqrMagnitude > 0.0000001f)
            {
                newStart.x += delta.x;
                newEnd.x += delta.x;
                newSharedY += delta.y;
            }
        }

        if (canEditStart)
        {
            Vector3 startHandlePos = new Vector3(newStart.x, newSharedY, newStart.z);
            float startHandleSize = HandleUtility.GetHandleSize(startHandlePos) * 0.16f;
            Handles.color = StartColor;
            Vector3 movedStart = Handles.Slider2D(
                startHandlePos,
                Vector3.forward,
                Vector3.right,
                Vector3.up,
                startHandleSize,
                Handles.RectangleHandleCap,
                Vector2.zero
            );

            Vector3 deltaStart = movedStart - startHandlePos;
            newStart.x += deltaStart.x;
            newSharedY += deltaStart.y;
        }

        if (canEditEnd)
        {
            if (showStartHandle)
            {
                Vector3 endHandlePos = new Vector3(newEnd.x, newSharedY, newEnd.z);
                float endHandleSize = HandleUtility.GetHandleSize(endHandlePos) * 0.16f;
                Handles.color = EndColor;
                Vector3 movedEnd = Handles.Slider2D(
                    endHandlePos,
                    Vector3.forward,
                    Vector3.right,
                    Vector3.up,
                    endHandleSize,
                    Handles.RectangleHandleCap,
                    Vector2.zero
                );

                Vector3 deltaEnd = movedEnd - endHandlePos;
                newEnd.x += deltaEnd.x;
                newSharedY += deltaEnd.y;
            }
            else
            {
                Vector3 endHandlePos = newEnd;
                float endHandleSize = HandleUtility.GetHandleSize(endHandlePos) * 0.16f;
                Handles.color = EndColor;
                Vector3 movedEnd = Handles.Slider2D(
                    endHandlePos,
                    Vector3.forward,
                    Vector3.right,
                    Vector3.up,
                    endHandleSize,
                    Handles.RectangleHandleCap,
                    Vector2.zero
                );

                newEnd = movedEnd;
            }
        }

        if (showStartHandle)
        {
            if (canEditStart)
                newStart.y = newSharedY;

            if (canEditEnd)
                newEnd.y = newSharedY;
        }

        if (!EditorGUI.EndChangeCheck())
            return;

        Undo.RecordObject(clipAsset, "Move Player Action Path Points");

        if (canEditStart)
            clipAsset.startWorldPosition = newStart;

        if (canEditEnd)
            clipAsset.endWorldPosition = newEnd;

        EditorUtility.SetDirty(clipAsset);
        TimelineEditor.Refresh(RefreshReason.ContentsModified);
    }

    private static void DrawCubePointHandle(PlayerActionClip clipAsset, Vector3 current, Action<Vector3> setter, string label, Color color)
    {
        float size = HandleUtility.GetHandleSize(current) * 0.18f;
        DrawCubeHandleVisual(current, size, color, label);

        EditorGUI.BeginChangeCheck();

        Handles.color = color;
        Vector3 next = Handles.Slider2D(
            current,
            Vector3.forward,
            Vector3.right,
            Vector3.up,
            HandleUtility.GetHandleSize(current) * 0.16f,
            Handles.RectangleHandleCap,
            Vector2.zero
        );

        if (!EditorGUI.EndChangeCheck())
            return;

        Undo.RecordObject(clipAsset, $"Move {label} Point");
        setter(next);
        EditorUtility.SetDirty(clipAsset);
        TimelineEditor.Refresh(RefreshReason.ContentsModified);
    }

    private static Vector3 ResolvePosition(bool useTransform, Transform target, Vector3 worldPosition)
    {
        if (useTransform && target != null)
            return target.position;

        return worldPosition;
    }

    private static void DrawCubeHandleVisual(Vector3 position, float size, Color fillColor, string label)
    {
        float fillSize = size * 0.72f;
        float outlineSize = size * 1.35f;

        Handles.color = fillColor;
        Handles.CubeHandleCap(0, position, Quaternion.identity, fillSize, EventType.Repaint);

        Handles.color = Color.white;
        Handles.DrawWireCube(position, Vector3.one * outlineSize);

        DrawCenteredWorldLabel(position + Vector3.up * (size * 1.5f), label);
    }

    private static void DrawCenteredWorldLabel(Vector3 worldPosition, string text)
    {
        Handles.BeginGUI();

        Vector2 guiPoint = HandleUtility.WorldToGUIPoint(worldPosition);
        GUIContent content = new GUIContent(text);
        Vector2 textSize = CenteredLabelStyle.CalcSize(content);

        Rect rect = new Rect(
            guiPoint.x - (textSize.x * 0.5f),
            guiPoint.y - (textSize.y * 0.5f),
            textSize.x,
            textSize.y
        );

        GUI.Label(rect, content, CenteredLabelStyle);

        Handles.EndGUI();
    }
}