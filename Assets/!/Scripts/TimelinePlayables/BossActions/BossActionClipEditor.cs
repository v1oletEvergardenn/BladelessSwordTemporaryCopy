using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine.Timeline;
using UnityEngine;

[CustomTimelineEditor(typeof(BossActionClip))]
public class BossActionClipEditor : ClipEditor
{
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

        // --- Clamp factor to valid range ---
        int maxFactor = Mathf.Max(0, action.FactorCount - 1);
        if (asset.factor > maxFactor)
        {
            Undo.RecordObject(asset, "Clamp BossActionClip Factor");
            asset.factor = maxFactor;
            EditorUtility.SetDirty(asset);
        }

        // --- Auto-sync duration ---
        double incoming = action.GetDuration(asset.factor);
        if (!Mathf.Approximately((float)asset.actionDuration, (float)incoming))
        {
            Undo.RecordObject(asset, "Auto-Sync BossActionClip Duration");
            asset.actionDuration = incoming;
            EditorUtility.SetDirty(asset);
        }

        // --- Lock duration only if this factor is fixed ---
        if (action.IsFixedDuration(asset.factor))
            clip.duration = asset.actionDuration;

        clip.displayName = $"f{asset.factor}";
    }
}

[CustomEditor(typeof(BossActionClip))]
public class BossActionClipInspector : Editor
{
    public override void OnInspectorGUI()
    {
        var clip = (BossActionClip)target;

        // Draw factor as a clamped int field instead of free input
        IEnemyAction boundAction = FindBoundAction(clip);
        int maxFactor = boundAction != null ? Mathf.Max(0, boundAction.FactorCount - 1) : 99;

        EditorGUI.BeginChangeCheck();

        int newFactor = EditorGUILayout.IntSlider(
            new GUIContent("Factor", "Selects the action variant. Clamped to the bound IEnemyAction's factorData list size."),
            clip.factor, 0, maxFactor);

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(clip, "Change BossActionClip Factor");
            clip.factor = newFactor;
            EditorUtility.SetDirty(clip);
            TimelineEditor.Refresh(RefreshReason.ContentsModified);
        }

        // Draw remaining fields (actionDuration) as read-only for clarity
        using (new EditorGUI.DisabledScope(true))
        {
            EditorGUILayout.DoubleField(
                new GUIContent("Action Duration", "Auto-synced from factorData[factor].duration on the bound IEnemyAction."),
                clip.actionDuration);
        }

        EditorGUILayout.Space();

        if (boundAction != null)
        {
            bool isFixed = boundAction.IsFixedDuration(clip.factor);
            string mode = isFixed
                ? $"factor {clip.factor}: duration FIXED at {clip.actionDuration:F2}s — clip cannot be resized."
                : $"factor {clip.factor}: duration FREE — resize the clip as needed. ({clip.actionDuration:F2}s is the action reference value)";
            EditorGUILayout.HelpBox(mode, isFixed ? MessageType.Info : MessageType.None);
        }
        else
        {
            EditorGUILayout.HelpBox("Bind an IEnemyAction to this track's binding slot to enable auto-sync.", MessageType.Warning);
        }
    }

    private IEnemyAction FindBoundAction(BossActionClip clip)
    {
        var directors = FindObjectsOfType<UnityEngine.Playables.PlayableDirector>();
        foreach (var director in directors)
        {
            if (director.playableAsset is not UnityEngine.Timeline.TimelineAsset timelineAsset) continue;
            foreach (var track in timelineAsset.GetOutputTracks())
            {
                foreach (var timelineClip in track.GetClips())
                {
                    if (timelineClip.asset == clip && director.GetGenericBinding(track) is IEnemyAction action)
                        return action;
                }
            }
        }
        return null;
    }
}