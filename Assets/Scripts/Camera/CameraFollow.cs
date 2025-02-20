using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] public Transform _player;
    [SerializeField] private float _flipYTime = 0.5f;
    [HideInInspector] public Vector3 offset;
    public Vector3 fallingOffset;
    public Vector3 normalOffset;
    public bool useOffset = true;
    public bool showOffsetPos;

    private Coroutine _turnCoroutine;
    private CharacterController2D player;
    private bool _isFacingRight;
    public Transform camFollow;
    [HideInInspector] public Transform limitCamFollow;
    public static CameraFollow instance;
    public CinemachineVirtualCamera cam;
    [HideInInspector] public CinemachineVirtualCamera limitCam;
    public List<Transform> targets;
    private Vector3 center = Vector3.zero;
    public float minZoom = 2.5f;
    public float maxZoom = 14f;
    public float zoomLimiter_x = 30f;
    public float zoomLimiter_y = 30f;
    public Vector2 minLimit = new Vector2(1, 1);
    public Vector2 maxLimit = new Vector2(10, 10);

    public Vector2 currentLimit = Vector2.zero;

    private Vector3 velocity;
    private float xAmount;
    public bool activate = false;

    private float normalOrthoSize = 4f;

    private void Awake()
    {
        if (instance == null) { instance = this; }
        else { Destroy(this.gameObject); }
        limitCamFollow = camFollow;
    }

    // Start is called before the first frame update
    private void Start()
    {
        normalOrthoSize = cam.m_Lens.OrthographicSize;
        player = CharacterController2D.instance;
        _isFacingRight = player.m_FacingRight;
        camFollow.SetParent(null);
        offset = normalOffset;
        limitCam = cam;
    }

    // Update is called once per frame
    private void Update()
    {
        Vector3 _tempOffset = offset;
        if (!useOffset) { _tempOffset = Vector3.zero; }
        camFollow.position = new Vector3(Mathf.SmoothDamp(camFollow.position.x, player.transform.position.x + _tempOffset.x, ref xAmount, 0.1f), player.transform.position.y + _tempOffset.y, camFollow.position.z);

        if (!activate) { return; }

        var bound = new Bounds(player.transform.position, Vector3.zero);
        for (int i = 0; i < targets.Count; i++)
        {
            if (targets[i].gameObject.activeInHierarchy)
            {
                if (targets[i].TryGetComponent<IDamagable>(out IDamagable a))
                {
                    bound.Encapsulate(a.GetHitPos());
                }
                else
                {
                    bound.Encapsulate(targets[i].position);
                }
            }
        }
        center = bound.center;
        //get the center of targeted follow objects.

        if (targets.Count != 0)
        {
            limitCam.m_Lens.OrthographicSize = Mathf.Lerp(limitCam.m_Lens.OrthographicSize, Mathf.Lerp(minZoom, maxZoom, Mathf.Max((bound.size.x + 2) / zoomLimiter_x, (bound.size.y + 2) / zoomLimiter_y)), Time.deltaTime * 5);
            currentLimit.x = Mathf.Lerp(maxLimit.x, minLimit.x, Mathf.InverseLerp(minZoom, maxZoom, limitCam.m_Lens.OrthographicSize));
            currentLimit.y = Mathf.Lerp(maxLimit.y, minLimit.y, Mathf.InverseLerp(minZoom, maxZoom, limitCam.m_Lens.OrthographicSize));
        }
        else
        {
            limitCam.m_Lens.OrthographicSize = normalOrthoSize;
            currentLimit = maxLimit;
        }
        float x = Mathf.Clamp(center.x, transform.position.x - currentLimit.x / 2, transform.position.x + currentLimit.x / 2);
        float y = Mathf.Clamp(center.y, transform.position.y - currentLimit.y / 2, transform.position.y + currentLimit.y / 2);
        //update the orthographic size based on the distance of the targets.(bound)

        Vector3 tempOffset = offset;
        if (!useOffset) { tempOffset = Vector3.zero; }
        limitCamFollow.position = Vector3.SmoothDamp(limitCamFollow.position, new Vector3(x + tempOffset.x, y + tempOffset.y, limitCamFollow.position.z), ref velocity, 0.1f);
    }

    public void CallTurn()
    {
        StopAllCoroutines();
        _turnCoroutine = StartCoroutine(FlipYLerp());
    }

    public void Deactivate()
    {
        CameraManager.instance.SwtichToNormalCam();
        currentLimit = maxLimit;
        activate = false;
        useOffset = true;
        targets.Clear();
    }

    /// <summary>
    /// flip around Y axis smoothly.
    /// </summary>
    /// <returns></returns>
    private IEnumerator FlipYLerp()
    {
        float startRotation = camFollow.transform.localEulerAngles.y;
        float endRotationAmount = 0f;
        endRotationAmount += DetermineEndRotation();
        float yRotation = 0f;

        float elapsedTime = 0f;
        while (elapsedTime < _flipYTime)
        {
            elapsedTime += Time.deltaTime;

            yRotation = Mathf.Lerp(startRotation, endRotationAmount, (elapsedTime / _flipYTime));
            camFollow.rotation = Quaternion.Euler(0f, yRotation, 0f);

            yield return null;
        }
    }

    private float DetermineEndRotation()
    {
        _isFacingRight = !_isFacingRight;

        if (_isFacingRight) { return 0; }
        else { return 180f; }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(transform.position, new Vector3(minLimit.x, minLimit.y, 1));
        Gizmos.DrawWireCube(transform.position, new Vector3(maxLimit.x, maxLimit.y, 1));
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, new Vector3(currentLimit.x, currentLimit.y, 1));

        if (showOffsetPos)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(_player.position + normalOffset, 0.1f);
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(_player.position + fallingOffset, 0.1f);
        }
    }

    public void ChangeOffset(Vector2 desireOffset)
    {
        StopAllCoroutines();
        StartCoroutine(IEChangeOffset(desireOffset));
    }

    public IEnumerator IEChangeOffset(Vector2 desireOffset)
    {
        float elapsedTime = 0f;
        float lerpTime = 0.4f;
        Vector2 startAmount = offset;
        Vector2 endAmount = desireOffset;
        while (elapsedTime < lerpTime)
        {
            elapsedTime += Time.deltaTime;
            Vector2 lerpedPanAmount = Vector2.Lerp(startAmount, endAmount, (elapsedTime / lerpTime));
            offset = lerpedPanAmount;

            yield return null;
        }
    }
}