using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

/// <summary>
/// Custom Timeline track for IEnemyAction clips.
/// Appears under "Add Track > YYF > Boss Action Track" in the Timeline window.
/// Bind an IEnemyAction MonoBehaviour to this track's binding slot in the director.
/// </summary>
[TrackBindingType(typeof(IEnemyAction))]
[TrackClipType(typeof(BossActionClip))]
[TrackColor(0.8f, 0.2f, 0.2f)]
public class BossActionTrack : TrackAsset
{
    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
    {
        return ScriptPlayable<BossActionMixerBehaviour>.Create(graph, inputCount);
    }
}