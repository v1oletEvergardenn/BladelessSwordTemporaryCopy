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
    private Vector3 center = Vector3.zero;
    public float minZoom = 2.5f;
    public BoxCollider2D col;
    public bool CancelOffset = false;
    public bool includePlayer = false;
    public float y_limit_low;
    public bool DEBUG = false;
    public bool collideToTrigger = true;
    [Range(0.1f, 1f)] public float cameraPositionSmoothTime = 0.2f;
    [Range(0.1f, 10f)] public float cameraOrthoSmoothTime = 0.2f;

    /// <summary>
    /// once touched the collision, update the camera zoom on Player, change the limit variable to this.
    /// </summary>
    public void UpdateLimit()
    {
        CameraManager.SwitchPixelPerfectCamera(false);
        CameraFollow camZoom = CameraFollow.instance;
        cam.GetComponent<CameraRegister>().SwitchThisCam();
        camZoom.limitCam = cam;
        camZoom.cameraPositionSmoothTime = cameraPositionSmoothTime;
        camZoom.cameraOrthoSmoothTime = cameraOrthoSmoothTime;
        camZoom.activate = true;
        camZoom.minZoom = minZoom;
        camZoom.targets = new List<Transform>(targets);
        camZoom.limitCamFollow = cam.transform;
        camZoom.y_limit_low = y_limit_low + transform.position.y;
        if (includePlayer) { camZoom.targets.Add(camZoom._player); }
        camZoom.transform.position = transform.position;
        if (CancelOffset) { camZoom.useOffset = false; }
    }

    /// <summary>
    /// once left the area, deactivate the camera zoom.
    /// </summary>
    public void Deactivate()
    {
        CameraManager.SwitchPixelPerfectCamera(true);
        CameraFollow.instance.Deactivate();
    }

    private void OnDrawGizmos()
    {
        if (!DEBUG) { return; }
        Gizmos.color = Color.blue;
        //Gizmos.DrawWireCube(transform.position, new Vector3(minLimit.x, minLimit.y, 1));
        //Gizmos.DrawWireCube(transform.position, new Vector3(maxLimit.x, maxLimit.y, 1));
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position + new Vector3(col.offset.x, col.offset.y, 0), new Vector3(col.size.x, col.size.y, 0));
        Gizmos.DrawLine(transform.position + new Vector3(4, y_limit_low), transform.position + new Vector3(-4, y_limit_low));
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 6 | collision.gameObject.layer == 14)
        {
            if (collideToTrigger)
            {
                UpdateLimit();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 6 | collision.gameObject.layer == 14)
        {
            if (collideToTrigger)
            {
                Deactivate();
            }
        }
    }
}