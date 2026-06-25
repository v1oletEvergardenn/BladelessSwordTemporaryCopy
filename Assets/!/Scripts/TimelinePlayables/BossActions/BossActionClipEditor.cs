using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[CustomTimelineEditor(typeof(BossActionClip))]
public class BossActionClipEditor : ClipEditor
{
    [InitializeOnLoadMethod]
    private static void RegisterSceneGui()
    {
        SceneView.duringSceneGui -= DrawSelectedClipTarget;
        SceneView.duringSceneGui += DrawSelectedClipTarget;
    }

    public override void OnClipChanged(TimelineClip clip)
    {
        var asset = clip.asset as BossActionClip;
        if (asset == null) return;

        var director = TimelineEditor.inspectedDirector;
        if (director == null || clip.GetParentTrack() is not TrackAsset track) return;

        if (director.GetGenericBinding(track) is not IEnemyAction action)
        {
            clip.displayName = $"(No Action Bound) [f{asset.factor}]";
            return;
        }

        int maxFactor = Mathf.Max(0, action.FactorCount - 1);
        if (asset.factor > maxFactor)
        {
            Undo.RecordObject(asset, "Clamp BossActionClip Factor");
            asset.factor = maxFactor;
            EditorUtility.SetDirty(asset);
        }

        double incoming = action.GetDuration(asset.factor);
        if (!Mathf.Approximately((float)asset.actionDuration, (float)incoming))
        {
            Undo.RecordObject(asset, "Auto-Sync BossActionClip Duration");
            asset.actionDuration = incoming;
            EditorUtility.SetDirty(asset);
        }

        if (action.IsFixedDuration(asset.factor))
            clip.duration = asset.actionDuration;

        clip.displayName = $"f{asset.factor}";
    }

    private static void DrawSelectedClipTarget(SceneView sceneView)
    {
        TimelineClip selectedClip = TimelineEditor.selectedClip;
        if (selectedClip == null) return;
        if (selectedClip.asset is not BossActionClip clipAsset) return;

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

[CustomEditor(typeof(BossActionClip))]
public class BossActionClipInspector : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        var clip = (BossActionClip)target;
        IEnemyAction boundAction = FindBoundAction(clip);
        int maxFactor = boundAction != null ? Mathf.Max(0, boundAction.FactorCount - 1) : 99;

        EditorGUI.BeginChangeCheck();

        int newFactor = EditorGUILayout.IntSlider(
            new GUIContent("Factor", "Selects the action variant. Clamped to the bound IEnemyAction factorData size."),
            clip.factor, 0, maxFactor);

        if (newFactor != clip.factor)
        {
            Undo.RecordObject(clip, "Change BossActionClip Factor");
            clip.factor = newFactor;
            EditorUtility.SetDirty(clip);
        }

        SerializedProperty targetPlayerProp = serializedObject.FindProperty("targetPlayer");
        SerializedProperty useTargetTransformProp = serializedObject.FindProperty("useTargetTransform");
        SerializedProperty targetProp = serializedObject.FindProperty("target");
        SerializedProperty targetWorldPositionProp = serializedObject.FindProperty("targetWorldPosition");
        SerializedProperty showTargetGizmoProp = serializedObject.FindProperty("showTargetGizmo");
        SerializedProperty targetGizmoColorProp = serializedObject.FindProperty("targetGizmoColor");
        SerializedProperty targetWorldGizmoLabelProp = serializedObject.FindProperty("targetWorldGizmoLabel");

        EditorGUILayout.PropertyField(targetPlayerProp, new GUIContent("Target Player"));

        if (!targetPlayerProp.boolValue)
        {
            EditorGUILayout.PropertyField(useTargetTransformProp, new GUIContent("Use Target Transform"));

            if (useTargetTransformProp.boolValue)
            {
                EditorGUILayout.PropertyField(targetProp, new GUIContent("Target"));
            }
            else
            {
                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUILayout.PropertyField(targetWorldPositionProp, new GUIContent("Target World Position"));
                }
                EditorGUILayout.HelpBox("Move Target World Position in Scene view with the position handle.", MessageType.None);
            }

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(showTargetGizmoProp, new GUIContent("Show Target Gizmo"));

            if (showTargetGizmoProp.boolValue)
            {
                EditorGUILayout.PropertyField(targetGizmoColorProp, new GUIContent("Target Gizmo Color"));

                if (!useTargetTransformProp.boolValue)
                {
                    EditorGUILayout.PropertyField(targetWorldGizmoLabelProp, new GUIContent("World Gizmo Label"));
                }
            }
        }

        using (new EditorGUI.DisabledScope(true))
        {
            EditorGUILayout.DoubleField(
                new GUIContent("Action Duration", "Auto-synced from factorData[factor].duration on bound IEnemyAction."),
                clip.actionDuration);
        }

        EditorGUILayout.Space();

        if (boundAction != null)
        {
            bool isFixed = boundAction.IsFixedDuration(clip.factor);
            string mode = isFixed
                ? $"factor {clip.factor}: duration FIXED at {clip.actionDuration:F2}s — clip cannot be resized."
                : $"factor {clip.factor}: duration FREE — resize clip as needed. ({clip.actionDuration:F2}s reference)";
            EditorGUILayout.HelpBox(mode, isFixed ? MessageType.Info : MessageType.None);
        }
        else
        {
            EditorGUILayout.HelpBox("Bind an IEnemyAction to this track binding to enable auto-sync.", MessageType.Warning);
        }

        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            TimelineEditor.Refresh(RefreshReason.ContentsModified);
        }
        else
        {
            serializedObject.ApplyModifiedProperties();
        }
    }

    private IEnemyAction FindBoundAction(BossActionClip clip)
    {
        PlayableDirector[] directors = FindObjectsOfType<PlayableDirector>();
        foreach (PlayableDirector director in directors)
        {
            if (director.playableAsset is not TimelineAsset timelineAsset) continue;

            foreach (TrackAsset track in timelineAsset.GetOutputTracks())
            {
                foreach (TimelineClip timelineClip in track.GetClips())
                {
                    if (timelineClip.asset == clip && director.GetGenericBinding(track) is IEnemyAction action)
                        return action;
                }
            }
        }

        return null;
    }
}