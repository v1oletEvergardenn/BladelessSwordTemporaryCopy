using System.Collections;
using PixelCrushers.DialogueSystem.SequencerCommands;
using UnityEngine;

namespace PixelCrushers.DialogueSystem.SequencerCommands
{
    [AddComponentMenu("")]
    public class SequencerCommandCamFollow : SequencerCommand
    {
        private IEnumerator Start()
        {
            if (CameraManager.instance == null)
            {
                Debug.LogWarning("[SequencerCommandCamFollow] CameraManager instance not found.");
                Stop();
                yield break;
            }

            string first = GetParameter(0, "speaker");
            float duration = Mathf.Max(0f, GetParameterAsFloat(1, 0f));

            if (IsResetToken(first))
            {
                CameraManager.ResetFollowTarget(duration);
                if (duration > 0f)
                {
                    yield return WaitForDialogueDuration(duration);
                }

                Stop();
                yield break;
            }

            Transform target = GetSubject(0, speaker);
            if (target == null)
            {
                Debug.LogWarning($"[SequencerCommandCamFollow] Target '{first}' not found.");
                Stop();
                yield break;
            }

            CameraManager.EnsureSkipSafeSnapshot();
            CameraManager.SetTemporaryFollowTarget(target, duration);

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