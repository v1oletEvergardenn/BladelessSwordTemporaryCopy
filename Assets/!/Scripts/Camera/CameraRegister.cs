using Cinemachine;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class CameraRegister : MonoBehaviour
{
    public bool isMainCam = false;

    private void Start()
    {
        CameraManager.Register(GetComponent<CinemachineVirtualCamera>());
        gameObject.AddComponent<CinemachinePixelPerfect>();
        if (isMainCam)
        {
            CameraManager.instance.playerNormalCam = GetComponent<CinemachineVirtualCamera>();
            CameraManager.instance._framingTransposer = CameraManager.instance.playerNormalCam.GetCinemachineComponent<CinemachineFramingTransposer>();
            CameraManager.instance._normYPanAmount = CameraManager.instance._framingTransposer.m_YDamping;
        }
    }

    private void OnDisable()
    {
        CameraManager.UnRegister(GetComponent<CinemachineVirtualCamera>());
    }

    public void SwitchThisCam()
    {
        CameraManager.SwitchCamera(GetComponent<CinemachineVirtualCamera>());
    }

    public void SwitchToNormalCam()
    {
        CameraManager.instance.SwtichToNormalCam();
    }
}