using EditorAttributes;
using System;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using static UnityEngine.EventSystems.EventTrigger;
using Void = EditorAttributes.Void;

public enum PlayerMovementStateType
{
    Normal,
    WindWalking,
    StandingOnTemple
}

/// <summary>
/// Centralized lock keys used by movement-related systems.
/// </summary>
public static class MovementLockKeys
{
    public const string RunningToTarget = "RunningToTarget";
    public const string RunningToPosition = "RunningToPosition";
    public const string Floating = "floating";
    public const string DoubleJumping = "doubleJumping";
    public const string SwordTeleport = "swordTeleport";
}

[SelectionBase]
public partial class PlayerControl : MonoBehaviour
{
    #region Singleton & References

    public static PlayerControl instance;

    [Title("references", 15)]
    public Transform body;

    public Transform leg;
    [HideInInspector] public Animator anim;
    [HideInInspector] public Animator legAnim;
    [HideInInspector] public Rigidbody2D rb;
    private PlayerAttack playerAttack;
    private Energy energy;
    private Health health;
    private InputPlayer inputPlayer;
    private CapsuleCollider2D capsuleCollider;
    [SerializeField] private Transform pointer;
    [SerializeField] private CameraFollow camFollow;

    [SerializeField] public PlayerMovementStateType movementState = PlayerMovementStateType.Normal;
    private MovementStateMachine movementStateMachine;

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
    [FoldoutGroup("Movement Variables", nameof(FacingRight), nameof(m_AirControl), nameof(m_MovementSmoothing), nameof(useRunningSpeed), nameof(runSpeed), nameof(walkSpeed), nameof(airRunSpeed))]
    [SerializeField] private Void movementGroupHold;

    [HideInInspector] public bool m_AirControl = false;
    [SerializeField, HideInInspector, Range(0, .3f)] private float m_MovementSmoothing = .05f;
    [SerializeField, HideInInspector] public bool FacingRight = true;
    [HideInInspector] public bool camFollowDirection = false;
    [SerializeField, HideInInspector] private bool useRunningSpeed = true;
    [SerializeField, HideInInspector] private float runSpeed = 10f;
    [SerializeField, HideInInspector] private float walkSpeed = 3f;
    [SerializeField, HideInInspector] private float airRunSpeed = 5f;
    private Vector3 m_Velocity = Vector3.zero;

    [HideInInspector] public bool isRunningToTarget { get; private set; } = false;
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
    public Coroutine co_teleport;

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
        rb.gravityScale = 0f;
        useRunningSpeed = true;

        InitializeMovementStates();
    }

    private void Update()
    {
        movementStateMachine?.Tick();
    }

    private void FixedUpdate()
    {
        movementStateMachine?.FixedTick();
    }

    #endregion Unity Lifecycle
}