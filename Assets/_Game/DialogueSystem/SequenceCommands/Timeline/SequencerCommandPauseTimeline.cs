using UnityEngine;

namespace PixelCrushers.DialogueSystem.SequencerCommands
{
    [AddComponentMenu("")]
    public class SequencerCommandPauseTimeline : SequencerCommand
    {
        private void Start()
        {
            TimelineManager manager = TimelineManager.instance;
            if (manager == null)
                manager = Object.FindObjectOfType<TimelineManager>();

            if (manager != null)
                manager.PauseCurrentTimeline();

            Stop();
        }
    }
}