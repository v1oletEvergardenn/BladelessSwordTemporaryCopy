using UnityEngine;
using PixelCrushers.DialogueSystem;

public class ActorTypewriterSubtitlePanel : StandardUISubtitlePanel
{
    [SerializeField] private AudioClip defaultClip;
    [SerializeField] private AudioClip[] defaultAlternateClips;

    public override void SetContent(Subtitle subtitle)
    {
        var typewriter = GetTypewriter();
        if (typewriter != null)
        {
            typewriter.audioClip = defaultClip;
            typewriter.alternateAudioClips = defaultAlternateClips;

            var speaker = subtitle != null ? subtitle.speakerInfo.transform : null;
            if (speaker != null && speaker.TryGetComponent<ActorTypewriterVoice>(out var voice))
            {
                typewriter.audioClip = voice.PrimaryClip != null ? voice.PrimaryClip : defaultClip;
                typewriter.alternateAudioClips = voice.AlternateClips != null ? voice.AlternateClips : defaultAlternateClips;
            }
        }

        base.SetContent(subtitle);
    }
}