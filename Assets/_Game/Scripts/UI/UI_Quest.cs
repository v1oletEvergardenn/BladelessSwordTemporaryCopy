using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI_Quest : MonoBehaviour
{
    public Color completeColor;
    public QuestProgress linkedQuest;

    public void UpdateUI(QuestProgress quest)
    {
        linkedQuest = quest;
        GetComponent<TextMeshProUGUI>().text = linkedQuest.quest.questDescription;
        GetComponent<TextMeshProUGUI>().color = linkedQuest.IsCompleted ? completeColor : Color.white;
    }
}