using System.Collections;
using PixelCrushers.DialogueSystem.SequencerCommands;
using UnityEngine;

namespace PixelCrushers.DialogueSystem.SequencerCommands
{
    [AddComponentMenu("")]
    public class SequencerCommandCamPop : SequencerCommand
    {
        private IEnumerator Start()
        {
            float duration = Mathf.Max(0f, GetParameterAsFloat(0, 1f));
            CameraManager.CameraEase ease = CameraManager.ParseEase(GetParameter(1, "InOutSine"), CameraManager.CameraEase.InOutSine);

            string cutToken = GetParameter(2, string.Empty);
            bool cut = IsCutToken(cutToken);

            CameraManager.PopCutsceneState(duration, ease, cut);

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