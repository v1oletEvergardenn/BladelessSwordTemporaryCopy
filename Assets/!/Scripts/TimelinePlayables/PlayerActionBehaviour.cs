using UnityEngine;
using UnityEngine.Playables;

/// <summary>
/// Executes one player timeline action on the first active frame of the clip.
/// Uses runtime singleton handler (no Timeline binding required).
/// </summary>
public class PlayerActionBehaviour : PlayableBehaviour
{
    public PlayerTimelineActionType actionType;
    private bool _started;

    [Header("Target (Move / Repel)")]
    public Vector3 targetPosition;

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
                handler.MoveTo(targetPosition);
                break;

            case PlayerTimelineActionType.Repel:
                handler.RepelTo(targetPosition);
                break;
        }
    }

    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        if (!Application.isPlaying) return;
        _started = false;
    }
}