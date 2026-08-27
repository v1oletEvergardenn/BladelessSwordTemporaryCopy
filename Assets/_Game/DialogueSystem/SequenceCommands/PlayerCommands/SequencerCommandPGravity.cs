using UnityEngine;

namespace PixelCrushers.DialogueSystem.SequencerCommands
{
    [AddComponentMenu("")]
    public class SequencerCommandPGravity : SequencerCommandPlayerBase
    {
        private void Start()
        {
            var player = PlayerControl.instance;
            if (player == null)
            {
                Stop();
                return;
            }

            string token = GetParameter(0, "on").Trim().ToLowerInvariant();
            bool enable = token == "on" || token == "true" || token == "1";
            player.EnableGravity(enable);

            Stop();
        }
    }
}