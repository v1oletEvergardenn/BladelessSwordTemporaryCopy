using UnityEngine;

namespace PixelCrushers.DialogueSystem.SequencerCommands
{
    [AddComponentMenu("")]
    public class SequencerCommandPDoubleJump : SequencerCommandPlayerBase
    {
        private void Start()
        {
            float hold = Mathf.Max(0f, GetParameterAsFloat(0, 0.2f));
            PlayerSequenceForce.ForceDoubleJump(hold);
            Stop();
        }
    }
}