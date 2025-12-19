using System;
using UnityEngine;
using EditorAttributes;

public class Emoji : MonoBehaviour
{
    public Animator emojiAnim;
    public EmotionType emoji;
    [SerializeField, ButtonField("PlayEmoji", "PlayEmoji")] public EditorAttributes.Void Void1;

    public static Emoji instance;

    public void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public void PlayEmotion(EmotionType emotionType)
    {
        string enumName = GetEnumName<EmotionType>((int)emotionType);
        emojiAnim.Play(enumName, 0, 0f);
    }

    public string GetEnumName<T>(int value)
    {
        string name = "";
        name = Enum.Parse(typeof(T), Enum.GetName(typeof(T), value)).ToString();
        return name;
    }

    private void PlayEmoji()
    {
        PlayEmotion(emoji);
    }
}