using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public enum PlayerMoveExecutionMode
{
    TimelineFixedDuration,
    TimelineFixedSpeed,
    LegacyRunToPosition
}

[Serializable]
public class PlayerActionClip : PlayableAsset, ITimelineClipAsset
{
    [Header("Action")]
    public PlayerTimelineActionType actionType;

    [ShowIf(nameof(ShowUseRunSpeedField))]
    public bool useRunSpeed = false;

    [ShowIf(nameof(IsMoveAction))]
    public PlayerMoveExecutionMode moveMode = PlayerMoveExecutionMode.TimelineFixedDuration;

    [ShowIf(nameof(ShowMoveSpeedField))]
    [Min(0.01f)]
    public float moveSpeed = 3f;

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
        behaviour.useRunSpeed = moveMode == PlayerMoveExecutionMode.LegacyRunToPosition && useRunSpeed;

        Transform resolvedStart = IsTimelineMove && useStartTransform ? startTarget.Resolve(graph.GetResolver()) : null;
        Transform resolvedEnd = useEndTransform ? endTarget.Resolve(graph.GetResolver()) : null;

        behaviour.startPosition = resolvedStart != null ? resolvedStart.position : startWorldPosition;
        behaviour.endPosition = resolvedEnd != null ? resolvedEnd.position : endWorldPosition;

        return playable;
    }

    private bool IsMoveAction =>
        actionType == PlayerTimelineActionType.MoveTo;

    private bool IsTimelineMove =>
        IsMoveAction && moveMode != PlayerMoveExecutionMode.LegacyRunToPosition;

    private bool ShowUseRunSpeedField =>
        IsMoveAction && moveMode == PlayerMoveExecutionMode.LegacyRunToPosition;

    private bool ShowMoveSpeedField =>
        IsMoveAction && moveMode == PlayerMoveExecutionMode.TimelineFixedSpeed;

    private bool ShowUseStartTransformField =>
        IsTimelineMove;

    private bool ShowStartTransformField =>
        IsTimelineMove && useStartTransform;

    private bool ShowUseEndTransformField =>
        IsTimelineMove;

    private bool ShowEndTransformField =>
        IsTimelineMove && useEndTransform;
}