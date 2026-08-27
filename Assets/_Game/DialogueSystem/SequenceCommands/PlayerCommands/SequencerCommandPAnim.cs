using UnityEngine;

namespace PixelCrushers.DialogueSystem.SequencerCommands
{
    [AddComponentMenu("")]
    public class SequencerCommandPAnim : SequencerCommandPlayerBase
    {
        private void Start()
        {
            var player = PlayerControl.instance;
            if (player == null || player.anim == null)
            {
                Stop();
                return;
            }

            string state = GetParameter(0, string.Empty);
            float normalizedTime = Mathf.Clamp01(GetParameterAsFloat(1, 0f));

            if (!string.IsNullOrWhiteSpace(state))
            {
                player.anim.Play(state, 0, normalizedTime);
            }

            Stop();
        }
    }
}