using UnityEngine;

namespace PixelCrushers.DialogueSystem.SequencerCommands
{
    [AddComponentMenu("")]
    public class SequencerCommandPStop : SequencerCommandPlayerBase
    {
        private void Start()
        {
            var player = PlayerControl.instance;
            if (player != null)
            {
                player.SetIsRunningToTarget(false);
                player.StopMovement();
            }

            Stop();
        }
    }
}