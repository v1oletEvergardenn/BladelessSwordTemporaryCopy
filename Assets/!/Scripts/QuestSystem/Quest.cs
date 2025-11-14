using EditorAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

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

    public void OnAction(ObjectiveType type, string actionID)
    {
        QuestManager.OnAction(type, actionID);
    }
}

[System.Serializable]
public struct QuestObjID
{
    public ObjectiveType type;
    [SerializeField] public bool useDropDown;

    [SerializeField, ShowField(nameof(useDropDown))]
    [ObjectiveIDDropdown]
    public string dropDownObjectiveID;

    [SerializeField, HideField(nameof(useDropDown))]
    public string customObjectiveID;

    public string GetObjectiveID()
    {
        return useDropDown ? dropDownObjectiveID : customObjectiveID;
    }
}

[System.Serializable]
public class QuestObjectives
{
    [TextArea(2, 5)]
    public string objectiveDescription;

    [Space(20)]
    public ObjectiveType type;

    [SerializeField] public bool useDropDown;

    [SerializeField, ShowField(nameof(useDropDown))]
    [OnValueChanged(nameof(SetObjectiveID))]
    [ObjectiveIDDropdown]
    public string dropDownObjectiveID;

    [SerializeField, HideField(nameof(useDropDown))]
    [OnValueChanged(nameof(SetObjectiveID))]
    public string customObjectiveID;

    [HideInInspector] public string objectiveID;

    [Space(20)]
    public int requiredAmount;

    [HideInInspector] public int currentAmount;
    public bool fromZero = true;
    public int layer = 0; // The step/layer this objective belongs to

    [Space(20)]
    public UnityEvent onObjectiveComplete;

    [HideInInspector] public bool isCompleted => currentAmount >= requiredAmount;

    public void SetObjectiveID()
    {
        if (useDropDown)
            objectiveID = dropDownObjectiveID;
        else
            objectiveID = customObjectiveID;
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
                dropDownObjectiveID = obj.dropDownObjectiveID,
                customObjectiveID = obj.customObjectiveID,
                useDropDown = obj.useDropDown,
                objectiveDescription = obj.objectiveDescription,
                type = obj.type,
                requiredAmount = obj.requiredAmount,
                currentAmount = 0,
                fromZero = obj.fromZero,
                layer = obj.layer
            };

            objectives.Add(newObjective);
            newObjective.SetObjectiveID();
        }
    }

    // Returns the current active layer (lowest incomplete layer)
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

    // Only objectives in the current layer are active
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