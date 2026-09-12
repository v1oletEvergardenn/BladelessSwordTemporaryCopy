using UnityEngine;

namespace PixelCrushers.DialogueSystem.SequencerCommands
{
    [AddComponentMenu("")]
    public class SequencerCommandPHS : SequencerCommandPlayerBase
    {
        private void Start()
        {
            bool left = ParseLeftRight(GetParameter(0, "right"), false);
            string hsToken = GetParameter(1, HSEnum.HSCounterAttack.ToString());

            if (!PlayerSequenceForce.TryParseHSEnum(hsToken, out HSEnum hsEnum))
            {
                Debug.LogWarning($"[SequencerCommandPHS] Invalid HSEnum '{hsToken}'.");
                Stop();
                return;
            }

            PlayerSequenceForce.ForceHS(hsEnum, left);
            Stop();
        }
    }
}