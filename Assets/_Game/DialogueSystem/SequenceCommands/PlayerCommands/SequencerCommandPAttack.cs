using System.Collections;
using UnityEngine;

namespace PixelCrushers.DialogueSystem.SequencerCommands
{
    [AddComponentMenu("")]
    public class SequencerCommandPAttack : SequencerCommandPlayerBase
    {
        private IEnumerator Start()
        {
            bool left = ParseLeftRight(GetParameter(0, "right"), false);

            bool success = PlayerSequenceForce.ForceAttack(left);

            // Optional override: PAttack(left, 0.25)
            float waitDuration = Mathf.Max(0f, GetParameterAsFloat(1, 0f));

            if (waitDuration <= 0f)
            {
                // Default to player attack animation time so chained attacks are visible.
                if (PlayerAttack.instance != null)
                {
                    waitDuration = Mathf.Max(0.01f, PlayerAttack.instance.attackAnimationTime);
                }
                else
                {
                    waitDuration = 0.2f;
                }
            }

            if (success)
            {
                yield return WaitForDialogueSeconds(waitDuration);
            }

            Stop();
        }
    }
}