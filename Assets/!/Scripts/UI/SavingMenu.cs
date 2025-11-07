using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.InputSystem;

public class SavingMenu : MonoBehaviour
{
    public GameObject SavingUI;
    public List<Button> slotButtons = new List<Button>();
    public Button autoSaveSlot;

    public Material slotsMat;
    public float initialYPos = 175f;
    public float moveAmount = 255f;
    protected Tween moveUPTween;
    protected Tween moveDOWNTween;

    public InputActionReference _navigateReference;
    protected Selectable _lastSelected;

    // Start is called before the first frame update
    private void Start()
    {
        SavingUI.SetActive(false);
    }

    public void Awake()
    {
        InitializeSlots();
    }

    public void InitializeSlots()
    {
        // Initialize each slot button
        foreach (var btn in slotButtons)
        {
            AddSelectionListeners(btn);
            Material newMat = new Material(slotsMat);
        }
        AddSelectionListeners(autoSaveSlot);
        Material mat = new Material(slotsMat);
    }

    public void OnEnable()
    {
        _navigateReference.action.performed += OnNavigate;
        foreach (var btn in slotButtons)
        {
            btn.GetComponent<RectTransform>().anchoredPosition =
                new Vector3(btn.GetComponent<RectTransform>().anchoredPosition.x,
                initialYPos);
        }
        autoSaveSlot.GetComponent<RectTransform>().anchoredPosition =
            new Vector3(autoSaveSlot.GetComponent<RectTransform>().anchoredPosition.x,
            initialYPos);
    }

    public void OnDisable()
    {
        _navigateReference.action.performed -= OnNavigate;
        moveUPTween?.Kill(true);
        moveDOWNTween?.Kill(true);
    }

    public void AddSelectionListeners(Selectable selectable)
    {
        EventTrigger trigger = selectable.gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = selectable.gameObject.AddComponent<EventTrigger>();
        }

        // select event
        EventTrigger.Entry selectEntry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.Select
        };
        selectEntry.callback.AddListener(OnSelect);
        trigger.triggers.Add(selectEntry);

        // deselect event
        EventTrigger.Entry deselectEntry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.Deselect
        };
        deselectEntry.callback.AddListener(OnDeselect);

        trigger.triggers.Add(deselectEntry);

        //pointerenter event
        EventTrigger.Entry pointerEnter = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerEnter
        };
        pointerEnter.callback.AddListener(OnPointerEnter);
        trigger.triggers.Add(pointerEnter);
        //pointerexit event
        EventTrigger.Entry pointerExit = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerExit
        };
        pointerExit.callback.AddListener(OnPointerExit);
        trigger.triggers.Add(pointerExit);
    }

    public void OnSelect(BaseEventData eventData)
    {
        _lastSelected = eventData.selectedObject.GetComponent<Selectable>();
        // Handle select event
        moveUPTween = eventData.selectedObject.GetComponent<RectTransform>().DOAnchorPosY(initialYPos + moveAmount, 0.2f);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        // Handle deselect event
        moveDOWNTween = eventData.selectedObject.GetComponent<RectTransform>().DOAnchorPosY(initialYPos, 0.2f);
    }

    public void OnPointerEnter(BaseEventData eventData)
    {
        PointerEventData pointerEventData = eventData as PointerEventData;
        if (pointerEventData != null)
        {
            Selectable sel = pointerEventData.pointerEnter.GetComponentInParent<Selectable>();
            if (sel == null)
            {
                sel = pointerEventData.pointerEnter.GetComponentInChildren<Selectable>();
            }
            pointerEventData.selectedObject = sel.gameObject;
        }
    }

    public void OnPointerExit(BaseEventData eventData)
    {
        PointerEventData pointerEventData = eventData as PointerEventData;
        if (pointerEventData != null)
        {
            pointerEventData.selectedObject = null;
        }
    }

    public void OnNavigate(InputAction.CallbackContext context)
    {
        if (EventSystem.current.currentSelectedGameObject == null && _lastSelected != null)
        {
            EventSystem.current.SetSelectedGameObject(_lastSelected.gameObject);
        }
    }

    public void StartNewGameMenu()
    {
        MoveSlots(false);
        UpdateSlotsTexts();
        // Enable all slot buttons
        foreach (var btn in slotButtons)
            btn.interactable = true;
        autoSaveSlot.gameObject.SetActive(false);
        EventSystem.current.SetSelectedGameObject(slotButtons[0].gameObject);
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
            autoSaveSlot.gameObject.SetActive(true);
            EventSystem.current.SetSelectedGameObject(autoSaveSlot.gameObject);
            autoSaveSlot.onClick.RemoveAllListeners();
            autoSaveSlot.onClick.AddListener(() =>
            {
                WarningSystem.ShowWarning("Load the auto save?",
                () =>
                {
                    StartCoroutine(LoadGame(-1)); // Assuming -1 indicates auto-save
                });
            });
            MoveSlots(true);
        }
        else
        {
            autoSaveSlot.gameObject.SetActive(false);
            EventSystem.current.SetSelectedGameObject(slotButtons[0].gameObject);
            MoveSlots(false);
        }

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

    private bool isRight = true;

    private void MoveSlots(bool right)
    {
        if (right != isRight)
        {
            foreach (var btn in slotButtons)
            {
                RectTransform rt = btn.GetComponent<RectTransform>();
                float targetX = right ? rt.anchoredPosition.x + 104f : rt.anchoredPosition.x - 104f;
                rt.anchoredPosition = new Vector2(targetX, rt.anchoredPosition.y);
            }
            isRight = right;
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