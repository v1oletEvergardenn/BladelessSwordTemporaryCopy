using UnityEngine;
using UnityEngine.Playables;

/// <summary>
/// Timeline behaviour for player actions.
/// </summary>
public class PlayerActionBehaviour : PlayableBehaviour
{
    public PlayerTimelineActionType actionType;
    public PlayerTimelineDirection direction;
    public HSEnum hsEnum;

    public PlayerMoveSpeedOption speedOption;
    public float customSpeed;
    public bool useStartPosition;
    public bool useStartTransform;
    public Transform startTarget;
    public Vector3 startWorldPosition;
    public bool useEndTransform;
    public Transform endTarget;
    public Vector3 endWorldPosition;

    public bool useTeleportTargetTransform;
    public Transform teleportTarget;
    public Vector3 teleportWorldPosition;

    public bool useRepelDistance;
    public float repelDistance;
    public bool useRepelTargetTransform;
    public Transform repelTarget;
    public Vector3 repelWorldPosition;

    public float doubleJumpHold;
    public bool gravityEnabled;
    public PlayerTimelineMoveState moveState;
    public string animStateName;

    private bool _triggered;
    private PlayerControl _controller;
    private Transform _playerTransform;

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        if (info.effectiveWeight <= 0f)
            return;

        if (!Application.isPlaying || _triggered)
            return;

        _triggered = true;
        ResolvePlayerContext();

        PlayerTimeLineActions actions = PlayerTimeLineActions.instance;
        if (actions == null)
            return;

        switch (actionType)
        {
            case PlayerTimelineActionType.MoveTo:
                ExecuteMoveTo(actions);
                break;

            case PlayerTimelineActionType.Repel:
                if (useRepelDistance)
                    actions.RepelByDistance(repelDistance);
                else
                    actions.RepelTo(ResolveWorldPosition(useRepelTargetTransform, repelTarget, repelWorldPosition));
                break;

            case PlayerTimelineActionType.Attack:
                actions.Attack(direction == PlayerTimelineDirection.Left);
                break;

            case PlayerTimelineActionType.AttackHS:
                actions.AttackHS(direction == PlayerTimelineDirection.Left);
                break;

            case PlayerTimelineActionType.HSAbility:
                actions.HSAbility(hsEnum, direction == PlayerTimelineDirection.Left);
                break;

            case PlayerTimelineActionType.TeleportTo:
                actions.TeleportTo(ResolveWorldPosition(useTeleportTargetTransform, teleportTarget, teleportWorldPosition));
                break;

            case PlayerTimelineActionType.Face:
                actions.Face(direction == PlayerTimelineDirection.Right);
                break;

            case PlayerTimelineActionType.Stop:
                actions.StopMovement();
                break;

            case PlayerTimelineActionType.Jump:
                actions.Jump();
                break;

            case PlayerTimelineActionType.DoubleJump:
                actions.DoubleJump(doubleJumpHold);
                break;

            case PlayerTimelineActionType.Gravity:
                actions.SetGravity(gravityEnabled);
                break;

            case PlayerTimelineActionType.ClearInput:
                actions.ClearInput();
                break;

            case PlayerTimelineActionType.MoveState:
                actions.SetMoveState(moveState);
                break;

            case PlayerTimelineActionType.BodyAnim:
                actions.PlayBodyAnim(animStateName);
                break;

            case PlayerTimelineActionType.LegAnim:
                actions.PlayLegAnim(animStateName);
                break;
        }
    }

    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        _triggered = false;
    }

    private void ExecuteMoveTo(PlayerTimeLineActions actions)
    {
        if (useStartPosition && _playerTransform != null)
        {
            Vector3 start = ResolveWorldPosition(useStartTransform, startTarget, startWorldPosition);
            _playerTransform.position = new Vector3(start.x, _playerTransform.position.y, _playerTransform.position.z);
        }

        Vector3 end = ResolveWorldPosition(useEndTransform, endTarget, endWorldPosition);

        if (speedOption == PlayerMoveSpeedOption.WalkSpeed)
            actions.MoveTo(end, false);
        else if (speedOption == PlayerMoveSpeedOption.CustomSpeed)
            actions.MoveTo(end, customSpeed);
        else
            actions.MoveTo(end, true);
    }

    private static Vector3 ResolveWorldPosition(bool useTransform, Transform target, Vector3 worldPosition)
    {
        if (useTransform && target != null)
            return target.position;

        return worldPosition;
    }

    private void ResolvePlayerContext()
    {
        if (_controller == null)
            _controller = PlayerControl.instance;

        if (_controller == null)
            _controller = Object.FindObjectOfType<PlayerControl>();

        if (_playerTransform == null && _controller != null)
            _playerTransform = _controller.transform;
    }
}