using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
public class BossActionClip : PlayableAsset, ITimelineClipAsset
{
    [Header("Target Gizmo")]
    [ShowIf(nameof(ShowTargetSettings))]
    public bool showTargetGizmo = true;

    [ShowIf(nameof(ShowGizmoColorField))]
    public Color targetGizmoColor = Color.red;

    [ShowIf(nameof(ShowWorldTargetLabelField))]
    public string targetWorldGizmoLabel = "Target";

    [Header("Action")]
    public int factor = 0;

    public double actionDuration = 1.0;

    [Header("Target")]
    public bool targetPlayer = true;

    [SerializeField] public bool attackFromLeft = true;

    [ShowIf(nameof(ShowUseTargetTransformField))]
    public bool useTargetTransform = false;

    [ShowIf(nameof(ShowTargetTransformField))]
    public ExposedReference<Transform> target;

    [HideInInspector]
    public Vector3 targetWorldPosition;

    public override double duration => actionDuration;
    public ClipCaps clipCaps => ClipCaps.None;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<BossActionBehaviour>.Create(graph);
        BossActionBehaviour behaviour = playable.GetBehaviour();

        behaviour.factor = factor;
        behaviour.targetPlayer = targetPlayer;
        behaviour.attackFromLeft = attackFromLeft;
        behaviour.useTargetTransform = useTargetTransform;
        behaviour.targetWorldPosition = targetWorldPosition;
        behaviour.target = target.Resolve(graph.GetResolver());

        return playable;
    }

    private bool ShowTargetSettings => !targetPlayer;

    private bool ShowUseTargetTransformField => !targetPlayer;

    private bool ShowTargetTransformField => !targetPlayer && useTargetTransform;

    private bool ShowGizmoColorField => !targetPlayer && showTargetGizmo;

    private bool ShowWorldTargetLabelField => !targetPlayer && showTargetGizmo && !useTargetTransform;
}