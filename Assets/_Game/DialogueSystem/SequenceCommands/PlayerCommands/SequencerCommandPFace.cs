using UnityEngine;

namespace PixelCrushers.DialogueSystem.SequencerCommands
{
    [AddComponentMenu("")]
    public class SequencerCommandPFace : SequencerCommandPlayerBase
    {
        private void Start()
        {
            var player = PlayerControl.instance;
            if (player == null)
            {
                Stop();
                return;
            }

            bool faceLeft = ParseLeftRight(GetParameter(0, "right"), false);
            player.Face(!faceLeft);

            Stop();
        }
    }
}