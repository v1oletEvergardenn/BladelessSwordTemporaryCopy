using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
public class BossActionClip : PlayableAsset, ITimelineClipAsset
{
    public int factor = 0;
    public ExposedReference<Transform> target;
    public double actionDuration = 1.0;

    public override double duration => actionDuration;
    public ClipCaps clipCaps => ClipCaps.None;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<BossActionBehaviour>.Create(graph);
        BossActionBehaviour behaviour = playable.GetBehaviour();
        behaviour.factor = factor;
        behaviour.target = target.Resolve(graph.GetResolver());
        return playable;
    }
}