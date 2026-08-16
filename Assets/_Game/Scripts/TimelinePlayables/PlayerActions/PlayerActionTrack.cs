using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[TrackClipType(typeof(PlayerActionClip))]
[TrackBindingType(typeof(SignalReceiver))]
[TrackColor(0.2f, 0.6f, 1.0f)]
public class PlayerActionTrack : TrackAsset
{
    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
    {
        ScriptPlayable<PlayerActionMixerBehaviour> playable = ScriptPlayable<PlayerActionMixerBehaviour>.Create(graph, inputCount);
        playable.GetBehaviour().Initialize(this);
        return playable;
    }
}