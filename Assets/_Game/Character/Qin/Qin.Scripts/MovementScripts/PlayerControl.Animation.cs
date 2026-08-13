using UnityEngine;

public partial class PlayerControl
{
    #region Flipping & Facing

    public void FaceTarget(Transform target)
    {
        if ((target.position.x <= transform.position.x && FacingRight) ||
            (target.position.x > transform.position.x && !FacingRight))
        {
            Flip();
        }
    }

    public void FaceTarget(Vector3 pos)
    {
        if ((pos.x <= transform.position.x && FacingRight) ||
            (pos.x > transform.position.x && !FacingRight))
        {
            Flip();
        }
    }

    public void Face(bool right)
    {
        if (FacingRight != right)
        {
            Flip();
        }
    }

    /// <summary>
    /// Handles direction flip rules during regular movement and counter-attack states.
    /// Counter-attack path preserves animation continuity before applying final facing change.
    /// </summary>
    public void HandleFlipping(float move)
    {
        if (playerAttack.isCounterAttacking)
        {
            if (isRunning)
            {
                bool shouldFlip = (move > 0f && !FacingRight) || (move < 0f && FacingRight);
                if (!shouldFlip)
                    return;

                var state = anim.GetCurrentAnimatorStateInfo(0);
                float duration = state.normalizedTime;
                if (IsAttackRunState(state) && playerAttack.isAttackingLeft == (move > 0f))
                {
                    if (playerAttack.attackIndex == 1 && duration < (35f / 71f))
                        anim.Play("attack_back_" + playerAttack.attackIndex, 0, duration * (71f / 35f));
                    else if (playerAttack.attackIndex == 2)
                        anim.Play("attack_back_" + playerAttack.attackIndex, 0, duration);
                    else
                        anim.Play(anim.GetBool("storm") ? "storm_pre_run" : "run_combat");
                }

                Flip();
                return;
            }

            if (playerAttack.isAttackingLeft == FacingRight)
                Flip();

            return;
        }

        if (move > 0f && !FacingRight) Flip();
        else if (move < 0f && FacingRight) Flip();
    }

    public void Flip(bool ignoreCamFollowFlip = false)
    {
        if (!CanFlip()) return;
        var state = anim.GetCurrentAnimatorStateInfo(0);
        if ((state.IsName("attack_back_" + playerAttack.attackIndex) ||
            state.IsName("HS_attack_back_" + playerAttack.attackIndex)) &&
            playerAttack.isAttackingLeft == FacingRight)
            return;

        FacingRight = !FacingRight;
        transform.Rotate(new Vector3(0, 1, 0), 180);
        playerAttack.counterAttackPoint.Rotate(new Vector3(1, 0, 0), 180);

        if (!ignoreCamFollowFlip && camFollowDirection != FacingRight)
        {
            camFollowDirection = !camFollowDirection;
            camFollow.CallTurn();
        }
    }

    public void ForceFlip()
    {
        FacingRight = !FacingRight;
        transform.Rotate(new Vector3(0, 1, 0), 180);
        playerAttack.counterAttackPoint.Rotate(new Vector3(1, 0, 0), 180);

        if (camFollowDirection != FacingRight)
        {
            camFollowDirection = !camFollowDirection;
            camFollow.CallTurn();
        }
    }

    #endregion Flipping & Facing

    #region Animation Helpers

    private void UpdateLegAnimator()
    {
        var s = anim.GetCurrentAnimatorStateInfo(0);
        if (s.IsName("run") ||
            s.IsName("run_combat") ||
            s.IsName("attack_run_1") ||
            s.IsName("attack_run_2") ||
            s.IsName("drawback_run") ||
            s.IsName("storm_pre_run") ||
            s.IsName("storm_ready_run") ||
            s.IsName("HS_attack_run_1") ||
            s.IsName("HS_attack_run_2"))
        {
            legAnim.SetBool("isRunning", isRunning);
        }
        else
        {
            legAnim.SetBool("isRunning", false);
        }
    }

    /// <summary>
    /// Switches player to falling variants based on the current combat animation context.
    /// Preserves normalized time to keep transitions visually continuous.
    /// </summary>
    private void HandleFallingAnimation()
    {
        if (rb.velocity.y < -1 && !isGrounded && !isFalling)
        {
            if (!playerAttack.isDefending)
            {
                var state = anim.GetCurrentAnimatorStateInfo(0);
                float duration = state.normalizedTime;
                if (IsAttackRunState(state) || IsAttackJumpState(state) || IsAttackIdleState(state))
                    anim.Play("attack_fall_" + playerAttack.attackIndex, 0, duration);
                else if (IsHSAttackRunState(state) || IsHSAttackJumpState(state) || IsHSAttackIdleState(state))
                    anim.Play("HS_attack_fall_" + playerAttack.attackIndex, 0, duration);
                else if (state.IsName("attack_idle_after") || state.IsName("attack_jump_after"))
                    anim.Play("attack_fall_after", 0, duration);
                else if (IsStormReadyState(state))
                    anim.Play("storm_ready_fall", 0, duration);
                else if (IsStormPreState(state))
                    anim.Play("storm_pre_fall", 0, duration);
                else if (state.IsName("tele_pre_jump"))
                    anim.Play("tele_pre_fall", 0, duration);
                else
                {
                    if (playerAttack.isPreparingStorm)
                        anim.Play("storm_ready_fall");
                    else if (canSwitchNormalAnim)
                        PlayAnimClipInCombat("pre_fall", "pre_fall_combat");
                }
            }
            isFalling = true;
            isJumping = false;
        }
        else if (isGrounded)
        {
            isFalling = false;
        }
    }

    /// <summary>
    /// Controls camera Y damping changes when the player starts/stops falling.
    /// Prevents repeated damping lerps by checking camera manager flags.
    /// </summary>
    private void HandleCameraDamping()
    {
        if (!isGrounded &&
            rb.velocity.y < _fallSpeedYDampingChangeThreshold &&
            !CameraManager.instance.isLerpingYDaming &&
            !CameraManager.instance.lerpedFromPlayerFalling)
        {
            CameraManager.instance.LerpYDamping(true);
            CameraFollow.instance.ChangeOffset(CameraFollow.instance.fallingOffset);
        }

        if ((isGrounded || rb.velocity.y >= 0f) &&
            !CameraManager.instance.isLerpingYDaming &&
            CameraManager.instance.lerpedFromPlayerFalling)
        {
            CameraManager.instance.lerpedFromPlayerFalling = false;
            CameraManager.instance.LerpYDamping(false);
            CameraFollow.instance.ChangeOffset(CameraFollow.instance.normalOffset);
        }
    }

    /// <summary>
    /// Chooses run/idle animation transitions when grounded.
    /// Includes attack-chain, heavy-attack, drawback, and storm variants.
    /// </summary>
    public void HandleGroundedAnimationTransitions(AnimatorStateInfo state, bool isRunning)
    {
        float duration = state.normalizedTime;
        if (isRunning)
        {
            if (IsAttackRunState(state) && playerAttack.isAttackingLeft == FacingRight)
            {
                if (playerAttack.attackIndex == 1 && duration < (35f / 71f))
                    anim.Play("attack_back_" + playerAttack.attackIndex, 0, duration * (71f / 35f));
                else if (playerAttack.attackIndex == 2)
                    anim.Play("attack_back_" + playerAttack.attackIndex, 0, duration);
                else
                    anim.Play(anim.GetBool("storm") ? "storm_pre_run" : "run_combat");
            }
            else if (IsHSAttackRunState(state) && playerAttack.isAttackingLeft == FacingRight)
            {
                if (playerAttack.attackIndex == 1 && duration < (35f / 71f))
                    anim.Play("HS_attack_back_" + playerAttack.attackIndex, 0, duration * (71f / 35f));
                else if (playerAttack.attackIndex == 2)
                    anim.Play("HS_attack_back_" + playerAttack.attackIndex, 0, duration);
                else
                    anim.Play(anim.GetBool("storm") ? "storm_pre_run" : "run_combat");
            }
            else if (state.IsName("attack_idle_" + playerAttack.attackIndex))
                anim.Play("attack_run_" + playerAttack.attackIndex, 0, duration);
            else if (state.IsName("HS_attack_idle_" + playerAttack.attackIndex))
                anim.Play("HS_attack_run_" + playerAttack.attackIndex, 0, duration);
            else if (state.IsName("drawback_idle"))
                anim.Play("drawback_run", 0, duration);
            else if (IsStormReadyState(state) && !state.IsName("storm_ready_run"))
                anim.Play("storm_ready_run", 0, duration);
            else if (IsStormPreState(state) && !state.IsName("storm_pre_run"))
                anim.Play("storm_pre_run", 0, duration);
        }
        else
        {
            if (IsAttackRunState(state))
                anim.Play("attack_idle_" + playerAttack.attackIndex, 0, duration);
            else if (IsHSAttackRunState(state))
                anim.Play("HS_attack_idle_" + playerAttack.attackIndex, 0, duration);
            else if (state.IsName("drawback_run"))
                anim.Play("drawback_idle", 0, duration);
            else if (state.IsName("attack_jump_after") || state.IsName("attack_fall_after"))
                anim.Play("attack_idle_after", 0, duration);
            else if (IsStormReadyState(state) && !state.IsName("storm_ready_idle"))
                anim.Play("storm_ready_idle", 0, duration);
            else if (IsStormPreState(state) && !state.IsName("storm_pre_idle"))
                anim.Play("storm_pre_idle", 0, duration);
        }
    }

    /// <summary>
    /// Resolves landing behavior and maps airborne attack variants back to grounded equivalents.
    /// </summary>
    private void HandleLandingAnimation(AnimatorStateInfo state)
    {
        if (!isJumping && !playerAttack.isDefending)
        {
            if (!state.IsName("slash") && !state.IsName("slash_end") && canSwitchNormalAnim)
                PlayAnimClipInCombat("land", "land_combat");
            isFalling = false;
            isJumping = false;
        }

        CameraFollow.instance.ChangeOffset(CameraFollow.instance.normalOffset);

        float duration = state.normalizedTime;
        if (!isJumping && IsAttackJumpOrFallState(state))
        {
            if (isRunning)
            {
                if (playerAttack.isAttackingLeft == FacingRight)
                {
                    if (playerAttack.attackIndex == 1 && duration < (35f / 71f))
                        anim.Play("attack_back_" + playerAttack.attackIndex, 0, duration * (71f / 35f));
                    else if (playerAttack.attackIndex == 2)
                        anim.Play("attack_back_" + playerAttack.attackIndex, 0, duration);
                    else
                        anim.Play(anim.GetBool("storm") ? "storm_pre_run" : "run_combat");
                }
                else
                    anim.Play("attack_run_" + playerAttack.attackIndex, 0, duration);
            }
            else
            {
                anim.Play("attack_idle_" + playerAttack.attackIndex, 0, duration);
            }
        }
        else if (!isJumping && IsHSAttackJumpOrFallState(state))
        {
            if (isRunning)
            {
                if (playerAttack.isAttackingLeft == FacingRight)
                {
                    if (playerAttack.attackIndex == 1 && duration < (35f / 71f))
                        anim.Play("HS_attack_back_" + playerAttack.attackIndex, 0, duration * (71f / 35f));
                    else if (playerAttack.attackIndex == 2)
                        anim.Play("HS_attack_back_" + playerAttack.attackIndex, 0, duration);
                }
                else
                {
                    anim.Play("HS_attack_run_" + playerAttack.attackIndex, 0, duration);
                }
            }
            else
            {
                anim.Play("HS_attack_idle_" + playerAttack.attackIndex, 0, duration);
            }
        }
    }

    /// <summary>
    /// Maps current animation state into its jump variant while preserving chain timing.
    /// Returns true when a custom transition was applied.
    /// </summary>
    private bool HandleJumpAnimationTransitions(AnimatorStateInfo state, float duration)
    {
        if (IsAttackRunOrIdleState(state))
        {
            anim.Play("attack_jump_" + playerAttack.attackIndex, 0, duration);
            return true;
        }

        if (IsHSAttackRunOrIdleState(state))
        {
            anim.Play("HS_attack_jump_" + playerAttack.attackIndex, 0, duration);
            return true;
        }

        if (state.IsName("attack_back_" + playerAttack.attackIndex))
        {
            anim.Play("attack_jump_1", 0, duration * (35f / 71f));
            playerAttack.attackIndex = 1;
            return true;
        }

        if (state.IsName("HS_attack_back_" + playerAttack.attackIndex))
        {
            anim.Play("HS_attack_jump_1", 0, duration * (35f / 71f));
            playerAttack.attackIndex = 1;
            return true;
        }

        if (state.IsName("attack_fall_after") || state.IsName("attack_idle_after"))
        {
            anim.Play("attack_jump_after", 0, duration);
            return true;
        }

        if (IsStormReadyState(state))
        {
            anim.Play("storm_ready_jump", 0, duration);
            return true;
        }

        if (IsStormPreState(state))
        {
            anim.Play("storm_pre_jump", 0, duration);
            return true;
        }

        return false;
    }

    private bool IsAttackIdleState(AnimatorStateInfo s) => s.IsName("attack_idle_" + playerAttack.attackIndex);

    private bool IsAttackRunState(AnimatorStateInfo s) => s.IsName("attack_run_" + playerAttack.attackIndex);

    private bool IsAttackJumpState(AnimatorStateInfo s) => s.IsName("attack_jump_" + playerAttack.attackIndex);

    private bool IsAttackFallState(AnimatorStateInfo s) => s.IsName("attack_fall_" + playerAttack.attackIndex);

    private bool IsHSAttackJumpState(AnimatorStateInfo s) => s.IsName("HS_attack_jump_" + playerAttack.attackIndex);

    private bool IsHSAttackFallState(AnimatorStateInfo s) => s.IsName("HS_attack_fall_" + playerAttack.attackIndex);

    private bool IsHSAttackRunState(AnimatorStateInfo s) => s.IsName("HS_attack_run_" + playerAttack.attackIndex);

    private bool IsHSAttackIdleState(AnimatorStateInfo s) => s.IsName("HS_attack_idle_" + playerAttack.attackIndex);

    private bool IsStormReadyState(AnimatorStateInfo s) =>
        s.IsName("storm_ready_idle") || s.IsName("storm_ready_jump") ||
        s.IsName("storm_ready_fall") || s.IsName("storm_ready_run");

    private bool IsStormPreState(AnimatorStateInfo s) =>
        s.IsName("storm_pre_idle") || s.IsName("storm_pre_jump") ||
        s.IsName("storm_pre_fall") || s.IsName("storm_pre_run");

    private bool IsAttackRunOrIdleState(AnimatorStateInfo s) =>
        s.IsName("attack_run_" + playerAttack.attackIndex) || s.IsName("attack_idle_" + playerAttack.attackIndex);

    private bool IsHSAttackRunOrIdleState(AnimatorStateInfo s) =>
        s.IsName("HS_attack_run_" + playerAttack.attackIndex) || s.IsName("HS_attack_idle_" + playerAttack.attackIndex);

    private bool IsAttackJumpOrFallState(AnimatorStateInfo s) =>
        s.IsName("attack_jump_" + playerAttack.attackIndex) || s.IsName("attack_fall_" + playerAttack.attackIndex);

    private bool IsHSAttackJumpOrFallState(AnimatorStateInfo s) =>
        s.IsName("HS_attack_jump_" + playerAttack.attackIndex) || s.IsName("HS_attack_fall_" + playerAttack.attackIndex);

    public void SyncRunningAnim()
    {
        var state = anim.GetCurrentAnimatorStateInfo(0);
        var legState = legAnim.GetCurrentAnimatorStateInfo(0);
        anim.Play(state.fullPathHash, 0, legState.normalizedTime);
    }

    #endregion Animation Helpers

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
}