using UnityEngine;

namespace PixelCrushers.DialogueSystem.SequencerCommands
{
    [AddComponentMenu("")]
    public class SequencerCommandPTeleportTo : SequencerCommandPlayerBase
    {
        private void Start()
        {
            string p0 = GetParameter(0, string.Empty);
            string p1 = GetParameter(1, string.Empty);

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
                    return;
                }

                target = t.position;
            }

            PlayerSequenceForce.ForceTeleportTo(target);
            Stop();
        }
    }
}