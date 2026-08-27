using PixelCrushers.DialogueSystem.SequencerCommands;
using UnityEngine;

namespace PixelCrushers.DialogueSystem.SequencerCommands
{
    [AddComponentMenu("")]
    public class SequencerCommandCamPush : SequencerCommand
    {
        private void Start()
        {
            CameraManager.PushCutsceneState();
            Stop();
        }
    }
}