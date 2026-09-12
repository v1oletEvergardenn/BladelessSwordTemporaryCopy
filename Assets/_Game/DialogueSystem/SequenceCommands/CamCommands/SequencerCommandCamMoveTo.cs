using System.Collections;
using System.Globalization;
using UnityEngine;
using Cinemachine;

namespace PixelCrushers.DialogueSystem.SequencerCommands
{
    [AddComponentMenu("")]
    public class SequencerCommandCamMoveTo : SequencerCommand
    {
        private const float ArriveThreshold = 0.01f;

        private static Transform _moveAnchor;

        private IEnumerator Start()
        {
            if (CameraManager.instance == null || CameraManager.instance.activeCamera == null)
            {
                Debug.LogWarning("[SequencerCommandCamMoveTo] CameraManager or active camera not found.");
                Stop();
                yield break;
            }

            string p0 = GetParameter(0, "0");
            string p1 = GetParameter(1, "1");
            string p2 = GetParameter(2, "relative");

            if (IsResetToken(p0))
            {
                float delay = Mathf.Max(0f, GetParameterAsFloat(1, 0f));
                CameraManager.ResetFollowTarget(delay);

                if (delay > 0f)
                    yield return WaitForDialogueSeconds(delay);

                Stop();
                yield break;
            }

            if (!EnsureMoveAnchorBound(true))
            {
                Stop();
                yield break;
            }

            float duration;
            string spaceToken;
            ParseDurationAndSpace(p1, p2, out duration, out spaceToken);

            Vector3 target = ResolveTargetPosition(p0, spaceToken);

            yield return MoveAnchorTo(target, duration);

            Stop();
        }

        private static bool IsResetToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return false;

            token = token.Trim().ToLowerInvariant();
            return token == "reset" || token == "default" || token == "back";
        }

        private static void ParseDurationAndSpace(string p1, string p2, out float duration, out string spaceToken)
        {
            duration = 1f;
            spaceToken = "relative";

            if (TryParseFloat(p1, out float d1))
            {
                duration = Mathf.Max(0f, d1);
                spaceToken = string.IsNullOrWhiteSpace(p2) ? "relative" : p2;
                return;
            }

            if (!string.IsNullOrWhiteSpace(p1))
                spaceToken = p1;

            if (TryParseFloat(p2, out float d2))
                duration = Mathf.Max(0f, d2);
        }

        private Vector3 ResolveTargetPosition(string targetToken, string spaceToken)
        {
            if (TryParseFloat(targetToken, out float x))
            {
                bool relative = ParseRelative(spaceToken);
                float targetX = relative ? _moveAnchor.position.x + x : x;
                return new Vector3(targetX, _moveAnchor.position.y, _moveAnchor.position.z);
            }

            Transform target = GetSubject(0, null);

            if (target == null && !string.IsNullOrWhiteSpace(targetToken))
            {
                GameObject go = GameObject.Find(targetToken);
                if (go != null)
                    target = go.transform;
            }

            if (target == null)
            {
                Debug.LogWarning($"[SequencerCommandCamMoveTo] Target '{targetToken}' not found.");
                return _moveAnchor.position;
            }

            return target.position;
        }

        private IEnumerator MoveAnchorTo(Vector3 target, float duration)
        {
            if (_moveAnchor == null)
                yield break;

            Vector3 start = _moveAnchor.position;

            if (duration <= 0f)
            {
                _moveAnchor.position = target;
                yield break;
            }

            float elapsed = 0f;
            while (_moveAnchor != null && elapsed < duration)
            {
                elapsed += Mathf.Max(DialogueTime.deltaTime, 0f);
                float t = Mathf.Clamp01(elapsed / duration);
                _moveAnchor.position = Vector3.Lerp(start, target, t);

                if (Vector3.Distance(_moveAnchor.position, target) <= ArriveThreshold)
                    break;

                yield return null;
            }

            if (_moveAnchor != null)
                _moveAnchor.position = target;
        }

        private static bool EnsureMoveAnchorBound(bool initializeFromCurrentFollow)
        {
            if (CameraManager.instance == null || CameraManager.instance.activeCamera == null)
                return false;

            CinemachineVirtualCamera active = CameraManager.instance.activeCamera;

            if (_moveAnchor == null)
            {
                GameObject go = new GameObject("DialogueCamMoveAnchor");
                go.hideFlags = HideFlags.HideAndDontSave;
                _moveAnchor = go.transform;
                initializeFromCurrentFollow = true;
            }

            if (initializeFromCurrentFollow)
            {
                if (active.Follow != null)
                    _moveAnchor.position = active.Follow.position;
                else
                    _moveAnchor.position = active.transform.position;
            }

            if (active.Follow != _moveAnchor)
            {
                CameraManager.EnsureSkipSafeSnapshot();
                CameraManager.SetTemporaryFollowTarget(_moveAnchor, 0f);
            }

            return true;
        }

        private static bool ParseRelative(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return true;

            token = token.Trim().ToLowerInvariant();
            if (token == "set" || token == "absolute")
                return false;

            return true;
        }

        private static bool TryParseFloat(string value, out float number)
        {
            return float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out number);
        }

        private static IEnumerator WaitForDialogueSeconds(float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Mathf.Max(DialogueTime.deltaTime, 0f);
                yield return null;
            }
        }
    }
}