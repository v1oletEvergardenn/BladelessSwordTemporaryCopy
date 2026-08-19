using UnityEngine;

public class ActorTypewriterVoice : MonoBehaviour
{
    [SerializeField] private AudioClip primaryClip;
    [SerializeField] private AudioClip[] alternateClips;

    public AudioClip PrimaryClip => primaryClip;
    public AudioClip[] AlternateClips => alternateClips;
}