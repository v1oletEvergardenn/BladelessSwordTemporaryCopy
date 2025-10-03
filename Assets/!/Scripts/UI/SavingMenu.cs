using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SavingMenu : MonoBehaviour
{
    public GameObject SavingUI;
    public List<Button> slotButtons = new List<Button>();
    public Button AutoSaveSlot;

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
                    WarningSystem.ShowWarning("This slot already has a saved game. Do you want to overwrite it?",
                    () =>
                    {
                        WarningSystem.ShowWarning("Are you sure to overwrite it? you will lose the save forever",
                        () =>
                        {
                            StartCoroutine(CreateNewGame(slot));
                        });
                    });
                });
            }
            else
            {
                slotButtons[i].onClick.AddListener(() =>
                {
                    StartCoroutine(CreateNewGame(slot));
                });
            }
        }
    }

    public void LoadGameMenu()
    {
        if (System.IO.File.Exists(SaveSystem.SaveFileName(-1)))
        {
            AutoSaveSlot.gameObject.SetActive(true);
            AutoSaveSlot.onClick.RemoveAllListeners();
            AutoSaveSlot.onClick.AddListener(() =>
            {
                WarningSystem.ShowWarning("Load the auto save?",
                () =>
                {
                    StartCoroutine(LoadGame(-1)); // Assuming -1 indicates auto-save
                });
            });
        }
        else { AutoSaveSlot.gameObject.SetActive(false); }

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
                    WarningSystem.ShowWarning("Load this saved game?",
                    () =>
                    {
                        StartCoroutine(LoadGame(slot));
                    });
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
                displayText = $"{System.IO.Path.GetFileName(path)}\n{lastWriteTime:yyyy-MM-dd HH:mm}";
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

    // Example stub for creating a new game
    private IEnumerator CreateNewGame(int slot)
    {
        yield return StartCoroutine(MenuManager.Fade(true));
        Debug.Log($"Creating new game in slot {slot}.");
        SaveSystem.currentSaveSlot = slot;
        // Initialize new game data here
        GameManager.instance.CreateNewGame(slot);
        yield return StartCoroutine(MenuManager.Fade(false));
        // Transition to the new game scene as needed
        // e.g., SceneManager.LoadScene("GameScene");
    }

    // Example stub for loading a game
    public IEnumerator LoadGame(int slot)
    {
        yield return StartCoroutine(MenuManager.Fade(true));
        SaveSystem.Load(slot);
        yield return StartCoroutine(MenuManager.Fade(false));
    }
}