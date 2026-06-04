using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

/// <summary>
/// The clip asset that lives on the BossActionTrack.
/// Configure 'factor' here per clip — e.g. 0 = black fish, 1 = white fish.
/// </summary>
[Serializable]
public class BossActionClip : PlayableAsset, ITimelineClipAsset
{
    [Tooltip("Selects the action variant. Each integer value maps to an index in timeToHitPlayerPerFactor on the bound IEnemyAction.")]
    public int factor = 0;

    [Tooltip("Auto-synced from the bound IEnemyAction's timeToHitPlayerPerFactor at the matching factor index.")]
    public double actionDuration = 1.0;

    public override double duration => actionDuration;

    public ClipCaps clipCaps => ClipCaps.None;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<BossActionBehaviour>.Create(graph);
        playable.GetBehaviour().factor = factor;
        return playable;
    }
}