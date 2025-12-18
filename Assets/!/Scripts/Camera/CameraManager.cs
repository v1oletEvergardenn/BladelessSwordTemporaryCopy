using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.Windows.WebCam;

public class CameraManager : MonoBehaviour
{
    private static List<CinemachineVirtualCamera> cameras = new List<CinemachineVirtualCamera>();

    public CinemachineVirtualCamera activeCamera = null;
    public CinemachineVirtualCamera playerNormalCam;
    public static CinemachineVirtualCamera beforeActiveCam = null;

    public static CameraManager instance;

    [Header("controls for lerping the Y damping during Player falling/jumping")]
    [SerializeField] private float _fallPanAmount = 0.25f;

    [SerializeField] private float _fallYPanTime = 0.35f;
    public float _fallSpeedYDampingChangeThreshold = -15f;
    public bool isLerpingYDaming { get; private set; }
    public bool lerpedFromPlayerFalling { get; set; }

    [HideInInspector] public CinemachineFramingTransposer _framingTransposer;
    [HideInInspector] public float _normYPanAmount;

    [Header("PixelPerfectCameraSetting")]
    public Camera mainCam;

    public bool askForSwitchPixelPerfectCamera = false;

    private List<float> pixelCameraOrthographicSizes = new List<float> {
        1.534091f,
        1.6875f,
        1.875f,
        2.109375f,
        2.410714f,
        2.8125f,
        3.375f,
        4.21875f,
        5.625f,
        8.4375f,
        16.875f };

    public Vector2 desiredOrthographicSizeThreshold = Vector2.one;

    private bool desiredPixelPerfectCamState = true;

    private void Awake()
    {
        if (instance == null) { instance = this; }
        else { Destroy(this.gameObject); }
    }

    private void Update()
    {
        mainCam = Camera.main;
        if (mainCam.TryGetComponent<PixelPerfectCamera>(out PixelPerfectCamera cam))
        {
            if (cam.enabled != desiredPixelPerfectCamState)
            {
                if (mainCam.orthographicSize <= desiredOrthographicSizeThreshold.x || mainCam.orthographicSize >= desiredOrthographicSizeThreshold.y)
                {
                    mainCam.GetComponent<PixelPerfectCamera>().enabled = desiredPixelPerfectCamState;
                }
            }
            //Debug.Log($"OrthoSize: {mainCam.orthographicSize}, PixelRatio: {mainCam.GetComponent<PixelPerfectCamera>().pixelRatio}");
        }
    }

    private List<float> PixelCameraOrthoSizes()
    {
        pixelCameraOrthographicSizes.Clear();
        int steps = 10;
        int screenHeight = Screen.height;

        for (int n = 1; n <= steps; n++)
        {
            float orthoSize = (float)screenHeight / (2f * mainCam.GetComponent<PixelPerfectCamera>().assetsPPU * n);
            pixelCameraOrthographicSizes.Add(orthoSize);
        }

        pixelCameraOrthographicSizes.Reverse(); // Largest at last index
        return pixelCameraOrthographicSizes;
    }

    public static void SwitchPixelPerfectCamera(bool ask)
    {
        instance.desiredPixelPerfectCamState = ask;
        float currentOrthoSize = instance.mainCam.orthographicSize;
        List<float> tempOrthoList = instance.PixelCameraOrthoSizes();

        if (currentOrthoSize <= tempOrthoList[0])//if smaller than the first one
        {
            instance.desiredOrthographicSizeThreshold = new Vector2(0, tempOrthoList[0]);
        }
        else if (currentOrthoSize >= tempOrthoList[tempOrthoList.Count - 1])//if bigger than the biggest one
        {
            instance.desiredOrthographicSizeThreshold = new Vector2(tempOrthoList[tempOrthoList.Count - 1], Mathf.Infinity);
        }
        else
        {
            for (int i = 0; i < tempOrthoList.Count - 1; i++)
            {
                if (currentOrthoSize >= tempOrthoList[i] && currentOrthoSize <= tempOrthoList[i + 1])
                {
                    instance.desiredOrthographicSizeThreshold = new Vector2(tempOrthoList[i], tempOrthoList[i + 1]);
                    break;
                }
            }
        }
    }

    public void SwitchToNormalCam()
    {
        mainCam.GetComponent<PixelPerfectCamera>().enabled = true;
        if (playerNormalCam == null)
        {
            GameObject cam = GameObject.Find("CM_normalCam");
            playerNormalCam = cam.GetComponent<CinemachineVirtualCamera>();
        }
        SwitchCamera(playerNormalCam);
        _framingTransposer = playerNormalCam.GetCinemachineComponent<CinemachineFramingTransposer>();
        _normYPanAmount = _framingTransposer.m_YDamping;
    }

    public static bool IsActiveCamera(CinemachineVirtualCamera camera)
    {
        return camera == instance.activeCamera;
    }

    public static void SwitchCamera(CinemachineVirtualCamera newCam)
    {
        newCam.Priority = 10;
        if (beforeActiveCam != instance.activeCamera) { beforeActiveCam = instance.activeCamera; }
        instance.activeCamera = newCam;

        foreach (CinemachineVirtualCamera cam in cameras)
        {
            if (cam != newCam) { cam.Priority = 0; }
        }
    }

    public static void SwtichToPreviousCamera()
    {
        beforeActiveCam.Priority = 10;
        foreach (CinemachineVirtualCamera cam in cameras)
        {
            if (cam != beforeActiveCam) { cam.Priority = 0; }
        }
    }

    public static void SwitchBounceQTECamera(CinemachineVirtualCamera newCam)
    {
        newCam.Priority = 10;
        beforeActiveCam = instance.activeCamera;
        instance.activeCamera = newCam;

        foreach (CinemachineVirtualCamera cam in cameras)
        {
            if (cam != newCam) { cam.Priority = 0; }
        }
    }

    public static void Register(CinemachineVirtualCamera camera)
    {
        cameras.Add(camera);
    }

    public static void Restore()
    {
        SwitchCamera(beforeActiveCam);
    }

    public static void UnRegister(CinemachineVirtualCamera camera)
    {
        cameras.Remove(camera);
    }

    public void LerpYDamping(bool isPlayerFalling)
    {
        StartCoroutine(LerpYAction(isPlayerFalling));
    }

    private IEnumerator LerpYAction(bool isPlayerFalling)
    {
        isLerpingYDaming = true;

        float startDampAmount = _framingTransposer.m_YDamping;
        float endDampAmount = 0f;

        if (isPlayerFalling)
        {
            endDampAmount = _fallPanAmount;
            lerpedFromPlayerFalling = true;
        }
        else
        {
            endDampAmount = _normYPanAmount;
        }

        float elapsedTime = 0f;
        while (elapsedTime < _fallYPanTime)
        {
            elapsedTime += Time.deltaTime;
            float lerpedPanAmount = Mathf.Lerp(startDampAmount, endDampAmount, (elapsedTime / _fallYPanTime));
            _framingTransposer.m_YDamping = lerpedPanAmount;

            yield return null;
        }

        isLerpingYDaming = false;
    }

    public bool isPlayerNormalCamera()
    {
        return activeCamera == playerNormalCam;
    }
}