using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[TrackClipType(typeof(CameraActionClip))]
[TrackColor(0.35f, 0.55f, 1.0f)]
public class CameraActionTrack : TrackAsset
{
    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
    {
        return ScriptPlayable<CameraActionMixerBehaviour>.Create(graph, inputCount);
    }
}