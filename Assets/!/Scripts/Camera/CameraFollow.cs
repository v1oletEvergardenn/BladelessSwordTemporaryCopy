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
    public float targetZoom;
    public float y_limit_low;
    private Vector3 velocity;
    private float xAmount;
    public bool activate = false;

    private float normalOrthoSize = 4f;

    private void Awake()
    {
        if (instance == null) { instance = this; }
    }

    // Start is called before the first frame update
    private void Start()
    {
        limitCamFollow = camFollow;
        normalOrthoSize = cam.m_Lens.OrthographicSize;
        player = CharacterController2D.instance;
        _isFacingRight = player.FacingRight;
        camFollow.SetParent(null);
        offset = normalOffset;
        limitCam = cam;
    }

    private Bounds _bound;

    // Update is called once per frame
    private void Update()
    {
        Vector3 _tempOffset = offset;
        if (!useOffset) { _tempOffset = Vector3.zero; }
        if (float.IsNaN(camFollow.position.x) || float.IsNaN(xAmount))
        {
            Debug.LogWarning("[CameraFollow] NaN detected! Resetting camFollow position and velocity.");
            camFollow.position = new Vector3(player.transform.position.x, camFollow.position.y, camFollow.position.z);
            xAmount = 0f;
        }

        camFollow.position = new Vector3(
            Mathf.SmoothDamp(camFollow.position.x, player.transform.position.x + _tempOffset.x, ref xAmount, 0.1f),
            player.transform.position.y + _tempOffset.y,
            camFollow.position.z);

        if (!activate) { return; }

        var bound = new Bounds(player.transform.position, Vector3.zero);
        for (int i = 0; i < targets.Count; i++)
        {
            if (targets[i].gameObject.activeInHierarchy)
            {
                if (targets[i].TryGetComponent<CameraFollowCondition>(out CameraFollowCondition condition))
                {
                    if (!condition.CheckCameraFollowCondition()) { continue; }
                }

                if (targets[i].TryGetComponent<IDamagable>(out IDamagable a))
                {
                    bound.Encapsulate(a.GetHitPos());
                    bound.Encapsulate(a.GetHitPos() + new Vector3(3, 3));
                    bound.Encapsulate(a.GetHitPos() - new Vector3(3, 3));
                }
                else
                {
                    bound.Encapsulate(targets[i].position);
                    Vector3 v = new Vector3(3, 3);
                    if (targets[i].gameObject.TryGetComponent<CameraBoundOffset>(out CameraBoundOffset obj))
                    {
                        v = new Vector3(obj.offset, obj.offset);
                    }
                    bound.Encapsulate(targets[i].position + v);
                    bound.Encapsulate(targets[i].position - v);
                }
            }
        }
        center = bound.center;
        _bound = bound;
        //get the center of targeted follow objects.

        if (targets.Count != 0)
        {
            float screenAspect = (float)Screen.width / (float)Screen.height;
            float camHeight = limitCam.m_Lens.OrthographicSize * 2;
            float camWidth = 2.0f * limitCam.m_Lens.OrthographicSize * screenAspect;
            float orthoSize_width = ((bound.size.x + 3) / 2) / screenAspect;
            float orthoSize_height = (bound.size.y + 3) / 2;
            targetZoom = MathF.Max(orthoSize_width, orthoSize_height);

            limitCam.m_Lens.OrthographicSize = Mathf.Lerp(limitCam.m_Lens.OrthographicSize, targetZoom, Time.unscaledDeltaTime * 5);
            //currentLimit.x = Mathf.Lerp(minLimit.x, maxLimit.x, Mathf.InverseLerp(minZoom, targetZoom, limitCam.m_Lens.OrthographicSize));
            //currentLimit.y = Mathf.Lerp(minLimit.y, maxLimit.y, Mathf.InverseLerp(minZoom, targetZoom, limitCam.m_Lens.OrthographicSize));
        }
        else
        {
            limitCam.m_Lens.OrthographicSize = normalOrthoSize;
            //currentLimit = maxLimit;
        }
        //float x = Mathf.Clamp(center.x, transform.position.x - currentLimit.x / 2, transform.position.x + currentLimit.x / 2);
        //float y = Mathf.Clamp(center.y, transform.position.y - currentLimit.y / 2, transform.position.y + currentLimit.y / 2);
        //update the orthographic size based on the distance of the targets.(bound)

        Vector3 tempOffset = offset;
        if (!useOffset) { tempOffset = Vector3.zero; }
        float y = center.y + tempOffset.y;
        if (y <= y_limit_low) { y = y_limit_low; }
        limitCamFollow.position = Vector3.SmoothDamp(limitCamFollow.position, new Vector3(center.x + tempOffset.x, y, limitCamFollow.position.z), ref velocity, 0.1f);
    }

    public void CallTurn()
    {
        StopAllCoroutines();
        _turnCoroutine = StartCoroutine(FlipYLerp());
    }

    public void Deactivate()
    {
        CameraManager.instance.SwitchToNormalCam();
        //currentLimit = maxLimit;
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
            elapsedTime += Time.unscaledDeltaTime;

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
        //Gizmos.DrawWireCube(transform.position, new Vector3(minLimit.x, minLimit.y, 1));
        //Gizmos.DrawWireCube(transform.position, new Vector3(maxLimit.x, maxLimit.y, 1));
        //Gizmos.color = Color.red;
        //Gizmos.DrawWireCube(transform.position, new Vector3(currentLimit.x, currentLimit.y, 1));

        if (showOffsetPos)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(_player.position + normalOffset, 0.1f);
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(_player.position + fallingOffset, 0.1f);
        }
        Gizmos.DrawWireCube(_bound.center, _bound.size);
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
            elapsedTime += Time.unscaledDeltaTime;
            Vector2 lerpedPanAmount = Vector2.Lerp(startAmount, endAmount, (elapsedTime / lerpTime));
            offset = lerpedPanAmount;

            yield return null;
        }
    }
}