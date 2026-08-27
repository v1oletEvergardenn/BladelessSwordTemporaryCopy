using System.Collections;
using PixelCrushers.DialogueSystem.SequencerCommands;
using UnityEngine;

namespace PixelCrushers.DialogueSystem.SequencerCommands
{
    [AddComponentMenu("")]
    public class SequencerCommandCamShake : SequencerCommand
    {
        private IEnumerator Start()
        {
            if (CameraManager.instance == null)
            {
                Debug.LogWarning("[SequencerCommandCamShake] CameraManager instance not found.");
                Stop();
                yield break;
            }

            float duration = Mathf.Max(0f, GetParameterAsFloat(0, 0.5f));
            float amplitude = Mathf.Max(0f, GetParameterAsFloat(1, 0.5f));
            float frequency = Mathf.Max(0f, GetParameterAsFloat(2, 15f));
            CameraManager.CameraEase ease = CameraManager.ParseEase(
                GetParameter(3, "InOutSine"),
                CameraManager.CameraEase.InOutSine);

            CameraManager.EnsureSkipSafeSnapshot();
            CameraManager.ShakeCurrentCamera(amplitude, frequency, duration, ease);

            if (duration > 0f)
            {
                yield return WaitForDialogueDuration(duration);
            }

            Stop();
        }

        private static IEnumerator WaitForDialogueDuration(float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Mathf.Max(DialogueTime.deltaTime, 0f);
                yield return null;
            }
        }
    }
}