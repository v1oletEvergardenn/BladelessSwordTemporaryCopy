using UnityEngine;
using UnityEngine.Playables;

namespace PixelCrushers.DialogueSystem.SequencerCommands
{
    [AddComponentMenu("")]
    public class SequencerCommandPlayTimeLine : SequencerCommand
    {
        private void Start()
        {
            TimelineManager manager = TimelineManager.instance;
            if (manager == null)
                manager = Object.FindObjectOfType<TimelineManager>();

            if (manager == null)
            {
                Stop();
                return;
            }

            string timelineName = GetParameter(0, string.Empty);
            if (string.IsNullOrWhiteSpace(timelineName))
            {
                if (DialogueDebug.LogWarnings)
                    Debug.LogWarning("Dialogue System: PlayTimeLine() requires a timeline/director name.");
                Stop();
                return;
            }

            PlayableDirector director = FindDirector(timelineName);
            if (director == null)
            {
                if (DialogueDebug.LogWarnings)
                    Debug.LogWarning("Dialogue System: PlayTimeLine(" + timelineName + "): PlayableDirector not found.");
                Stop();
                return;
            }

            manager.PlayTimeline(director);
            Stop();
        }

        private static PlayableDirector FindDirector(string timelineName)
        {
            GameObject go = Tools.GameObjectHardFind(timelineName);
            if (go != null)
            {
                PlayableDirector byObject = go.GetComponent<PlayableDirector>();
                if (byObject != null)
                    return byObject;
            }

            PlayableDirector[] directors = Object.FindObjectsOfType<PlayableDirector>();
            for (int i = 0; i < directors.Length; i++)
            {
                PlayableDirector d = directors[i];
                if (d == null)
                    continue;

                if (string.Equals(d.name, timelineName, System.StringComparison.OrdinalIgnoreCase))
                    return d;

                if (d.playableAsset != null &&
                    string.Equals(d.playableAsset.name, timelineName, System.StringComparison.OrdinalIgnoreCase))
                    return d;
            }

            return null;
        }
    }
}