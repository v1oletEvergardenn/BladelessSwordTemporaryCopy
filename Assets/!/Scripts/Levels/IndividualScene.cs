using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class IndividualScene : MonoBehaviour
{
    public Transform playerSpawnPoint;
    public UnityEvent onSceneLoaded;

    private void Start()
    {
        if (playerSpawnPoint == null)
        {
            playerSpawnPoint = transform;
        }
        LevelManager.instance.currentScene = this;
        GameManager.instance.player.transform.position = playerSpawnPoint.position;
        onSceneLoaded?.Invoke();
    }
}