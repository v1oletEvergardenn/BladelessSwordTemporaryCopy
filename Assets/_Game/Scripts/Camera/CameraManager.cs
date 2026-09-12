using Cinemachine;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public enum CameraEase
    {
        Linear,
        InSine,
        OutSine,
        InOutSine,
        InQuad,
        OutQuad,
        InOutQuad
    }

    [Serializable]
    public class CameraLibraryEntry
    {
        [HorizontalGroup("Row", 0.38f), LabelWidth(38)]
        public string key;

        [HorizontalGroup("Row", 0.62f), LabelWidth(52)]
        public CinemachineVirtualCamera camera;
    }

    [Header("Noise (Handheld)")]
    [SerializeField] public NoiseSettings handheldNormalMild;

    private struct CutsceneCameraState
    {
        public CinemachineVirtualCamera activeCamera;
        public CinemachineVirtualCamera previousCamera;
        public bool hasZoom;
        public float zoom;
        public Transform followTarget;
        public bool hasPanOffset;
        public Vector3 panOffset;
        public CinemachineBlendDefinition.Style blendStyle;
        public float blendTime;
    }

    private const int ActivePriority = 10;
    private const int InactivePriority = 0;
    private const string NormalCameraObjectName = "CM_normalCam";

    private static readonly List<CinemachineVirtualCamera> cameras = new List<CinemachineVirtualCamera>();

    private readonly Dictionary<string, CinemachineVirtualCamera> _cameraLookup =
        new Dictionary<string, CinemachineVirtualCamera>(StringComparer.OrdinalIgnoreCase);

    private readonly Dictionary<CinemachineVirtualCamera, float> _defaultZoomByCamera =
        new Dictionary<CinemachineVirtualCamera, float>();

    private readonly Dictionary<CinemachineVirtualCamera, Transform> _defaultFollowByCamera =
        new Dictionary<CinemachineVirtualCamera, Transform>();

    private readonly Dictionary<CinemachineVirtualCamera, Vector3> _defaultPanOffsetByCamera =
        new Dictionary<CinemachineVirtualCamera, Vector3>();

    private readonly Dictionary<CinemachineVirtualCamera, Vector3> _defaultPositionByCamera =
        new Dictionary<CinemachineVirtualCamera, Vector3>();

    private readonly Dictionary<CinemachineVirtualCamera, FollowTarget> _offsetProxyFollowerByCamera =
        new Dictionary<CinemachineVirtualCamera, FollowTarget>();

    private readonly Dictionary<CinemachineVirtualCamera, Vector3> _offsetProxyValueByCamera =
        new Dictionary<CinemachineVirtualCamera, Vector3>();

    private readonly Stack<CutsceneCameraState> _cutsceneStateStack = new Stack<CutsceneCameraState>();

    #region Runtime State

    [FoldoutGroup("Runtime"), ShowInInspector, ReadOnly]
    public CinemachineVirtualCamera activeCamera = null;

    public static CinemachineVirtualCamera beforeActiveCam = null;

    [FoldoutGroup("Runtime"), ShowInInspector, ReadOnly]
    public bool isLerpingYDaming { get; private set; }

    [FoldoutGroup("Runtime"), ShowInInspector, ReadOnly]
    public bool lerpedFromPlayerFalling { get; set; }

    [HideInInspector] public CinemachineFramingTransposer _framingTransposer;
    [HideInInspector] public float _normYPanAmount;

    [FoldoutGroup("Runtime"), ShowInInspector, ReadOnly]
    private int _savedStateCount => _cutsceneStateStack.Count;

    private Coroutine co_yLerp;
    private Coroutine co_zoom;
    private Coroutine co_shake;
    private Coroutine co_offset;
    private Coroutine co_followReset;

    #endregion Runtime State

    #region References

    [Title("Camera References")]
    [SerializeField, FoldoutGroup("References")]
    public CinemachineVirtualCamera playerNormalCam;

    [SerializeField, FoldoutGroup("References")]
    public Camera mainCam;

    [SerializeField, FoldoutGroup("References"), ListDrawerSettings(ShowFoldout = true, DraggableItems = true)]
    private List<CameraLibraryEntry> cutsceneCameras = new List<CameraLibraryEntry>();

    #endregion References

    #region Damping Settings

    [Title("Y Damping")]
    [SerializeField, FoldoutGroup("Damping"), Min(0f)]
    private float _fallPanAmount = 0.25f;

    [SerializeField, FoldoutGroup("Damping"), Min(0f)]
    private float _fallYPanTime = 0.35f;

    [SerializeField, FoldoutGroup("Damping")]
    public float _fallSpeedYDampingChangeThreshold = -15f;

    #endregion Damping Settings

    #region Singleton

    public static CameraManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            BuildCameraLookup();
            return;
        }

        if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void OnValidate()
    {
        BuildCameraLookup();
    }

    #endregion Singleton

    #region Camera Lookup

    private void BuildCameraLookup()
    {
        _cameraLookup.Clear();

        for (int i = 0; i < cutsceneCameras.Count; i++)
        {
            CameraLibraryEntry entry = cutsceneCameras[i];
            if (entry == null || string.IsNullOrWhiteSpace(entry.key) || entry.camera == null)
            {
                continue;
            }

            _cameraLookup[entry.key.Trim()] = entry.camera;
        }
    }

    public static bool TryGetCamera(string keyOrName, out CinemachineVirtualCamera camera)
    {
        camera = null;
        if (instance == null || string.IsNullOrWhiteSpace(keyOrName))
        {
            return false;
        }

        return instance.TryResolveCamera(keyOrName, out camera);
    }

    private bool TryResolveCamera(string keyOrName, out CinemachineVirtualCamera camera)
    {
        camera = null;
        if (string.IsNullOrWhiteSpace(keyOrName))
        {
            return false;
        }

        if (_cameraLookup.TryGetValue(keyOrName.Trim(), out camera))
        {
            return camera != null;
        }

        GameObject go = GameObject.Find(keyOrName);
        if (go == null)
        {
            return false;
        }

        camera = go.GetComponent<CinemachineVirtualCamera>();
        return camera != null;
    }

    #endregion Camera Lookup

    #region Camera Switching

    public void SwitchToNormalCam()
    {
        SwitchToPlayerCameraInternal(0f, CameraEase.InOutSine, false);
    }

    public static void SwitchToPlayerCamera(float duration = 1f, CameraEase ease = CameraEase.InOutSine, bool cut = false)
    {
        if (instance == null)
        {
            return;
        }

        instance.SwitchToPlayerCameraInternal(duration, ease, cut);
    }

    private void SwitchToPlayerCameraInternal(float duration, CameraEase ease, bool cut)
    {
        if (playerNormalCam == null)
        {
            GameObject cam = GameObject.Find(NormalCameraObjectName);
            if (cam == null)
            {
                Debug.LogError($"[CameraManager] Could not find normal camera GameObject: {NormalCameraObjectName}");
                return;
            }

            playerNormalCam = cam.GetComponent<CinemachineVirtualCamera>();
            if (playerNormalCam == null)
            {
                Debug.LogError($"[CameraManager] {NormalCameraObjectName} does not have a {nameof(CinemachineVirtualCamera)} component.");
                return;
            }
        }

        CachePlayerFramingTransposer();
        SwitchCamera(playerNormalCam, duration, ease, cut);
    }

    private void CachePlayerFramingTransposer()
    {
        if (playerNormalCam == null)
        {
            return;
        }

        _framingTransposer = playerNormalCam.GetCinemachineComponent<CinemachineFramingTransposer>();
        if (_framingTransposer != null)
        {
            _normYPanAmount = _framingTransposer.m_YDamping;
        }
    }

    public static bool IsActiveCamera(CinemachineVirtualCamera camera)
    {
        return instance != null && camera == instance.activeCamera;
    }

    public static void SwitchCamera(CinemachineVirtualCamera newCam)
    {
        SwitchCamera(newCam, 0f, CameraEase.InOutSine, false);
    }

    public static void SwitchCamera(CinemachineVirtualCamera newCam, float duration, CameraEase ease, bool cut = false)
    {
        if (instance == null)
        {
            Debug.LogError("[CameraManager] Instance is null. Ensure CameraManager exists in the scene.");
            return;
        }

        instance.SwitchCameraInternal(newCam, duration, ease, cut);
    }

    private void SwitchCameraInternal(CinemachineVirtualCamera newCam, float duration, CameraEase ease, bool cut)
    {
        if (newCam == null)
        {
            Debug.LogWarning("[CameraManager] SwitchCamera was called with a null camera.");
            return;
        }

        if (beforeActiveCam != activeCamera)
        {
            beforeActiveCam = activeCamera;
        }

        ApplyBlend(duration, ease, cut);

        activeCamera = newCam;
        RecordCameraDefaults(newCam);

        newCam.Priority = ActivePriority;

        foreach (CinemachineVirtualCamera cam in cameras)
        {
            if (cam != null && cam != newCam)
            {
                cam.Priority = InactivePriority;
            }
        }

        if (newCam == playerNormalCam)
        {
            CachePlayerFramingTransposer();
        }
    }

    private void ApplyBlend(float duration, CameraEase ease, bool cut)
    {
        CinemachineBrain brain = GetBrain();
        if (brain == null)
        {
            return;
        }

        float blendTime = Mathf.Max(0f, duration);
        CinemachineBlendDefinition.Style style = cut || blendTime <= 0f
            ? CinemachineBlendDefinition.Style.Cut
            : ToCinemachineBlendStyle(ease);

        brain.m_DefaultBlend = new CinemachineBlendDefinition(style, cut ? 0f : blendTime);
    }

    private static CinemachineBlendDefinition.Style ToCinemachineBlendStyle(CameraEase ease)
    {
        switch (ease)
        {
            case CameraEase.Linear:
                return CinemachineBlendDefinition.Style.Linear;

            case CameraEase.InSine:
            case CameraEase.InQuad:
                return CinemachineBlendDefinition.Style.EaseIn;

            case CameraEase.OutSine:
            case CameraEase.OutQuad:
                return CinemachineBlendDefinition.Style.EaseOut;

            default:
                return CinemachineBlendDefinition.Style.EaseInOut;
        }
    }

    private CinemachineBrain GetBrain()
    {
        if (mainCam != null && mainCam.TryGetComponent(out CinemachineBrain brain))
        {
            return brain;
        }

        Camera currentMain = Camera.main;
        if (currentMain != null && currentMain.TryGetComponent(out CinemachineBrain mainBrain))
        {
            return mainBrain;
        }

        return FindObjectOfType<CinemachineBrain>();
    }

    public static void SwitchToPreviousCamera()
    {
        SwitchToPreviousCamera(0f, CameraEase.InOutSine, false);
    }

    public static void SwitchToPreviousCamera(float duration, CameraEase ease, bool cut = false)
    {
        if (beforeActiveCam == null)
        {
            Debug.LogWarning("[CameraManager] No previous camera available to switch to.");
            return;
        }

        SwitchCamera(beforeActiveCam, duration, ease, cut);
    }

    public static void SwtichToPreviousCamera()
    {
        SwitchToPreviousCamera();
    }

    public static void SwitchBounceQTECamera(CinemachineVirtualCamera newCam)
    {
        if (instance == null)
        {
            Debug.LogError("[CameraManager] Instance is null. Ensure CameraManager exists in the scene.");
            return;
        }

        if (newCam == null)
        {
            Debug.LogWarning("[CameraManager] SwitchBounceQTECamera was called with a null camera.");
            return;
        }

        beforeActiveCam = instance.activeCamera;
        instance.activeCamera = newCam;
        instance.RecordCameraDefaults(newCam);

        newCam.Priority = ActivePriority;

        foreach (CinemachineVirtualCamera cam in cameras)
        {
            if (cam != null && cam != newCam)
            {
                cam.Priority = InactivePriority;
            }
        }
    }

    public static void Register(CinemachineVirtualCamera camera)
    {
        if (camera == null)
        {
            return;
        }

        if (!cameras.Contains(camera))
        {
            cameras.Add(camera);
        }
    }

    public static void UnRegister(CinemachineVirtualCamera camera)
    {
        if (camera == null)
        {
            return;
        }

        cameras.Remove(camera);
    }

    public static void Restore()
    {
        if (beforeActiveCam == null)
        {
            Debug.LogWarning("[CameraManager] Restore failed because beforeActiveCam is null.");
            return;
        }

        SwitchCamera(beforeActiveCam);
    }

    #endregion Camera Switching

    #region Cutscene State (Skip-safe)

    public static void PushCutsceneState()
    {
        if (instance == null)
        {
            return;
        }

        instance.PushCutsceneStateInternal();
    }

    public static void EnsureSkipSafeSnapshot()
    {
        if (instance == null)
        {
            return;
        }

        if (instance._cutsceneStateStack.Count == 0)
        {
            instance.PushCutsceneStateInternal();
        }
    }

    public static void PopCutsceneState(float duration = 1f, CameraEase ease = CameraEase.InOutSine, bool cut = false)
    {
        if (instance == null)
        {
            return;
        }

        instance.PopCutsceneStateInternal(duration, ease, cut);
    }

    private void PushCutsceneStateInternal()
    {
        CutsceneCameraState state = new CutsceneCameraState
        {
            activeCamera = activeCamera,
            previousCamera = beforeActiveCam
        };

        if (activeCamera != null)
        {
            if (TryGetOrthographicSize(activeCamera, out float size))
            {
                state.hasZoom = true;
                state.zoom = size;
            }

            state.followTarget = activeCamera.Follow;

            CinemachineFramingTransposer transposer = activeCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
            if (transposer != null)
            {
                state.hasPanOffset = true;
                state.panOffset = transposer.m_TrackedObjectOffset;
            }
        }

        CinemachineBrain brain = GetBrain();
        if (brain != null)
        {
            state.blendStyle = brain.m_DefaultBlend.m_Style;
            state.blendTime = brain.m_DefaultBlend.m_Time;
        }
        else
        {
            state.blendStyle = CinemachineBlendDefinition.Style.EaseInOut;
            state.blendTime = 1f;
        }

        _cutsceneStateStack.Push(state);
    }

    private void PopCutsceneStateInternal(float duration, CameraEase ease, bool cut)
    {
        if (_cutsceneStateStack.Count == 0)
        {
            Debug.LogWarning("[CameraManager] No saved cutscene state to restore.");
            return;
        }

        CutsceneCameraState state = _cutsceneStateStack.Pop();

        if (state.activeCamera != null)
        {
            SwitchCameraInternal(state.activeCamera, duration, ease, cut);
            beforeActiveCam = state.previousCamera;

            if (state.followTarget != null)
            {
                state.activeCamera.Follow = state.followTarget;
            }

            if (state.hasZoom)
            {
                ZoomCurrentCamera(state.zoom, duration, ease, false);
            }

            if (state.hasPanOffset)
            {
                TweenPanOffset(state.panOffset.x, state.panOffset.y, duration, false, ease);
            }
        }

        CinemachineBrain brain = GetBrain();
        if (brain != null)
        {
            brain.m_DefaultBlend = new CinemachineBlendDefinition(state.blendStyle, state.blendTime);
        }
    }

    #endregion Cutscene State (Skip-safe)

    #region Lens Zoom

    public static Coroutine ZoomCurrentCamera(float zoomValue, float duration = 1f, CameraEase ease = CameraEase.InOutSine, bool relative = false)
    {
        if (instance == null)
        {
            return null;
        }

        return instance.ZoomCurrentCameraInternal(zoomValue, duration, ease, relative);
    }

    public static Coroutine ResetCurrentCameraZoom(float duration = 1f, CameraEase ease = CameraEase.InOutSine)
    {
        if (instance == null)
        {
            return null;
        }

        return instance.ResetCurrentCameraZoomInternal(duration, ease);
    }

    private Coroutine ZoomCurrentCameraInternal(float zoomValue, float duration, CameraEase ease, bool relative)
    {
        if (activeCamera == null)
        {
            Debug.LogWarning("[CameraManager] Cannot zoom because activeCamera is null.");
            return null;
        }

        if (!TryGetOrthographicSize(activeCamera, out float startSize))
        {
            Debug.LogWarning("[CameraManager] Active camera is not orthographic. Zoom command ignored.");
            return null;
        }

        RecordCameraDefaults(activeCamera);

        float target = relative ? startSize + zoomValue : zoomValue;

        if (co_zoom != null)
        {
            StopCoroutine(co_zoom);
        }

        co_zoom = StartCoroutine(TweenZoomRoutine(activeCamera, startSize, target, Mathf.Max(0f, duration), ease));
        return co_zoom;
    }

    private Coroutine ResetCurrentCameraZoomInternal(float duration, CameraEase ease)
    {
        if (activeCamera == null)
        {
            return null;
        }

        RecordCameraDefaults(activeCamera);

        if (!_defaultZoomByCamera.TryGetValue(activeCamera, out float defaultZoom))
        {
            return null;
        }

        return ZoomCurrentCameraInternal(defaultZoom, duration, ease, false);
    }

    private IEnumerator TweenZoomRoutine(CinemachineVirtualCamera camera, float from, float to, float duration, CameraEase ease)
    {
        if (duration <= 0f)
        {
            SetOrthographicSize(camera, to);
            co_zoom = null;
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Mathf.Max(Time.deltaTime, 0f);
            float t = Mathf.Clamp01(elapsed / duration);
            float eased = EvaluateEase(ease, t);
            SetOrthographicSize(camera, Mathf.Lerp(from, to, eased));
            yield return null;
        }

        SetOrthographicSize(camera, to);
        co_zoom = null;
    }

    private bool TryGetOrthographicSize(CinemachineVirtualCamera camera, out float size)
    {
        size = 0f;
        if (camera == null)
        {
            return false;
        }

        if (!camera.m_Lens.Orthographic)
        {
            return false;
        }

        size = camera.m_Lens.OrthographicSize;
        return true;
    }

    private void SetOrthographicSize(CinemachineVirtualCamera camera, float size)
    {
        if (camera == null)
        {
            return;
        }

        var lens = camera.m_Lens;
        lens.OrthographicSize = size;
        camera.m_Lens = lens;
    }

    #endregion Lens Zoom

    #region Follow Override

    public static void SetTemporaryFollowTarget(Transform target, float duration = 0f)
    {
        if (instance == null)
        {
            return;
        }

        instance.SetTemporaryFollowTargetInternal(target, duration);
    }

    public static void ResetFollowTarget(float delay = 0f)
    {
        if (instance == null)
        {
            return;
        }

        instance.ResetFollowTargetInternal(delay);
    }

    private void SetTemporaryFollowTargetInternal(Transform target, float duration)
    {
        if (activeCamera == null)
        {
            Debug.LogWarning("[CameraManager] Cannot override follow target because activeCamera is null.");
            return;
        }

        if (target == null)
        {
            Debug.LogWarning("[CameraManager] Temporary follow target is null.");
            return;
        }

        RecordCameraDefaults(activeCamera);
        activeCamera.Follow = target;

        if (co_followReset != null)
        {
            StopCoroutine(co_followReset);
        }

        if (duration > 0f)
        {
            co_followReset = StartCoroutine(ResetFollowAfterDelay(Mathf.Max(0f, duration)));
        }
    }

    private void ResetFollowTargetInternal(float delay)
    {
        if (activeCamera == null)
        {
            return;
        }

        if (co_followReset != null)
        {
            StopCoroutine(co_followReset);
        }

        if (delay <= 0f)
        {
            ApplyDefaultFollow(activeCamera);
            return;
        }

        co_followReset = StartCoroutine(ResetFollowAfterDelay(delay));
    }

    private IEnumerator ResetFollowAfterDelay(float delay)
    {
        float elapsed = 0f;
        while (elapsed < delay)
        {
            elapsed += Mathf.Max(Time.deltaTime, 0f);
            yield return null;
        }

        ApplyDefaultFollow(activeCamera);
        co_followReset = null;
    }

    private void ApplyDefaultFollow(CinemachineVirtualCamera camera)
    {
        if (camera == null)
        {
            return;
        }

        if (_defaultFollowByCamera.TryGetValue(camera, out Transform follow))
        {
            camera.Follow = follow;
        }
    }

    #endregion Follow Override

    #region Camera Shake

    public static Coroutine ShakeCurrentCamera(float amplitude, float frequency, float duration, CameraEase ease = CameraEase.OutSine)
    {
        if (instance == null)
        {
            return null;
        }

        return instance.ShakeCurrentCameraInternal(amplitude, frequency, duration, ease);
    }

    private Coroutine ShakeCurrentCameraInternal(float amplitude, float frequency, float duration, CameraEase ease)
    {
        if (activeCamera == null)
        {
            Debug.LogWarning("[CameraManager] Cannot shake because activeCamera is null.");
            return null;
        }

        CinemachineBasicMultiChannelPerlin noise =
            activeCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

        if (noise == null)
        {
            Debug.LogWarning("[CameraManager] Active camera has no CinemachineBasicMultiChannelPerlin for shake.");
            return null;
        }

        if (co_shake != null)
        {
            StopCoroutine(co_shake);
        }

        co_shake = StartCoroutine(ShakeRoutine(noise, Mathf.Max(0f, amplitude), Mathf.Max(0f, frequency), Mathf.Max(0f, duration), ease));
        return co_shake;
    }

    private IEnumerator ShakeRoutine(CinemachineBasicMultiChannelPerlin noise, float amplitude, float frequency, float duration, CameraEase ease)
    {
        float baseAmplitude = noise.m_AmplitudeGain;
        float baseFrequency = noise.m_FrequencyGain;

        if (duration <= 0f)
        {
            noise.m_AmplitudeGain = baseAmplitude;
            noise.m_FrequencyGain = baseFrequency;
            co_shake = null;
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Mathf.Max(Time.deltaTime, 0f);
            float t = Mathf.Clamp01(elapsed / duration);
            float strength = 1f - EvaluateEase(ease, t);

            noise.m_AmplitudeGain = baseAmplitude + (amplitude * strength);
            noise.m_FrequencyGain = baseFrequency + (frequency * strength);
            yield return null;
        }

        noise.m_AmplitudeGain = baseAmplitude;
        noise.m_FrequencyGain = baseFrequency;
        co_shake = null;
    }

    #endregion Camera Shake

    #region Pan Offset

    public static Coroutine TweenPanOffset(float x, float y, float duration = 1f, bool relative = true, CameraEase ease = CameraEase.InOutSine)
    {
        if (instance == null)
        {
            return null;
        }

        return instance.TweenPanOffsetInternal(x, y, duration, relative, ease);
    }

    public static Coroutine ResetPanOffset(float duration = 1f, CameraEase ease = CameraEase.InOutSine)
    {
        if (instance == null)
        {
            return null;
        }

        return instance.ResetPanOffsetInternal(duration, ease);
    }

    public static void CancelPanOffsetTween()
    {
        if (instance == null)
        {
            return;
        }

        instance.StopOffsetTweenAndRestore();
    }

    private Coroutine TweenPanOffsetInternal(float x, float y, float duration, bool relative, CameraEase ease)
    {
        if (activeCamera == null)
        {
            Debug.LogWarning("[CameraManager] Cannot pan offset because activeCamera is null.");
            return null;
        }

        RecordCameraDefaults(activeCamera);

        CinemachineFramingTransposer transposer = activeCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        if (transposer != null)
        {
            Vector3 from = transposer.m_TrackedObjectOffset;
            Vector3 to = relative
                ? new Vector3(from.x + x, from.y + y, from.z)
                : new Vector3(x, y, from.z);

            StopOffsetTweenAndRestore();
            co_offset = StartCoroutine(TweenPanOffsetRoutine(transposer, from, to, Mathf.Max(0f, duration), ease));
            return co_offset;
        }

        return TweenPanOffsetWithoutTransposer(activeCamera, x, y, duration, relative, ease);
    }

    private Coroutine ResetPanOffsetInternal(float duration, CameraEase ease)
    {
        if (activeCamera == null)
        {
            return null;
        }

        RecordCameraDefaults(activeCamera);

        CinemachineFramingTransposer transposer = activeCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        if (transposer != null)
        {
            if (!_defaultPanOffsetByCamera.TryGetValue(activeCamera, out Vector3 defaultOffset))
            {
                return null;
            }

            return TweenPanOffsetInternal(defaultOffset.x, defaultOffset.y, duration, false, ease);
        }

        return ResetPanOffsetWithoutTransposer(activeCamera, duration, ease);
    }

    private Coroutine TweenPanOffsetWithoutTransposer(CinemachineVirtualCamera camera, float x, float y, float duration, bool relative, CameraEase ease)
    {
        StopOffsetTweenAndRestore();

        if (camera.Follow != null)
        {
            FollowTarget follower = GetOrCreateOffsetProxyFollower(camera, camera.Follow);
            Vector3 from = _offsetProxyValueByCamera.TryGetValue(camera, out Vector3 current) ? current : Vector3.zero;
            Vector3 to = relative
                ? new Vector3(from.x + x, from.y + y, from.z)
                : new Vector3(x, y, from.z);

            co_offset = StartCoroutine(TweenProxyOffsetRoutine(camera, follower, from, to, Mathf.Max(0f, duration), ease));
            return co_offset;
        }

        Vector3 fromPos = camera.transform.position;
        Vector3 basePos = _defaultPositionByCamera.TryGetValue(camera, out Vector3 defaultPos) ? defaultPos : fromPos;
        Vector3 toPos = relative
            ? fromPos + new Vector3(x, y, 0f)
            : new Vector3(basePos.x + x, basePos.y + y, fromPos.z);

        co_offset = StartCoroutine(TweenCameraPositionRoutine(camera, fromPos, toPos, Mathf.Max(0f, duration), ease));
        return co_offset;
    }

    private Coroutine ResetPanOffsetWithoutTransposer(CinemachineVirtualCamera camera, float duration, CameraEase ease)
    {
        if (_offsetProxyFollowerByCamera.TryGetValue(camera, out FollowTarget follower) && follower != null)
        {
            Vector3 from = _offsetProxyValueByCamera.TryGetValue(camera, out Vector3 current) ? current : follower.offset;
            Vector3 to = Vector3.zero;

            co_offset = StartCoroutine(ResetProxyOffsetRoutine(camera, follower, from, to, Mathf.Max(0f, duration), ease));
            return co_offset;
        }

        if (_defaultPositionByCamera.TryGetValue(camera, out Vector3 defaultPos))
        {
            co_offset = StartCoroutine(TweenCameraPositionRoutine(camera, camera.transform.position, defaultPos, Mathf.Max(0f, duration), ease));
            return co_offset;
        }

        return null;
    }

    private FollowTarget GetOrCreateOffsetProxyFollower(CinemachineVirtualCamera camera, Transform sourceFollow)
    {
        if (!_offsetProxyFollowerByCamera.TryGetValue(camera, out FollowTarget follower) || follower == null)
        {
            GameObject go = new GameObject($"{camera.name}_OffsetProxy");
            go.hideFlags = HideFlags.HideAndDontSave;
            follower = go.AddComponent<FollowTarget>();
            _offsetProxyFollowerByCamera[camera] = follower;
        }

        follower.target = sourceFollow;
        if (!_offsetProxyValueByCamera.TryGetValue(camera, out Vector3 current))
            current = Vector3.zero;

        follower.offset = current;
        camera.Follow = follower.transform;
        return follower;
    }

    private IEnumerator TweenProxyOffsetRoutine(CinemachineVirtualCamera camera, FollowTarget follower, Vector3 from, Vector3 to, float duration, CameraEase ease)
    {
        try
        {
            if (follower == null)
                yield break;

            if (duration <= 0f)
            {
                follower.offset = to;
                _offsetProxyValueByCamera[camera] = to;
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < duration && follower != null)
            {
                elapsed += Mathf.Max(Time.deltaTime, 0f);
                float t = Mathf.Clamp01(elapsed / duration);
                float eased = EvaluateEase(ease, t);
                follower.offset = Vector3.Lerp(from, to, eased);
                yield return null;
            }

            if (follower != null)
                follower.offset = to;

            _offsetProxyValueByCamera[camera] = to;
        }
        finally
        {
            co_offset = null;
            RestoreRawOffsetMode();
        }
    }

    private IEnumerator ResetProxyOffsetRoutine(CinemachineVirtualCamera camera, FollowTarget follower, Vector3 from, Vector3 to, float duration, CameraEase ease)
    {
        yield return TweenProxyOffsetRoutine(camera, follower, from, to, duration, ease);

        if (camera != null && _defaultFollowByCamera.TryGetValue(camera, out Transform defaultFollow))
        {
            camera.Follow = defaultFollow;
        }
    }

    private IEnumerator TweenCameraPositionRoutine(CinemachineVirtualCamera camera, Vector3 from, Vector3 to, float duration, CameraEase ease)
    {
        try
        {
            if (camera == null)
                yield break;

            if (duration <= 0f)
            {
                camera.transform.position = to;
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < duration && camera != null)
            {
                elapsed += Mathf.Max(Time.deltaTime, 0f);
                float t = Mathf.Clamp01(elapsed / duration);
                float eased = EvaluateEase(ease, t);
                camera.transform.position = Vector3.Lerp(from, to, eased);
                yield return null;
            }

            if (camera != null)
                camera.transform.position = to;
        }
        finally
        {
            co_offset = null;
            RestoreRawOffsetMode();
        }
    }

    private IEnumerator TweenPanOffsetRoutine(CinemachineFramingTransposer transposer, Vector3 from, Vector3 to, float duration, CameraEase ease)
    {
        ApplyRawOffsetMode(transposer);

        try
        {
            if (duration <= 0f)
            {
                transposer.m_TrackedObjectOffset = to;
                yield break;
            }

            Vector3 delta = to - from;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Mathf.Max(Time.deltaTime, 0f);
                float t = Mathf.Clamp01(elapsed / duration);
                float easedT = EvaluateEase(ease, t);
                transposer.m_TrackedObjectOffset = from + (delta * easedT);
                yield return null;
            }

            transposer.m_TrackedObjectOffset = to;
        }
        finally
        {
            co_offset = null;
            RestoreRawOffsetMode();
        }
    }

    private void StopOffsetTweenAndRestore()
    {
        if (co_offset != null)
        {
            StopCoroutine(co_offset);
            co_offset = null;
        }

        RestoreRawOffsetMode();
    }

    private void ApplyRawOffsetMode(CinemachineFramingTransposer transposer)
    {
        if (transposer == null)
        {
            return;
        }

        if (!_hasRawOffsetState || _rawOffsetTransposer != transposer)
        {
            _rawOffsetTransposer = transposer;
            _rawOffsetXDamping = transposer.m_XDamping;
            _rawOffsetYDamping = transposer.m_YDamping;
            _rawOffsetZDamping = transposer.m_ZDamping;
            _hasRawOffsetState = true;
        }

        transposer.m_XDamping = 0f;
        transposer.m_YDamping = 0f;
        transposer.m_ZDamping = 0f;
    }

    private void RestoreRawOffsetMode()
    {
        if (!_hasRawOffsetState || _rawOffsetTransposer == null)
        {
            _hasRawOffsetState = false;
            _rawOffsetTransposer = null;
            return;
        }

        _rawOffsetTransposer.m_XDamping = _rawOffsetXDamping;
        _rawOffsetTransposer.m_YDamping = _rawOffsetYDamping;
        _rawOffsetTransposer.m_ZDamping = _rawOffsetZDamping;

        _hasRawOffsetState = false;
        _rawOffsetTransposer = null;
    }

    #endregion Pan Offset

    #region Y Damping Lerp

    public void LerpYDamping(bool isPlayerFalling)
    {
        if (_framingTransposer == null)
        {
            Debug.LogWarning("[CameraManager] Cannot lerp Y damping because framing transposer is null.");
            return;
        }

        if (co_yLerp != null)
        {
            StopCoroutine(co_yLerp);
        }

        co_yLerp = StartCoroutine(LerpYAction(isPlayerFalling));
    }

    private IEnumerator LerpYAction(bool isPlayerFalling)
    {
        isLerpingYDaming = true;

        float startDampAmount = _framingTransposer.m_YDamping;
        float endDampAmount = isPlayerFalling ? _fallPanAmount : _normYPanAmount;
        lerpedFromPlayerFalling = isPlayerFalling;

        if (_fallYPanTime <= 0f)
        {
            _framingTransposer.m_YDamping = endDampAmount;
            isLerpingYDaming = false;
            yield break;
        }

        float elapsedTime = 0f;
        while (elapsedTime < _fallYPanTime)
        {
            elapsedTime += Mathf.Max(TimeScaleManager.PlayerDt, 0f);
            float t = Mathf.Clamp01(elapsedTime / _fallYPanTime);
            _framingTransposer.m_YDamping = Mathf.Lerp(startDampAmount, endDampAmount, t);
            yield return null;
        }

        _framingTransposer.m_YDamping = endDampAmount;
        isLerpingYDaming = false;
        co_yLerp = null;
    }

    #endregion Y Damping Lerp

    #region Utility

    private void RecordCameraDefaults(CinemachineVirtualCamera camera)
    {
        if (camera == null)
        {
            return;
        }

        if (!_defaultZoomByCamera.ContainsKey(camera) && camera.m_Lens.Orthographic)
        {
            _defaultZoomByCamera[camera] = camera.m_Lens.OrthographicSize;
        }

        if (!_defaultFollowByCamera.ContainsKey(camera))
        {
            _defaultFollowByCamera[camera] = camera.Follow;
        }

        if (!_defaultPanOffsetByCamera.ContainsKey(camera))
        {
            CinemachineFramingTransposer transposer = camera.GetCinemachineComponent<CinemachineFramingTransposer>();
            if (transposer != null)
            {
                _defaultPanOffsetByCamera[camera] = transposer.m_TrackedObjectOffset;
            }
        }

        if (!_defaultPositionByCamera.ContainsKey(camera))
        {
            _defaultPositionByCamera[camera] = camera.transform.position;
        }
    }

    public static CameraEase ParseEase(string value, CameraEase fallback = CameraEase.InOutSine)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return fallback;
        }

        string token = value.Trim().Replace("_", string.Empty).Replace("-", string.Empty).ToLowerInvariant();

        switch (token)
        {
            case "linear":
                return CameraEase.Linear;

            case "insine":
            case "easeinsine":
                return CameraEase.InSine;

            case "outsine":
            case "easeoutsine":
                return CameraEase.OutSine;

            case "inoutsine":
            case "easeinoutsine":
            case "easeinout":
                return CameraEase.InOutSine;

            case "inquad":
            case "easeinquad":
                return CameraEase.InQuad;

            case "outquad":
            case "easeoutquad":
                return CameraEase.OutQuad;

            case "inoutquad":
            case "easeinoutquad":
                return CameraEase.InOutQuad;

            default:
                return fallback;
        }
    }

    private static float EvaluateEase(CameraEase ease, float t)
    {
        t = Mathf.Clamp01(t);

        switch (ease)
        {
            case CameraEase.Linear:
                return t;

            case CameraEase.InSine:
                return 1f - Mathf.Cos((t * Mathf.PI) * 0.5f);

            case CameraEase.OutSine:
                return Mathf.Sin((t * Mathf.PI) * 0.5f);

            case CameraEase.InOutSine:
                return -(Mathf.Cos(Mathf.PI * t) - 1f) * 0.5f;

            case CameraEase.InQuad:
                return t * t;

            case CameraEase.OutQuad:
                return 1f - ((1f - t) * (1f - t));

            case CameraEase.InOutQuad:
                return t < 0.5f ? 2f * t * t : 1f - (Mathf.Pow(-2f * t + 2f, 2f) * 0.5f);

            default:
                return t;
        }
    }

    public bool IsPlayerNormalCamera()
    {
        return activeCamera == playerNormalCam;
    }

    public bool isPlayerNormalCamera()
    {
        return IsPlayerNormalCamera();
    }

    #endregion Utility

    private bool _hasRawOffsetState;
    private CinemachineFramingTransposer _rawOffsetTransposer;
    private float _rawOffsetXDamping;
    private float _rawOffsetYDamping;
    private float _rawOffsetZDamping;

    private void OnDisable()
    {
        StopOffsetTweenAndRestore();
    }
}