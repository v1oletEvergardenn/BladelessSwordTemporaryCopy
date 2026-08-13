using UnityEngine;

public partial class PlayerControl
{
    #region Movement Entry Points

    public void SwordTeleport()
    {
        if (movementStateMachine != null)
        {
            movementStateMachine.SwordTeleport();
            return;
        }

        SwordTeleportNormalState();
    }

    public void Move(float move)
    {
        if (movementStateMachine != null)
        {
            movementStateMachine.Move(move);
            return;
        }

        MoveNormalState(move, GetSpeed());
    }

    public void Move(float move, float speed)
    {
        if (movementStateMachine != null)
        {
            movementStateMachine.Move(move, speed);
            return;
        }

        MoveNormalState(move, speed);
    }

    public void Jump()
    {
        if (movementStateMachine != null)
        {
            movementStateMachine.Jump();
            return;
        }

        JumpNormalState();
    }

    public void DoubleJump(float holdTime)
    {
        if (movementStateMachine != null)
        {
            movementStateMachine.DoubleJump(holdTime);
            return;
        }

        DoubleJumpNormalState(holdTime);
    }

    #endregion Movement Entry Points

    #region Movement States

    /// <summary>
    /// Registers every available movement state and activates the initial state.
    /// Falls back to <see cref="PlayerMovementStateType.Normal"/> when the serialized state is not registered.
    /// </summary>
    private void InitializeMovementStates()
    {
        movementStateMachine = new MovementStateMachine();
        movementStateMachine.Register(new NormalMovementState(this));
        movementStateMachine.Register(new WindWalkingMovementState(this));

        if (!movementStateMachine.ChangeState(movementState))
        {
            movementState = PlayerMovementStateType.Normal;
            movementStateMachine.ChangeState(movementState);
        }
    }

    /// <summary>
    /// Requests a runtime state transition.
    /// The backing enum is updated only when the state machine accepts the transition.
    /// </summary>
    public void SetMovementState(PlayerMovementStateType newState)
    {
        if (movementState == newState)
            return;

        if (movementStateMachine == null)
        {
            movementState = newState;
            return;
        }

        if (movementStateMachine.ChangeState(newState))
            movementState = newState;
    }

    /// <summary>
    /// Per-frame update for the Normal movement state.
    /// Keeps movement, animation, camera damping, and run-to-target behavior in sync.
    /// </summary>
    internal void TickNormalState()
    {
        UpdateLegAnimator();
        UpdateCoyoteTimer();
        isFloating = Float();
        teleportTimer += TimeScaleManager.PlayerDt;
        HandleFallingAnimation();
        HandleCameraDamping();
        if (isRunningToTarget) CheckRunToPos();
    }

    /// <summary>
    /// Physics update for the Normal movement state.
    /// Applies manual gravity and updates grounded status.
    /// </summary>
    internal void FixedTickNormalState()
    {
        float playerScale = TimeScaleManager.PlayerScale;
        rb.velocity += Physics2D.gravity * gravity * TimeScaleManager.FixedDelta(TimeChannel.Player) * playerScale;
        GroundCheck();
    }

    /// <summary>
    /// Core horizontal movement logic used by the Normal state.
    /// Handles locks, smoothing, air-control speed overrides, run animation transitions, and facing direction.
    /// </summary>
    internal void MoveNormalState(float move, float speed)
    {
        if (!CanMove())
        {
            SetRunningState(false);
            rb.velocity = Vector3.SmoothDamp(
                rb.velocity,
                new Vector2(0f, rb.velocity.y),
                ref m_Velocity,
                m_MovementSmoothing,
                Mathf.Infinity,
                TimeScaleManager.Delta(TimeChannel.Player)
            );
            hasTriggeredMoveAction = false;
            return;
        }

        if (!(isGrounded || m_AirControl))
            return;

        bool moving = move != 0;
        SetRunningState(moving);

        if (moving && !hasTriggeredMoveAction)
        {
            QuestManager.OnAction(GameManager.instance.playerQuestActionKey.Move);
            hasTriggeredMoveAction = true;
        }

        if (!isGrounded && m_AirControl)
        {
            speed = isFloating ? 0f : airRunSpeed;
            SetRunningState(false);
        }

        var state = anim.GetCurrentAnimatorStateInfo(0);

        if (isGrounded && !isJumping)
            HandleGroundedAnimationTransitions(state, moving);

        float playerScale = TimeScaleManager.PlayerScale;
        Vector3 targetVelocity = new Vector2(move * speed * playerScale, rb.velocity.y);

        rb.velocity = Vector3.SmoothDamp(
            rb.velocity,
            targetVelocity,
            ref m_Velocity,
            m_MovementSmoothing,
            Mathf.Infinity,
            TimeScaleManager.Delta(TimeChannel.Player)
        );

        HandleFlipping(move);
    }

    /// <summary>
    /// Executes a Normal-state jump using coyote time and contextual animation transition mapping.
    /// </summary>
    internal void JumpNormalState()
    {
        if (!CanJump() || coyoteTimer <= 0f) return;

        SoundManager.PlaySound("jump", random: true);
        coyoteTimer = 0f;
        isGrounded = false;
        float x = rb.velocity.x;
        rb.velocity = new Vector2(x, m_JumpForce * TimeScaleManager.PlayerScale);
        isJumping = true;

        QuestManager.OnAction(GameManager.instance.playerQuestActionKey.Jump);
        var state = anim.GetCurrentAnimatorStateInfo(0);
        float duration = state.normalizedTime;

        if (HandleJumpAnimationTransitions(state, duration))
            return;

        if (canSwitchNormalAnim)
            PlayAnimClipInCombat("jump", "jump_combat");
    }

    /// <summary>
    /// Executes a Normal-state double jump.
    /// Consumes energy, scales force by hold time, updates lock/state flags, and resolves jump-attack side effects.
    /// </summary>
    internal void DoubleJumpNormalState(float holdTime)
    {
        if (!CanDoubleJump() || isGrounded) return;
        if (!energy.DoubleJumpConsume()) return;

        float x = rb.velocity.x;
        float strength = Mathf.Lerp(MinDoubleJumpForceMultiplier, DoubleJumpForceMultiplier, holdTime / DoubleJumpForceTime);

        SoundManager.PlaySound("sword_jump");
        rb.velocity = new Vector2(x, m_JumpForce * strength * TimeScaleManager.PlayerScale);

        ActionLock.Add(MovementLockKeys.DoubleJumping, Lock.SwordJump);
        isFloating = false;
        isFalling = false;
        isJumping = true;

        QuestManager.OnAction(GameManager.instance.playerQuestActionKey.SwordJump);
        PlayAnimClipInCombat("sword_jump_after", "sword_jump_after_combat");

        bool hit = playerAttack.JumpAttack();
        if (hit)
        {
            ActionLock.Remove(MovementLockKeys.DoubleJumping);
            floatTriggered = false;
        }
    }

    /// <summary>
    /// Starts Normal-state sword teleport when lock, cooldown, and energy checks pass.
    /// </summary>
    internal void SwordTeleportNormalState()
    {
        if (!CanTeleport()) return;
        if (teleportTimer <= teleportCD || !energy.TeleportConsume()) return;
        teleportTimer = 0f;
        teleported = false;
        QuestManager.OnAction(GameManager.instance.playerQuestActionKey.swordTeleport);
        co_teleport = StartCoroutine(TeleportCoroutine(FacingRight));
    }

    #endregion Movement States
}