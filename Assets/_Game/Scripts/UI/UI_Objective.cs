using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UI_Objective : MonoBehaviour
{
    public TextMeshProUGUI text;
    public Image tickBox;
    public Sprite completedTickBoxSprite;
    public Sprite uncompletedTickBoxSprite;
    public QuestObjectives linkedObjective;

    public void CompleteObjective(Color color)
    {
        text.color = color;
        tickBox.color = color;
        tickBox.sprite = completedTickBoxSprite;
    }

    public void UpdateUI()
    {
        text.text = linkedObjective.objectiveDescription +
            " (" + linkedObjective.currentAmount + "/" +
            linkedObjective.requiredAmount + ")";

        // Reset state first (important for pooled reuse)
        text.color = Color.white;
        tickBox.color = Color.white;
        tickBox.sprite = uncompletedTickBoxSprite;

        if (linkedObjective.isCompleted)
        {
            CompleteObjective(Color.green);
        }
    }
}