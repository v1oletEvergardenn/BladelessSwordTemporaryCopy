using Cinemachine;
using UnityEngine;
using UnityEngine.Playables;

/// <summary>
/// Timeline behaviour for camera actions.
/// </summary>
public class CameraActionBehaviour : PlayableBehaviour
{
    public CameraTimelineActionType actionType;
    public bool useClipDuration;
    public float duration;
    public CameraManager.CameraEase ease;

    public CameraTimelineTargetType switchTargetType;
    public string cameraKeyOrName;
    public CinemachineVirtualCamera cameraReference;
    public bool cut;

    public bool useStartPosition;
    public bool useStartTransform;
    public Transform startTarget;
    public Vector3 startWorldPosition;
    public bool useEndTransform;
    public Transform endTarget;
    public Vector3 endWorldPosition;

    public CameraResetTargets resetTargets;

    public float zoomValue;
    public bool zoomRelative;

    public Transform followTarget;

    public float shakeAmplitude;
    public float shakeFrequency;

    public float offsetX;
    public float offsetY;
    public bool offsetRelative;

    private bool _triggered;

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        if (info.effectiveWeight <= 0f) return;
        if (!Application.isPlaying || _triggered) return;

        CameraTimeLineActions actions = CameraTimeLineActions.instance;
        if (actions == null && CameraManager.instance != null)
        {
            actions = CameraManager.instance.GetComponent<CameraTimeLineActions>();
            if (actions == null) actions = CameraManager.instance.gameObject.AddComponent<CameraTimeLineActions>();
            CameraTimeLineActions.instance = actions;
        }

        if (actions == null) return;

        _triggered = true;
        float timedDuration = GetDuration(playable);

        switch (actionType)
        {
            case CameraTimelineActionType.CamPush:
                actions.PushState();
                break;

            case CameraTimelineActionType.CamPop:
                actions.PopState(timedDuration, ease, cut);
                break;

            case CameraTimelineActionType.CamSwitch:
                actions.SwitchCamera(switchTargetType, cameraKeyOrName, cameraReference, timedDuration, ease, cut);
                break;

            case CameraTimelineActionType.CamMoveTo:
                ExecuteMoveTo(actions, timedDuration);
                break;

            case CameraTimelineActionType.CamReset:
                actions.Reset(resetTargets, timedDuration, ease);
                break;

            case CameraTimelineActionType.CamZoom:
                actions.Zoom(zoomValue, timedDuration, ease, zoomRelative);
                break;

            case CameraTimelineActionType.CamWait:
                break;

            case CameraTimelineActionType.CamFollow:
                actions.Follow(followTarget, timedDuration);
                break;

            case CameraTimelineActionType.CamShake:
                actions.Shake(timedDuration, shakeAmplitude, shakeFrequency, ease);
                break;

            case CameraTimelineActionType.CamOffset:
                actions.Offset(offsetX, offsetY, timedDuration, offsetRelative, ease);
                break;
        }
    }

    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        _triggered = false;
    }

    private void ExecuteMoveTo(CameraTimeLineActions actions, float durationValue)
    {
        if (useStartPosition)
        {
            Vector3 start = ResolveWorldPosition(useStartTransform, startTarget, startWorldPosition);
            actions.SetMoveStartPosition(start);
        }

        Vector3 end = ResolveWorldPosition(useEndTransform, endTarget, endWorldPosition);
        actions.MoveTo(end, durationValue, ease);
    }

    private static Vector3 ResolveWorldPosition(bool useTransform, Transform target, Vector3 worldPosition)
    {
        return useTransform && target != null ? target.position : worldPosition;
    }

    private float GetDuration(Playable playable)
    {
        return useClipDuration ? Mathf.Max(0f, (float)playable.GetDuration()) : Mathf.Max(0f, duration);
    }
}