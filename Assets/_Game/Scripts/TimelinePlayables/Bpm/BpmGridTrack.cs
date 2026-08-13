using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[TrackClipType(typeof(BpmBarGuideClip))]
[TrackColor(0.95f, 0.45f, 0.2f)]
public class BpmGridTrack : TrackAsset
{
    public const double MinBpm = 40.0d;
    public const double MaxBpm = 260.0d;
    public const int MinBeatsPerBar = 1;
    public const int MaxBeatsPerBar = 12;
    public const int MinBarCount = 1;
    public const int MaxBarCount = 256;
    public const int MinSubdivisionsPerBeat = 1;
    public const int MaxSubdivisionsPerBeat = 8;
    public const double MinStartOffsetSeconds = 0.0d;
    public const double MaxStartOffsetSeconds = 60.0d;

    [Min(1.0f)] public double bpm = 120.0d;
    [Min(1)] public int beatsPerBar = 4;
    [Min(1)] public int barCount = 16;
    [Min(1)] public int subdivisionsPerBeat = 1;
    [Min(0.0f)] public double startOffsetSeconds = 0.0d;

    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
    {
        return ScriptPlayable<BpmBarGuideBehaviour>.Create(graph, inputCount);
    }

#if UNITY_EDITOR

    public void ClampValues()
    {
        bpm = Math.Max(MinBpm, Math.Min(MaxBpm, bpm));
        beatsPerBar = Math.Max(MinBeatsPerBar, Math.Min(MaxBeatsPerBar, beatsPerBar));
        barCount = Math.Max(MinBarCount, Math.Min(MaxBarCount, barCount));
        subdivisionsPerBeat = Math.Max(MinSubdivisionsPerBeat, Math.Min(MaxSubdivisionsPerBeat, subdivisionsPerBeat));
        startOffsetSeconds = Math.Max(MinStartOffsetSeconds, Math.Min(MaxStartOffsetSeconds, startOffsetSeconds));
    }

    public void RegenerateAllFromSettings()
    {
        ClampValues();
        RegenerateMarkersFromSettings();
        RegenerateBarClipsFromSettings();
    }

    public void RegenerateMarkersFromSettings()
    {
        ClampValues();
        DeleteGeneratedMarkers();

        double beatDuration = RhythmTiming.BeatDuration(bpm);
        double stepDuration = beatDuration / subdivisionsPerBeat;
        int totalBeats = beatsPerBar * barCount;
        int totalSteps = totalBeats * subdivisionsPerBeat;

        for (int bar = 0; bar < barCount; bar++)
        {
            double barTime = startOffsetSeconds + (bar * beatsPerBar * beatDuration);
            BpmBarMarker barMarker = CreateMarker<BpmBarMarker>(barTime);
            barMarker.SetData(bar + 1);
        }

        for (int step = 0; step <= totalSteps; step++)
        {
            double time = startOffsetSeconds + (step * stepDuration);
            BpmBeatMarker marker = CreateMarker<BpmBeatMarker>(time);

            int beatIndex = step / subdivisionsPerBeat;
            int subdivisionIndex = step % subdivisionsPerBeat;
            int barIndex = (beatIndex / beatsPerBar) + 1;
            int beatInBar = beatIndex % beatsPerBar;
            bool isBarStart = beatInBar == 0 && subdivisionIndex == 0;

            marker.SetData(beatIndex, subdivisionIndex, barIndex, beatInBar, isBarStart);
        }
    }

    public void RegenerateBarClipsFromSettings()
    {
        ClampValues();
        DeleteGeneratedBarClips();

        double beatDuration = RhythmTiming.BeatDuration(bpm);
        double barDuration = Math.Max(0.01d, beatsPerBar * beatDuration);

        for (int bar = 0; bar < barCount; bar++)
        {
            TimelineClip clip = CreateClip<BpmBarGuideClip>();
            clip.start = startOffsetSeconds + (bar * barDuration);
            clip.duration = barDuration;
            clip.displayName = "Bar " + (bar + 1);

            BpmBarGuideClip clipAsset = clip.asset as BpmBarGuideClip;
            if (clipAsset != null)
                clipAsset.barIndex = bar + 1;
        }
    }

    private void DeleteGeneratedMarkers()
    {
        List<IMarker> toDelete = new List<IMarker>();
        foreach (IMarker marker in GetMarkers())
        {
            if (marker is BpmBeatMarker || marker is BpmBarMarker)
                toDelete.Add(marker);
        }

        for (int i = 0; i < toDelete.Count; i++)
            DeleteMarker(toDelete[i]);
    }

    private void DeleteGeneratedBarClips()
    {
        List<TimelineClip> toDelete = new List<TimelineClip>();
        foreach (TimelineClip clip in GetClips())
        {
            if (clip.asset is BpmBarGuideClip)
                toDelete.Add(clip);
        }

        for (int i = 0; i < toDelete.Count; i++)
            DeleteClip(toDelete[i]);
    }

#endif
}