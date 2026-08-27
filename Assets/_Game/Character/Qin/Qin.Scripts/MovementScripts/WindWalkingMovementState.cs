using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class WindWalkingMovementState : IMovementState
{
    public WindWalkingMovementState(PlayerControl controller) : base(controller)
    {
        base.controller = controller;
        base.playerAttack = PlayerAttack.instance;
        base.energy = Energy.instance;
        base.health = Health.instance;
        base.anim = controller.anim;
        base.legAnim = controller.legAnim;
        base.rb = controller.rb;
    }

    public override PlayerMovementStateType Type => PlayerMovementStateType.WindWalking;

    private const float MoveInputThreshold = 0.01f;
    private const float StepDistance = 1.1f;
    private const float StepMoveDuration = 0.42f;

    private const float StepIdleDuration = 0.4f;

    private const string BodyWindIdle = "wind_idle";
    private const string LegWindWalk1 = "leg_wind_walk_1";
    private const string LegWindWalk2 = "leg_wind_walk_2";

    private Tween _stepTween;
    private float _idleTimer;
    private bool _isStepping;
    private bool _hasMoveInput;
    private float _lastInputDirection;
    private int _nextStepIndex;

    public override void Enter()
    {
        _isStepping = false;
        _idleTimer = 0f;
        _hasMoveInput = false;
        _lastInputDirection = FacingRight ? 1f : -1f;
        _nextStepIndex = 1;

        ApplyBodyIdle();
        PlayIfNotCurrent(legAnim, "leg_wind_idle_1");
        ActionLock.AddExcept("WindWalking", Lock.Move | Lock.Attack | Lock.Flip);
    }

    public override void Exit()
    {
        KillStepTween();
        controller.SetRunningState(false);

        if (controller.rb != null)
            controller.rb.velocity = new Vector2(0f, controller.rb.velocity.y);
        ActionLock.Remove("WindWalking");
    }

    public override void Tick()
    {
        controller.TickNormalState();
        UpdateStepFlow();

        if (_isStepping && (CheckNameLeg("leg_wind_idle_2") || CheckNameLeg("leg_wind_idle_1")))
        {
            PlayIfNotCurrent(legAnim, "leg_wind_walk_" + _nextStepIndex, _stepTween.ElapsedPercentage());
        }
    }

    public override void FixedTick()
    {
        controller.FixedTickNormalState();
    }

    /// <summary>
    /// Handles horizontal movement with fixed-distance tweened wind-walking steps.
    /// </summary>
    /// <param name="input">Horizontal movement input.</param>
    /// <param name="speed">Movement speed override (unused in wind-walking).</param>
    public override void Move(float input, float speed)
    {
        _hasMoveInput = Mathf.Abs(input) > MoveInputThreshold;

        if (_hasMoveInput)
            _lastInputDirection = Mathf.Sign(input);

        // Prevent post-attack auto-flip when counter-attack is active in wind-walking.
        if (_hasMoveInput && !_isStepping && !playerAttack.isCounterAttacking)
            controller.HandleFlipping(_lastInputDirection);

        if (!_isStepping && _idleTimer <= 0f)
            TryStartStep();
    }

    public override string GetAttackAnimName()
    {
        string indexStr = playerAttack.attackIndex.ToString();

        if (FacingRight == isAttackingLeft)
        {
            legAnim.Play("leg_wind_back_attack");
            return $"walk_attack_back";
        }
        else
        {
            return $"walk_wind_attack_{indexStr}";
        }
    }

    public override void HandleFallingAnimation()
    {
        legAnim.Play("empty");
        //isAttacking
        if (IsAttackRunState() || IsAttackJumpState() || IsAttackIdleState())
        {
            PlayAnim(AttackClip("fall"));
            return;
        }

        //isHSAttacking
        if (IsHSAttackRunState() || IsHSAttackJumpState() || IsHSAttackIdleState())
        {
            PlayAnim(HSAttackClip("fall"));
            return;
        }

        //is attack after
        if (CheckName("attack_idle_after") || CheckName("attack_jump_after"))
        {
            PlayAnim("attack_fall_after");
            return;
        }

        //teleport in air
        if (CheckName("tele_pre_jump"))
        {
            PlayAnim("tele_pre_fall");
            return;
        }

        //storm state
        if (IsStormReadyState())
        {
            PlayAnim("storm_ready_fall");
            return;
        }
        else if (IsStormPreState())
        {
            PlayAnim("storm_pre_fall");
            return;
        }
        else
        {
            if (playerAttack.isPreparingStorm)
            {
                PlayAnim("storm_ready_fall");
            }
            else if (controller.canSwitchNormalAnim)
                PlayAnimClipInCombat("pre_fall", "pre_fall_combat");
        }
    }

    /// <summary>
    /// Resolves landing behavior and maps airborne attack variants back to grounded equivalents.
    /// </summary>
    public override void HandleLandingAnimation()
    {
        var state = anim.GetCurrentAnimatorStateInfo(0);

        if (!controller.isJumping && !playerAttack.isDefending)
        {
            PlayAnim("wind_idle");
            legAnim.Play("leg_wind_idle_" + _nextStepIndex);
            controller.isFalling = false;
            controller.isJumping = false;
        }

        CameraFollow.instance.ChangeOffset(CameraFollow.instance.normalOffset);

        float duration = state.normalizedTime;
        if (!controller.isJumping && IsAttackJumpOrFallState())
        {
            HandleLandingAttackTransition(duration);
        }
    }

    private void HandleLandingAttackTransition(float duration)
    {
        if (controller.isRunning)
        {
            if (playerAttack.isAttackingLeft == controller.FacingRight)
                PlayBackAttack(false);
            else
                PlayAnim("walk_wind_attack_" + playerAttack.attackIndex);
        }
    }

    public override string AttackBackClip(bool isHSAttack) => ("walk_attack_back");

    public override void PlayBackAttack(bool isHSAttack)
    {
        float duration = anim.GetCurrentAnimatorStateInfo(0).normalizedTime;
        if (playerAttack.attackIndex == 1 && duration < controller.BackAttackBlendThreshold)
        {
            PlayAnim(AttackBackClip(isHSAttack), duration * controller.BackAttackDurationScale);
            legAnim.Play("leg_wind_back_attack");
            return;
        }

        if (playerAttack.attackIndex == 2)
        {
            PlayAnim(AttackBackClip(isHSAttack));
            legAnim.Play("leg_wind_back_attack");
            return;
        }
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
            controller.SetRunningState(false);
            return;
        }

        if (!controller.CanMove())
        {
            controller.SetRunningState(false);
            return;
        }

        StartStep(_lastInputDirection);
    }

    private void StartStep(float direction)
    {
        KillStepTween();

        _isStepping = true;
        controller.SetRunningState(true);

        // Ensure facing is updated once, before step begins.
        controller.HandleFlipping(direction);

        PlayWalkAnimationForStep();

        Vector3 pos = controller.transform.position;
        float targetX = pos.x + direction * StepDistance;

        _stepTween = controller.transform
            .DOMoveX(targetX, StepMoveDuration)
            .SetEase(Ease.InSine)
            .SetTimeDt(controller, TimeChannel.Player)
            .OnComplete(OnStepComplete);
    }

    private void OnStepComplete()
    {
        _isStepping = false;
        controller.SetRunningState(false);

        if (controller.rb != null)
            controller.rb.velocity = new Vector2(0f, controller.rb.velocity.y);

        _idleTimer = StepIdleDuration;
    }

    private void PlayWalkAnimationForStep()
    {
        if (CheckNameLeg("leg_wind_back_attack")) return;
        string clip = _nextStepIndex == 1 ? LegWindWalk1 : LegWindWalk2;
        PlayIfNotCurrent(controller.legAnim, clip);
        _nextStepIndex = _nextStepIndex == 1 ? 2 : 1;
    }

    private void ApplyBodyIdle()
    {
        PlayIfNotCurrent(controller.anim, BodyWindIdle);
    }

    private void KillStepTween()
    {
        if (_stepTween == null)
            return;

        if (_stepTween.active)
            _stepTween.Kill();

        _stepTween = null;
    }

    private static void PlayIfNotCurrent(Animator animator, string stateName, float duration = 0f)
    {
        if (animator == null)
            return;

        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
        if (state.IsName(stateName))
            return;

        animator.Play(stateName, 0, duration);
    }
}