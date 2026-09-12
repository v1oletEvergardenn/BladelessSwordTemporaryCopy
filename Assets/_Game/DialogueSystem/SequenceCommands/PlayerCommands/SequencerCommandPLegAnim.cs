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
            if (!string.IsNullOrWhiteSpace(state))
            {
                player.legAnim.Play(state);
            }

            Stop();
        }
    }
}