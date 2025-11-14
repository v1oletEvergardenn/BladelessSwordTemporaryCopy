using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YingyangFishLevelTutorial : MonoBehaviour
{
    public Quest tutorialQuest;

    // Start is called before the first frame update
    private void Start()
    {
        QuestManager.Initialize();
        QuestManager.StartQuest(tutorialQuest);
    }

    // Update is called once per frame
    private void Update()
    {
        if (UnityEngine.Input.GetKeyDown(KeyCode.Space))
        {
            //QuestManager.DebugActiveQuests();
        }
    }
}