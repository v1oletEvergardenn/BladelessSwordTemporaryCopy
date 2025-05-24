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
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;

[SelectionBase]
public class CharacterController2D : MonoBehaviour
{
    public static CharacterController2D instance;
    [Title("references", 15)][SerializeField] public Animator anim;
    [HideInInspector] public Rigidbody2D rb;
    private PlayerAttack playerAttack;
    private Energy energy;
    private InputPlayer inputPlayer;
    private CapsuleCollider2D capsuleCollider;
    [SerializeField] private Transform pointer;
    [SerializeField] private CameraFollow camFollow;

    [Title("GroundCheck", 15)][SerializeField] public LayerMask m_WhatIsGround;
    [SerializeField] private float coyoteTime = 0.2f;
    [HideInInspector] public float coyoteTimer = 0f;
    public bool isGrounded;
    [HideInInspector] public bool enableGroundCheck = true;

    [Title("Variables", 15)]

    #region MOVEMENT VARIABLES

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

    #endregion MOVEMENT VARIABLES

    #region JUMP VARIABLES

    [FoldoutGroup("Jump&&floating Variables", nameof(m_JumpForce), nameof(DoubleJumpForceMultiplier), nameof(MinDoubleJumpForceMultiplier), nameof(DoubleJumpForceTime), nameof(floatingSpeed), nameof(floatingRumblingSpeed))]
    [SerializeField] private Void JumpGroupHold;

    [SerializeField, HideInInspector, Range(0f, 20f)] private float m_JumpForce = 5f;
    [SerializeField, HideInInspector, Range(1f, 6f)] private float DoubleJumpForceMultiplier = 1f;
    [SerializeField, HideInInspector, Range(0f, 2f)] private float MinDoubleJumpForceMultiplier = 1f;
    [SerializeField, HideInInspector, Range(0f, 4f)] private float DoubleJumpForceTime = 1f;
    [SerializeField, HideInInspector, Range(0f, 20f)] private float floatingSpeed;
    [SerializeField, HideInInspector, MinMaxSlider(0, 1, true)] public Vector2 floatingRumblingSpeed;
    public bool input_floating;
    [HideInInspector] public float floatingTime = 0f;
    [HideInInspector] public bool resetRumbleJump = false;
    public bool isJumping = false;
    public bool isFloating;

    [HideInInspector] public bool floatTriggered = false;

    //new

    public bool isFalling = false;
    [HideInInspector] public bool isRunning = false;

    private float _fallSpeedYDampingChangeThreshold;

    #endregion JUMP VARIABLES

    #region TELEPORT VARIABLES

    [FoldoutGroup("Teleport Variables", nameof(teleportCD), nameof(TeleportDistance), nameof(TeleportDuration), nameof(TeleportSword), nameof(teleportCheckLayer))]
    [SerializeField] private Void teleGroupHold;

    [SerializeField, HideInInspector, Range(1, 10f)] public float TeleportDistance = 3f;
    [SerializeField, HideInInspector, Range(0, 1f)] public float TeleportDuration = 0.3f;
    [SerializeField, HideInInspector] public GameObject TeleportSword;
    [SerializeField, HideInInspector, Range(0, 1f)] public float teleportCD = 1f;
    [SerializeField, HideInInspector] public LayerMask teleportCheckLayer;
    private float teleportTimer = 0f;
    private bool teleported = false;

    #endregion TELEPORT VARIABLES

    [HideInInspector] public bool canFlip = true;
    [HideInInspector] public bool canMove = true;
    [HideInInspector] public bool canJump = true;
    [HideInInspector] public bool canDoubleJump;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        FacingRight = true;
        rb = GetComponent<Rigidbody2D>();
        playerAttack = GetComponent<PlayerAttack>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();
        energy = GetComponent<Energy>();
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
        if (isGrounded && !isJumping) { coyoteTimer = coyoteTime; }
        else { coyoteTimer -= Time.deltaTime; }
        isFloating = Float();

        teleportTimer += Time.deltaTime;

        //falling check
        if (rb.velocity.y < -1 && !isGrounded && !isFalling)
        {
            if (!playerAttack.isDefending)
            {
                if (
                anim.GetCurrentAnimatorStateInfo(0).IsName((playerAttack.isHS_attack ? "HS_" : "") + "attack_run_" + playerAttack.attackIndex) ||
                 anim.GetCurrentAnimatorStateInfo(0).IsName((playerAttack.isHS_attack ? "HS_" : "") + "attack_idle_" + playerAttack.attackIndex) ||
                 anim.GetCurrentAnimatorStateInfo(0).IsName((playerAttack.isHS_attack ? "HS_" : "") + "attack_jump_" + playerAttack.attackIndex))
                {
                    float duration = anim.GetCurrentAnimatorStateInfo(0).normalizedTime;
                    anim.Play((playerAttack.isHS_attack ? "HS_" : "") + "attack_fall_" + playerAttack.attackIndex, 0, duration);
                }//swtich attack animation
                else if (anim.GetCurrentAnimatorStateInfo(0).IsName("attack_jump_after") ||
                    anim.GetCurrentAnimatorStateInfo(0).IsName("attack_idle_after"))
                {
                    float duration = anim.GetCurrentAnimatorStateInfo(0).normalizedTime;
                    anim.Play("attack_fall_after", 0, duration);
                }//switch attack after animation
                else if ((anim.GetCurrentAnimatorStateInfo(0).IsName("storm_ready_jump") ||
                       anim.GetCurrentAnimatorStateInfo(0).IsName("storm_ready_idle") ||
                       anim.GetCurrentAnimatorStateInfo(0).IsName("storm_ready_run")))
                {
                    float duration = anim.GetCurrentAnimatorStateInfo(0).normalizedTime;
                    anim.Play("storm_ready_fall", 0, duration);
                }
                else if ((anim.GetCurrentAnimatorStateInfo(0).IsName("storm_pre_jump") ||
                       anim.GetCurrentAnimatorStateInfo(0).IsName("storm_pre_idle") ||
                       anim.GetCurrentAnimatorStateInfo(0).IsName("storm_pre_run")))
                {
                    float duration = anim.GetCurrentAnimatorStateInfo(0).normalizedTime;
                    anim.Play("storm_pre_fall", 0, duration);
                }
                else if ((anim.GetCurrentAnimatorStateInfo(0).IsName("tele_pre_jump")))
                {
                    float duration = anim.GetCurrentAnimatorStateInfo(0).normalizedTime;
                    anim.Play("tele_pre_fall", 0, duration);
                }
                else
                {
                    if (playerAttack.isPreparingStorm) { anim.Play("storm_ready_fall"); }
                    else { PlayAnimClipInCombat("pre_fall", "pre_fall_combat"); }
                }//play normal falling animation if not attacking
            }

            isFalling = true;
            isJumping = false;
        }
        else if (isGrounded) { isFalling = false; }

        //check if falling past the threshold to change camera Daming speed
        if (rb.velocity.y < _fallSpeedYDampingChangeThreshold && !CameraManager.instance.isLerpingYDaming && !CameraManager.instance.lerpedFromPlayerFalling)
        {
            CameraManager.instance.LerpYDamping(true);
            CameraFollow.instance.ChangeOffset(CameraFollow.instance.fallingOffset);
        }
        //if we are standing still or not falling, set the damping back to normal
        if (rb.velocity.y >= 0 && !CameraManager.instance.isLerpingYDaming && CameraManager.instance.lerpedFromPlayerFalling)
        {
            CameraManager.instance.lerpedFromPlayerFalling = false;
            CameraManager.instance.LerpYDamping(false);
            CameraFollow.instance.ChangeOffset(CameraFollow.instance.normalOffset);
        }

        if (isRunningToTarget) { CheckRunToPos(); }
    }

    private void FixedUpdate()
    {
        GroundCheck();
    }

    #region BASIC MOVEMENT

    public void Move(float move)
    {
        if (!canMove)
        {
            anim.SetBool("isRunning", false);
            isRunning = false;
            rb.velocity = Vector3.SmoothDamp(rb.velocity, (Vector3)new Vector2(0, rb.velocity.y), ref m_Velocity, m_MovementSmoothing);
            return;
        }

        if (!(isGrounded || m_AirControl))
            return;

        string hsPrefix = playerAttack.isHS_attack ? "HS_" : "";
        float speed = runSpeed;
        bool moving = move != 0;
        anim.SetBool("isRunning", moving);
        isRunning = moving;

        // Air control logic
        if (!isGrounded && m_AirControl)
        {
            speed = isFloating ? 0f : airRunSpeed;
            isRunning = false;
            anim.SetBool("isRunning", false);
        }

        // Cache animation state info
        var state = anim.GetCurrentAnimatorStateInfo(0);

        // Animation transitions when grounded and not jumping
        if (isGrounded && !isJumping)
        {
            float duration = state.normalizedTime;

            if (isRunning)
            {
                if (state.IsName(hsPrefix + "attack_run_" + playerAttack.attackIndex)
                    && playerAttack.isAttackingLeft != inputPlayer.leftPointLeft)
                {
                    if (playerAttack.attackIndex == 1 && duration < (35f / 71f))
                    {
                        anim.Play(hsPrefix + "attack_back", 0, duration * (71f / 35f));
                    }
                    else
                    {
                        if (anim.GetBool("storm"))
                            anim.Play("storm_pre_run");
                        else
                            anim.Play("run_combat");
                    }
                }
                else if (state.IsName(hsPrefix + "attack_idle_" + playerAttack.attackIndex))
                {
                    anim.Play(hsPrefix + "attack_run_" + playerAttack.attackIndex, 0, duration);
                }
                else if (state.IsName("drawback_idle"))
                {
                    anim.Play("drawback_run", 0, duration);
                }
                else if (IsStormReadyState(state))
                {
                    anim.Play("storm_ready_run", 0, duration);
                }
                else if (IsStormPreState(state))
                {
                    anim.Play("storm_pre_run", 0, duration);
                }
            }
            else // Not running
            {
                if (state.IsName(hsPrefix + "attack_run_" + playerAttack.attackIndex))
                {
                    anim.Play(hsPrefix + "attack_idle_" + playerAttack.attackIndex, 0, duration);
                }
                else if (state.IsName("drawback_run"))
                {
                    anim.Play("drawback_idle", 0, duration);
                }
                else if (state.IsName("attack_jump_after") ||
                         state.IsName("attack_fall_after"))
                {
                    anim.Play("attack_idle_after", 0, duration);
                }
                else if (IsStormReadyState(state))
                {
                    anim.Play("storm_ready_idle", 0, duration);
                }
                else if (IsStormPreState(state))
                {
                    anim.Play("storm_pre_idle", 0, duration);
                }
            }
        }

        // Apply horizontal movement
        Vector3 targetVelocity = new Vector2(move * speed, rb.velocity.y);
        rb.velocity = Vector3.SmoothDamp(rb.velocity, targetVelocity, ref m_Velocity, m_MovementSmoothing);

        // Handle flipping
        if (isRunningToTarget)
        {
            if (playerAttack.isAttacking && playerAttack.isAttackingLeft == FacingRight)
            {
                Flip();
            }
            else if (!playerAttack.isAttacking)
            {
                if (move > 0 && !FacingRight) Flip();
                else if (move < 0 && FacingRight) Flip();
            }
        }
        else if (playerAttack.isAttacking)
        {
            if (playerAttack.isAttackingLeft == FacingRight) { Flip(); }
        }
        else
        {
            if (move > 0 && !FacingRight) Flip();
            else if (move < 0 && FacingRight) Flip();
        }

        // Local helper functions for animation state checks
        bool IsStormReadyState(AnimatorStateInfo s) =>
            s.IsName("storm_ready_idle") ||
            s.IsName("storm_ready_jump") ||
            s.IsName("storm_ready_fall");

        bool IsStormPreState(AnimatorStateInfo s) =>
            s.IsName("storm_pre_idle") ||
            s.IsName("storm_pre_jump") ||
            s.IsName("storm_pre_fall");
    }

    public void CheckRunToPos()
    {
        float dir = -1;
        if (!runToLeft) { dir = 1; }
        if (runToTarget.x < transform.position.x == runToLeft)
        {
            Move(dir);
        }
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

        // Cache animation state info
        var state = anim.GetCurrentAnimatorStateInfo(0);

        // If just landed (was not grounded last frame)
        if (!wasGrounded)
        {
            if (!isJumping && !playerAttack.isDefending)
            {
                if (!state.IsName("slash") && !state.IsName("slash_end"))
                {
                    PlayAnimClipInCombat("land", "land_combat");
                }
                isFalling = false;
                isJumping = false;
            }

            CameraFollow.instance.ChangeOffset(CameraFollow.instance.normalOffset);

            string hsPrefix = playerAttack.isHS_attack ? "HS_" : "";

            if (!isJumping &&
                (state.IsName(hsPrefix + "attack_jump_" + playerAttack.attackIndex) ||
                 state.IsName(hsPrefix + "attack_fall_" + playerAttack.attackIndex)))
            {
                float duration = state.normalizedTime;
                if (isRunning)
                {
                    if (playerAttack.isAttackingLeft != inputPlayer.leftPointLeft)
                    {
                        if (playerAttack.attackIndex == 1 && duration < (35f / 71f))
                        {
                            anim.Play(hsPrefix + "attack_back", 0, duration * (71f / 35f));
                        }
                        else
                        {
                            if (anim.GetBool("storm"))
                                anim.Play("storm_pre_run");
                            else
                                anim.Play("run_combat");
                        }
                    }
                    else
                    {
                        anim.Play(hsPrefix + "attack_run_" + playerAttack.attackIndex, 0, duration);
                    }
                }
                else
                {
                    anim.Play(hsPrefix + "attack_idle_" + playerAttack.attackIndex, 0, duration);
                }
            }
        }
    }

    private bool Float()
    {
        if (!canJump) { return false; }

        if (input_floating && !isGrounded && isFalling && canDoubleJump && !playerAttack.isPreparingStorm && !playerAttack.isDefending)
        {
            if (!floatTriggered) { SoundManager.PlaySound("sword_jump_floating"); floatTriggered = true; }
            if (!energy.FloatingConsume()) { return false; }

            float x = rb.velocity.x;
            rb.velocity = new Vector2(x, -floatingSpeed * Time.deltaTime);
            if (!isFloating)
            {
                PlayAnimClipInCombat("sword_jump_pre", "sword_jump_pre_combat");
            }
            if (!resetRumbleJump) { Gamepad.current.SetMotorSpeeds(floatingRumblingSpeed.x, 0); resetRumbleJump = true; }
            return true;
        }//is going to float
        else
        {
            if (resetRumbleJump) { Gamepad.current.SetMotorSpeeds(0f, 0f); resetRumbleJump = false; }
            return false;
        }
    } //floating check

    public void Jump()
    {
        if (!canJump) return;

        // Only allow jump if within coyote time
        if (coyoteTimer <= 0f) return;

        // Cache HS prefix for animation names
        string hsPrefix = playerAttack.isHS_attack ? "HS_" : "";

        coyoteTimer = 0f;
        isGrounded = false;
        float x = rb.velocity.x;
        rb.velocity = new Vector2(x, m_JumpForce);
        isJumping = true;

        var state = anim.GetCurrentAnimatorStateInfo(0);

        // Attack jump/fall/back transitions
        if (state.IsName(hsPrefix + "attack_run_" + playerAttack.attackIndex) ||
            state.IsName(hsPrefix + "attack_idle_" + playerAttack.attackIndex) ||
            state.IsName(hsPrefix + "attack_back"))
        {
            float duration = state.normalizedTime;
            if (playerAttack.attackIndex == 1)
                anim.Play(hsPrefix + "attack_jump_" + playerAttack.attackIndex, 0, duration - (36f / 71f));
            else
                anim.Play(hsPrefix + "attack_jump_" + playerAttack.attackIndex, 0, duration);
            return;
        }

        // Attack after transitions
        if (state.IsName(hsPrefix + "attack_fall_after") ||
            state.IsName(hsPrefix + "attack_idle_after"))
        {
            float duration = state.normalizedTime;
            anim.Play(hsPrefix + "attack_jump_after", 0, duration);
            return;
        }

        // Storm ready transitions
        if (IsStormReadyState(state))
        {
            float duration = state.normalizedTime;
            anim.Play("storm_ready_jump", 0, duration);
            return;
        }

        // Storm pre transitions
        if (IsStormPreState(state))
        {
            float duration = state.normalizedTime;
            anim.Play("storm_pre_jump", 0, duration);
            return;
        }

        // Default jump
        PlayAnimClipInCombat("jump", "jump_combat");

        // Local helper functions for animation state checks
        bool IsStormReadyState(AnimatorStateInfo s) =>
            s.IsName("storm_ready_fall") ||
            s.IsName("storm_ready_idle") ||
            s.IsName("storm_ready_run");

        bool IsStormPreState(AnimatorStateInfo s) =>
            s.IsName("storm_pre_fall") ||
            s.IsName("storm_pre_idle") ||
            s.IsName("storm_pre_run");
    }

    public void DoubleJump(float holdTime)
    {
        if (!canDoubleJump) { return; }
        float x = rb.velocity.x;
        float strength = holdTime / DoubleJumpForceTime;
        strength = Mathf.Clamp(strength, MinDoubleJumpForceMultiplier, DoubleJumpForceMultiplier);
        if (canDoubleJump && !isGrounded)
        {
            SoundManager.PlaySound("sword_jump");
            rb.velocity = new Vector2(x, m_JumpForce * strength);
            canDoubleJump = false;
            isFloating = false;
            isFalling = false;
            isJumping = true;

            //counter attack
            PlayAnimClipInCombat("sword_jump_after", "sword_jump_after_combat");
            bool hit = playerAttack.JumpAttack();
            if (hit)
            {
                canDoubleJump = true;
                floatTriggered = false;
            }
        }
    }//double jump and jump counter attack

    public void Flip(bool ignoreCamFollowFlip = false)
    {
        if (!canFlip) { return; }
        if (playerAttack.isInAttackAnim) { return; }

        if (playerAttack.isAttacking)
        {
            if (playerAttack.isAttackingLeft == FacingRight) { }
        }
        else
        {
            if (anim.GetCurrentAnimatorStateInfo(0).IsName((playerAttack.isHS_attack ? "HS_" : "") + "attack_back")) { return; }
        }
        print("flipped");
        //flip player
        FacingRight = !FacingRight;
        transform.Rotate(new Vector3(0, 1, 0), 180);
        //pointer.Rotate(new Vector3(0, 1, 0), 180);
        playerAttack.counterAttackPoint.Rotate(new Vector3(1, 0, 0), 180);
        if (!ignoreCamFollowFlip && camFollowDirection != FacingRight)
        {
            camFollowDirection = !camFollowDirection;
            camFollow.CallTurn();
        }
    }//flip the character

    public void FaceTarget(Transform target)
    {
        if (target.position.x <= transform.position.x && FacingRight
            || target.position.x > transform.position.x && !FacingRight)
        {
            Flip();
        }
    }

    public void SwordTeleport()
    {
        if (teleportTimer <= teleportCD) { return; }
        teleported = false;

        StartCoroutine(TeleportCoroutine(FacingRight));
    }

    public IEnumerator RunToPosition(Vector3 target)
    {
        bool originalEnabled = InputMaster.instance._defendAction.enabled;
        InputMaster.instance._defendAction.Disable();
        runToLeft = runToTarget.x < transform.position.x;
        inputPlayer.leftPointLeft = runToLeft;
        isRunningToTarget = true;
        runToTarget = target;
        yield return new WaitUntil(() => !isRunningToTarget);
        if (originalEnabled)
        {
            InputMaster.instance._defendAction.Enable();
        }
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

        if (dir.x <= 0 && FacingRight) { Flip(); }
        else if (dir.x >= 0 && !FacingRight) { Flip(); }
        gameObject.layer = 14; //player_dash
        AnimSetBool.instance.Anim_Teleport(0);
        if (isFalling) { anim.Play("tele_pre_fall"); }
        else if (isJumping) { anim.Play("tele_pre_jump"); }
        else { anim.Play("tele_pre_idle"); }
        TeleportSword.transform.position = start;

        // Set rotation to face the target position at the start
        TeleportSword.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);

        TeleportSword.SetActive(true);

        bool finished = false;
        TeleportSword.transform.DOMove(pos, TeleportDuration).SetEase(Ease.Linear).OnComplete(() => { finished = true; });

        yield return new WaitUntil(() => finished);
        TeleportToSword();
        gameObject.layer = 6; //player_dash
        yield return null;
    }

    public IEnumerator TeleportCoroutine(bool right)
    {
        if (playerAttack.isAimingRightStick)
        {
            if (inputPlayer.rightPointLeft == FacingRight) { Flip(); }
        }
        else if (inputPlayer.leftAttackDir != Vector2.zero) { if (inputPlayer.leftPointLeft == FacingRight) { Flip(); } }
        gameObject.layer = 14; //player_dash
        AnimSetBool.instance.Anim_Teleport(0);
        if (isFalling) { anim.Play("tele_pre_fall"); }
        else if (isJumping) { anim.Play("tele_pre_jump"); }
        else { anim.Play("tele_pre_idle"); }
        TeleportSword.transform.position = transform.position + new Vector3(0f, 1.2f, 0f);
        TeleportSword.transform.eulerAngles = inputPlayer.pointer.transform.eulerAngles;
        TeleportSword.SetActive(true);
        float elapsedTime = 0f;
        teleportTimer = 0f;

        while (elapsedTime < TeleportDuration)
        {
            elapsedTime += Time.deltaTime;
            Vector3 dist = TeleportSword.transform.right * (TeleportDistance / TeleportDuration);
            TeleportSword.GetComponent<Rigidbody2D>().velocity = dist;
            yield return null;
        }
        TeleportToSword();
        gameObject.layer = 6; //player_dash
        yield return null;
    }

    public void TeleportToSword()
    {
        if (teleported) { return; }
        teleported = true;
        RaycastHit2D hit = Physics2D.Raycast(TeleportSword.transform.position, Vector2.down, 1.2f, teleportCheckLayer);
        RaycastHit2D hit_horizontal = Physics2D.Raycast(TeleportSword.transform.position, TeleportSword.transform.right, 0.7f, teleportCheckLayer);
        float offset_y = 0f;
        float offset_x = 0f;
        if (hit.collider != null)
        {
            offset_y = 1.2f - hit.distance;
        }
        if (hit_horizontal.collider != null)
        {
            offset_x = hit_horizontal.distance + 0.1f;
            if (FacingRight)
            {
                offset_x = -hit_horizontal.distance - 0.1f;
            }
        }
        rb.velocity = Vector3.zero;

        AnimSetBool.instance.Anim_Teleport(1);
        teleportTimer = 0f;
        TeleportSword.SetActive(false);
        anim.SetBool("isCombat", true);
        rb.velocity = new Vector2(rb.velocity.x, -0.1f);
        PlayerAttack.instance.combatTimer = 2f;
        if (isFalling) { anim.Play("tele_fall"); }
        else if (isRunning) { anim.Play("tele_run"); }
        else { anim.Play("tele_idle"); }
        transform.position = TeleportSword.transform.position + new Vector3(offset_x, offset_y - 1.2f, 0);
    }

    #endregion BASIC MOVEMENT

    public void EnableGravity(bool enable)
    {
        rb.gravityScale = enable ? gravity : 0;
    }

    public void StopMovement()
    {
        if (isJumping) { return; }
        rb.velocity = Vector3.zero;
    }

    public void PlayAnimClipInCombat(string normalClip, string combatClip)
    {
        if (playerAttack.isInCombat) { anim.Play(combatClip); }
        else { anim.Play(normalClip); }
    }

    public Quaternion CalculateWantedRotation(Vector3 _targetPos)
    {
        float angle = Mathf.Atan2(_targetPos.y - transform.position.y, _targetPos.x - transform.position.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(new Vector3(0, 0, angle));
        return targetRotation;
    }
}