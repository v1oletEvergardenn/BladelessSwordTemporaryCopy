using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class SoundType
{
    public string tag;
    public AudioClip clip;
}

public enum Music
{
    QianXiao,
    WaterBoss,
    Forest
}

[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{
    [SerializeField] private SoundType[] soundList;
    public static SoundManager instance;
    private Dictionary<string, AudioClip> soundDcitionary;

    // Start is called before the first frame update

    [SerializeField] private int soundFXSourcePoolSize = 15;
    private AudioSource[] soundFXSources;
    private int soundFXSourceIndex = 0;

    private void Awake()
    {
        if (instance == null) { instance = this; }
    }

    private void Start()
    {
        soundDcitionary = new Dictionary<string, AudioClip>();

        foreach (SoundType sound in soundList)
        {
            soundDcitionary.Add(sound.tag, sound.clip);
        }

        soundFXSources = new AudioSource[soundFXSourcePoolSize];
        for (int i = 0; i < soundFXSourcePoolSize; i++)
        {
            var source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            soundFXSources[i] = source;
        }
    }

    public static void PlaySound(string tag, float volume = 1)
    {
        if (!instance.soundDcitionary.ContainsKey(tag))
        {
            return;
        }
        var source = instance.GetNextSoundFXSource();
        source.PlayOneShot(instance.soundDcitionary[tag], volume);
    }

    public static void PlaySound(AudioClip clip, float volume = 1)
    {
        var source = instance.GetNextSoundFXSource();
        source.PlayOneShot(clip, volume);
    }

    private AudioSource GetNextSoundFXSource()
    {
        soundFXSourceIndex = (soundFXSourceIndex + 1) % soundFXSources.Length;
        return soundFXSources[soundFXSourceIndex];
    }

    public static void SwitchMusic(int music)
    {
    }
}