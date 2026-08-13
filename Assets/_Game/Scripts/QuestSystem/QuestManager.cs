using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class QuestManager
{
    private static readonly Dictionary<string, QuestProgress> activeQuests = new();
    private static readonly HashSet<string> completedQuests = new();
    private static readonly HashSet<string> failedQuests = new();
    private static readonly Dictionary<string, int> globalObjectiveProgress = new();

    public static void OnAction(string actionID)
    {
        if (string.IsNullOrEmpty(actionID)) return;
        QuestAction action = new QuestAction(actionID);
        OnAction(action);
    }

    public static void OnAction(QuestAction actionTaken)
    {
        if (GameManager.instance != null && GameManager.instance.GamePaused) return;

        if (string.IsNullOrEmpty(actionTaken.actionID)) return;

        if (!globalObjectiveProgress.ContainsKey(actionTaken.actionID))
            globalObjectiveProgress[actionTaken.actionID] = 0;
        globalObjectiveProgress[actionTaken.actionID] += 1;

        var questsToComplete = new List<string>();

        foreach (var quest in activeQuests)
        {
            QuestProgress progress = quest.Value;
            bool updated = false;
            int previousLayer = progress.CurrentLayer;

            for (int i = 0; i < progress.ActiveObjectives.Count(); i++)
            {
                var obj = progress.ActiveObjectives.ElementAt(i);
                if (obj.objectiveID == actionTaken.actionID && !obj.isCompleted)
                {
                    if (obj.fromZero)
                        obj.currentAmount += 1;
                    else
                        obj.currentAmount = globalObjectiveProgress[actionTaken.actionID];

                    if (obj.currentAmount > obj.requiredAmount)
                        obj.currentAmount = obj.requiredAmount;

                    if (obj.currentAmount == obj.requiredAmount)
                        progress.quest.objectives[progress.objectives.IndexOf(obj)].onObjectiveComplete?.Invoke();

                    updated = true;
                }
            }

            if (updated && progress.IsCompleted)
                questsToComplete.Add(progress.QuestID);
            else if (updated && progress.CurrentLayer != previousLayer)
                TriggerObjectiveBeginEvents(progress);
        }

        for (int i = 0; i < questsToComplete.Count; i++)
            CompleteQuest(questsToComplete[i]);

        UpdateUI();
    }

    public static IEnumerable<QuestProgress> GetActiveQuests()
    {
        return activeQuests.Values;
    }

    public static void StartQuest(Quest quest)
    {
        if (quest == null || string.IsNullOrEmpty(quest.questID)) return;

        if (!activeQuests.ContainsKey(quest.questID) && !completedQuests.Contains(quest.questID))
        {
            var progress = new QuestProgress(quest);

            foreach (var obj in progress.objectives)
            {
                if (!obj.fromZero)
                {
                    globalObjectiveProgress.TryGetValue(obj.objectiveID, out int allTimeCount);
                    obj.currentAmount = allTimeCount;
                }
                else
                {
                    obj.currentAmount = 0;
                }
            }

            activeQuests[quest.questID] = progress;
            TriggerObjectiveBeginEvents(progress);
            UpdateUI();
        }
    }

    private static void TriggerObjectiveBeginEvents(QuestProgress progress)
    {
        int activeLayer = progress.CurrentLayer;
        if (activeLayer < 0) return;

        for (int i = 0; i < progress.objectives.Count; i++)
        {
            QuestObjectives runtimeObjective = progress.objectives[i];
            if (runtimeObjective.layer != activeLayer || runtimeObjective.isCompleted || runtimeObjective.hasBegun)
                continue;

            runtimeObjective.hasBegun = true;
            progress.quest.objectives[i].onObjectiveBegin?.Invoke();
        }
    }

    public static void CompleteQuest(string questID)
    {
        if (activeQuests.TryGetValue(questID, out var progress))
        {
            completedQuests.Add(questID);
            activeQuests.Remove(questID);
            Debug.Log($"Quest {progress.quest.questName} completed!");
            progress.quest.onQuestComplete?.Invoke();
            UpdateUI();
        }
    }

    public static IEnumerable<string> GetCompletedQuests() => completedQuests;

    public static IEnumerable<string> GetFailedQuests() => failedQuests;

    public static void UpdateUI()
    {
        if (MenuManager.instance == null) return;
        MenuManager.instance.UpdateQuestUI();
    }

    public static void ClearAllActiveQuests()
    {
        activeQuests.Clear();
        UpdateUI();
        Debug.Log("All active quests have been cleared.");
    }

    public static void ClearCompletedQuests()
    {
        completedQuests.Clear();
        Debug.Log("All completed quests have been cleared.");
    }

    public static void Initialize()
    {
        activeQuests.Clear();
        completedQuests.Clear();
        failedQuests.Clear();
        globalObjectiveProgress.Clear();
        UpdateUI();
    }
}

public struct QuestAction
{
    public string actionID;

    public QuestAction(string actionID)
    {
        this.actionID = actionID;
    }

    public void Act()
    {
        QuestManager.OnAction(this);
    }
}