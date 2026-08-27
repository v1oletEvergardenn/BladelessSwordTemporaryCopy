using UnityEngine;

namespace PixelCrushers.DialogueSystem.SequencerCommands
{
    [AddComponentMenu("")]
    public class SequencerCommandPLegAnim : SequencerCommandPlayerBase
    {
        private void Start()
        {
            var player = PlayerControl.instance;
            if (player == null || player.legAnim == null)
            {
                Stop();
                return;
            }

            string state = GetParameter(0, string.Empty);
            float normalizedTime = Mathf.Clamp01(GetParameterAsFloat(1, 0f));

            if (!string.IsNullOrWhiteSpace(state))
            {
                player.legAnim.Play(state, 0, normalizedTime);
            }

            Stop();
        }
    }
}