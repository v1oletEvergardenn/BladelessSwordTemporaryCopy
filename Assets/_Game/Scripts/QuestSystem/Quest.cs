using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;

public class Quest : MonoBehaviour
{
    public string questID;
    public string questName;
    [TextArea(2, 5)] public string questDescription;
    public List<QuestObjectives> objectives;
    public UnityEvent onQuestComplete;

    public void StartQuest()
    {
        QuestManager.StartQuest(this);
    }

    public void OnAction(string actionID)
    {
        QuestManager.OnAction(actionID);
    }
}

[System.Serializable]
public class QuestObjectives
{
    [GUIColor(nameof(GetObjectiveDescriptionColor))]
    public string objectiveDescription;

    public QuestActionKey objectiveKey;

    [SerializeField, ObjectiveKeyActionDropdown(nameof(objectiveKey))]
    public string objectiveAction;

    [HideInInspector] public string objectiveID;

    public int requiredAmount;

    [HideInInspector] public int currentAmount;
    public bool fromZero = true;
    public int layer = 0;

    public UnityEvent onObjectiveBegin;
    public UnityEvent onObjectiveComplete;

    [HideInInspector] public bool hasBegun;
    [HideInInspector] public bool isCompleted => currentAmount >= requiredAmount;

    private Color GetObjectiveDescriptionColor()
    {
        return (layer & 1) == 0
            ? new Color(0.58f, 0.70f, 1.00f, 1.00f)
            : new Color(1.00f, 0.68f, 0.58f, 1.00f);
    }

    public void SetObjectiveID()
    {
        if (objectiveKey == null || string.IsNullOrEmpty(objectiveAction))
        {
            objectiveID = string.Empty;
            return;
        }

        // If already composed (AssetName.action), keep as-is
        objectiveID = objectiveAction.Contains(".")
            ? objectiveAction
            : objectiveKey.ActionID(objectiveAction);
    }
}

[System.Serializable]
public class QuestProgress
{
    public Quest quest;
    public List<QuestObjectives> objectives;

    public QuestProgress(Quest quest)
    {
        this.quest = quest;
        objectives = new List<QuestObjectives>();

        foreach (var obj in quest.objectives)
        {
            var newObjective = new QuestObjectives
            {
                objectiveDescription = obj.objectiveDescription,
                objectiveKey = obj.objectiveKey,
                objectiveAction = obj.objectiveAction,
                requiredAmount = obj.requiredAmount,
                currentAmount = 0,
                fromZero = obj.fromZero,
                layer = obj.layer,
                hasBegun = false
            };

            objectives.Add(newObjective);
            newObjective.SetObjectiveID();
        }
    }

    public int CurrentLayer
    {
        get
        {
            int minIncompleteLayer = int.MaxValue;
            foreach (var obj in objectives)
            {
                if (!obj.isCompleted && obj.layer < minIncompleteLayer)
                    minIncompleteLayer = obj.layer;
            }
            return minIncompleteLayer == int.MaxValue ? -1 : minIncompleteLayer;
        }
    }

    public IEnumerable<QuestObjectives> ActiveObjectives
    {
        get
        {
            int layer = CurrentLayer;
            return objectives.FindAll(o => o.layer == layer);
        }
    }

    public bool IsCompleted => objectives.TrueForAll(o => o.isCompleted);
    public string QuestID => quest.questID;
}