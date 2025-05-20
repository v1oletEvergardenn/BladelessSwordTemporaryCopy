using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;
using UnityEngine.UI;
using UnityEngine.Events;

[System.Serializable]
public class TutorialPage
{
    [HideInInspector] public string sentenceIndex;
    public Sprite image;
    [TextArea(3, 20)] public string context;
}

public class Tutorial : MonoBehaviour, ISelectHandler
{
    public TutorialManager tutorialManager;
    public bool isJumpOut = false;
    public string title;
    public List<TutorialPage> tutorialPage = new List<TutorialPage>();
    

    public void OnSelect(BaseEventData eventData)
    {
        if (!isJumpOut)
        {
            tutorialManager.UpdateInformation(title, tutorialPage);
        }
       
    }

    public void UpdateInformation()
    {
        if (isJumpOut)
        {
            tutorialManager.UpdateInformation(title, tutorialPage);
        }
    }



}