using System;
using Cinemachine;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
public class CameraActionClip : PlayableAsset, ITimelineClipAsset
{
    [Header("Action")]
    public CameraTimelineActionType actionType = CameraTimelineActionType.CamSwitch;

    [Header("Shared Timing / Ease")]
    public bool useClipDuration = true;

    [ShowIf(nameof(ShowManualDurationField))]
    [Min(0f)]
    public float duration = 1f;

    public CameraManager.CameraEase ease = CameraManager.CameraEase.InOutSine;

    [Header("CamSwitch")]
    [ShowIf(nameof(IsSwitchAction))]
    public CameraTimelineTargetType switchTargetType = CameraTimelineTargetType.Player;

    [ShowIf(nameof(ShowCameraKeyField))]
    public string cameraKeyOrName;

    [ShowIf(nameof(ShowCameraReferenceField))]
    public ExposedReference<CinemachineVirtualCamera> cameraReference;

    [ShowIf(nameof(IsCutFieldVisible))]
    public bool cut = false;

    [Header("CamMoveTo")]
    [ShowIf(nameof(IsMoveToAction))]
    public bool useStartPosition = false;

    [ShowIf(nameof(ShowMoveStartTransformToggleField))]
    public bool useStartTransform = false;

    [ShowIf(nameof(ShowMoveStartTransformField))]
    public ExposedReference<Transform> startTarget;

    [ShowIf(nameof(ShowMoveStartWorldPositionField))]
    public Vector3 startWorldPosition;

    [ShowIf(nameof(IsMoveToAction))]
    public bool useEndTransform = false;

    [ShowIf(nameof(ShowMoveEndTransformField))]
    public ExposedReference<Transform> endTarget;

    [ShowIf(nameof(ShowMoveEndWorldPositionField))]
    public Vector3 endWorldPosition;

    [Header("CamReset")]
    [ShowIf(nameof(IsResetAction))]
    [EnumToggleButtons]
    public CameraResetTargets resetTargets = CameraResetTargets.All;

    [Header("CamZoom")]
    [ShowIf(nameof(IsZoomAction))]
    public float zoomValue = 0f;

    [ShowIf(nameof(IsZoomAction))]
    public bool zoomRelative = false;

    [Header("CamFollow")]
    [ShowIf(nameof(IsFollowAction))]
    public ExposedReference<Transform> followTarget;

    [Header("CamShake")]
    [ShowIf(nameof(IsShakeAction))]
    [Min(0f)]
    public float shakeAmplitude = 1f;

    [ShowIf(nameof(IsShakeAction))]
    [Min(0f)]
    public float shakeFrequency = 20f;

    [Header("CamOffset")]
    [ShowIf(nameof(IsOffsetAction))]
    public float offsetX = 0f;

    [ShowIf(nameof(IsOffsetAction))]
    public float offsetY = 0f;

    [ShowIf(nameof(IsOffsetAction))]
    public bool offsetRelative = true;

    public ClipCaps clipCaps => ClipCaps.None;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        ScriptPlayable<CameraActionBehaviour> playable = ScriptPlayable<CameraActionBehaviour>.Create(graph);
        CameraActionBehaviour behaviour = playable.GetBehaviour();

        behaviour.actionType = actionType;
        behaviour.useClipDuration = useClipDuration;
        behaviour.duration = duration;
        behaviour.ease = ease;

        behaviour.switchTargetType = switchTargetType;
        behaviour.cameraKeyOrName = cameraKeyOrName;
        behaviour.cameraReference = cameraReference.Resolve(graph.GetResolver());
        behaviour.cut = cut;

        behaviour.useStartPosition = useStartPosition;
        behaviour.useStartTransform = useStartTransform;
        behaviour.startTarget = useStartTransform ? startTarget.Resolve(graph.GetResolver()) : null;
        behaviour.startWorldPosition = startWorldPosition;
        behaviour.useEndTransform = useEndTransform;
        behaviour.endTarget = useEndTransform ? endTarget.Resolve(graph.GetResolver()) : null;
        behaviour.endWorldPosition = endWorldPosition;

        behaviour.resetTargets = resetTargets;

        behaviour.zoomValue = zoomValue;
        behaviour.zoomRelative = zoomRelative;

        behaviour.followTarget = followTarget.Resolve(graph.GetResolver());

        behaviour.shakeAmplitude = shakeAmplitude;
        behaviour.shakeFrequency = shakeFrequency;

        behaviour.offsetX = offsetX;
        behaviour.offsetY = offsetY;
        behaviour.offsetRelative = offsetRelative;

        return playable;
    }

    private bool IsSwitchAction => actionType == CameraTimelineActionType.CamSwitch;
    private bool IsMoveToAction => actionType == CameraTimelineActionType.CamMoveTo;
    private bool IsResetAction => actionType == CameraTimelineActionType.CamReset;
    private bool IsZoomAction => actionType == CameraTimelineActionType.CamZoom;
    private bool IsFollowAction => actionType == CameraTimelineActionType.CamFollow;
    private bool IsShakeAction => actionType == CameraTimelineActionType.CamShake;
    private bool IsOffsetAction => actionType == CameraTimelineActionType.CamOffset;

    private bool IsTimedAction =>
        actionType == CameraTimelineActionType.CamSwitch ||
        actionType == CameraTimelineActionType.CamMoveTo ||
        actionType == CameraTimelineActionType.CamReset ||
        actionType == CameraTimelineActionType.CamZoom ||
        actionType == CameraTimelineActionType.CamWait ||
        actionType == CameraTimelineActionType.CamFollow ||
        actionType == CameraTimelineActionType.CamShake ||
        actionType == CameraTimelineActionType.CamOffset ||
        actionType == CameraTimelineActionType.CamPop;

    private bool ShowManualDurationField => IsTimedAction && !useClipDuration;
    private bool ShowCameraKeyField => IsSwitchAction && switchTargetType == CameraTimelineTargetType.KeyOrName;
    private bool ShowCameraReferenceField => IsSwitchAction && switchTargetType == CameraTimelineTargetType.CameraReference;
    private bool IsCutFieldVisible => IsSwitchAction || actionType == CameraTimelineActionType.CamPop;

    private bool ShowMoveStartTransformToggleField => IsMoveToAction && useStartPosition;
    private bool ShowMoveStartTransformField => IsMoveToAction && useStartPosition && useStartTransform;
    private bool ShowMoveStartWorldPositionField => IsMoveToAction && useStartPosition && !useStartTransform;
    private bool ShowMoveEndTransformField => IsMoveToAction && useEndTransform;
    private bool ShowMoveEndWorldPositionField => IsMoveToAction && !useEndTransform;
}