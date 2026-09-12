using PixelCrushers.DialogueSystem;
using UnityEngine;
using UnityEngine.Playables;

[DisallowMultipleComponent]
public class CutSceneTrigger : MonoBehaviour
{
    [SerializeField] private PlayableDirector _director;
    public DialogueSystemTrigger trigger;
    public bool oneShot = false;
    public bool triggered = false;

    private void Awake()
    {
        if (_director == null)
            _director = GetComponent<PlayableDirector>();
    }

    public void StartConversation()
    {
        if (oneShot)
        {
            if (triggered) return;
            else triggered = true;
        }

        if (trigger != null)
            trigger.OnUse();

        if (_director == null)
            return;

        if (TimelineManager.instance != null)
            TimelineManager.instance.PlayTimeline(_director);
        else
            _director.Play();
    }
}