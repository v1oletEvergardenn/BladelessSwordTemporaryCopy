using UnityEngine;
using UnityEngine.Playables;

/// <summary>
/// Runtime logic for a boss action clip in the Timeline.
/// Calls Act on first active frame (after binding is ready).
/// </summary>
public class BossActionBehaviour : PlayableBehaviour
{
    public float factor;
    public bool targetPlayer;
    public bool useTargetTransform;
    public bool attackFromLeft;
    public Transform target;
    public Vector3 targetWorldPosition;

    private IEnemyAction _action;
    private bool _started;
    private Transform _runtimeWorldTarget;

    public void Bind(IEnemyAction action, MonoBehaviour host)
    {
        _action = action;
    }

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        if (!Application.isPlaying) return;
        if (_started) return;
        if (_action == null) return;
        if (info.effectiveWeight <= 0f) return;

        _started = true;
        _action.SetAttackDirection(attackFromLeft);
        _action.Act(factor, ResolveTarget());
    }

    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        if (!Application.isPlaying) return;
        _started = false;
    }

    public override void OnPlayableDestroy(Playable playable)
    {
        if (_runtimeWorldTarget != null)
        {
            Object.Destroy(_runtimeWorldTarget.gameObject);
            _runtimeWorldTarget = null;
        }
    }

    private Transform ResolveTarget()
    {
        if (targetPlayer) return null;
        if (useTargetTransform) return target;

        if (_runtimeWorldTarget == null)
        {
            GameObject go = new GameObject("BossTimelineWorldTarget");
            go.hideFlags = HideFlags.HideAndDontSave;
            _runtimeWorldTarget = go.transform;
        }

        _runtimeWorldTarget.position = targetWorldPosition;
        return _runtimeWorldTarget;
    }
}