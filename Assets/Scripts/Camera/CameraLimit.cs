using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Cinemachine;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class CameraLimit : MonoBehaviour
{
    [SerializeField] private List<Transform> targets;
    [SerializeField] private CinemachineVirtualCamera cam;
    [SerializeField] private Transform camPos;
    private Vector3 center = Vector3.zero;
    public float minZoom = 2.5f;
    public float maxZoom = 14f;
    public float zoomLimiter_x = 30f;
    public float zoomLimiter_y = 30f;
    public Vector2 minLimit = new Vector2(1, 1);
    public Vector2 maxLimit = new Vector2(10, 10);

    public bool CancelOffset = false;
    public bool includePlayer = false;

    /// <summary>
    /// once touched the collision, update the camera zoom on Player, change the limit variable to this.
    /// </summary>
    public void UpdateLimit()
    {
        CameraFollow camZoom = CameraFollow.instance;
        cam.GetComponent<CameraRegister>().SwitchThisCam();
        camZoom.limitCam = cam;
        camZoom.activate = true;
        camZoom.minZoom = minZoom;
        camZoom.maxZoom = maxZoom;
        camZoom.targets = new List<Transform>(targets);
        camZoom.limitCamFollow = camPos;
        if (includePlayer) { camZoom.targets.Add(camZoom._player); }
        camZoom.zoomLimiter_x = zoomLimiter_x;
        camZoom.zoomLimiter_y = zoomLimiter_y;
        camZoom.minLimit = minLimit;
        camZoom.maxLimit = maxLimit;
        camZoom.transform.position = transform.position;
        if (CancelOffset) { camZoom.useOffset = false; }
    }

    /// <summary>
    /// once left the area, deactivate the camera zoom.
    /// </summary>
    public void Deactivate()
    {
        CameraFollow.instance.Deactivate();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(transform.position, new Vector3(minLimit.x, minLimit.y, 1));
        Gizmos.DrawWireCube(transform.position, new Vector3(maxLimit.x, maxLimit.y, 1));
        Gizmos.color = Color.red;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 6 | collision.gameObject.layer == 14)
        {
            CameraManager.SwitchPixelPerfectCamera(false);
            UpdateLimit();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 6 | collision.gameObject.layer == 14)
        {
            CameraManager.SwitchPixelPerfectCamera(true);
            Deactivate();
        }
    }
}