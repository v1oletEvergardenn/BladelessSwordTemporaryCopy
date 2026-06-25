using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
public class PlayerActionClip : PlayableAsset, ITimelineClipAsset
{
    [Header("Target Gizmo")]
    [ShowIf(nameof(UsesTarget))]
    public bool showMoveTargetGizmo = true;

    [ShowIf(nameof(ShowGizmoColorField))]
    public Color moveTargetGizmoColor = Color.cyan;

    [ShowIf(nameof(ShowWorldTargetLabelField))]
    public string moveWorldTargetGizmoLabel = "Target";

    [Header("Action")]
    public PlayerTimelineActionType actionType;

    [Header("Target")]
    [ShowIf(nameof(ShowUseTargetTransformField))]
    public bool useTargetTransform = false;

    [ShowIf(nameof(ShowTargetTransformField))]
    public ExposedReference<Transform> moveTarget;

    [HideInInspector]
    public Vector3 moveWorldPosition;

    public ClipCaps clipCaps => ClipCaps.None;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<PlayerActionBehaviour>.Create(graph);
        PlayerActionBehaviour behaviour = playable.GetBehaviour();

        behaviour.actionType = actionType;

        bool canUseTransformTarget = actionType == PlayerTimelineActionType.MoveTo && useTargetTransform;
        Transform targetTransform = canUseTransformTarget ? moveTarget.Resolve(graph.GetResolver()) : null;
        behaviour.targetPosition = targetTransform != null ? targetTransform.position : moveWorldPosition;

        return playable;
    }

    private bool UsesTarget =>
        actionType == PlayerTimelineActionType.MoveTo ||
        actionType == PlayerTimelineActionType.Repel;

    private bool ShowUseTargetTransformField =>
        actionType == PlayerTimelineActionType.MoveTo;

    private bool ShowTargetTransformField =>
        actionType == PlayerTimelineActionType.MoveTo && useTargetTransform;

    private bool ShowGizmoColorField =>
        UsesTarget && showMoveTargetGizmo;

    private bool ShowWorldTargetLabelField =>
        UsesTarget && showMoveTargetGizmo &&
        (actionType == PlayerTimelineActionType.Repel || (actionType == PlayerTimelineActionType.MoveTo && !useTargetTransform));
}