using System.Collections;
using UnityEngine;
using UnityEngine.Playables;

/// <summary>
/// Runtime logic for a boss action clip in the Timeline.
/// Calls Act_coroutine on enter and CancelAct on exit.
/// </summary>
public class BossActionBehaviour : PlayableBehaviour
{
    /// <summary>
    /// factor value passed into Act_coroutine — set this in BossActionClip.
    /// e.g. 0 = black fish, 1 = white fish for YYF_Swing
    /// </summary>
    public float factor;

    private IEnemyAction _action;
    private MonoBehaviour _coroutineHost;
    private bool _started;

    /// <summary>
    /// Called by BossActionMixerBehaviour to inject the bound IEnemyAction
    /// from the PlayableDirector's binding slot.
    /// </summary>
    public void Bind(IEnemyAction action, MonoBehaviour host)
    {
        _action = action;
        _coroutineHost = host;
    }

    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        if (_action == null || Application.isPlaying == false) return;
        if (_started) return;

        _started = true;
        _action.Act(factor);
    }
}