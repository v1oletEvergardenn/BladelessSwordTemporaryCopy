using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Serialization;
using UnityEngine.Timeline;
using Sirenix.OdinInspector;

public enum PlayerMoveExecutionMode
{
    TimelineFixedDuration,
    TimelineFixedSpeed,
    RunToPosition
}

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

    [ShowIf(nameof(IsRunToPositionMove))]
    [FormerlySerializedAs("legacySpeedOption")]
    public PlayerMoveSpeedOption speedOption = PlayerMoveSpeedOption.RunSpeed;

    [ShowIf(nameof(ShowCustomSpeedField))]
    [FormerlySerializedAs("legacyCustomSpeed")]
    [Min(0.01f)]
    public float customSpeed = 3f;

    [ShowIf(nameof(IsRunToPositionMove))]
    [FormerlySerializedAs("useLegacyStartPosition")]
    public bool useStartPosition = false;

    [ShowIf(nameof(IsMoveAction))]
    public PlayerMoveExecutionMode moveMode = PlayerMoveExecutionMode.TimelineFixedDuration;

    [ShowIf(nameof(ShowMoveSpeedField))]
    [Min(0.01f)]
    public float moveSpeed = 3f;

    [Header("Repel")]
    [ShowIf(nameof(IsRepelAction))]
    public bool useRepelGizmoPosition = true;

    [ShowIf(nameof(ShowRepelDistanceField))]
    public float repelDistance = 2f;

    [Header("Path")]
    [ShowIf(nameof(ShowUseStartTransformField))]
    public bool useStartTransform = false;

    [ShowIf(nameof(ShowStartTransformField))]
    public ExposedReference<Transform> startTarget;

    [HideInInspector]
    public Vector3 startWorldPosition;

    [ShowIf(nameof(ShowUseEndTransformField))]
    public bool useEndTransform = false;

    [ShowIf(nameof(ShowEndTransformField))]
    public ExposedReference<Transform> endTarget;

    [HideInInspector]
    public Vector3 endWorldPosition;

    public ClipCaps clipCaps => ClipCaps.None;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        ScriptPlayable<PlayerActionBehaviour> playable = ScriptPlayable<PlayerActionBehaviour>.Create(graph);
        PlayerActionBehaviour behaviour = playable.GetBehaviour();

        behaviour.actionType = actionType;
        behaviour.moveMode = moveMode;
        behaviour.useTimelineMotion = IsTimelineMove;
        behaviour.speedOption = speedOption;
        behaviour.customSpeed = customSpeed;
        behaviour.useStartPosition = IsRunToPositionMove && useStartPosition;
        behaviour.useRepelGizmoPosition = IsRepelAction && useRepelGizmoPosition;
        behaviour.repelDistance = repelDistance;

        Transform resolvedStart = IsTimelineMove && useStartTransform ? startTarget.Resolve(graph.GetResolver()) : null;
        Transform resolvedEnd = useEndTransform ? endTarget.Resolve(graph.GetResolver()) : null;

        behaviour.startPosition = resolvedStart != null ? resolvedStart.position : startWorldPosition;
        behaviour.endPosition = resolvedEnd != null ? resolvedEnd.position : endWorldPosition;

        return playable;
    }

    private bool IsMoveAction =>
        actionType == PlayerTimelineActionType.MoveTo;

    private bool IsRepelAction =>
        actionType == PlayerTimelineActionType.Repel;

    private bool IsTimelineMove =>
        IsMoveAction && moveMode != PlayerMoveExecutionMode.RunToPosition;

    private bool IsRunToPositionMove =>
        IsMoveAction && moveMode == PlayerMoveExecutionMode.RunToPosition;

    private bool ShowCustomSpeedField =>
        IsRunToPositionMove && speedOption == PlayerMoveSpeedOption.CustomSpeed;

    private bool ShowMoveSpeedField =>
        IsMoveAction && moveMode == PlayerMoveExecutionMode.TimelineFixedSpeed;

    private bool ShowRepelDistanceField =>
        IsRepelAction && !useRepelGizmoPosition;

    private bool ShowUseStartTransformField =>
        IsTimelineMove;

    private bool ShowStartTransformField =>
        IsTimelineMove && useStartTransform;

    private bool ShowUseEndTransformField =>
        IsTimelineMove;

    private bool ShowEndTransformField =>
        IsTimelineMove && useEndTransform;
}