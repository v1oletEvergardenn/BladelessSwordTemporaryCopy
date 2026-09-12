using UnityEngine;
using UnityEngine.Playables;

[DisallowMultipleComponent]
public class TimelineManager : MonoBehaviour
{
    public static TimelineManager instance;

    [SerializeField] private PlayableDirector _currentTimeline;

    public PlayableDirector CurrentTimeline => _currentTimeline;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            return;
        }

        if (instance != this)
            Destroy(gameObject);
    }

    public void SetCurrentTimeline(PlayableDirector director)
    {
        if (director == null)
            return;

        _currentTimeline = director;
    }

    public void PlayTimeline(PlayableDirector director)
    {
        if (director == null)
            return;

        SetCurrentTimeline(director);
        director.Play();
    }

    public void PauseCurrentTimeline()
    {
        if (_currentTimeline == null)
            return;

        if (_currentTimeline.state == PlayState.Playing)
            _currentTimeline.Pause();
    }

    public void ResumeCurrentTimeline()
    {
        if (_currentTimeline == null)
            return;

        if (_currentTimeline.state != PlayState.Playing)
            _currentTimeline.Resume();
    }
}