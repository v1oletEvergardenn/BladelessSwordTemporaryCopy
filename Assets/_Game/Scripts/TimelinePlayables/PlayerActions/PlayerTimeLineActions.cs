using UnityEngine;
using PixelCrushers.DialogueSystem.SequencerCommands;

public enum PlayerTimelineActionType
{
    MoveTo,
    Repel,
    Attack,
    AttackHS,
    HSAbility,
    TeleportTo,
    Face,
    Stop,
    Jump,
    DoubleJump,
    Gravity,
    ClearInput,
    MoveState,
    BodyAnim,
    LegAnim
}

public enum PlayerTimelineDirection
{
    Left,
    Right
}

public enum PlayerTimelineMoveState
{
    Normal,
    WindWalking,
    StandingOnTemple
}

[DisallowMultipleComponent]
public class PlayerTimeLineActions : MonoBehaviour
{
    private const string TimelineCustomMoveLockKey = "TimelineCustomMove";

    public static PlayerTimeLineActions instance;

    [HideInInspector] public PlayerAttack playerAttack;
    [HideInInspector] public PlayerControl controller;
    [HideInInspector] public Health playerHealth;
    [HideInInspector] public Energy playerEnergy;
    [HideInInspector] public InputPlayer inputPlayer;
    [HideInInspector] public Animator anim;

    private Coroutine _customMoveRoutine;

    private void Awake()
    {
        if (instance == null)
            instance = this;
    }

    private void Start()
    {
        AssignRef();
    }

    public void MoveTo(Vector3 worldPosition, bool isRun)
    {
        AssignRef();
        StopCustomMoveIfRunning();

        if (controller == null)
            return;

        if (isRun) controller.RunToPosition(worldPosition);
        else controller.WalkToPosition(worldPosition);
    }

    public void MoveTo(Transform worldPosition, bool isRun)
    {
        if (worldPosition == null)
            return;

        MoveTo(worldPosition.position, isRun);
    }

    public void MoveTo(Vector3 worldPosition, float customSpeed)
    {
        AssignRef();

        if (controller == null)
            return;

        StopCustomMoveIfRunning();
        _customMoveRoutine = StartCoroutine(MoveToCustomSpeedCoroutine(worldPosition, Mathf.Max(0.01f, customSpeed)));
    }

    public bool Attack(bool attackLeft)
    {
        AssignRef();
        return PlayerSequenceForce.ForceAttack(attackLeft);
    }

    public bool AttackHS(bool attackLeft)
    {
        AssignRef();
        return PlayerSequenceForce.ForceAttackHS(attackLeft);
    }

    public bool HSAbility(HSEnum hsEnum, bool attackLeft)
    {
        AssignRef();
        return PlayerSequenceForce.ForceHS(hsEnum, attackLeft);
    }

    public bool TeleportTo(Vector3 worldPosition)
    {
        AssignRef();
        return PlayerSequenceForce.ForceTeleportTo(worldPosition);
    }

    public bool TeleportTo(Transform target)
    {
        if (target == null)
            return false;

        return TeleportTo(target.position);
    }

    public void Face(bool faceRight)
    {
        AssignRef();
        if (controller != null)
            controller.Face(faceRight);
    }

    public void StopMovement()
    {
        AssignRef();
        if (controller != null)
        {
            controller.SetIsRunningToTarget(false);
            controller.StopMovement();
        }
    }

    public bool Jump()
    {
        AssignRef();
        return PlayerSequenceForce.ForceJump();
    }

    public bool DoubleJump(float holdTime)
    {
        AssignRef();
        return PlayerSequenceForce.ForceDoubleJump(Mathf.Max(0f, holdTime));
    }

    public void SetGravity(bool enabled)
    {
        AssignRef();
        if (controller != null)
            controller.EnableGravity(enabled);
    }

    public void ClearInput()
    {
        AssignRef();
        if (inputPlayer != null)
            inputPlayer.DisableAllActions();
    }

    public void SetMoveState(PlayerTimelineMoveState moveState)
    {
        AssignRef();
        if (controller == null)
            return;

        switch (moveState)
        {
            case PlayerTimelineMoveState.Normal:
                controller.SetMovementState(PlayerMovementStateType.Normal);
                break;

            case PlayerTimelineMoveState.WindWalking:
                controller.SetMovementState(PlayerMovementStateType.WindWalking);
                break;

            case PlayerTimelineMoveState.StandingOnTemple:
                controller.SetMovementState(PlayerMovementStateType.StandingOnTemple);
                break;
        }
    }

    public void PlayBodyAnim(string stateName)
    {
        AssignRef();
        if (controller == null || controller.anim == null || string.IsNullOrWhiteSpace(stateName))
            return;

        controller.anim.Play(stateName);
    }

    public void PlayLegAnim(string stateName)
    {
        AssignRef();
        if (controller == null || controller.legAnim == null || string.IsNullOrWhiteSpace(stateName))
            return;

        controller.legAnim.Play(stateName);
    }

    public void RepelTo(Vector3 worldPosition)
    {
        AssignRef();
        if (playerHealth != null)
            playerHealth.RepelToPosition(worldPosition);
    }

    public void RepelTo(Transform target)
    {
        if (target == null)
            return;

        RepelTo(target.position);
    }

    public void RepelByDistance(float distance)
    {
        AssignRef();
        if (playerHealth != null)
            playerHealth.RepelWithoutDirection(distance);
    }

    private System.Collections.IEnumerator MoveToCustomSpeedCoroutine(Vector3 targetPosition, float speed)
    {
        if (inputPlayer != null)
            inputPlayer.movementInputUpdateLock.Add(TimelineCustomMoveLockKey);

        while (controller != null)
        {
            Vector3 current = controller.transform.position;
            float deltaX = targetPosition.x - current.x;
            if (Mathf.Abs(deltaX) <= 0.05f)
                break;

            float dir = Mathf.Sign(deltaX);
            controller.Move(dir, speed);
            yield return null;
        }

        if (controller != null)
        {
            controller.Move(0f, speed);
            Vector3 p = controller.transform.position;
            controller.transform.position = new Vector3(targetPosition.x, p.y, p.z);
        }

        if (inputPlayer != null)
            inputPlayer.movementInputUpdateLock.Remove(TimelineCustomMoveLockKey);

        _customMoveRoutine = null;
    }

    private void StopCustomMoveIfRunning()
    {
        if (_customMoveRoutine == null)
            return;

        StopCoroutine(_customMoveRoutine);
        _customMoveRoutine = null;

        if (inputPlayer != null)
            inputPlayer.movementInputUpdateLock.Remove(TimelineCustomMoveLockKey);
    }

    public void AssignRef()
    {
        if (playerAttack == null) playerAttack = PlayerAttack.instance;
        if (controller == null) controller = PlayerControl.instance;
        if (inputPlayer == null) inputPlayer = InputPlayer.instance;
        if (playerHealth == null) playerHealth = Health.instance;
        if (playerEnergy == null) playerEnergy = Energy.instance;
        if (anim == null && playerAttack != null) anim = playerAttack.anim;
    }
}