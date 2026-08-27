using System.Collections;
using PixelCrushers.DialogueSystem.SequencerCommands;
using UnityEngine;

namespace PixelCrushers.DialogueSystem.SequencerCommands
{
    [AddComponentMenu("")]
    public class SequencerCommandCamOffset : SequencerCommand
    {
        private IEnumerator Start()
        {
            if (CameraManager.instance == null)
            {
                Debug.LogWarning("[SequencerCommandCamOffset] CameraManager instance not found.");
                Stop();
                yield break;
            }

            string p0 = GetParameter(0, "0");

            if (IsResetToken(p0))
            {
                float resetDuration = Mathf.Max(0f, GetParameterAsFloat(1, 1f));
                CameraManager.CameraEase resetEase = CameraManager.ParseEase(GetParameter(2, "InOutSine"), CameraManager.CameraEase.InOutSine);
                CameraManager.ResetPanOffset(resetDuration, resetEase);

                if (resetDuration > 0f)
                {
                    yield return WaitForDialogueDuration(resetDuration);
                }

                Stop();
                yield break;
            }

            float x = GetParameterAsFloat(0, 0f);
            float y = GetParameterAsFloat(1, 0f);
            float duration = Mathf.Max(0f, GetParameterAsFloat(2, 1f));
            bool relative = IsRelativeMode(GetParameter(3, "relative"));
            CameraManager.CameraEase ease = CameraManager.ParseEase(GetParameter(4, "InOutSine"), CameraManager.CameraEase.InOutSine);

            CameraManager.EnsureSkipSafeSnapshot();
            CameraManager.TweenPanOffset(x, y, duration, relative, ease);

            if (duration > 0f)
            {
                yield return WaitForDialogueDuration(duration);
            }

            Stop();
        }

        private static bool IsResetToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return false;
            }

            token = token.Trim().ToLowerInvariant();
            return token == "reset" || token == "default" || token == "back";
        }

        private static bool IsRelativeMode(string mode)
        {
            if (string.IsNullOrWhiteSpace(mode))
            {
                return true;
            }

            mode = mode.Trim().ToLowerInvariant();

            if (mode == "absolute" || mode == "set")
            {
                return false;
            }

            if (mode == "relative" || mode == "delta" || mode == "add")
            {
                return true;
            }

            return true;
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

        private void OnDisable()
        {
            if (isPlaying)
            {
                CameraManager.CancelPanOffsetTween();
            }
        }
    }
}