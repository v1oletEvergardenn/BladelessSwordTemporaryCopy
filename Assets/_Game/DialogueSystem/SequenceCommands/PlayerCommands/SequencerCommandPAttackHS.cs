using System.Collections;
using UnityEngine;

namespace PixelCrushers.DialogueSystem.SequencerCommands
{
    [AddComponentMenu("")]
    public class SequencerCommandPAttackHS : SequencerCommandPlayerBase
    {
        private IEnumerator Start()
        {
            bool left = ParseLeftRight(GetParameter(0, "right"), false);

            bool success = PlayerSequenceForce.ForceAttackHS(left);

            // Optional override: PAttackHS(left, 0.3)
            float waitDuration = Mathf.Max(0f, GetParameterAsFloat(1, 0f));

            if (waitDuration <= 0f)
            {
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