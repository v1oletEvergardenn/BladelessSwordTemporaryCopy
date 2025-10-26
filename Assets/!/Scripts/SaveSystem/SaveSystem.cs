using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveSystem
{
    public static int currentSaveSlot = 0;
    public static SaveData _saveData = new SaveData();

    // Returns the full path for a given slot index
    public static string SaveFileName(int slot)
    {
        string saveFileName = Application.persistentDataPath + $"/savefile_{slot}.save";
        return saveFileName;
    }

    // Save to a specific slot
    public static void Save()
    {
        HandleSaveData();
        File.WriteAllText(SaveFileName(currentSaveSlot), JsonUtility.ToJson(_saveData, true));
    }

    public static void AutoSave()
    {
        HandleSaveData();
        _saveData.autoSaveSlot = currentSaveSlot;
        File.WriteAllText(SaveFileName(-1), JsonUtility.ToJson(_saveData, true));
    }

    public static void Save(int slot)
    {
        HandleSaveData();
        File.WriteAllText(SaveFileName(slot), JsonUtility.ToJson(_saveData, true));
    }

    public static void HandleSaveData()
    {
        _saveData.newGameCreated = true;
        PlayerSave.instance.Save(ref _saveData.playerData);
        PlayerSave.instance.Save(ref _saveData.levelData);
        PlayerSave.instance.Save(ref _saveData.heartSwordData);
        Debug.Log("saving at slot" + currentSaveSlot);
    }

    public static void Load(int slot)
    {
        string path = SaveFileName(slot);
        if (!File.Exists(path))
        {
            Debug.LogWarning($"Save file for slot {slot} does not exist: {path}");
            //new Game
            GameManager.instance.CreateNewGame(slot);
        }
        else
        {
            string saveContent = File.ReadAllText(SaveFileName(slot));

            _saveData = JsonUtility.FromJson<SaveData>(saveContent);
            if (!_saveData.newGameCreated)
            {
                Debug.LogWarning($"creating new game at slot{slot}");
                GameManager.instance.CreateNewGame(slot);
            }
            else
            {
                if (slot == -1) currentSaveSlot = _saveData.autoSaveSlot;
                else currentSaveSlot = slot;
                HandleLoadData();
            }
        }
    }

    private static void HandleLoadData()
    {
        SceneManager.LoadScene(_saveData.levelData.lastSavedScene);
        GameManager.instance.LoadPlayer();
    }
}

[System.Serializable]
public struct SaveData
{
    public PlayerSaveData playerData;
    public LevelSaveData levelData;
    public HeartSwordSaveData heartSwordData;
    public int autoSaveSlot;
    public bool newGameCreated;
    // Add other game data here as needed
}