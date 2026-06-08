using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

/// <summary>
/// Custom Timeline track for player action clips.
/// Appears under "Add Track > YYF > Player Action Track".
/// </summary>
[TrackClipType(typeof(PlayerActionClip))]
[TrackColor(0.2f, 0.6f, 1.0f)]
public class PlayerActionTrack : TrackAsset
{
    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
    {
        return ScriptPlayable<PlayerActionMixerBehaviour>.Create(graph, inputCount);
    }
}