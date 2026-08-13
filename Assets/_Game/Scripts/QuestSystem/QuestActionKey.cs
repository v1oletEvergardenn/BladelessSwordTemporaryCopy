using UnityEngine;

public class QuestActionKey : ScriptableObject
{
    public string ActionID(string actionName)
    {
        if (string.IsNullOrEmpty(actionName))
            return string.Empty;

        return name + "." + actionName;
    }

    public void OnAction(string actionID)
    {
        QuestManager.OnAction(actionID);
    }
}