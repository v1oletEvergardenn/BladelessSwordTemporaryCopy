using UnityEngine;

namespace PixelCrushers.DialogueSystem.SequencerCommands
{
    [AddComponentMenu("")]
    public class SequencerCommandPClearInput : SequencerCommandPlayerBase
    {
        private void Start()
        {
            if (InputPlayer.instance != null)
            {
                InputPlayer.instance.DisableAllActions();
            }

            Stop();
        }
    }
}