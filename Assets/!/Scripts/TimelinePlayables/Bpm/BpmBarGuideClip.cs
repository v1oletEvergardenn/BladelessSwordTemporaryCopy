using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
public class BpmBarGuideClip : PlayableAsset, ITimelineClipAsset
{
    [Min(1)] public int barIndex = 1;

    public ClipCaps clipCaps => ClipCaps.None;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        return ScriptPlayable<BpmBarGuideBehaviour>.Create(graph);
    }
}

public class BpmBarGuideBehaviour : PlayableBehaviour
{
    // Implement behavior here if needed
}