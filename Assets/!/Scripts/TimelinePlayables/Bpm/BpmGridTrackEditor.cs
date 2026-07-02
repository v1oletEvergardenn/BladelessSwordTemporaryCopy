using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;

[CustomEditor(typeof(BpmGridTrack))]
public class BpmGridTrackEditor : Editor
{
    public override void OnInspectorGUI()
    {
        BpmGridTrack track = (BpmGridTrack)target;

        EditorGUI.BeginChangeCheck();

        double bpm = EditorGUILayout.Slider("Bpm", (float)track.bpm, (float)BpmGridTrack.MinBpm, (float)BpmGridTrack.MaxBpm);
        int beatsPerBar = EditorGUILayout.IntSlider("Beats Per Bar", track.beatsPerBar, BpmGridTrack.MinBeatsPerBar, BpmGridTrack.MaxBeatsPerBar);
        int barCount = EditorGUILayout.IntSlider("Bar Count", track.barCount, BpmGridTrack.MinBarCount, BpmGridTrack.MaxBarCount);
        int subdivisionsPerBeat = EditorGUILayout.IntSlider("Subdivisions Per Beat", track.subdivisionsPerBeat, BpmGridTrack.MinSubdivisionsPerBeat, BpmGridTrack.MaxSubdivisionsPerBeat);
        double startOffsetSeconds = EditorGUILayout.Slider("Start Offset Seconds", (float)track.startOffsetSeconds, (float)BpmGridTrack.MinStartOffsetSeconds, (float)BpmGridTrack.MaxStartOffsetSeconds);

        bool changed = EditorGUI.EndChangeCheck();
        if (changed)
        {
            Undo.RecordObject(track, "Edit BPM Grid Settings");

            track.bpm = bpm;
            track.beatsPerBar = beatsPerBar;
            track.barCount = barCount;
            track.subdivisionsPerBeat = subdivisionsPerBeat;
            track.startOffsetSeconds = startOffsetSeconds;
            track.ClampValues();

            EditorUtility.SetDirty(track);
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Regenerate Markers"))
        {
            Undo.RecordObject(track, "Regenerate BPM Markers");
            track.RegenerateMarkersFromSettings();
            EditorUtility.SetDirty(track);
            if (track.timelineAsset != null)
                EditorUtility.SetDirty(track.timelineAsset);
            TimelineEditor.Refresh(RefreshReason.ContentsModified);
        }

        if (GUILayout.Button("Regenerate Bar Clips"))
        {
            Undo.RecordObject(track, "Regenerate BPM Bar Clips");
            track.RegenerateBarClipsFromSettings();
            EditorUtility.SetDirty(track);
            if (track.timelineAsset != null)
                EditorUtility.SetDirty(track.timelineAsset);
            TimelineEditor.Refresh(RefreshReason.ContentsModified);
        }

        if (GUILayout.Button("Regenerate All (Markers + Bar Clips)"))
        {
            Undo.RecordObject(track, "Regenerate BPM Grid");
            track.RegenerateAllFromSettings();
            EditorUtility.SetDirty(track);
            if (track.timelineAsset != null)
                EditorUtility.SetDirty(track.timelineAsset);
            TimelineEditor.Refresh(RefreshReason.ContentsModified);
        }

        EditorGUILayout.HelpBox("No auto-update. Changes are applied only when you press regenerate buttons.", MessageType.Info);
    }
}