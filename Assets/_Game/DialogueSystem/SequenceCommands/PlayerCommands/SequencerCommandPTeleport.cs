using System.Collections;
using UnityEngine;

namespace PixelCrushers.DialogueSystem.SequencerCommands
{
    [AddComponentMenu("")]
    public class SequencerCommandPTeleport : SequencerCommandPlayerBase
    {
        private IEnumerator Start()
        {
            bool wait = ParseWait(GetParameter(0, "wait"), true);
            if (PlayerSequenceForce.ForceTeleportForward() && wait)
            {
                float fallback = PlayerControl.instance != null ? (PlayerControl.instance.TeleportDuration + 0.35f) : 1f;
                yield return PlayerSequenceForce.WaitTeleportDone(fallback);
            }

            Stop();
        }
    }
}