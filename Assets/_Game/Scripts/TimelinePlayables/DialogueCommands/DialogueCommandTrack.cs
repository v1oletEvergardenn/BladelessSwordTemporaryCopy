using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[TrackClipType(typeof(DialogueCommandClip))]
[TrackBindingType(typeof(GameObject))]
[TrackColor(0.55f, 0.72f, 0.95f)]
public class DialogueCommandTrack : TrackAsset
{
    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
    {
        return ScriptPlayable<DialogueCommandMixerBehaviour>.Create(graph, inputCount);
    }
}