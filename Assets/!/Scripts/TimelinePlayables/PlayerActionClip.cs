using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
public class PlayerActionClip : PlayableAsset, ITimelineClipAsset
{
    [Header("Action")]
    public PlayerTimelineActionType actionType;

    [Header("Move To")]
    public bool useTargetTransform = false;

    public ExposedReference<Transform> moveTarget;
    public Vector3 moveWorldPosition;
    public bool faceTargetAfterMove = true;

    [Header("Attack")]
    public bool attackLeft = true;

    public bool consumeEnergy = false;

    public ClipCaps clipCaps => ClipCaps.None;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<PlayerActionBehaviour>.Create(graph);
        PlayerActionBehaviour behaviour = playable.GetBehaviour();

        behaviour.actionType = actionType;
        behaviour.moveWorldPosition = moveWorldPosition;
        behaviour.faceTargetAfterMove = faceTargetAfterMove;
        behaviour.attackLeft = attackLeft;
        behaviour.consumeEnergy = consumeEnergy;

        if (useTargetTransform)
        {
            Transform resolvedTarget = moveTarget.Resolve(graph.GetResolver());
            behaviour.hasResolvedTarget = resolvedTarget != null;
            behaviour.resolvedTargetPosition = resolvedTarget != null ? resolvedTarget.position : default;
        }
        else
        {
            behaviour.hasResolvedTarget = false;
            behaviour.resolvedTargetPosition = default;
        }

        return playable;
    }
}