using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Serialization;
using UnityEngine.Timeline;
using Sirenix.OdinInspector;

public enum PlayerMoveSpeedOption
{
    RunSpeed,
    WalkSpeed,
    CustomSpeed
}

[Serializable]
public class PlayerActionClip : PlayableAsset, ITimelineClipAsset
{
    [Header("Action")]
    public PlayerTimelineActionType actionType;

    [ShowIf(nameof(ShowDirectionField))]
    public PlayerTimelineDirection direction = PlayerTimelineDirection.Right;

    [ShowIf(nameof(ShowHSField))]
    public HSEnum hsEnum = HSEnum.HSCounterAttack;

    [ShowIf(nameof(IsMoveAction))]
    [FormerlySerializedAs("legacySpeedOption")]
    public PlayerMoveSpeedOption speedOption = PlayerMoveSpeedOption.RunSpeed;

    [ShowIf(nameof(ShowCustomSpeedField))]
    [FormerlySerializedAs("legacyCustomSpeed")]
    [Min(0.01f)]
    public float customSpeed = 3f;

    [ShowIf(nameof(IsMoveAction))]
    [FormerlySerializedAs("useLegacyStartPosition")]
    public bool useStartPosition = false;

    [Header("MoveTo Path")]
    [ShowIf(nameof(ShowMoveStartTransformToggleField))]
    public bool useStartTransform = false;

    [ShowIf(nameof(ShowMoveStartTransformField))]
    public ExposedReference<Transform> startTarget;

    [ShowIf(nameof(ShowMoveStartWorldPositionField))]
    public Vector3 startWorldPosition;

    [ShowIf(nameof(ShowMoveEndTransformToggleField))]
    public bool useEndTransform = false;

    [ShowIf(nameof(ShowMoveEndTransformField))]
    public ExposedReference<Transform> endTarget;

    [ShowIf(nameof(ShowMoveEndWorldPositionField))]
    public Vector3 endWorldPosition;

    [Header("TeleportTo Target")]
    [ShowIf(nameof(IsTeleportToAction))]
    public bool useTeleportTargetTransform = false;

    [ShowIf(nameof(ShowTeleportTargetField))]
    public ExposedReference<Transform> teleportTarget;

    [ShowIf(nameof(ShowTeleportWorldPositionField))]
    public Vector3 teleportWorldPosition;

    [Header("Repel")]
    [ShowIf(nameof(IsRepelAction))]
    public bool useRepelDistance = false;

    [ShowIf(nameof(ShowRepelDistanceField))]
    public float repelDistance = 2f;

    [ShowIf(nameof(ShowRepelTransformToggleField))]
    public bool useRepelTargetTransform = false;

    [ShowIf(nameof(ShowRepelTransformField))]
    public ExposedReference<Transform> repelTarget;

    [ShowIf(nameof(ShowRepelWorldPositionField))]
    public Vector3 repelWorldPosition;

    [ShowIf(nameof(ShowDoubleJumpField))]
    [Min(0f)]
    public float doubleJumpHold = 0.2f;

    [ShowIf(nameof(ShowGravityField))]
    public bool gravityEnabled = true;

    [ShowIf(nameof(ShowMoveStateField))]
    public PlayerTimelineMoveState moveState = PlayerTimelineMoveState.Normal;

    [ShowIf(nameof(ShowAnimStateField))]
    public string animStateName = "idle";

    public ClipCaps clipCaps => ClipCaps.None;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        ScriptPlayable<PlayerActionBehaviour> playable = ScriptPlayable<PlayerActionBehaviour>.Create(graph);
        PlayerActionBehaviour behaviour = playable.GetBehaviour();

        behaviour.actionType = actionType;
        behaviour.direction = direction;
        behaviour.hsEnum = hsEnum;

        behaviour.speedOption = speedOption;
        behaviour.customSpeed = customSpeed;
        behaviour.useStartPosition = useStartPosition;
        behaviour.useStartTransform = useStartTransform;
        behaviour.startTarget = useStartTransform ? startTarget.Resolve(graph.GetResolver()) : null;
        behaviour.startWorldPosition = startWorldPosition;
        behaviour.useEndTransform = useEndTransform;
        behaviour.endTarget = useEndTransform ? endTarget.Resolve(graph.GetResolver()) : null;
        behaviour.endWorldPosition = endWorldPosition;

        behaviour.useTeleportTargetTransform = useTeleportTargetTransform;
        behaviour.teleportTarget = useTeleportTargetTransform ? teleportTarget.Resolve(graph.GetResolver()) : null;
        behaviour.teleportWorldPosition = teleportWorldPosition;

        behaviour.useRepelDistance = useRepelDistance;
        behaviour.repelDistance = repelDistance;
        behaviour.useRepelTargetTransform = useRepelTargetTransform;
        behaviour.repelTarget = useRepelTargetTransform ? repelTarget.Resolve(graph.GetResolver()) : null;
        behaviour.repelWorldPosition = repelWorldPosition;

        behaviour.doubleJumpHold = doubleJumpHold;
        behaviour.gravityEnabled = gravityEnabled;
        behaviour.moveState = moveState;
        behaviour.animStateName = animStateName;

        return playable;
    }

    private bool IsMoveAction =>
        actionType == PlayerTimelineActionType.MoveTo;

    private bool IsRepelAction =>
        actionType == PlayerTimelineActionType.Repel;

    private bool IsTeleportToAction =>
        actionType == PlayerTimelineActionType.TeleportTo;

    private bool ShowDirectionField =>
        actionType == PlayerTimelineActionType.Attack ||
        actionType == PlayerTimelineActionType.AttackHS ||
        actionType == PlayerTimelineActionType.HSAbility ||
        actionType == PlayerTimelineActionType.Face;

    private bool ShowHSField =>
        actionType == PlayerTimelineActionType.HSAbility;

    private bool ShowCustomSpeedField =>
        IsMoveAction && speedOption == PlayerMoveSpeedOption.CustomSpeed;

    private bool ShowMoveStartTransformToggleField =>
        IsMoveAction && useStartPosition;

    private bool ShowMoveStartTransformField =>
        IsMoveAction && useStartPosition && useStartTransform;

    private bool ShowMoveStartWorldPositionField =>
        IsMoveAction && useStartPosition && !useStartTransform;

    private bool ShowMoveEndTransformToggleField =>
        IsMoveAction;

    private bool ShowMoveEndTransformField =>
        IsMoveAction && useEndTransform;

    private bool ShowMoveEndWorldPositionField =>
        IsMoveAction && !useEndTransform;

    private bool ShowTeleportTargetField =>
        IsTeleportToAction && useTeleportTargetTransform;

    private bool ShowTeleportWorldPositionField =>
        IsTeleportToAction && !useTeleportTargetTransform;

    private bool ShowRepelDistanceField =>
        IsRepelAction && useRepelDistance;

    private bool ShowRepelTransformToggleField =>
        IsRepelAction && !useRepelDistance;

    private bool ShowRepelTransformField =>
        IsRepelAction && !useRepelDistance && useRepelTargetTransform;

    private bool ShowRepelWorldPositionField =>
        IsRepelAction && !useRepelDistance && !useRepelTargetTransform;

    private bool ShowDoubleJumpField =>
        actionType == PlayerTimelineActionType.DoubleJump;

    private bool ShowGravityField =>
        actionType == PlayerTimelineActionType.Gravity;

    private bool ShowMoveStateField =>
        actionType == PlayerTimelineActionType.MoveState;

    private bool ShowAnimStateField =>
        actionType == PlayerTimelineActionType.BodyAnim ||
        actionType == PlayerTimelineActionType.LegAnim;
}