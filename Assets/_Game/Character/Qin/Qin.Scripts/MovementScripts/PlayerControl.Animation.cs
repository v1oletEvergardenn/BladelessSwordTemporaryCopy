using UnityEngine;

public partial class PlayerControl
{
    [HideInInspector] public float BackAttackBlendThreshold = 35f / 71f;
    [HideInInspector] public float BackAttackDurationScale = 71f / 35f;

    #region Link To MovementState

    private void HandleFallingAnimation()
    {
        if (rb.velocity.y < -1 && !isGrounded && !isFalling)
        {
            if (!playerAttack.isDefending)
            {
                HandleMovementStateOrNormal(
               movementStateMachine != null ? movementStateMachine.HandleFallingAnimation : null,
               HandleFallingAnimationNormalState);
            }

            isFalling = true;
            isJumping = false;
        }
        else if (isGrounded)
        {
            isFalling = false;
        }
    }

    public void HandleGroundedAnimation()
    {
        HandleMovementStateOrNormal(
            movementStateMachine != null ? movementStateMachine.HandleGroundedAnimation : null,
            HandleGroundedAnimationNormalState);
    }

    public void HandleLandingAnimation()
    {
        HandleMovementStateOrNormal(
            movementStateMachine != null ? movementStateMachine.HandleLandingAnimation : null,
            HandleLandingAnimationNormalState);
    }

    public void HandleJumpAnimation()
    {
        HandleMovementStateOrNormal(
            movementStateMachine != null ? movementStateMachine.HandleJumpAnimation : null,
            HandleJumpAnimationNormalState);
    }

    private void HandleMovementStateOrNormal(System.Action movementStateHandler, System.Action normalHandler)
    {
        if (movementStateHandler != null)
        {
            movementStateHandler();
            return;
        }

        normalHandler();
    }

    #endregion Link To MovementState

    #region Fall Anim

    public void HandleFallingAnimationNormalState()
    {
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
            else if (canSwitchNormalAnim)
                PlayAnimClipInCombat("pre_fall", "pre_fall_combat");
        }
    }

    #endregion Fall Anim

    #region Ground Anim

    /// <summary>
    /// Chooses run/idle animation transitions when grounded.
    /// Includes attack-chain, heavy-attack, drawback, and storm variants.
    /// </summary>
    public void HandleGroundedAnimationNormalState()
    {
        if (!isRunning)
        {
            //attack Run
            if (IsAttackRunState())
            {
                PlayAnim(AttackClip("idle"));
                return;
            }
            //HS attack Run
            if (IsHSAttackRunState())
            {
                PlayAnim(HSAttackClip("idle"));
                return;
            }
            //drawback run
            if (CheckName("drawback_run"))
            {
                PlayAnim("drawback_idle");
                return;
            }

            if (CheckName("attack_jump_after") || CheckName("attack_fall_after"))
            {
                PlayAnim("attack_idle_after");
                return;
            }

            if (IsStormReadyState() && !CheckName("storm_ready_idle"))
            {
                PlayAnim("storm_ready_idle");
                return;
            }

            if (IsStormPreState() && !CheckName("storm_pre_idle"))
            {
                PlayAnim("storm_pre_idle");
                return;
            }
        }
        //running state
        else
        {
            //back attack running
            if (IsAttackRunState() && playerAttack.isAttackingLeft == FacingRight)
            {
                PlayBackAttack(false);
                return;
            }

            //HS back attack running
            if (IsHSAttackRunState() &&
                playerAttack.isAttackingLeft == FacingRight)
            {
                PlayBackAttack(true);
                return;
            }

            //attack idle
            if (CheckName(AttackClip("idle")))
            {
                PlayAnim(AttackClip("run"));
                return;
            }

            //HS attack idle
            if (CheckName(HSAttackClip("idle")))
            {
                PlayAnim(HSAttackClip("run"));
                return;
            }

            //drawback idle
            if (CheckName("drawback_idle"))
            {
                PlayAnim("drawback_run");
                return;
            }

            //storm
            if (IsStormReadyState() && !CheckName("storm_ready_run"))
            {
                PlayAnim("storm_ready_run");
                return;
            }

            if (IsStormPreState() && !CheckName("storm_pre_run"))
            {
                PlayAnim("storm_pre_run");
                return;
            }
        }
    }

    #endregion Ground Anim

    #region Landing Anim

    /// <summary>
    /// Resolves landing behavior and maps airborne attack variants back to grounded equivalents.
    /// </summary>
    public void HandleLandingAnimationNormalState()
    {
        var state = anim.GetCurrentAnimatorStateInfo(0);

        if (!isJumping && !playerAttack.isDefending)
        {
            if (canSwitchNormalAnim) PlayAnimClipInCombat("land", "land_combat");

            isFalling = false;
            isJumping = false;
        }

        CameraFollow.instance.ChangeOffset(CameraFollow.instance.normalOffset);
        if (!isJumping && IsAttackJumpOrFallState())
        {
            if (isRunning)
            {
                if (playerAttack.isAttackingLeft == FacingRight)
                    PlayBackAttack(false);
                else
                    PlayAnim(AttackClip("run"));
            }
            else
            {
                PlayAnim(AttackClip("idle"));
            }
        }
        else if (!isJumping && IsHSAttackJumpOrFallState())
        {
            if (isRunning)
            {
                if (playerAttack.isAttackingLeft == FacingRight)
                {
                    PlayBackAttack(true);
                }
                else
                {
                    PlayAnim(HSAttackClip("run"));
                }
            }
            else
            {
                PlayAnim(HSAttackClip("idle"));
            }
        }
    }

    #endregion Landing Anim

    #region Jump Anim

    /// <summary>
    /// Maps current animation state into its jump variant while preserving chain timing.
    /// Returns true when a custom transition was applied.
    /// </summary>
    public void HandleJumpAnimationNormalState()
    {
        float duration = anim.GetCurrentAnimatorStateInfo(0).normalizedTime;
        if (IsAttackRunOrIdleState())
        {
            PlayAnim(AttackClip("jump"));
            return;
        }

        if (IsHSAttackRunOrIdleState())
        {
            PlayAnim(HSAttackClip("jump"));
            return;
        }

        if (CheckName(AttackBackClip(false)))
        {
            PlayAnim("attack_jump_1", duration * BackAttackBlendThreshold);
            playerAttack.attackIndex = 1;
            return;
        }

        if (CheckName(AttackBackClip(true)))
        {
            PlayAnim("HS_attack_jump_1", duration * BackAttackBlendThreshold);
            playerAttack.attackIndex = 1;
            return;
        }

        if (CheckName("attack_fall_after") || CheckName("attack_idle_after"))
        {
            PlayAnim("attack_jump_after");
            return;
        }

        if (IsStormReadyState())
        {
            PlayAnim("storm_ready_jump");
            return;
        }

        if (IsStormPreState())
        {
            PlayAnim("storm_pre_jump");
            return;
        }
    }

    #endregion Jump Anim

    #region Animation Helpers

    public void PlayBackAttack(bool isHSAttack)
    {
        float duration = anim.GetCurrentAnimatorStateInfo(0).normalizedTime;
        if (playerAttack.attackIndex == 1 && duration < BackAttackBlendThreshold)
        {
            PlayAnim(AttackBackClip(isHSAttack), duration * BackAttackDurationScale);
            return;
        }

        if (playerAttack.attackIndex == 2)
        {
            PlayAnim(AttackBackClip(isHSAttack));
            return;
        }
    }

    public string AttackClip(string phase) => "attack_" + phase + "_" + playerAttack.attackIndex;

    public string HSAttackClip(string phase) => "HS_attack_" + phase + "_" + playerAttack.attackIndex;

    public string AttackBackClip(bool isHSAttack) =>
        (isHSAttack ? "HS_attack_back_" : "attack_back_") + playerAttack.attackIndex;

    public bool IsAttackIdleState() => CheckName(AttackClip("idle"));

    public bool IsAttackRunState() => CheckName(AttackClip("run"));

    public bool IsAttackJumpState() => CheckName(AttackClip("jump"));

    public bool IsAttackFallState() => CheckName(AttackClip("fall"));

    public bool IsHSAttackJumpState() => CheckName(HSAttackClip("jump"));

    public bool IsHSAttackFallState() => CheckName(HSAttackClip("fall"));

    public bool IsHSAttackRunState() => CheckName(HSAttackClip("run"));

    public bool IsHSAttackIdleState() => CheckName(HSAttackClip("idle"));

    public bool IsStormReadyState() =>
        CheckName("storm_ready_idle") ||
        CheckName("storm_ready_jump") ||
        CheckName("storm_ready_fall") ||
        CheckName("storm_ready_run");

    public bool IsStormPreState() =>
        CheckName("storm_pre_idle") ||
        CheckName("storm_pre_jump") ||
        CheckName("storm_pre_fall") ||
        CheckName("storm_pre_run");

    public bool IsAttackRunOrIdleState() =>
        CheckName(AttackClip("run")) ||
        CheckName(AttackClip("idle"));

    public bool IsHSAttackRunOrIdleState() =>
        CheckName(HSAttackClip("run")) ||
        CheckName(HSAttackClip("idle"));

    public bool IsAttackJumpOrFallState() =>
        CheckName(AttackClip("jump")) ||
        CheckName(AttackClip("fall"));

    public bool IsHSAttackJumpOrFallState() =>
        CheckName(HSAttackClip("jump")) ||
        CheckName(HSAttackClip("fall"));

    public bool CheckName(string name) => anim.GetCurrentAnimatorStateInfo(0).IsName(name);

    public void PlayAnim(string clip, float duration = -1)
    {
        if (duration == -1) duration = anim.GetCurrentAnimatorStateInfo(0).normalizedTime;
        anim.Play(clip, 0, duration);
    }

    #endregion Animation Helpers

    #region Utility

    public void SyncRunningAnim()
    {
        var state = anim.GetCurrentAnimatorStateInfo(0);
        var legState = legAnim.GetCurrentAnimatorStateInfo(0);
        anim.Play(state.fullPathHash, 0, legState.normalizedTime);
    }

    private int lastLegStateHash = -1;

    private void DetectLegAnimationChangeAndResetBody()
    {
        var legState = legAnim.GetCurrentAnimatorStateInfo(0);
        int currentHash = legState.fullPathHash;

        if (currentHash == lastLegStateHash) return;

        lastLegStateHash = currentHash;
        ResetBodyToOrigin();
    }

    public void ResetBodyToOrigin()
    {
        body.localPosition = Vector3.zero;
    }

    private void UpdateLegAnimator()
    {
        var state = anim.GetCurrentAnimatorStateInfo(0);
        legAnim.SetBool("isRunning", IsLegRunningState(state) && isRunning);
    }

    private bool IsLegRunningState(AnimatorStateInfo state)
    {
        return state.IsName("run") ||
               state.IsName("run_combat") ||
               state.IsName("attack_run_1") ||
               state.IsName("attack_run_2") ||
               state.IsName("drawback_run") ||
               state.IsName("storm_pre_run") ||
               state.IsName("storm_ready_run") ||
               state.IsName("HS_attack_run_1") ||
               state.IsName("HS_attack_run_2");
    }

    #endregion Utility
}