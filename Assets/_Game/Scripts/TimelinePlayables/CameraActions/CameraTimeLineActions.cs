using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Flags]
public enum CameraResetTargets
{
    None = 0,
    MoveTo = 1 << 0,
    Follow = 1 << 1,
    Zoom = 1 << 2,
    Offset = 1 << 3,
    All = MoveTo | Follow | Zoom | Offset
}

public enum CameraTimelineActionType
{
    CamSwitch,
    CamMoveTo,
    CamReset,
    CamZoom,
    CamWait,
    CamFollow,
    CamShake,
    CamOffset,
    CamPush,
    CamPop
}

public enum CameraTimelineTargetType
{
    Player,
    Previous,
    KeyOrName,
    CameraReference
}

[DisallowMultipleComponent]
public class CameraTimeLineActions : MonoBehaviour
{
    public static CameraTimeLineActions instance;

    private readonly Dictionary<CinemachineVirtualCamera, Vector3> _defaultDirectPositionByCamera =
        new Dictionary<CinemachineVirtualCamera, Vector3>();

    private Coroutine _directMoveCoroutine;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
            return;
        }

        instance = this;
    }

    public void PushState()
    {
        CameraManager.PushCutsceneState();
    }

    public void PopState(float duration, CameraManager.CameraEase ease, bool cut)
    {
        CameraManager.PopCutsceneState(Mathf.Max(0f, duration), ease, cut);
    }

    public void SwitchCamera(CameraTimelineTargetType targetType, string cameraKeyOrName, CinemachineVirtualCamera cameraRef, float duration, CameraManager.CameraEase ease, bool cut)
    {
        CameraManager.EnsureSkipSafeSnapshot();

        switch (targetType)
        {
            case CameraTimelineTargetType.Player:
                CameraManager.SwitchToPlayerCamera(duration, ease, cut);
                return;

            case CameraTimelineTargetType.Previous:
                CameraManager.SwitchToPreviousCamera(duration, ease, cut);
                return;

            case CameraTimelineTargetType.CameraReference:
                if (cameraRef != null)
                    CameraManager.SwitchCamera(cameraRef, duration, ease, cut);
                return;

            case CameraTimelineTargetType.KeyOrName:
                if (CameraManager.TryGetCamera(cameraKeyOrName, out CinemachineVirtualCamera cam))
                    CameraManager.SwitchCamera(cam, duration, ease, cut);
                return;
        }
    }

    public bool SetMoveStartPosition(Vector3 worldPosition)
    {
        if (TryGetOffsetMoveContext(out CinemachineVirtualCamera active, out CinemachineFramingTransposer transposer))
        {
            Vector3 reference = GetFollowReferencePosition(active);
            Vector3 current = transposer.m_TrackedObjectOffset;
            Vector3 absolute = new Vector3(worldPosition.x - reference.x, worldPosition.y - reference.y, current.z);

            CameraManager.EnsureSkipSafeSnapshot();
            CameraManager.TweenPanOffset(absolute.x, absolute.y, 0f, false, CameraManager.CameraEase.Linear);
            return true;
        }

        if (!TryGetActiveCamera(out active))
            return false;

        CacheDirectDefaultPosition(active);
        StopDirectMoveTween();

        Vector3 currentPos = active.transform.position;
        Vector3 targetPos = new Vector3(worldPosition.x, worldPosition.y, currentPos.z);

        CameraManager.EnsureSkipSafeSnapshot();
        active.transform.position = targetPos;
        return true;
    }

    public bool MoveTo(Vector3 worldPosition, float duration, CameraManager.CameraEase ease)
    {
        if (TryGetOffsetMoveContext(out CinemachineVirtualCamera active, out CinemachineFramingTransposer transposer))
        {
            Vector3 reference = GetFollowReferencePosition(active);
            Vector3 current = transposer.m_TrackedObjectOffset;
            Vector3 absolute = new Vector3(worldPosition.x - reference.x, worldPosition.y - reference.y, current.z);

            CameraManager.EnsureSkipSafeSnapshot();
            CameraManager.TweenPanOffset(absolute.x, absolute.y, Mathf.Max(0f, duration), false, ease);
            return true;
        }

        if (!TryGetActiveCamera(out active))
            return false;

        CacheDirectDefaultPosition(active);
        StopDirectMoveTween();

        Vector3 from = active.transform.position;
        Vector3 to = new Vector3(worldPosition.x, worldPosition.y, from.z);

        CameraManager.EnsureSkipSafeSnapshot();
        float safeDuration = Mathf.Max(0f, duration);

        if (safeDuration <= 0f)
        {
            active.transform.position = to;
            return true;
        }

        _directMoveCoroutine = StartCoroutine(TweenDirectMoveRoutine(active, from, to, safeDuration, ease));
        return true;
    }

    public bool MoveTo(Transform target, float duration, CameraManager.CameraEase ease)
    {
        if (target == null)
            return false;

        return MoveTo(target.position, duration, ease);
    }

    public void ResetMoveTo(float duration, CameraManager.CameraEase ease)
    {
        float safeDuration = Mathf.Max(0f, duration);

        if (TryGetOffsetMoveContext(out _, out _))
        {
            CameraManager.ResetPanOffset(safeDuration, ease);
            return;
        }

        if (!TryGetActiveCamera(out CinemachineVirtualCamera active))
            return;

        if (!_defaultDirectPositionByCamera.TryGetValue(active, out Vector3 defaultPos))
            return;

        StopDirectMoveTween();

        if (safeDuration <= 0f)
        {
            active.transform.position = defaultPos;
            return;
        }

        _directMoveCoroutine = StartCoroutine(
            TweenDirectMoveRoutine(active, active.transform.position, defaultPos, safeDuration, ease));
    }

    public void Zoom(float value, float duration, CameraManager.CameraEase ease, bool relative)
    {
        CameraManager.EnsureSkipSafeSnapshot();
        CameraManager.ZoomCurrentCamera(value, Mathf.Max(0f, duration), ease, relative);
    }

    public void ZoomReset(float duration, CameraManager.CameraEase ease)
    {
        CameraManager.ResetCurrentCameraZoom(Mathf.Max(0f, duration), ease);
    }

    public void Follow(Transform target, float autoResetDelay)
    {
        if (target == null)
            return;

        CameraManager.EnsureSkipSafeSnapshot();
        CameraManager.SetTemporaryFollowTarget(target, Mathf.Max(0f, autoResetDelay));
    }

    public void FollowReset(float delay)
    {
        CameraManager.ResetFollowTarget(Mathf.Max(0f, delay));
    }

    public void Shake(float duration, float amplitude, float frequency, CameraManager.CameraEase ease)
    {
        CameraManager.EnsureSkipSafeSnapshot();
        CameraManager.ShakeCurrentCamera(
            Mathf.Max(0f, amplitude),
            Mathf.Max(0f, frequency),
            Mathf.Max(0f, duration),
            ease);
    }

    public void Offset(float x, float y, float duration, bool relative, CameraManager.CameraEase ease)
    {
        CameraManager.EnsureSkipSafeSnapshot();
        CameraManager.TweenPanOffset(x, y, Mathf.Max(0f, duration), relative, ease);
    }

    public void OffsetReset(float duration, CameraManager.CameraEase ease)
    {
        CameraManager.ResetPanOffset(Mathf.Max(0f, duration), ease);
    }

    public void Reset(CameraResetTargets targets, float duration, CameraManager.CameraEase ease)
    {
        if ((targets & CameraResetTargets.MoveTo) != 0)
            ResetMoveTo(duration, ease);

        if ((targets & CameraResetTargets.Follow) != 0)
            FollowReset(duration);

        if ((targets & CameraResetTargets.Zoom) != 0)
            ZoomReset(duration, ease);

        if ((targets & CameraResetTargets.Offset) != 0)
            OffsetReset(duration, ease);
    }

    private static bool TryGetOffsetMoveContext(out CinemachineVirtualCamera active, out CinemachineFramingTransposer transposer)
    {
        active = null;
        transposer = null;

        if (!TryGetActiveCamera(out active))
            return false;

        transposer = active.GetCinemachineComponent<CinemachineFramingTransposer>();
        if (transposer == null)
            return false;

        // Offset-based world move is only valid when there is a follow target.
        return active.Follow != null;
    }

    private static bool TryGetActiveCamera(out CinemachineVirtualCamera active)
    {
        active = null;

        if (CameraManager.instance == null || CameraManager.instance.activeCamera == null)
            return false;

        active = CameraManager.instance.activeCamera;
        return true;
    }

    private static Vector3 GetFollowReferencePosition(CinemachineVirtualCamera active)
    {
        if (active != null && active.Follow != null)
            return active.Follow.position;

        return active != null ? active.transform.position : Vector3.zero;
    }

    private void CacheDirectDefaultPosition(CinemachineVirtualCamera camera)
    {
        if (camera == null)
            return;

        if (!_defaultDirectPositionByCamera.ContainsKey(camera))
        {
            _defaultDirectPositionByCamera[camera] = camera.transform.position;
        }
    }

    private void StopDirectMoveTween()
    {
        if (_directMoveCoroutine != null)
        {
            StopCoroutine(_directMoveCoroutine);
            _directMoveCoroutine = null;
        }
    }

    private IEnumerator TweenDirectMoveRoutine(CinemachineVirtualCamera camera, Vector3 from, Vector3 to, float duration, CameraManager.CameraEase ease)
    {
        if (camera == null)
        {
            _directMoveCoroutine = null;
            yield break;
        }

        if (duration <= 0f)
        {
            camera.transform.position = to;
            _directMoveCoroutine = null;
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (camera == null)
            {
                _directMoveCoroutine = null;
                yield break;
            }

            elapsed += Mathf.Max(Time.deltaTime, 0f);
            float t = Mathf.Clamp01(elapsed / duration);
            float eased = EvaluateEase(ease, t);
            camera.transform.position = Vector3.LerpUnclamped(from, to, eased);
            yield return null;
        }

        if (camera != null)
        {
            camera.transform.position = to;
        }

        _directMoveCoroutine = null;
    }

    private static float EvaluateEase(CameraManager.CameraEase ease, float t)
    {
        t = Mathf.Clamp01(t);

        switch (ease)
        {
            case CameraManager.CameraEase.Linear:
                return t;

            case CameraManager.CameraEase.InSine:
                return 1f - Mathf.Cos((t * Mathf.PI) * 0.5f);

            case CameraManager.CameraEase.OutSine:
                return Mathf.Sin((t * Mathf.PI) * 0.5f);

            case CameraManager.CameraEase.InOutSine:
                return -(Mathf.Cos(Mathf.PI * t) - 1f) * 0.5f;

            case CameraManager.CameraEase.InQuad:
                return t * t;

            case CameraManager.CameraEase.OutQuad:
                return 1f - ((1f - t) * (1f - t));

            case CameraManager.CameraEase.InOutQuad:
                return t < 0.5f
                    ? 2f * t * t
                    : 1f - (Mathf.Pow(-2f * t + 2f, 2f) * 0.5f);

            default:
                return t;
        }
    }
}