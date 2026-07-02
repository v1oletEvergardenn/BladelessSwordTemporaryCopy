using UnityEngine;
using UnityEngine.Playables;

/// <summary>
/// Timeline behaviour for player actions.
/// </summary>
public class PlayerActionBehaviour : PlayableBehaviour
{
    public PlayerTimelineActionType actionType;
    public bool useTimelineMotion;
    public PlayerMoveExecutionMode moveMode;
    public PlayerMoveSpeedOption speedOption;
    public float customSpeed;
    public bool useStartPosition;
    public bool useRepelGizmoPosition;
    public float repelDistance;

    [Header("Move Path")]
    public Vector3 startPosition;

    public Vector3 endPosition;

    private bool _triggered;
    private CharacterController2D _controller;
    private Transform _playerTransform;

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        if (info.effectiveWeight <= 0f)
            return;

        if (actionType == PlayerTimelineActionType.MoveTo)
        {
            if (moveMode == PlayerMoveExecutionMode.RunToPosition)
            {
                if (!Application.isPlaying || _triggered)
                    return;

                _triggered = true;
                ResolvePlayerContext();

                if (useStartPosition && _playerTransform != null)
                    _playerTransform.position = new Vector3(startPosition.x, _playerTransform.position.y, _playerTransform.position.z);

                PlayerTimeLineActions handler = PlayerTimeLineActions.instance;
                if (handler == null)
                    return;

                if (speedOption == PlayerMoveSpeedOption.WalkSpeed)
                    handler.MoveTo(endPosition, false);
                else if (speedOption == PlayerMoveSpeedOption.CustomSpeed)
                    handler.MoveTo(endPosition, customSpeed);
                else
                    handler.MoveTo(endPosition, true);

                return;
            }

            if (useTimelineMotion)
            {
                ResolvePlayerContext();
                if (_playerTransform == null)
                    return;

                double duration = playable.GetDuration();
                float t = duration > double.Epsilon
                    ? Mathf.Clamp01((float)(playable.GetTime() / duration))
                    : 1f;

                float sampledX = Mathf.Lerp(startPosition.x, endPosition.x, t);
                float dir = startPosition.x >= endPosition.x ? -1f : 1f;

                InputPlayer.instance.movementInputUpdateLock.Add("isTimelineMoving");
                if (Application.isPlaying && _controller != null)
                    _controller.Move(dir, 0);

                Vector3 p = _playerTransform.position;
                _playerTransform.position = new Vector3(sampledX, p.y, p.z);
                return;
            }
        }

        if (!Application.isPlaying || _triggered)
            return;

        _triggered = true;

        PlayerTimeLineActions actions = PlayerTimeLineActions.instance;
        if (actions == null)
            return;

        if (actionType == PlayerTimelineActionType.Repel)
        {
            if (useRepelGizmoPosition)
                actions.RepelTo(endPosition);
            else
                actions.RepelByDistance(repelDistance);
        }
    }

    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        _triggered = false;

        if (Application.isPlaying && _controller != null && useTimelineMotion)
        {
            _controller.Move(0f);
            InputPlayer.instance.movementInputUpdateLock.Remove("isTimelineMoving");
        }
    }

    private void ResolvePlayerContext()
    {
        if (_controller == null)
            _controller = CharacterController2D.instance;

        if (_controller == null)
            _controller = Object.FindObjectOfType<CharacterController2D>();

        if (_playerTransform == null && _controller != null)
            _playerTransform = _controller.transform;
    }
}