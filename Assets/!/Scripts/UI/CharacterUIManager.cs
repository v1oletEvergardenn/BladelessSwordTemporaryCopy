using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterUIManager : MonoBehaviour
{
    public static CharacterUIManager instance;

    public Image TopBlackEdge;
    public Image BotBlackEdge;

    private void Awake()
    {
        instance = this;
    }

    public static void ShowBlackEdge(bool show)
    {
        instance.TopBlackEdge.GetComponent<Animator>().SetBool("show", show);
        instance.BotBlackEdge.GetComponent<Animator>().SetBool("show", show);
    }
}