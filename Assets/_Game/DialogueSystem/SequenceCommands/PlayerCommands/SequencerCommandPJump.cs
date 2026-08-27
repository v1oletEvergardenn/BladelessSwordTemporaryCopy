using UnityEngine;

namespace PixelCrushers.DialogueSystem.SequencerCommands
{
    [AddComponentMenu("")]
    public class SequencerCommandPJump : SequencerCommandPlayerBase
    {
        private void Start()
        {
            PlayerSequenceForce.ForceJump();
            Stop();
        }
    }
}