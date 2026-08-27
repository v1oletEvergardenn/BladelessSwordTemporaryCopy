using System.Collections;
using UnityEngine;

namespace PixelCrushers.DialogueSystem.SequencerCommands
{
    [AddComponentMenu("")]
    public abstract class SequencerCommandPlayerBase : SequencerCommand
    {
        protected static bool ParseLeftRight(string token, bool defaultLeft = false)
        {
            if (string.IsNullOrWhiteSpace(token)) return defaultLeft;
            token = token.Trim().ToLowerInvariant();
            if (token == "left" || token == "l") return true;
            if (token == "right" || token == "r") return false;
            return defaultLeft;
        }

        protected static bool ParseWait(string token, bool defaultWait = true)
        {
            if (string.IsNullOrWhiteSpace(token)) return defaultWait;
            token = token.Trim().ToLowerInvariant();
            if (token == "wait" || token == "true" || token == "1") return true;
            if (token == "nowait" || token == "false" || token == "0") return false;
            return defaultWait;
        }

        protected static bool ParseRunMode(string token, bool defaultRun = true)
        {
            if (string.IsNullOrWhiteSpace(token)) return defaultRun;
            token = token.Trim().ToLowerInvariant();
            if (token == "run") return true;
            if (token == "walk") return false;
            return defaultRun;
        }

        protected static bool TryParseFloat(string value, out float number)
        {
            return float.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out number);
        }

        protected static IEnumerator WaitForDialogueSeconds(float duration)
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