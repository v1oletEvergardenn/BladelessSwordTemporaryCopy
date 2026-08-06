using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// Handles player-follow camera behavior and optional dynamic framing/zoom
/// when a <see cref="CameraLimit"/> zone is active.
/// </summary>
public class CameraFollow : MonoBehaviour
{
    #region Inspector - Core References

    [FoldoutGroup("References"), Required, SerializeField]
    public Transform _player;

    [FoldoutGroup("References"), Required]
    public Transform camFollow;

    [FoldoutGroup("References"), Required]
    public CinemachineVirtualCamera cam;

    #endregion Inspector - Core References

    #region Inspector - Offsets

    [FoldoutGroup("Offsets")]
    public Vector3 fallingOffset;

    [FoldoutGroup("Offsets")]
    public Vector3 normalOffset;

    [FoldoutGroup("Offsets")]
    public bool useOffset = true;

    [FoldoutGroup("Offsets")]
    public bool showOffsetPos;

    [HideInInspector]
    public Vector3 offset;

    #endregion Inspector - Offsets

    #region Inspector - Runtime Camera Limit Settings

    [FoldoutGroup("Limit Runtime")]
    public List<Transform> targets = new List<Transform>();

    [FoldoutGroup("Limit Runtime")]
    public float minZoom = 2.5f;

    [FoldoutGroup("Limit Runtime")]
    public float targetZoom;

    [FoldoutGroup("Limit Runtime")]
    public float y_limit_low;

    [FoldoutGroup("Limit Runtime")]
    public bool activate = false;

    [FoldoutGroup("Limit Runtime"), Range(0.1f, 1f)]
    public float cameraPositionSmoothTime = 0.2f;

    [FoldoutGroup("Limit Runtime"), Range(0.1f, 10f)]
    public float cameraOrthoSmoothTime = 0.2f;

    [HideInInspector]
    public Transform limitCamFollow;

    [HideInInspector]
    public CinemachineVirtualCamera limitCam;

    [HideInInspector]
    public bool lockWhenNoTarget = false;

    [HideInInspector]
    public Vector3 noTargetPosition;

    [HideInInspector]
    public float noTargetOrthoSize = 4f;

    #endregion Inspector - Runtime Camera Limit Settings

    #region Singleton

    public static CameraFollow instance;

    #endregion Singleton

    #region Private Runtime State

    [SerializeField] private float _flipYTime = 0.5f;

    private Coroutine _turnCoroutine;
    private Coroutine _offsetCoroutine;
    private CharacterController2D player;
    private bool _isFacingRight;
    private Vector3 center = Vector3.zero;
    private Vector3 velocity;
    private float xAmount;
    private float normalOrthoSize = 4f;
    private Bounds _bound;

    #endregion Private Runtime State

    /// <summary>
    /// Initializes singleton reference.
    /// </summary>
    private void Awake()
    {
        if (instance == null || instance == this)
        {
            instance = this;
        }
        else
        {
            Debug.LogWarning("[CameraFollow] Duplicate instance detected. Keeping first instance.");
        }
    }

    /// <summary>
    /// Initializes follow references and defaults.
    /// </summary>
    private void Start()
    {
        if (camFollow == null || cam == null)
        {
            Debug.LogWarning("[CameraFollow] Missing camFollow or cam reference.");
            enabled = false;
            return;
        }

        if (player == null)
        {
            player = CharacterController2D.instance;
        }

        if (player == null || _player == null)
        {
            Debug.LogWarning("[CameraFollow] Missing player references.");
            enabled = false;
            return;
        }

        limitCamFollow = camFollow;
        normalOrthoSize = cam.m_Lens.OrthographicSize;
        noTargetOrthoSize = normalOrthoSize;
        noTargetPosition = camFollow.position;
        _isFacingRight = player.FacingRight;
        camFollow.SetParent(null);
        offset = normalOffset;
        limitCam = cam;
    }

    /// <summary>
    /// Updates regular follow and optional limit-camera logic.
    /// </summary>
    private void Update()
    {
        UpdatePlayerFollowPosition();

        if (!activate)
        {
            return;
        }

        UpdateLimitCamera();
    }

    /// <summary>
    /// Adds a dynamic target to the current limit camera.
    /// </summary>
    /// <param name="target">Target transform to add.</param>
    public static void AddTarget(Transform target)
    {
        if (instance == null || target == null)
        {
            return;
        }

        if (instance.targets == null)
        {
            instance.targets = new List<Transform>();
        }

        if (!instance.targets.Contains(target))
        {
            instance.targets.Add(target);
        }
    }

    /// <summary>
    /// Removes a dynamic target from the current limit camera.
    /// </summary>
    /// <param name="target">Target transform to remove.</param>
    public static void RemoveTarget(Transform target)
    {
        if (instance == null || target == null || instance.targets == null)
        {
            return;
        }

        if (instance.targets.Contains(target))
        {
            instance.targets.Remove(target);
        }
    }

    /// <summary>
    /// Starts smooth Y-axis camera pivot rotation.
    /// </summary>
    public void CallTurn()
    {
        if (_turnCoroutine != null)
        {
            StopCoroutine(_turnCoroutine);
        }

        _turnCoroutine = StartCoroutine(FlipYLerp());
    }

    /// <summary>
    /// Restores default camera mode and clears temporary limit data.
    /// </summary>
    public void Deactivate()
    {
        CameraManager.instance.SwitchToNormalCam();

        activate = false;
        useOffset = true;
        lockWhenNoTarget = false;

        if (targets != null)
        {
            targets.Clear();
        }
    }

    /// <summary>
    /// Draws debug gizmos for offset and current tracking bounds.
    /// </summary>
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;

        if (showOffsetPos && _player != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(_player.position + normalOffset, 0.1f);
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(_player.position + fallingOffset, 0.1f);
        }

        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(_bound.center, _bound.size);
    }

    /// <summary>
    /// Smoothly changes camera follow offset over time.
    /// </summary>
    /// <param name="desireOffset">Target offset.</param>
    public void ChangeOffset(Vector2 desireOffset)
    {
        if (_offsetCoroutine != null)
        {
            StopCoroutine(_offsetCoroutine);
        }

        _offsetCoroutine = StartCoroutine(IEChangeOffset(desireOffset));
    }

    /// <summary>
    /// Coroutine for smooth offset transition.
    /// </summary>
    /// <param name="desireOffset">Target offset.</param>
    /// <returns>Coroutine enumerator.</returns>
    public IEnumerator IEChangeOffset(Vector2 desireOffset)
    {
        float elapsedTime = 0f;
        float lerpTime = 0.4f;
        Vector2 startAmount = offset;
        Vector2 endAmount = desireOffset;

        while (elapsedTime < lerpTime)
        {
            elapsedTime += Time.unscaledDeltaTime;
            Vector2 lerpedPanAmount = Vector2.Lerp(startAmount, endAmount, elapsedTime / lerpTime);
            offset = lerpedPanAmount;
            yield return null;
        }

        offset = endAmount;
    }

    /// <summary>
    /// Updates basic player follow movement for the normal camera anchor.
    /// </summary>
    private void UpdatePlayerFollowPosition()
    {
        if (player == null || camFollow == null)
        {
            return;
        }

        Vector3 tempOffset = useOffset ? offset : Vector3.zero;

        if (float.IsNaN(camFollow.position.x) || float.IsNaN(xAmount))
        {
            Debug.LogWarning("[CameraFollow] NaN detected. Resetting follow values.");
            camFollow.position = new Vector3(player.transform.position.x, camFollow.position.y, camFollow.position.z);
            xAmount = 0f;
        }

        camFollow.position = new Vector3(
            Mathf.SmoothDamp(camFollow.position.x, player.transform.position.x + tempOffset.x, ref xAmount, cameraPositionSmoothTime),
            player.transform.position.y + tempOffset.y,
            camFollow.position.z);
    }

    /// <summary>
    /// Updates dynamic bounds, zoom, and position for the active limit camera.
    /// </summary>
    private void UpdateLimitCamera()
    {
        if (limitCam == null || limitCamFollow == null || player == null)
        {
            return;
        }

        bool hasNoTargets = targets == null || targets.Count == 0;
        if (hasNoTargets && lockWhenNoTarget)
        {
            limitCam.m_Lens.OrthographicSize = Mathf.Max(minZoom, noTargetOrthoSize);
            limitCamFollow.position = noTargetPosition;
            return;
        }

        if (!TryBuildTargetBounds(out Bounds bounds))
        {
            limitCam.m_Lens.OrthographicSize = Mathf.Max(minZoom, normalOrthoSize);
            return;
        }

        center = bounds.center;
        _bound = bounds;

        ApplyDynamicZoom(bounds);
        ApplyLimitCameraPosition();
    }

    /// <summary>
    /// Builds world bounds from player and all valid dynamic targets.
    /// </summary>
    /// <param name="bounds">Result bounds.</param>
    /// <returns>True if at least one valid target contributed.</returns>
    private bool TryBuildTargetBounds(out Bounds bounds)
    {
        bounds = new Bounds(player.transform.position, Vector3.zero);
        bool hasAnyTarget = false;

        if (targets == null)
        {
            return false;
        }

        for (int i = 0; i < targets.Count; i++)
        {
            Transform target = targets[i];
            if (target == null || !target.gameObject.activeInHierarchy)
            {
                continue;
            }

            if (target.TryGetComponent<CameraFollowCondition>(out CameraFollowCondition condition) &&
                !condition.CheckCameraFollowCondition())
            {
                continue;
            }

            hasAnyTarget = true;

            if (target.TryGetComponent<IDamagable>(out IDamagable damagable))
            {
                Vector3 hitPos = damagable.GetHitPos();
                bounds.Encapsulate(hitPos);
                bounds.Encapsulate(hitPos + new Vector3(3f, 3f));
                bounds.Encapsulate(hitPos - new Vector3(3f, 3f));
            }
            else
            {
                Vector3 margin = new Vector3(3f, 3f);
                if (target.TryGetComponent<CameraBoundOffset>(out CameraBoundOffset boundOffset))
                {
                    margin = new Vector3(boundOffset.offset, boundOffset.offset);
                }

                bounds.Encapsulate(target.position);
                bounds.Encapsulate(target.position + margin);
                bounds.Encapsulate(target.position - margin);
            }
        }

        return hasAnyTarget;
    }

    /// <summary>
    /// Applies orthographic zoom based on target bounds.
    /// </summary>
    /// <param name="bounds">Current dynamic bounds.</param>
    private void ApplyDynamicZoom(Bounds bounds)
    {
        float screenAspect = (float)Screen.width / Screen.height;
        float orthoSizeWidth = ((bounds.size.x + 3f) / 2f) / screenAspect;
        float orthoSizeHeight = (bounds.size.y + 3f) / 2f;

        targetZoom = Mathf.Max(minZoom, Mathf.Max(orthoSizeWidth, orthoSizeHeight));
        limitCam.m_Lens.OrthographicSize = Mathf.Lerp(
            limitCam.m_Lens.OrthographicSize,
            targetZoom,
            Time.unscaledDeltaTime * cameraOrthoSmoothTime);
    }

    /// <summary>
    /// Applies smooth camera anchor movement to the limit camera follow transform.
    /// </summary>
    private void ApplyLimitCameraPosition()
    {
        Vector3 tempOffset = useOffset ? offset : Vector3.zero;
        float y = center.y + tempOffset.y;
        if (y <= y_limit_low)
        {
            y = y_limit_low;
        }

        Vector3 desired = new Vector3(center.x + tempOffset.x, y, limitCamFollow.position.z);
        limitCamFollow.position = Vector3.SmoothDamp(limitCamFollow.position, desired, ref velocity, cameraPositionSmoothTime);
    }

    /// <summary>
    /// Smoothly flips camera pivot around Y axis.
    /// </summary>
    /// <returns>Coroutine enumerator.</returns>
    private IEnumerator FlipYLerp()
    {
        float startRotation = camFollow.transform.localEulerAngles.y;
        float endRotationAmount = DetermineEndRotation();
        float elapsedTime = 0f;

        while (elapsedTime < _flipYTime)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float yRotation = Mathf.Lerp(startRotation, endRotationAmount, elapsedTime / _flipYTime);
            camFollow.rotation = Quaternion.Euler(0f, yRotation, 0f);
            yield return null;
        }
    }

    /// <summary>
    /// Computes next Y rotation target based on facing direction.
    /// </summary>
    /// <returns>Y angle in degrees.</returns>
    private float DetermineEndRotation()
    {
        _isFacingRight = !_isFacingRight;
        return _isFacingRight ? 0f : 180f;
    }
}