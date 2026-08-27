using System.Collections;
using PixelCrushers.DialogueSystem.SequencerCommands;
using UnityEngine;

namespace PixelCrushers.DialogueSystem.SequencerCommands
{
    [AddComponentMenu("")]
    public class SequencerCommandCamZoom : SequencerCommand
    {
        private IEnumerator Start()
        {
            if (CameraManager.instance == null)
            {
                Debug.LogWarning("[SequencerCommandCamZoom] CameraManager instance not found.");
                Stop();
                yield break;
            }

            float value = GetParameterAsFloat(0, 0f);
            float duration = Mathf.Max(0f, GetParameterAsFloat(1, 1f));
            CameraManager.CameraEase ease = CameraManager.ParseEase(GetParameter(2, "InOutSine"), CameraManager.CameraEase.InOutSine);

            string mode = GetParameter(3, "absolute");
            bool relative = IsRelativeMode(mode);

            CameraManager.EnsureSkipSafeSnapshot();
            CameraManager.ZoomCurrentCamera(value, duration, ease, relative);

            if (duration > 0f)
            {
                yield return WaitForDialogueDuration(duration);
            }

            Stop();
        }

        private static bool IsRelativeMode(string mode)
        {
            if (string.IsNullOrWhiteSpace(mode))
            {
                return false;
            }

            mode = mode.Trim().ToLowerInvariant();
            return mode == "relative" || mode == "delta" || mode == "add";
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