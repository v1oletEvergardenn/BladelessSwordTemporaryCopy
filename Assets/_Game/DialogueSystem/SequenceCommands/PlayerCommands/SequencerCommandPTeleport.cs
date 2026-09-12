using UnityEngine;

namespace PixelCrushers.DialogueSystem.SequencerCommands
{
    [AddComponentMenu("")]
    public class SequencerCommandPTeleport : SequencerCommandPlayerBase
    {
        private void Start()
        {
            PlayerSequenceForce.ForceTeleportForward();
            Stop();
        }
    }
}