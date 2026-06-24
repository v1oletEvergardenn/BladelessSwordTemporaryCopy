using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using EditorAttributes;
using UnityEngine.InputSystem;

using static UnityEngine.EventSystems.EventTrigger;
using UnityEngine.InputSystem.XR;
using DG.Tweening;
using System;

using Void = EditorAttributes.Void;

[SelectionBase]
public class CharacterController2D : MonoBehaviour
{
    #region Singleton & References

    public static CharacterController2D instance;
    [Title("references", 15)][SerializeField] public Animator anim;
    [HideInInspector] public Rigidbody2D rb;
    private PlayerAttack playerAttack;
    private Energy energy;
    private Health health;
    private InputPlayer inputPlayer;
    private CapsuleCollider2D capsuleCollider;
    [SerializeField] private Transform pointer;
    [SerializeField] private CameraFollow camFollow;

    #endregion Singleton & References

    #region Ground Check

    [Title("GroundCheck", 15)][SerializeField] public LayerMask m_WhatIsGround;
    [SerializeField] private float coyoteTime = 0.2f;
    [HideInInspector] public float coyoteTimer = 0f;
    public bool isGrounded;
    [HideInInspector] public bool enableGroundCheck = true;

    #endregion Ground Check

    #region Movement Variables

    [Title("Variables", 15)]
    [FoldoutGroup("Movement Variables", nameof(FacingRight), nameof(m_AirControl), nameof(m_MovementSmoothing), nameof(runSpeed), nameof(airRunSpeed))]
    [SerializeField] private Void movementGroupHold;

    [HideInInspector] public bool m_AirControl = false;
    [SerializeField, HideInInspector, Range(0, .3f)] private float m_MovementSmoothing = .05f;
    [SerializeField, HideInInspector] public bool FacingRight = true;
    [HideInInspector] public bool camFollowDirection = false;
    [SerializeField, HideInInspector] private float runSpeed = 5f;
    [SerializeField, HideInInspector] private float airRunSpeed = 5f;
    private Vector3 m_Velocity = Vector3.zero;

    [HideInInspector] public bool isRunningToTarget = false;
    [HideInInspector] public Vector3 runToTarget;
    [HideInInspector] public float gravity;
    private bool runToLeft = false;
    [HideInInspector] public bool canSwitchNormalAnim = true;

    #endregion Movement Variables

    #region Jump & Floating Variables

    [FoldoutGroup("Jump&&floating Variables", nameof(m_JumpForce), nameof(DoubleJumpForceMultiplier), nameof(MinDoubleJumpForceMultiplier), nameof(DoubleJumpForceTime), nameof(floatingSpeed), nameof(floatingRumblingSpeed))]
    [SerializeField] private Void JumpGroupHold;

    [SerializeField, HideInInspector, Range(0f, 20f)] private float m_JumpForce = 5f;
    [SerializeField, HideInInspector, Range(1f, 6f)] private float DoubleJumpForceMultiplier = 1f;
    [SerializeField, HideInInspector, Range(0f, 2f)] private float MinDoubleJumpForceMultiplier = 1f;
    [SerializeField, HideInInspector, Range(0f, 4f)] private float DoubleJumpForceTime = 1f;
    [SerializeField, HideInInspector, Range(0f, 20f)] private float floatingSpeed;
    [SerializeField, HideInInspector, MinMaxSlider(0, 1, true)] public Vector2 floatingRumblingSpeed;
    [HideProperty] public bool input_floating;
    [HideProperty] public float floatingTime = 0f;
    [HideProperty] public bool resetRumbleJump = false;
    [HideProperty] public bool isJumping = false;
    [HideProperty] public bool isFloating;
    [HideProperty] public bool floatTriggered = false;
    [HideProperty] public bool isFalling = false;
    [HideProperty] public bool isRunning = false;
    private float _fallSpeedYDampingChangeThreshold;

    #endregion Jump & Floating Variables

    #region Teleport Variables

    [FoldoutGroup("Teleport Variables", nameof(teleportCD), nameof(TeleportDistance),
        nameof(TeleportDuration), nameof(TeleportSword), nameof(teleportCheckLayer))]
    [SerializeField] private Void teleGroupHold;

    [SerializeField, HideInInspector, Range(1, 10f)] public float TeleportDistance = 3f;
    [SerializeField, HideInInspector, Range(0, 1f)] public float TeleportDuration = 0.3f;
    [SerializeField, HideInInspector] public GameObject TeleportSword;
    [SerializeField, HideInInspector, Range(0, 1f)] public float teleportCD = 1f;
    [SerializeField, HideInInspector] public LayerMask teleportCheckLayer;
    private float teleportTimer = 0f;
    private bool teleported = false;

    #endregion Teleport Variables

    #region State Flags

    public bool CanFlip()
    { return ActionLock.Can(Lock.Flip); }

    public bool CanMove()
    { return ActionLock.Can(Lock.Move); }

    public bool CanJump()
    { return ActionLock.Can(Lock.Jump); }

    public bool CanDoubleJump()
    { return ActionLock.Can(Lock.SwordJump); }

    public bool CanTeleport()
    { return ActionLock.Can(Lock.SwordTeleport); }

    #endregion State Flags

    #region Unity Lifecycle

    private void Awake()
    {
        if (instance == null)
            instance = this;
        FacingRight = true;
        rb = GetComponent<Rigidbody2D>();
        playerAttack = GetComponent<PlayerAttack>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();
        energy = GetComponent<Energy>();
        health = GetComponent<Health>();
        camFollowDirection = FacingRight;
    }

    private void Start()
    {
        _fallSpeedYDampingChangeThreshold = CameraManager.instance._fallSpeedYDampingChangeThreshold;
        inputPlayer = InputPlayer.instance;
        gravity = rb.gravityScale;
    }

    private void Update()
    {
        UpdateCoyoteTimer();
        isFloating = Float();
        teleportTimer += VFXManager.isInBulletTime ? Time.unscaledDeltaTime : Time.deltaTime;
        HandleFallingAnimation();
        HandleCameraDamping();
        if (isRunningToTarget) CheckRunToPos();
    }

    private void FixedUpdate()
    {
        if (VFXManager.isInBulletTime)
        {
            rb.velocity += Physics2D.gravity * rb.gravityScale * Time.unscaledDeltaTime;
            rb.MovePosition(rb.position + rb.velocity * Time.unscaledDeltaTime);
        }
        GroundCheck();
    }

    #endregion Unity Lifecycle

    #region Basic Movement

    private bool hasTriggeredMoveAction = false;

    public void Move(float move)
    {
        if (!CanMove())
        {
            SetRunningState(false);
            rb.velocity = Vector3.SmoothDamp(
                rb.velocity,
                new Vector2(0, rb.velocity.y),
                ref m_Velocity,
                m_MovementSmoothing,
                Mathf.Infinity,
                VFXManager.isInBulletTime ? Time.unscaledDeltaTime : Time.deltaTime
            );
            hasTriggeredMoveAction = false; // Reset when movement is not allowed
            return;
        }

        if (!(isGrounded || m_AirControl))
            return;

        float speed = runSpeed;
        bool moving = move != 0;
        SetRunningState(moving);

        // Only trigger OnAction once per movement session
        if (moving)
        {
            if (!hasTriggeredMoveAction)
            {
                QuestManager.OnAction(ObjectiveType.PlayerInput, PlayerInputObjectiveIDs.Move);
                hasTriggeredMoveAction = true;
            }
        }
        else
        {
            hasTriggeredMoveAction = false; // Reset when player stops moving
        }

        if (!isGrounded && m_AirControl)
        {
            speed = isFloating ? 0f : airRunSpeed;
            SetRunningState(false);
        }

        var state = anim.GetCurrentAnimatorStateInfo(0);

        if (isGrounded && !isJumping)
            HandleGroundedAnimationTransitions(state, moving);

        Vector3 targetVelocity = new Vector2(move * speed, rb.velocity.y);
        rb.velocity = Vector3.SmoothDamp(
            rb.velocity,
            targetVelocity,
            ref m_Velocity,
            m_MovementSmoothing,
            Mathf.Infinity,
            VFXManager.isInBulletTime ? Time.unscaledDeltaTime : Time.deltaTime
        );

        HandleFlipping(move);
    }

    public void CheckRunToPos()
    {
        float dir = runToLeft ? -1 : 1;
        if (runToTarget.x < transform.position.x == runToLeft)
            Move(dir);
        else
        {
            Move(0);
            isRunningToTarget = false;
        }
    }

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
        ActionLock.Remove("doubleJumping");
        floatTriggered = false;
        var state = anim.GetCurrentAnimatorStateInfo(0);

        if (!wasGrounded)
            HandleLandingAnimation(state);
    }

    #endregion Basic Movement

    #region Jump & Floating

    private bool Float()
    {
        if (!CanJump() || !CanDoubleJump()) return false;

        if (input_floating && !isGrounded && isFalling)
        {
            if (!floatTriggered)
            {
                SoundManager.PlaySound("sword_jump_floating");
                floatTriggered = true;
            }
            float x = rb.velocity.x;
            rb.velocity = new Vector2(x, (VFXManager.isInBulletTime ? Time.unscaledDeltaTime : Time.deltaTime) * -floatingSpeed);
            if (!isFloating)
                PlayAnimClipInCombat("sword_jump_pre", "sword_jump_pre_combat");
            if (!resetRumbleJump)
            {
                Gamepad.current.SetMotorSpeeds(floatingRumblingSpeed.x, 0);
                resetRumbleJump = true;
            }
            ActionLock.Add("floating", Lock.Defend | Lock.Attack);
            return true;
        }
        else
        {
            if (resetRumbleJump)
            {
                Gamepad.current.SetMotorSpeeds(0f, 0f);
                resetRumbleJump = false;
            }
            ActionLock.Remove("floating");
            return false;
        }
    }

    public static void CancelJump(float mutiplier = 0.1f)
    {
        if (!instance.isJumping || instance.rb.velocity.y <= 0) return;
        instance.rb.velocity = new Vector2(instance.rb.velocity.x, instance.rb.velocity.y * mutiplier);
    }

    public void Jump()
    {
        if (!CanJump() || coyoteTimer <= 0f) return;

        coyoteTimer = 0f;
        isGrounded = false;
        float x = rb.velocity.x;
        rb.velocity = new Vector2(x, m_JumpForce);
        isJumping = true;
        QuestManager.OnAction(ObjectiveType.PlayerInput, PlayerInputObjectiveIDs.Jump);
        var state = anim.GetCurrentAnimatorStateInfo(0);
        float duration = state.normalizedTime;

        if (HandleJumpAnimationTransitions(state, duration))
            return;

        if (canSwitchNormalAnim)
            PlayAnimClipInCombat("jump", "jump_combat");
    }

    public void DoubleJump(float holdTime)
    {
        if (!CanDoubleJump() || isGrounded) return;
        if (!energy.DoubleJumpConsume()) return;
        float x = rb.velocity.x;
        float strength = Mathf.Lerp(MinDoubleJumpForceMultiplier, DoubleJumpForceMultiplier, holdTime / DoubleJumpForceTime);

        SoundManager.PlaySound("sword_jump");
        rb.velocity = new Vector2(x, m_JumpForce * strength);
        ActionLock.Add("doubleJumping", Lock.SwordJump);
        isFloating = false;
        isFalling = false;
        isJumping = true;
        QuestManager.OnAction(ObjectiveType.PlayerInput, PlayerInputObjectiveIDs.SwordJump);
        PlayAnimClipInCombat("sword_jump_after", "sword_jump_after_combat");
        bool hit = playerAttack.JumpAttack();
        if (hit)
        {
            ActionLock.Remove("doubleJumping");
            floatTriggered = false;
        }
    }

    #endregion Jump & Floating

    #region Flipping & Facing

    public void Flip(bool ignoreCamFollowFlip = false)
    {
        if (!CanFlip()) return;

        if (playerAttack.attackTimer <= playerAttack.attackAnimationTime)
        {
            if (playerAttack.isAttackingLeft != FacingRight) return;
        }
        else
        {
            var state = anim.GetCurrentAnimatorStateInfo(0);
            if (state.IsName("attack_back_" + playerAttack.attackIndex) ||
                state.IsName("HS_attack_back_" + playerAttack.attackIndex))
                return;
        }

        FacingRight = !FacingRight;
        transform.Rotate(new Vector3(0, 1, 0), 180);
        playerAttack.counterAttackPoint.Rotate(new Vector3(1, 0, 0), 180);

        if (!ignoreCamFollowFlip && camFollowDirection != FacingRight)
        {
            camFollowDirection = !camFollowDirection;
            camFollow.CallTurn();
        }
    }

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

    #endregion Flipping & Facing

    #region Teleportation

    public Coroutine co_teleport;

    public void SwordTeleport()
    {
        if (!CanTeleport()) return;
        if (teleportTimer <= teleportCD || !energy.TeleportConsume()) return;
        teleportTimer = 0f;
        teleported = false;
        QuestManager.OnAction(ObjectiveType.PlayerInput, PlayerInputObjectiveIDs.swordTeleport);
        co_teleport = StartCoroutine(TeleportCoroutine(FacingRight));
    }

    public IEnumerator RunToPositionCoroutine(Vector3 target, Action callBack = null, bool faceTarget = true)
    {
        bool originalEnabled = InputMaster.instance._defendAction.enabled;
        InputMaster.instance._defendAction.Disable();
        runToLeft = runToTarget.x < transform.position.x;
        inputPlayer.leftPointLeft = runToLeft;
        isRunningToTarget = true;
        runToTarget = target;
        yield return new WaitUntil(() => !isRunningToTarget);
        if (originalEnabled)
            InputMaster.instance._defendAction.Enable();
        yield return null;
        if (faceTarget)
            FaceTarget(target);
        callBack?.Invoke();
    }

    public void RunToPosition(Vector3 target, Action callBack = null, bool faceTarget = true)
    {
        StartCoroutine(RunToPositionCoroutine(target, callBack, faceTarget));
    }

    public void RunToPosition(float x, Action callBack = null, bool faceTarget = true)
    {
        RunToPosition(new Vector3(x, transform.position.y, 0), callBack, faceTarget);
    }

    public void DesignatedPositionTeleport(Vector3 pos)
    {
        teleported = false;
        StartCoroutine(DesignatedTeleport(pos));
    }

    public IEnumerator DesignatedTeleport(Vector3 pos)
    {
        Vector3 start = transform.position + new Vector3(0f, 1.2f, 0f);
        Vector3 dir = pos - start;

        if ((dir.x <= 0 && FacingRight) || (dir.x >= 0 && !FacingRight))
            Flip();

        gameObject.layer = 14; //player_dash
        ActionLock.AddExcept("swordTeleport", Lock.SwordTeleport);
        if (isFalling) anim.Play("tele_pre_fall");
        else if (isJumping) anim.Play("tele_pre_jump");
        else anim.Play("tele_pre_idle");

        TeleportSword.transform.position = start;
        TeleportSword.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
        TeleportSword.SetActive(true);

        bool finished = false;
        TeleportSword.transform.DOMove(pos, TeleportDuration).SetEase(Ease.Linear).OnComplete(() => finished = true);

        yield return new WaitUntil(() => finished);
        TeleportToSword();
        gameObject.layer = 6; //player_dash
        yield return null;
    }

    public IEnumerator TeleportCoroutine(bool right)
    {
        if (playerAttack.isAimingRightStick)
        {
            if (inputPlayer.rightPointLeft == FacingRight) Flip();
        }
        else if (inputPlayer.leftAttackDir != Vector2.zero)
        {
            if (inputPlayer.leftPointLeft == FacingRight) Flip();
        }

        gameObject.layer = 14; //player_dash
        ActionLock.AddExcept("swordTeleport", Lock.SwordTeleport);
        if (isFalling) anim.Play("tele_pre_fall");
        else if (isJumping) anim.Play("tele_pre_jump");
        else anim.Play("tele_pre_idle");

        TeleportSword.transform.position = transform.position + new Vector3(0f, 1.2f, 0f);
        TeleportSword.transform.eulerAngles = inputPlayer.pointer.transform.eulerAngles;
        TeleportSword.SetActive(true);

        Vector3 targetPos = TeleportSword.transform.position + TeleportSword.transform.right * TeleportDistance;
        bool finished = false;
        TeleportSword.transform.DOMove(targetPos, TeleportDuration)
            .SetEase(Ease.Linear)
            .SetUpdate(VFXManager.isInBulletTime)
            .OnComplete(() => finished = true);

        yield return new WaitUntil(() => finished);
        TeleportToSword();
        gameObject.layer = 6; //player_dash
        yield return null;
    }

    public void TeleportToSword()
    {
        if (teleported) return;
        teleported = true;
        gameObject.layer = 6; //player_dash

        RaycastHit2D hit = Physics2D.Raycast(TeleportSword.transform.position, Vector2.down, 1.2f, teleportCheckLayer);
        RaycastHit2D hit_horizontal = Physics2D.Raycast(TeleportSword.transform.position, TeleportSword.transform.right, 0.7f, teleportCheckLayer);
        float offset_y = hit.collider != null ? 1.2f - hit.distance : 0f;
        float offset_x = 0f;
        if (hit_horizontal.collider != null)
        {
            offset_x = FacingRight ? -hit_horizontal.distance - 0.1f : hit_horizontal.distance + 0.1f;
        }
        rb.velocity = Vector3.zero;
        ActionLock.Remove("swordTeleport");
        teleportTimer = 0f;
        TeleportSword.SetActive(false);
        anim.SetBool("isCombat", true);
        PlayerAttack.instance.combatTimer = 2f;
        if (isFalling) anim.Play("tele_fall");
        else if (isRunning) anim.Play("tele_run");
        else anim.Play("tele_idle");
        transform.position = TeleportSword.transform.position + new Vector3(offset_x, offset_y - 1.2f, 0);
        rb.velocity = Vector2.zero;
    }

    #endregion Teleportation

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

    #region Private Helpers

    private void UpdateCoyoteTimer()
    {
        if (isGrounded && !isJumping)
            coyoteTimer = coyoteTime;
        else
            coyoteTimer -= VFXManager.isInBulletTime ? Time.unscaledDeltaTime : Time.deltaTime;
    }

    private void SetRunningState(bool running)
    {
        anim.SetBool("isRunning", running);
        isRunning = running;
    }

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
                else if (state.IsName("attack_idle_after") ||
                state.IsName("attack_jump_after"))
                {
                    anim.Play("attack_fall_after", 0, duration);
                }
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

    private void HandleCameraDamping()
    {
        if (rb.velocity.y < _fallSpeedYDampingChangeThreshold && !CameraManager.instance.isLerpingYDaming && !CameraManager.instance.lerpedFromPlayerFalling)
        {
            CameraManager.instance.LerpYDamping(true);
            CameraFollow.instance.ChangeOffset(CameraFollow.instance.fallingOffset);
        }
        if (rb.velocity.y >= 0 && !CameraManager.instance.isLerpingYDaming && CameraManager.instance.lerpedFromPlayerFalling)
        {
            CameraManager.instance.lerpedFromPlayerFalling = false;
            CameraManager.instance.LerpYDamping(false);
            CameraFollow.instance.ChangeOffset(CameraFollow.instance.normalOffset);
        }
    }

    private void HandleGroundedAnimationTransitions(AnimatorStateInfo state, bool isRunning)
    {
        float duration = state.normalizedTime;
        if (isRunning)
        {
            if (IsAttackRunState(state) && playerAttack.isAttackingLeft != inputPlayer.leftPointLeft)
            {
                if (playerAttack.attackIndex == 1 && duration < (35f / 71f))
                    anim.Play("attack_back_" + playerAttack.attackIndex, 0, duration * (71f / 35f));
                else if (playerAttack.attackIndex == 2)
                    anim.Play("attack_back_" + playerAttack.attackIndex, 0, duration);
                else
                    anim.Play(anim.GetBool("storm") ? "storm_pre_run" : "run_combat");
            }
            else if (IsHSAttackRunState(state) && playerAttack.isAttackingLeft != inputPlayer.leftPointLeft)
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
            else if (state.IsName("attack_jump_after") ||
            state.IsName("attack_fall_after"))
            {
                anim.Play("attack_idle_after", 0, duration);
            }
            else if (IsStormReadyState(state) && !state.IsName("storm_ready_idle"))
                anim.Play("storm_ready_idle", 0, duration);
            else if (IsStormPreState(state) && !state.IsName("storm_pre_idle"))
                anim.Play("storm_pre_idle", 0, duration);
        }
    }

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
                if (playerAttack.isAttackingLeft != inputPlayer.leftPointLeft)
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
                anim.Play("attack_idle_" + playerAttack.attackIndex, 0, duration);
        }
        else if (!isJumping && IsHSAttackJumpOrFallState(state))
        {
            if (isRunning)
            {
                if (playerAttack.isAttackingLeft != inputPlayer.leftPointLeft)
                {
                    if (playerAttack.attackIndex == 1 && duration < (35f / 71f))
                        anim.Play("HS_attack_back_" + playerAttack.attackIndex, 0, duration * (71f / 35f));
                    else if (playerAttack.attackIndex == 2)
                        anim.Play("HS_attack_back_" + playerAttack.attackIndex, 0, duration);
                }
                else
                    anim.Play("HS_attack_run_" + playerAttack.attackIndex, 0, duration);
            }
            else
                anim.Play("HS_attack_idle_" + playerAttack.attackIndex, 0, duration);
        }
    }

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
        if (state.IsName("attack_fall_after") ||
            state.IsName("attack_idle_after"))
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

    private void HandleFlipping(float move)
    {
        if (isRunningToTarget)
        {
            if (playerAttack.isCounterAttacking && playerAttack.isAttackingLeft == FacingRight)
                Flip();
            else if (!playerAttack.isCounterAttacking)
            {
                if (move > 0 && !FacingRight) Flip();
                else if (move < 0 && FacingRight) Flip();
            }
        }
        else if (playerAttack.isCounterAttacking)
        {
            if (playerAttack.isAttackingLeft == FacingRight) Flip();
        }
        else
        {
            if (move > 0 && !FacingRight) Flip();
            else if (move < 0 && FacingRight) Flip();
        }
    }

    // Animation state helpers

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

    #endregion Private Helpers
}