using UnityEngine;
using UnityEngine.Playables;

/// <summary>
/// Runtime logic for a boss action clip in the Timeline.
/// Calls Act on first active frame (after binding is ready) and CancelAct on pause.
/// </summary>
public class BossActionBehaviour : PlayableBehaviour
{
    public float factor;
    public Transform target;

    private IEnemyAction _action;
    private bool _started;

    public void Bind(IEnemyAction action, MonoBehaviour host)
    {
        _action = action;
    }

    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        // Do not start here; binding may not be ready yet at t=0.
    }

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        if (!Application.isPlaying) return;
        if (_started) return;
        if (_action == null) return;
        if (info.effectiveWeight <= 0f) return; // clip not active yet

        _started = true;
        _action.Act(factor);
    }

    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        if (!Application.isPlaying) return;
        if (!_started) return;

        _started = false;
        _action?.CancelAct();
    }
}