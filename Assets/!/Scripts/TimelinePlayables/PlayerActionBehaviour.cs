using UnityEngine;
using UnityEngine.Playables;

/// <summary>
/// Executes one player timeline action on the first active frame of the clip.
/// Uses runtime singleton handler (no Timeline binding required).
/// </summary>
public class PlayerActionBehaviour : PlayableBehaviour
{
    public PlayerTimelineActionType actionType;

    public bool hasResolvedTarget;
    public Vector3 resolvedTargetPosition;
    public Vector3 moveWorldPosition;
    public bool faceTargetAfterMove;

    public bool attackLeft;
    public bool consumeEnergy;

    private bool _started;

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        if (!Application.isPlaying) return;
        if (_started) return;
        if (info.effectiveWeight <= 0f) return;

        _started = true;

        PlayerTimeLineActions handler = PlayerTimeLineActions.instance;
        if (handler == null)
            return;

        switch (actionType)
        {
            case PlayerTimelineActionType.MoveTo:
                Vector3 targetPosition = hasResolvedTarget ? resolvedTargetPosition : moveWorldPosition;
                handler.MoveTo(targetPosition);
                break;

            case PlayerTimelineActionType.Attack:
                handler.Attack(attackLeft);
                break;

            case PlayerTimelineActionType.Jump:
                handler.Jump();
                break;
        }
    }

    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        if (!Application.isPlaying) return;
        _started = false;
    }
}