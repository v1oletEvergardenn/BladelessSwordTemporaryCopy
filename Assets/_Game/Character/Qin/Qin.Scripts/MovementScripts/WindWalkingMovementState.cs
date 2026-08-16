using DG.Tweening;
using UnityEngine;

public class WindWalkingMovementState : IMovementState
{
    private const float MoveInputThreshold = 0.01f;

    // Each step travels exactly this world distance.
    private const float StepDistance = 1.1f;

    // Step cadence.
    private const float StepMoveDuration = 0.42f;

    private const float StepIdleDuration = 0.4f;

    private const string BodyWindIdle = "body_wind_idle";
    private const string LegWindWalk1 = "leg_wind_walk_1";
    private const string LegWindWalk2 = "leg_wind_walk_2";

    private readonly PlayerControl _controller;
    public PlayerMovementStateType Type => PlayerMovementStateType.WindWalking;

    private Tween _stepTween;
    private float _idleTimer;
    private bool _isStepping;
    private bool _hasMoveInput;
    private float _lastInputDirection;
    private int _nextStepIndex;

    public WindWalkingMovementState(PlayerControl controller)
    {
        _controller = controller;
    }

    public void Enter()
    {
        _isStepping = false;
        _idleTimer = 0f;
        _hasMoveInput = false;
        _lastInputDirection = _controller.FacingRight ? 1f : -1f;
        _nextStepIndex = 1;

        ApplyBodyIdle();
        PlayIfNotCurrent(_controller.legAnim, "leg_wind_idle_1");
    }

    public void Exit()
    {
        KillStepTween();
        _controller.SetRunningState(false);

        if (_controller.rb != null)
            _controller.rb.velocity = new Vector2(0f, _controller.rb.velocity.y);
    }

    public void Tick()
    {
        _controller.TickNormalState();
        UpdateStepFlow();
    }

    public void FixedTick()
    {
        _controller.FixedTickNormalState();
    }

    /// <summary>
    /// Handles horizontal movement with fixed-distance tweened wind-walking steps.
    /// </summary>
    /// <param name="input">Horizontal movement input.</param>
    /// <param name="speed">Movement speed override (unused in wind-walking).</param>
    public void Move(float input, float speed)
    {
        _hasMoveInput = Mathf.Abs(input) > MoveInputThreshold;

        if (_hasMoveInput)
            _lastInputDirection = Mathf.Sign(input);

        // Do not flip while stepping; it causes backward-looking movement.
        if (_hasMoveInput && !_isStepping)
            _controller.HandleFlipping(_lastInputDirection);

        // If not currently stepping and no idle wait is active, try to start immediately.
        if (!_isStepping && _idleTimer <= 0f)
            TryStartStep();
    }

    private void UpdateStepFlow()
    {
        if (_isStepping)
            return;

        if (_idleTimer > 0f)
        {
            _idleTimer -= TimeScaleManager.Delta(TimeChannel.Player);
            if (_idleTimer > 0f)
                return;

            _idleTimer = 0f;
        }

        TryStartStep();
    }

    private void TryStartStep()
    {
        if (!_hasMoveInput)
        {
            _controller.SetRunningState(false);
            return;
        }

        if (!_controller.CanMove())
        {
            _controller.SetRunningState(false);
            return;
        }

        StartStep(_lastInputDirection);
    }

    private void StartStep(float direction)
    {
        KillStepTween();

        _isStepping = true;
        _controller.SetRunningState(true);

        // Ensure facing is updated once, before step begins.
        _controller.HandleFlipping(direction);

        PlayWalkAnimationForStep();

        Vector3 pos = _controller.transform.position;
        float targetX = pos.x + direction * StepDistance;

        _stepTween = _controller.transform
            .DOMoveX(targetX, StepMoveDuration)
            .SetEase(Ease.InSine)
            .SetTimeDt(_controller, TimeChannel.Player)
            .OnComplete(OnStepComplete);
    }

    private void OnStepComplete()
    {
        _isStepping = false;
        _controller.SetRunningState(false);

        if (_controller.rb != null)
            _controller.rb.velocity = new Vector2(0f, _controller.rb.velocity.y);

        _idleTimer = StepIdleDuration;
    }

    private void PlayWalkAnimationForStep()
    {
        string clip = _nextStepIndex == 1 ? LegWindWalk1 : LegWindWalk2;
        PlayIfNotCurrent(_controller.legAnim, clip);
        _nextStepIndex = _nextStepIndex == 1 ? 2 : 1;
    }

    private void ApplyBodyIdle()
    {
        PlayIfNotCurrent(_controller.anim, BodyWindIdle);
    }

    private void KillStepTween()
    {
        if (_stepTween == null)
            return;

        if (_stepTween.active)
            _stepTween.Kill();

        _stepTween = null;
    }

    private static void PlayIfNotCurrent(Animator animator, string stateName)
    {
        if (animator == null)
            return;

        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
        if (state.IsName(stateName))
            return;

        animator.Play(stateName, 0, 0f);
    }

    public void Jump()
    {
        _controller.JumpNormalState();
    }

    public void DoubleJump(float holdTime)
    {
        _controller.DoubleJumpNormalState(holdTime);
    }

    public void SwordTeleport()
    {
        _controller.SwordTeleportNormalState();
    }
}