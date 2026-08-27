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

            string token = GetParameter(0, "right");
            string normalized = token.Trim().ToLowerInvariant();

            if (normalized == "left")
            {
                player.Face(false);
            }
            else if (normalized == "right")
            {
                player.Face(true);
            }
            else
            {
                Transform t = GetSubject(0, null);
                if (t != null) player.FaceTarget(t);
            }

            Stop();
        }
    }
}