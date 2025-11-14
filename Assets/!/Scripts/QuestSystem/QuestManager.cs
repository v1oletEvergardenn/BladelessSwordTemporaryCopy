using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Static quest manager: does not require a GameObject.
/// </summary>
public static class QuestManager
{
    // Active quests and their progress (questID -> QuestProgress)

    private static readonly Dictionary<string, QuestProgress> activeQuests = new();

    // Completed and failed quest IDs
    private static readonly HashSet<string> completedQuests = new();

    private static readonly HashSet<string> failedQuests = new();

    // Tracks all-time progress for each objectiveID (objectiveID -> total count)
    private static readonly Dictionary<string, int> globalObjectiveProgress = new();

    /// <summary>
    /// Call this when an action relevant to quests occurs (e.g., kill, collect).
    /// </summary>
    public static void OnAction(ObjectiveType type, string actionID)
    {
        QuestAction action = new QuestAction(type, actionID);
        OnAction(action);
    }

    public static void OnAction(QuestObjID actionID)
    {
        QuestAction action = new QuestAction(actionID.type, actionID.GetObjectiveID());
        OnAction(action);
    }

    /// <summary>
    /// Handles quest progress updates and completion checks.
    /// </summary>
    public static void OnAction(QuestAction actionTaken)
    {
        if (GameManager.instance.GamePaused) return;
        // Update global progress for all-time tracking
        if (!globalObjectiveProgress.ContainsKey(actionTaken.actionID))
            globalObjectiveProgress[actionTaken.actionID] = 0;
        globalObjectiveProgress[actionTaken.actionID] += 1;

        var questsToComplete = new List<string>();

        foreach (var quest in activeQuests)
        {
            QuestProgress progress = quest.Value;
            bool updated = false;

            // Only progress objectives in the current active layer
            for (int i = 0; i < progress.ActiveObjectives.Count(); i++)
            {
                var obj = progress.ActiveObjectives.ElementAt(i);
                if (obj.type == actionTaken.type && obj.objectiveID == actionTaken.actionID && !obj.isCompleted)
                {
                    if (obj.fromZero)
                    {
                        obj.currentAmount += 1;
                    }
                    else
                    {
                        obj.currentAmount = globalObjectiveProgress[actionTaken.actionID];
                    }

                    if (obj.currentAmount > obj.requiredAmount)
                        obj.currentAmount = obj.requiredAmount;

                    // Trigger objective complete event if just completed
                    if (obj.currentAmount == obj.requiredAmount)
                    {
                        progress.quest.objectives[progress.objectives.IndexOf(obj)].onObjectiveComplete?.Invoke();
                    }
                    updated = true;
                }
            }

            if (updated && progress.IsCompleted)
                questsToComplete.Add(progress.QuestID);
        }

        foreach (var questID in questsToComplete)
            CompleteQuest(questID);

        UpdateUI();
    }

    /// <summary>
    /// Returns all active quest progress objects.
    /// </summary>
    public static IEnumerable<QuestProgress> GetActiveQuests()
    {
        return activeQuests.Values;
    }

    public static void DebugActiveQuests()
    {
        Debug.Log("=== Active Quests ===");
        foreach (var questProgress in activeQuests.Values)
        {
            var quest = questProgress.quest;
            Debug.Log($"Quest: {quest.questName} (ID: {quest.questID}) - {quest.questDescription}");
            foreach (var obj in questProgress.objectives)
            {
                Debug.Log(
                    $"  Objective: {obj.objectiveDescription} | " +
                    $"Type: {obj.type} | " +
                    $"ID: {obj.objectiveID} | " +
                    $"Progress: {obj.currentAmount}/{obj.requiredAmount} | " +
                    $"FromZero: {obj.fromZero} | " +
                    $"Completed: {obj.isCompleted}"
                );
            }
        }
        Debug.Log("=====================");
    }

    /// <summary>
    /// Starts tracking a new quest and initializes objectives based on tracking mode.
    /// </summary>
    public static void StartQuest(Quest quest)
    {
        if (!activeQuests.ContainsKey(quest.questID) && !completedQuests.Contains(quest.questID))
        {
            var progress = new QuestProgress(quest);

            // Initialize each objective's currentAmount based on tracking mode
            foreach (var obj in progress.objectives)
            {
                if (!obj.fromZero)
                {
                    // All-time: set to global progress so far
                    globalObjectiveProgress.TryGetValue(obj.objectiveID, out int allTimeCount);
                    obj.currentAmount = allTimeCount;
                }
                else
                {
                    // From zero: always start at 0
                    obj.currentAmount = 0;
                }
            }

            activeQuests[quest.questID] = progress;
            UpdateUI();
        }
    }

    /// <summary>
    /// Marks a quest as completed and removes it from active tracking.
    /// </summary>
    public static void CompleteQuest(string questID)
    {
        if (activeQuests.TryGetValue(questID, out var progress))
        {
            completedQuests.Add(questID);
            activeQuests.Remove(questID);
            Debug.Log($"Quest {progress.quest.questName} completed!");

            // TODO: Add reward logic, notifications, etc.
            progress.quest.onQuestComplete?.Invoke();
            UpdateUI();
        }
    }

    /// <summary>
    /// Returns all completed quest IDs.
    /// </summary>
    public static IEnumerable<string> GetCompletedQuests()
    {
        return completedQuests;
    }

    /// <summary>
    /// Returns all failed quest IDs.
    /// </summary>
    public static IEnumerable<string> GetFailedQuests()
    {
        return failedQuests;
    }

    /// <summary>
    /// Updates the quest UI (calls MenuManager).
    /// </summary>
    public static void UpdateUI()
    {
        // If MenuManager is also static, call directly; otherwise, find or reference it as needed.
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
        Debug.Log("QuestManager initialized.");
    }
}

/// <summary>
/// Represents a single quest-related action (e.g., kill, collect).
/// </summary>
public struct QuestAction
{
    public string actionID;
    public ObjectiveType type;

    public QuestAction(ObjectiveType type, string _actionID)
    {
        this.type = type;
        this.actionID = _actionID;
    }

    public void Act()
    {
        QuestManager.OnAction(this);
    }
}