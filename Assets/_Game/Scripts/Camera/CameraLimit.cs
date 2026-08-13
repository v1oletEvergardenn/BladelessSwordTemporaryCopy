using System.Collections.Generic;
using Cinemachine;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// Defines a camera-limited zone. When the player enters this trigger,
/// camera behavior is overridden through <see cref="CameraFollow"/>.
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class CameraLimit : MonoBehaviour
{
    #region Inspector - References

    [FoldoutGroup("References"), SerializeField]
    private List<Transform> targets;

    [FoldoutGroup("References"), Required, SerializeField]
    private CinemachineVirtualCamera cam;

    [FoldoutGroup("References"), Required]
    public BoxCollider2D col;

    #endregion Inspector - References

    #region Inspector - Camera Settings

    [FoldoutGroup("Camera Settings"), MinValue(0.1f), SerializeField]
    private float defaultOrthoSize = 4f;

    [FoldoutGroup("Camera Settings"), MinValue(0.1f)]
    public float minZoom = 2.5f;

    [FoldoutGroup("Camera Settings")]
    public bool CancelOffset = false;

    [FoldoutGroup("Camera Settings")]
    public bool includePlayer = false;

    [FoldoutGroup("Camera Settings")]
    public float y_limit_low;

    [FoldoutGroup("Camera Settings"), Range(0.1f, 1f)]
    public float cameraPositionSmoothTime = 0.2f;

    [FoldoutGroup("Camera Settings"), Range(0.1f, 10f)]
    public float cameraOrthoSmoothTime = 0.2f;

    #endregion Inspector - Camera Settings

    #region Inspector - Trigger & Debug

    [FoldoutGroup("Trigger")]
    public bool collideToTrigger = true;

    [FoldoutGroup("Debug")]
    public bool DEBUG = false;

    #endregion Inspector - Trigger & Debug

    private const int PlayerLayer = 6;
    private const int CompanionLayer = 14;

    /// <summary>
    /// Ensures required local references are available at runtime.
    /// </summary>
    private void Awake()
    {
        if (col == null)
        {
            col = GetComponent<BoxCollider2D>();
        }
    }

    /// <summary>
    /// Keeps references valid while editing in inspector.
    /// </summary>
    private void OnValidate()
    {
        if (col == null)
        {
            col = GetComponent<BoxCollider2D>();
        }
    }

    /// <summary>
    /// Applies this zone's camera rules to the shared <see cref="CameraFollow"/> instance.
    /// </summary>
    public void UpdateLimit()
    {
        if (cam == null)
        {
            Debug.LogWarning("[CameraLimit] No camera assigned.");
            return;
        }

        CameraFollow camZoom = CameraFollow.instance;
        if (camZoom == null)
        {
            Debug.LogWarning("[CameraLimit] CameraFollow instance is null.");
            return;
        }

        CameraManager.SwitchPixelPerfectCamera(false);

        CameraRegister register = cam.GetComponent<CameraRegister>();
        if (register != null)
        {
            register.SwitchThisCam();
        }

        ApplyLimitToCameraFollow(camZoom);
        ConfigureNoTargetModeIfNeeded(camZoom);

        if (CancelOffset)
        {
            camZoom.useOffset = false;
        }
    }

    /// <summary>
    /// Restores default camera behavior when leaving this zone.
    /// </summary>
    public void Deactivate()
    {
        CameraManager.SwitchPixelPerfectCamera(true);

        if (CameraFollow.instance != null)
        {
            CameraFollow.instance.Deactivate();
        }
    }

    /// <summary>
    /// Draws debug gizmos for the trigger and Y-limit line.
    /// </summary>
    private void OnDrawGizmos()
    {
        if (!DEBUG || col == null)
        {
            return;
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(
            transform.position + new Vector3(col.offset.x, col.offset.y, 0f),
            new Vector3(col.size.x, col.size.y, 0f));

        Gizmos.DrawLine(
            transform.position + new Vector3(4f, y_limit_low),
            transform.position + new Vector3(-4f, y_limit_low));
    }

    /// <summary>
    /// Activates limit behavior on supported trigger enter.
    /// </summary>
    /// <param name="collision">The collider entering this trigger.</param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collideToTrigger || collision == null)
        {
            return;
        }

        if (IsSupportedTriggerLayer(collision.gameObject.layer))
        {
            UpdateLimit();
        }
    }

    /// <summary>
    /// Deactivates limit behavior on supported trigger exit.
    /// </summary>
    /// <param name="collision">The collider leaving this trigger.</param>
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collideToTrigger || collision == null)
        {
            return;
        }

        if (IsSupportedTriggerLayer(collision.gameObject.layer))
        {
            Deactivate();
        }
    }

    /// <summary>
    /// Copies zone camera settings into the shared camera follow controller.
    /// </summary>
    /// <param name="camZoom">Target camera follow controller.</param>
    private void ApplyLimitToCameraFollow(CameraFollow camZoom)
    {
        camZoom.limitCam = cam;
        camZoom.cameraPositionSmoothTime = cameraPositionSmoothTime;
        camZoom.cameraOrthoSmoothTime = cameraOrthoSmoothTime;
        camZoom.activate = true;
        camZoom.minZoom = minZoom;
        camZoom.targets = targets != null ? new List<Transform>(targets) : new List<Transform>();
        camZoom.limitCamFollow = cam.transform;
        camZoom.y_limit_low = y_limit_low + transform.position.y;

        if (includePlayer && camZoom._player != null && !camZoom.targets.Contains(camZoom._player))
        {
            camZoom.targets.Add(camZoom._player);
        }

        camZoom.transform.position = transform.position;
    }

    /// <summary>
    /// Applies special behavior when no target is available.
    /// </summary>
    /// <param name="camZoom">Target camera follow controller.</param>
    private void ConfigureNoTargetModeIfNeeded(CameraFollow camZoom)
    {
        bool noTargetMode = camZoom.targets == null || camZoom.targets.Count == 0;

        camZoom.lockWhenNoTarget = noTargetMode;
        camZoom.noTargetOrthoSize = defaultOrthoSize;
        camZoom.noTargetPosition = cam.transform.position;

        if (!noTargetMode)
        {
            return;
        }

        camZoom.limitCamFollow.position = transform.position;
        cam.m_Lens.OrthographicSize = Mathf.Max(minZoom, defaultOrthoSize);
    }

    /// <summary>
    /// Checks whether the triggering layer should control this camera limit.
    /// </summary>
    /// <param name="layer">Layer id of trigger source.</param>
    /// <returns>True if the layer is supported.</returns>
    private static bool IsSupportedTriggerLayer(int layer)
    {
        return layer == PlayerLayer || layer == CompanionLayer;
    }
}