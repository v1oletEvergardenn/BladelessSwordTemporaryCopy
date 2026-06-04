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

/// <summary>
/// Mixer that reads the bound IEnemyAction from the director and
/// injects it into each clip's BossActionBehaviour.
/// </summary>
public class BossActionMixerBehaviour : PlayableBehaviour
{
    private IEnemyAction _boundAction;
    private bool _resolved;

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        // playerData is the IEnemyAction bound in the PlayableDirector binding slot
        if (!_resolved)
        {
            _boundAction = playerData as IEnemyAction;
            _resolved = true;

            // Inject the bound action into every clip behaviour on this track
            int inputCount = playable.GetInputCount();
            for (int i = 0; i < inputCount; i++)
            {
                var inputPlayable = (ScriptPlayable<BossActionBehaviour>)playable.GetInput(i);
                BossActionBehaviour behaviour = inputPlayable.GetBehaviour();
                behaviour.Bind(_boundAction, _boundAction);
            }
        }
    }
}