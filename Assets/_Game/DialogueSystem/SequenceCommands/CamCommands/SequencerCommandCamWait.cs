using System.Collections;
using PixelCrushers.DialogueSystem.SequencerCommands;
using UnityEngine;

namespace PixelCrushers.DialogueSystem.SequencerCommands
{
    [AddComponentMenu("")]
    public class SequencerCommandCamWait : SequencerCommand
    {
        private IEnumerator Start()
        {
            float duration = Mathf.Max(0f, GetParameterAsFloat(0, 1f));

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Mathf.Max(DialogueTime.deltaTime, 0f);
                yield return null;
            }

            Stop();
        }
    }
}