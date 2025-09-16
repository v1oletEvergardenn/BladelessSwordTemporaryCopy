using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SavingMenu : MonoBehaviour
{
    public GameObject SavingUI;
    public List<Button> slotButtons = new List<Button>();

    // Start is called before the first frame update
    private void Start()
    {
        SavingUI.SetActive(false);
    }

    // Update is called once per frame
    private void Update()
    {
    }

    public void StartNewGameMenu()
    {
        UpdateSlotsTexts();
        // Enable all slot buttons
        foreach (var btn in slotButtons)
            btn.interactable = true;

        // Check each slot for existing save file
        for (int i = 0; i < slotButtons.Count; i++)
        {
            int slot = i;
            string path = SaveSystem.SaveFileName(slot);
            if (System.IO.File.Exists(path))
            {
                // Add listener to show override warning and create new game if confirmed
                slotButtons[i].onClick.AddListener(() =>
                {
                    ShowOverrideWarning(slot, () =>
                    {
                        CreateNewGame(slot);
                    });
                });
            }
            else
            {
                // Add listener to create a new game directly
                slotButtons[i].onClick.AddListener(() =>
                {
                    CreateNewGame(slot);
                });
            }
        }
    }

    public void LoadGameMenu()
    {
        UpdateSlotsTexts();
        // Disable buttons for slots without a save file
        for (int i = 0; i < slotButtons.Count; i++)
        {
            int slot = i;
            string path = SaveSystem.SaveFileName(slot);
            bool hasSave = System.IO.File.Exists(path);
            slotButtons[i].interactable = hasSave;

            if (hasSave)
            {
                // Add listener to load the game
                slotButtons[i].onClick.AddListener(() =>
                {
                    LoadGame(slot);
                });
            }
        }
    }

    public void UpdateSlotsTexts()
    {
        for (int i = 0; i < slotButtons.Count; i++)
        {
            int slot = i;
            string path = SaveSystem.SaveFileName(slot);
            slotButtons[i].onClick.RemoveAllListeners();
            string displayText;
            if (System.IO.File.Exists(path))
            {
                DateTime lastWriteTime = System.IO.File.GetLastWriteTime(path);
                displayText = $"{path}\n{lastWriteTime:yyyy-MM-dd HH:mm}";
            }
            else
            {
                displayText = $"Empty";
            }

            // Update button text (for Unity UI Text)
            Text textComponent = slotButtons[i].GetComponentInChildren<Text>();
            if (textComponent != null)
            {
                textComponent.text = displayText;
            }
            // If using TextMeshPro
            else
            {
                var tmpText = slotButtons[i].GetComponentInChildren<TMPro.TMP_Text>();
                if (tmpText != null)
                    tmpText.text = displayText;
            }
        }
        SavingUI.SetActive(true);
    }

    // Example stub for showing override warning (implement your own UI logic)
    public void ShowOverrideWarning(int slot, Action onConfirm)
    {
        // Show a UI dialog warning the player about overriding the save.
        // If the player confirms, call onConfirm().
        Debug.Log($"Override warning for slot {slot}. If confirmed, call onConfirm.");
        onConfirm?.Invoke(); // For now, auto-confirm for demonstration.
    }

    // Example stub for creating a new game
    private void CreateNewGame(int slot)
    {
        Debug.Log($"Creating new game in slot {slot}.");
        SaveSystem.currentSaveSlot = slot;
        // Initialize new game data here
        GameManager.instance.CreateNewGame(slot);
        LoadGame(slot);
        // Transition to the new game scene as needed
        // e.g., SceneManager.LoadScene("GameScene");
    }

    // Example stub for loading a game
    public void LoadGame(int slot)
    {
        Debug.Log($"Loading game from slot {slot}.");
        StartCoroutine(SceneTransition(slot));
    }

    public IEnumerator SceneTransition(int index)
    {
        yield return StartCoroutine(VFXManager.Fade(false));
        SaveSystem.Load(index);
        yield return StartCoroutine(VFXManager.Fade(true));
    }
}