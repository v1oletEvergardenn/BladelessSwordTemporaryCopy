using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

/// <summary>
/// Mixer for player action track.
/// Also auto-binds SignalReceiver from player instance when missing.
/// </summary>
public class PlayerActionMixerBehaviour : PlayableBehaviour
{
    private TrackAsset _track;
    private bool _resolvedBinding;

    public void Initialize(TrackAsset track)
    {
        _track = track;
        _resolvedBinding = false;
    }

    public override void OnGraphStart(Playable playable)
    {
        TryAutoBind(playable);
    }

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        if (_resolvedBinding)
            return;

        if (playerData is SignalReceiver)
        {
            _resolvedBinding = true;
            return;
        }

        TryAutoBind(playable);
    }

    private void TryAutoBind(Playable playable)
    {
        if (_track == null)
            return;

        PlayableDirector director = playable.GetGraph().GetResolver() as PlayableDirector;
        if (director == null)
            return;

        Object current = director.GetGenericBinding(_track);
        if (current is SignalReceiver)
        {
            _resolvedBinding = true;
            return;
        }

        PlayerControl player = PlayerControl.instance;
        if (player == null)
            player = Object.FindObjectOfType<PlayerControl>();

        if (player == null)
            return;

        SignalReceiver receiver = player.GetComponent<SignalReceiver>();
        if (receiver == null)
            return;

        director.SetGenericBinding(_track, receiver);
        _resolvedBinding = true;
    }
}