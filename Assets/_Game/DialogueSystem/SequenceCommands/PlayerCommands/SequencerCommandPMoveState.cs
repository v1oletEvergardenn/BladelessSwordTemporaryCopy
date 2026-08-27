using UnityEngine;

namespace PixelCrushers.DialogueSystem.SequencerCommands
{
    [AddComponentMenu("")]
    public class SequencerCommandPMoveState : SequencerCommandPlayerBase
    {
        private void Start()
        {
            var player = PlayerControl.instance;
            if (player == null)
            {
                Stop();
                return;
            }

            string token = GetParameter(0, "normal").Trim().ToLowerInvariant();
            switch (token)
            {
                case "normal":
                    player.SetMovementState(PlayerMovementStateType.Normal);
                    break;

                case "windwalking":
                case "wind":
                    player.SetMovementState(PlayerMovementStateType.WindWalking);
                    break;

                case "standingontemple":
                case "temple":
                    player.SetMovementState(PlayerMovementStateType.StandingOnTemple);
                    break;
            }

            Stop();
        }
    }
}