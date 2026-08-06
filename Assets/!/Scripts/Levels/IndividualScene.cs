using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class IndividualScene : MonoBehaviour
{
    public bool useSelfTransformForPlayerSpawnPoint = false;
    [HideIf("useSelfTransformForPlayerSpawnPoint")] public Transform playerSpawnPoint;
    public UnityEvent onSceneLoaded;
    public bool setPlayerHSPointInfinite;

    private void Start()
    {
        VFXManager.instance.StopRumble();
        if (playerSpawnPoint == null || useSelfTransformForPlayerSpawnPoint)
        {
            playerSpawnPoint = transform;
        }
        LevelManager.instance.currentScene = this;
        GameManager.instance.player.transform.position = playerSpawnPoint.position;
        onSceneLoaded?.Invoke();

        //QuestManager.Initialize();
    }

    private void Update()
    {
        if (setPlayerHSPointInfinite)
        {
            GameManager.instance.hsManager.SetCurrentHSPoint(GameManager.instance.hsManager.GetMaxHSpoint());
        }
    }
}