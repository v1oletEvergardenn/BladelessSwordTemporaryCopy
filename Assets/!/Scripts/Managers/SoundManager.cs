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

    public AudioSource soundFX_source;
    public AudioSource music_source;

    public AudioClip clip_Qianxiao;
    public AudioClip clip_WaterBoss;
    public AudioClip clip_Forest;
    // Start is called before the first frame update

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
    }

    public static void PlaySound(string tag, float volume = 1)
    {
        if (!instance.soundDcitionary.ContainsKey(tag))
        {
            return;
        }
        instance.soundFX_source.PlayOneShot(instance.soundDcitionary[tag], volume);
    }

    public static void PlaySound(AudioClip clip, float volume = 1)
    {
        instance.soundFX_source.PlayOneShot(clip, volume);
    }

    public static void SwitchMusic(int music)
    {
        if (music == -1)
        {
            instance.music_source.Stop();
            return;
        }
        switch ((Music)music)
        {
            case Music.QianXiao:
                instance.music_source.clip = instance.clip_Qianxiao;
                break;

            case Music.WaterBoss:
                instance.music_source.clip = instance.clip_WaterBoss;
                break;

            case Music.Forest:
                instance.music_source.clip = instance.clip_Forest;
                break;
        }
        instance.music_source.Play();
    }
}