using System.Collections;
using UnityEngine;

namespace PixelCrushers.DialogueSystem.SequencerCommands
{
    [AddComponentMenu("")]
    public class SequencerCommandPMoveTo : SequencerCommandPlayerBase
    {
        private const float ArriveThreshold = 0.05f;

        private IEnumerator Start()
        {
            var player = PlayerControl.instance;
            if (player == null)
            {
                Stop();
                yield break;
            }

            ActionLock.ClearAll();

            string targetToken = GetParameter(0, "0");
            string faceToken = GetParameter(1, "keep");
            string modeToken = GetParameter(2, "run");
            string spaceToken = GetParameter(3, "relative");

            bool relative = ParseRelative(spaceToken);
            bool faceRight = ResolveFacing(player, faceToken);

            float targetX;

            // PMoveTo(x, ...)
            if (TryParseFloat(targetToken, out float x))
            {
                targetX = relative ? player.transform.position.x + x : x;
            }
            // PMoveTo(gameObjectName, ...)
            else
            {
                Transform targetTransform = GetSubject(0, null);

                if (targetTransform == null && !string.IsNullOrWhiteSpace(targetToken))
                {
                    GameObject go = GameObject.Find(targetToken);
                    if (go != null) targetTransform = go.transform;
                }

                if (targetTransform == null)
                {
                    Debug.LogWarning($"[SequencerCommandPMoveTo] Target '{targetToken}' not found.");
                    Stop();
                    yield break;
                }

                // Target object always means move to its world X.
                targetX = targetTransform.position.x;
            }

            Vector3 target = new Vector3(targetX, player.transform.position.y, player.transform.position.z);

            if (TryParseFloat(modeToken, out float customWalkSpeed))
            {
                if (PlayerTimeLineActions.instance != null)
                {
                    PlayerTimeLineActions.instance.MoveTo(target, Mathf.Max(0.01f, customWalkSpeed));
                }
                else
                {
                    player.SetRunning(false);
                    player.WalkToPosition(target, faceRight);
                }
            }
            else
            {
                string mode = string.IsNullOrWhiteSpace(modeToken) ? "run" : modeToken.Trim().ToLowerInvariant();
                if (mode == "walk")
                {
                    player.SetRunning(false);
                    player.WalkToPosition(target, faceRight);
                }
                else
                {
                    player.SetRunning(true);
                    player.RunToPosition(target, faceRight);
                }
            }

            // Always wait until arrival.
            yield return new WaitUntil(() =>
                player == null || Mathf.Abs(player.transform.position.x - targetX) <= ArriveThreshold);

            if (player != null)
            {
                player.SetIsRunningToTarget(false);
                player.StopMovement();

                Vector3 p = player.transform.position;
                player.transform.position = new Vector3(targetX, p.y, p.z);
                player.Face(faceRight);
            }

            Stop();
        }

        private static bool ParseRelative(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return true;
            }

            token = token.Trim().ToLowerInvariant();
            if (token == "set" || token == "absolute")
            {
                return false;
            }

            return true;
        }

        private static bool ResolveFacing(PlayerControl player, string token)
        {
            if (player == null)
            {
                return true;
            }

            if (string.IsNullOrWhiteSpace(token))
            {
                return player.FacingRight;
            }

            token = token.Trim().ToLowerInvariant();
            if (token == "left") return false;
            if (token == "right") return true;

            return player.FacingRight;
        }
    }
}