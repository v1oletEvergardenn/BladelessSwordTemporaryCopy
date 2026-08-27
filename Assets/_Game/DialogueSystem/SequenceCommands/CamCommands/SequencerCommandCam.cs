using System.Collections;
using Cinemachine;
using PixelCrushers.DialogueSystem.SequencerCommands;
using UnityEngine;

namespace PixelCrushers.DialogueSystem.SequencerCommands
{
    [AddComponentMenu("")]
    public class SequencerCommandCam : SequencerCommand
    {
        private IEnumerator Start()
        {
            if (CameraManager.instance == null)
            {
                Debug.LogWarning("[SequencerCommandCam] CameraManager instance not found.");
                Stop();
                yield break;
            }

            string target = GetParameter(0, "player");
            float duration = Mathf.Max(0f, GetParameterAsFloat(1, 1f));

            string p2 = GetParameter(2, "InOutSine");
            string p3 = GetParameter(3, string.Empty);

            bool cut = IsCutToken(p2) || IsCutToken(p3);
            string easeToken = IsCutToken(p2) ? p3 : p2;
            CameraManager.CameraEase ease = CameraManager.ParseEase(easeToken, CameraManager.CameraEase.InOutSine);

            CameraManager.EnsureSkipSafeSnapshot();

            if (IsPlayerToken(target))
            {
                CameraManager.SwitchToPlayerCamera(duration, ease, cut);
            }
            else if (IsBackToken(target))
            {
                CameraManager.SwitchToPreviousCamera(duration, ease, cut);
            }
            else
            {
                if (!CameraManager.TryGetCamera(target, out CinemachineVirtualCamera cam))
                {
                    Debug.LogWarning($"[SequencerCommandCam] Camera not found for '{target}'.");
                    Stop();
                    yield break;
                }

                CameraManager.SwitchCamera(cam, duration, ease, cut);
            }

            if (duration > 0f && !cut)
            {
                yield return WaitForDialogueDuration(duration);
            }

            Stop();
        }

        private static bool IsCutToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return false;
            }

            token = token.Trim().ToLowerInvariant();
            return token == "cut" || token == "instant" || token == "now";
        }

        private static bool IsPlayerToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return false;
            }

            token = token.Trim().ToLowerInvariant();
            return token == "player" || token == "normal";
        }

        private static bool IsBackToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return false;
            }

            token = token.Trim().ToLowerInvariant();
            return token == "back" || token == "previous" || token == "last";
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