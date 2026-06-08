using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

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