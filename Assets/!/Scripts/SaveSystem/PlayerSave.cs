using EditorAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public struct PlayerSaveData
{
    public Vector3 pos;
    public float currentHealth;
    public float maxHealth;
    public float currentEnergy;
    public float maxEnergy;
    public float currentHSpoint;
    public float maxHSpoint;
}

[System.Serializable]
public struct LevelSaveData
{
    public string lastSavedScene;
}

public class PlayerSave : MonoBehaviour
{
    public static PlayerSave instance;

    private void Awake()
    {
        instance = this;
    }

    public void Save(ref PlayerSaveData data)
    {
        GameManager manager = GameManager.instance;
        data.currentHealth = manager.playerhealth.GetCurrentHealth();
        data.maxHealth = manager.playerhealth.GetMaxHealth();
        data.currentEnergy = manager.playerEnergy.currentEnergy;
        data.maxEnergy = manager.playerEnergy.maxEnergy;
        data.currentHSpoint = manager.hsManager.currentHS_point;
        data.maxHSpoint = manager.hsManager.GetMaxHSpoint();
        data.pos = manager.playerhealth.transform.position;
    }

    public void Load(PlayerSaveData data)
    {
        GameManager manager = GameManager.instance;
        if (manager.player == null) { manager.LoadAndCreatePlayer(); return; }

        manager.playerhealth.transform.position = data.pos;
        manager.playerhealth.SetMaxHealth(data.maxHealth);
        manager.playerhealth.SetCurrentHealth(data.currentHealth);
        manager.playerEnergy.maxEnergy = data.maxEnergy;
        manager.playerEnergy.currentEnergy = data.currentEnergy;
        manager.hsManager.SetMaxHSPoint(data.maxHSpoint);
        manager.hsManager.SetCurrentHSPoint(data.currentHSpoint);
    }

    public void Save(ref LevelSaveData data)
    {
        data.lastSavedScene = SceneManager.GetActiveScene().name;
    }
}