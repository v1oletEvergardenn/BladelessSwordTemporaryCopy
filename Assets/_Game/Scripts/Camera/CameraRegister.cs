using Cinemachine;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class CameraRegister : MonoBehaviour
{
    public bool isMainCam = false;
    public int priority = 0;

    private CinemachineVirtualCamera cam;

    private void Start()
    {
        cam = GetComponent<CinemachineVirtualCamera>();
        if (cam == null)
        {
            return;
        }

        CameraManager.Register(cam);

        gameObject.AddComponent<CinemachinePixelPerfect>();
        EnsureHandheldNoise();

        if (isMainCam)
        {
            CameraManager.instance.playerNormalCam = cam;
            CameraManager.instance._framingTransposer =
                CameraManager.instance.playerNormalCam.GetCinemachineComponent<CinemachineFramingTransposer>();

            if (CameraManager.instance._framingTransposer != null)
            {
                CameraManager.instance._normYPanAmount = CameraManager.instance._framingTransposer.m_YDamping;
            }

            CameraManager.SwitchCamera(CameraManager.instance.playerNormalCam);
        }
        else
        {
            cam.Priority = priority;
        }
    }

    private void OnDisable()
    {
        CameraManager.UnRegister(cam);
    }

    public void SwitchThisCam()
    {
        CameraManager.SwitchCamera(cam);
    }

    public void SwitchToNormalCam()
    {
        CameraManager.instance.SwitchToNormalCam();
    }

    private void EnsureHandheldNoise()
    {
        CinemachineBasicMultiChannelPerlin noise =
            cam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

        if (noise == null)
        {
            noise = cam.AddCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        }

        NoiseSettings profile = CameraManager.instance.handheldNormalMild;

        if (profile != null)
        {
            noise.m_NoiseProfile = profile;
            noise.m_AmplitudeGain = 0f;
            noise.m_FrequencyGain = 0f;
        }
        else
        {
            Debug.LogWarning(
                "[CameraRegister] 'handheld_normal_mild' NoiseSettings not found. Assign it in inspector or place it in a Resources path.");
        }
    }
}