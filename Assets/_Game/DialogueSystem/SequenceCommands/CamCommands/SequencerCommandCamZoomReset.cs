using System.Collections;
using PixelCrushers.DialogueSystem.SequencerCommands;
using UnityEngine;

namespace PixelCrushers.DialogueSystem.SequencerCommands
{
    [AddComponentMenu("")]
    public class SequencerCommandCamZoomReset : SequencerCommand
    {
        private IEnumerator Start()
        {
            if (CameraManager.instance == null)
            {
                Debug.LogWarning("[SequencerCommandCamZoomReset] CameraManager instance not found.");
                Stop();
                yield break;
            }

            float duration = Mathf.Max(0f, GetParameterAsFloat(0, 1f));
            CameraManager.CameraEase ease = CameraManager.ParseEase(GetParameter(1, "InOutSine"), CameraManager.CameraEase.InOutSine);

            CameraManager.ResetCurrentCameraZoom(duration, ease);

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