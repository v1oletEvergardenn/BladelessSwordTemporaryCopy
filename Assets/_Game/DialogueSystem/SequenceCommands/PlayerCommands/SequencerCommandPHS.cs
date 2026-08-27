using UnityEngine;

namespace PixelCrushers.DialogueSystem.SequencerCommands
{
    [AddComponentMenu("")]
    public class SequencerCommandPHS : SequencerCommandPlayerBase
    {
        private void Start()
        {
            bool left = ParseLeftRight(GetParameter(0, "right"), false);
            string abilityName = GetParameter(1, string.Empty);
            PlayerSequenceForce.ForceHS(abilityName, left);
            Stop();
        }
    }
}