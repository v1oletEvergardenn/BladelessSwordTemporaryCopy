using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public partial class PlayerControl
{
    #region Basic Movement

    private bool hasTriggeredMoveAction = false;

    /// <summary>
    /// Drives automatic run/walk toward a target X position.
    /// Stops near the target and releases the input lock.
    /// </summary>
    public void CheckRunToPos()
    {
        float deltaX = runToTarget.x - transform.position.x;
        const float stopDistance = 0.05f;

        if (Mathf.Abs(deltaX) <= stopDistance)
        {
            Move(0f, GetSpeed());
            rb.velocity = new Vector2(0f, rb.velocity.y);
            SetIsRunningToTarget(false);
            return;
        }

        float dir = Mathf.Sign(deltaX);
        runToLeft = dir < 0f;
        inputPlayer.leftPointLeft = runToLeft;
        Move(dir, GetSpeed());
    }

    public float GetSpeed() => useRunningSpeed ? runSpeed : walkSpeed;

    public float GetRunSpeed() => runSpeed;

    public float GetWalkSpeed() => walkSpeed;

    public void SetIsRunningToTarget(bool value)
    {
        isRunningToTarget = value;

        if (value)
            inputPlayer.movementInputUpdateLock.Add(MovementLockKeys.RunningToTarget);
        else
            inputPlayer.movementInputUpdateLock.Remove(MovementLockKeys.RunningToTarget);
    }

    /// <summary>
    /// Performs grounded detection and landing transition.
    /// Resets jump-related locks/flags when contact is found.
    /// </summary>
    private void GroundCheck()
    {
        if (!enableGroundCheck)
            return;

        bool wasGrounded = isGrounded;
        isGrounded = false;

        RaycastHit2D ray = Physics2D.Raycast(transform.position, Vector2.down, 0.1f, m_WhatIsGround);
        if (ray.collider == null)
            return;

        isGrounded = true;
        ActionLock.Remove(MovementLockKeys.DoubleJumping);
        floatTriggered = false;
        var state = anim.GetCurrentAnimatorStateInfo(0);

        if (!wasGrounded)
            HandleLandingAnimation(state);
    }

    #endregion Basic Movement

    #region Jump & Floating

    /// <summary>
    /// Handles floating while falling.
    /// Applies controlled downward velocity, rumble feedback, and action locks.
    /// </summary>
    private bool Float()
    {
        if (!CanJump() || !CanDoubleJump())
            return false;

        if (input_floating && !isGrounded && isFalling)
        {
            if (!floatTriggered)
            {
                SoundManager.PlaySound("sword_jump_floating");
                floatTriggered = true;
            }

            float x = rb.velocity.x;
            rb.velocity = new Vector2(x, TimeScaleManager.PlayerScale * -floatingSpeed);

            if (!isFloating)
                PlayAnimClipInCombat("sword_jump_pre", "sword_jump_pre_combat");

            var gamepad = Gamepad.current;
            if (!resetRumbleJump && gamepad != null)
            {
                gamepad.SetMotorSpeeds(floatingRumblingSpeed.x, 0f);
                resetRumbleJump = true;
            }

            ActionLock.Add(MovementLockKeys.Floating, Lock.Defend | Lock.Attack);
            return true;
        }

        if (resetRumbleJump)
        {
            var gamepad = Gamepad.current;
            if (gamepad != null)
                gamepad.SetMotorSpeeds(0f, 0f);

            resetRumbleJump = false;
        }

        ActionLock.Remove(MovementLockKeys.Floating);
        return false;
    }

    public static void CancelJump(float mutiplier = 0.1f)
    {
        if (!instance.isJumping || instance.rb.velocity.y <= 0) return;
        instance.rb.velocity = new Vector2(instance.rb.velocity.x, instance.rb.velocity.y * mutiplier);
    }

    #endregion Jump & Floating

    #region Move To Position

    /// <summary>
    /// Locks movement input and moves to target using current running speed mode.
    /// Unlocks input and restores facing when destination is reached.
    /// </summary>
    public IEnumerator RunToPositionCoroutine(Vector3 target, bool faceRight, Action callBack = null)
    {
        ActionLock.Add(MovementLockKeys.RunningToPosition, Lock.Defend | Lock.SwordTeleport);

        runToTarget = target;
        runToLeft = runToTarget.x < transform.position.x;
        inputPlayer.leftPointLeft = runToLeft;
        SetIsRunningToTarget(true);

        yield return new WaitUntil(() => !isRunningToTarget);
        ActionLock.Remove(MovementLockKeys.RunningToPosition);
        yield return null;
        Face(faceRight);
        callBack?.Invoke();
    }

    /// <summary>
    /// Same as run-to-position flow, but caller typically sets walking mode before starting it.
    /// </summary>
    public IEnumerator WalkToPositionCoroutine(Vector3 target, bool faceRight, Action callBack = null)
    {
        ActionLock.Add(MovementLockKeys.RunningToPosition, Lock.Defend | Lock.SwordTeleport);

        runToTarget = target;
        runToLeft = runToTarget.x < transform.position.x;
        inputPlayer.leftPointLeft = runToLeft;
        SetIsRunningToTarget(true);

        yield return new WaitUntil(() => !isRunningToTarget);
        ActionLock.Remove(MovementLockKeys.RunningToPosition);
        yield return null;
        Face(faceRight);
        callBack?.Invoke();
    }

    public void RunToPosition(Vector3 target, bool faceRight, Action callBack = null)
    {
        SetRunning(true);
        StartCoroutine(RunToPositionCoroutine(target, faceRight, callBack));
    }

    public void RunToPosition(Vector3 target, Action callBack = null)
    {
        SetRunning(true);
        StartCoroutine(RunToPositionCoroutine(target, FacingRight, callBack));
    }

    public void RunToPosition(float x, bool faceRight, Action callBack = null)
    {
        SetRunning(true);
        RunToPosition(new Vector3(x, transform.position.y, 0), faceRight, callBack);
    }

    public void WalkToPosition(Vector3 target, bool faceRight, Action callBack = null)
    {
        SetRunning(false);
        StartCoroutine(WalkToPositionCoroutine(target, faceRight, callBack));
    }

    public void WalkToPosition(Vector3 target, Action callBack = null)
    {
        SetRunning(false);
        StartCoroutine(WalkToPositionCoroutine(target, FacingRight, callBack));
    }

    public void WalkToPosition(float x, bool faceRight, Action callBack = null)
    {
        SetRunning(false);
        WalkToPosition(new Vector3(x, transform.position.y, 0), faceRight, callBack);
    }

    public void SetRunning(bool run) => useRunningSpeed = run;

    #endregion Move To Position

    #region Utility

    public void EnableGravity(bool enable)
    {
        rb.gravityScale = enable ? gravity : 0;
    }

    public void StopMovement()
    {
        if (isJumping) return;
        rb.velocity = Vector3.zero;
    }

    public void PlayAnimClipInCombat(string normalClip, string combatClip)
    {
        anim.Play(playerAttack.isInCombat ? combatClip : normalClip);
    }

    public Quaternion CalculateWantedRotation(Vector3 targetPos)
    {
        float angle = Mathf.Atan2(targetPos.y - transform.position.y, targetPos.x - transform.position.x) * Mathf.Rad2Deg;
        return Quaternion.Euler(0, 0, angle);
    }

    #endregion Utility

    #region Timing Helpers

    private void UpdateCoyoteTimer()
    {
        if (isGrounded && !isJumping)
            coyoteTimer = coyoteTime;
        else
            coyoteTimer -= TimeScaleManager.Delta(TimeChannel.Player);
    }

    public void SetRunningState(bool running)
    {
        anim.SetBool("isRunning", running);
        isRunning = running;
    }

    #endregion Timing Helpers
}