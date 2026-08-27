using System.Collections;
using UnityEngine;

namespace PixelCrushers.DialogueSystem.SequencerCommands
{
    [AddComponentMenu("")]
    public class SequencerCommandPTeleportTo : SequencerCommandPlayerBase
    {
        private IEnumerator Start()
        {
            string p0 = GetParameter(0, string.Empty);
            string p1 = GetParameter(1, string.Empty);

            bool wait = ParseWait(GetParameter(2, "wait"), true);

            Vector3 target;
            if (TryParseFloat(p0, out float x) && TryParseFloat(p1, out float y))
            {
                target = new Vector3(x, y, 0f);
            }
            else
            {
                Transform t = GetSubject(0, null);
                if (t == null)
                {
                    Stop();
                    yield break;
                }

                target = t.position;
            }

            if (PlayerSequenceForce.ForceTeleportTo(target) && wait)
            {
                float fallback = PlayerControl.instance != null ? (PlayerControl.instance.TeleportDuration + 0.35f) : 1f;
                yield return PlayerSequenceForce.WaitTeleportDone(fallback);
            }

            Stop();
        }
    }
}