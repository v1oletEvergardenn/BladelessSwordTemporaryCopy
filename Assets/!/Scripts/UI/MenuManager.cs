using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static ControllerInput;

public enum TutType
{
    basicMovement,
    dash,
    status,
    qi,
    hs_bar,
    counterAttack,
    defend,
    heartSwordAttack,
    swordJump,
    swordTeleport,
    swordStorm,
    bossBattle,
    levelMechanism,
    guard
}

public class MenuManager : MonoBehaviour
{
    #region Singleton

    public static MenuManager instance;

    private void Awake()
    {
        if (instance == null) { instance = this; }
    }

    #endregion Singleton

    #region Fields & Inspector

    [Header("Pause InGame Canvas")]
    public GameObject PauseGameCanvas;

    public List<GameObject> Tabs;
    private int currentIndexTab = 0;
    public bool canChangeTab = true;
    public bool canCloseMenu = true;

    [Header("Save Point Canvas")]
    public GameObject SavePointCanvas;

    public GameObject SavePointMenu;
    public GameObject HSAbilitySwapMenu;

    [Header("End Canvas")]
    public GameObject EndGameCanvas;

    [Header("Transition")]
    public GameObject FadeCanvas;

    public Image fadeOutImage;
    [Range(0.1f, 10f)] public float _fadeOutTime = 1f;
    [Range(0.1f, 10f)] public float _fadeInTime = 1f;

    [Header("Store")]
    public GameObject tradeItemPrefab;

    public GameObject StoreCanvas;
    public ScrollRect storeScrollRect;
    public Transform tradeItemsHolder;
    private List<GameObject> tradeItemPool = new List<GameObject>();

    [Header("Quest")]
    public Transform questHolder;

    public Transform border;
    public GameObject questUIPrefab;
    public GameObject objectiveUIPrefab;
    [HideInInspector] public List<UI_Objective> ui_objs = new List<UI_Objective>();

    #endregion Fields & Inspector

    #region Unity Methods

    private void Start()
    {
        // Initialize menu states
        PauseGameCanvas.SetActive(false);
        SavePointCanvas.SetActive(false);

        // Register input events for tab navigation and menu open/close
        InputMaster.instance.uiActions.FlipPage_LB.performed += ctx => PreviousTab();
        InputMaster.instance.uiActions.FlipPage_RB.performed += ctx => NextTab();
        InputMaster.instance._MenuOpenAction.performed += ctx => OpenPauseGameCanvas();
        InputMaster.instance.uiActions.MenuClose.performed += ctx => CloseMenu();

        UpdateQuestUI();
        TradeItemPoolGenerate();
    }

    #endregion Unity Methods

    #region Menu Open/Close Methods

    /// <summary>
    /// Closes the currently open menu (pause or save point).
    /// </summary>
    public void CloseMenu()
    {
        if (PauseGameCanvas.activeInHierarchy)
        {
            ClosePauseGameCanvas();
        }
        else if (SavePointCanvas.activeInHierarchy)
        {
            CloseSavePointCanvas();
        }
    }

    /// <summary>
    /// Opens the pause game canvas and sets up tab navigation.
    /// </summary>
    public void OpenPauseGameCanvas()
    {
        GameManager.instance.PauseGame();
        currentIndexTab = -1;
        NextTab();
        PauseGameCanvas.SetActive(true);
        canChangeTab = true;
        InputMaster.SwitchToUIAction();
    }

    /// <summary>
    /// Closes the pause game canvas and resumes gameplay.
    /// </summary>
    public void ClosePauseGameCanvas()
    {
        if (!canCloseMenu) return;
        GameManager.instance.UnpauseGame();
        PauseGameCanvas.SetActive(false);
        InputMaster.SwitchToGameplayAction();
    }

    /// <summary>
    /// Opens the save point canvas and sets up the UI.
    /// </summary>
    public void OpenSavePointCanvas()
    {
        SavePointCanvas.SetActive(true);
        SavePointMenu.SetActive(true);
        HSAbilitySwapMenu.SetActive(false);
        EventSystem.current.SetSelectedGameObject(SavePointMenu.GetComponent<FirstSelectObjectSerializer>().Selected());
        InputMaster.SwitchToUIAction();
    }

    /// <summary>
    /// Closes the save point canvas and resumes gameplay.
    /// </summary>
    public void CloseSavePointCanvas()
    {
        SavePointCanvas.SetActive(false);
        InputMaster.SwitchToGameplayAction();
    }

    /// <summary>
    /// Opens the store canvas for player interaction.
    /// </summary>
    /// <param name="store">The store to open.</param>
    public void OpenStoreCanvas(Store store)
    {
        EventSystem.current.SetSelectedGameObject(tradeItemPool[0]);
        StoreCanvas.SetActive(true);
        InputMaster.SwitchToUIAction();
        //storeScrollRect.verticalNormalizedPosition = 0f;
        UpdateStoreUI(store);
    }

    public void UpdateStoreUI(Store store)
    {
        int itemCount = store != null && store.itemsForSale != null ? store.itemsForSale.Length : 0;

        for (int i = 0; i < tradeItemPool.Count; i++)
        {
            GameObject itemObj = tradeItemPool[i];

            if (i < itemCount)
            {
                // Show and update reached item
                itemObj.SetActive(true);

                // Update the UI with the store item data
                var btn = itemObj.GetComponent<TradeItemBtn>();
                if (btn != null)
                {
                    var itemData = store.GetItem(i);
                    int remaining = store.GetRemainingQuantity(i);
                    bool soldOut = store.IsSoldOut(i);
                    btn.SetItem(store, i);
                }
            }
            else
            {
                // Hide not reached (unused) items
                itemObj.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Opens the end game canvas.
    /// </summary>
    public void EndCanvas()
    {
        GameManager.instance.PauseGame();
        EndGameCanvas.SetActive(true);
        EventSystem.current.SetSelectedGameObject(EndGameCanvas.GetComponent<FirstSelectObjectSerializer>().Selected());
    }

    #endregion Menu Open/Close Methods

    #region Tab Navigation

    /// <summary>
    /// Switches to the next tab in the pause menu.
    /// </summary>
    public void NextTab()
    {
        if (!canChangeTab) return;
        currentIndexTab++;
        ShowTab();
    }

    /// <summary>
    /// Switches to the previous tab in the pause menu.
    /// </summary>
    public void PreviousTab()
    {
        if (!canChangeTab) return;
        currentIndexTab--;
        ShowTab();
    }

    /// <summary>
    /// Displays the currently selected tab and updates selection.
    /// </summary>
    public void ShowTab()
    {
        currentIndexTab = currentIndexTab % Tabs.Count;
        if (currentIndexTab < 0) { currentIndexTab = Tabs.Count - 1; }
        foreach (GameObject obj in Tabs)
        {
            if (obj != null)
            {
                obj.SetActive(false);
                obj.GetComponent<FirstSelectObjectSerializer>().DisSelected();
            }
        }
        if (Tabs[currentIndexTab] != null)
        {
            Tabs[currentIndexTab].SetActive(true);
            EventSystem.current.SetSelectedGameObject(Tabs[currentIndexTab].GetComponent<FirstSelectObjectSerializer>().Selected());
        }
    }

    /// <summary>
    /// Enables or disables tab switching.
    /// </summary>
    public void CanSwitchTab(bool b)
    {
        canChangeTab = b;
    }

    /// <summary>
    /// Enables or disables menu closing.
    /// </summary>
    public void CanCloseMenu(bool b)
    {
        canCloseMenu = b;
    }

    #endregion Tab Navigation

    #region Quest UI

    /// <summary>
    /// Updates the quest UI to reflect current active quests and objectives.
    /// </summary>
    public void UpdateQuestUI()
    {
        foreach (Transform child in questHolder)
        {
            Destroy(child.gameObject);
        }
        if (QuestManager.GetActiveQuests().Count() > 0)
        {
            border.gameObject.SetActive(true);
            foreach (var quest in QuestManager.GetActiveQuests())
            {
                UI_Quest quests = Instantiate(questUIPrefab, questHolder).GetComponent<UI_Quest>();
                quests.name = quest.quest.name;
                quests.UpdateUI(quest);

                // Only show active objectives (current layer)
                foreach (var objective in quest.ActiveObjectives)
                {
                    UI_Objective obj = Instantiate(objectiveUIPrefab, questHolder).GetComponent<UI_Objective>();
                    ui_objs.Add(obj);
                    obj.linkedObjective = objective;
                    obj.UpdateUI();
                }
            }
            questHolder.GetComponent<ContentSizeFitter>().enabled = true;
            questHolder.GetComponent<ContentSizeFitter>().enabled = false;
        }
        else
        {
            border.gameObject.SetActive(false);
        }
    }

    #endregion Quest UI

    #region Save & Transition

    /// <summary>
    /// Saves the game using the SaveSystem.
    /// </summary>
    public void Save()
    {
        SaveSystem.Save();
    }

    /// <summary>
    /// Fades the screen in or out using DOTween.
    /// </summary>
    /// <param name="fadeIn">True for fade in (to transparent), false for fade out (to black).</param>
    /// <returns>IEnumerator for coroutine.</returns>
    public static IEnumerator Fade(bool fadeIn)
    {
        if (instance == null || instance.fadeOutImage == null)
            yield break;

        // Stop any existing tweens on the image to avoid overlap
        instance.fadeOutImage.DOKill();

        // Ensure the canvas is active
        if (instance.FadeCanvas != null)
            instance.FadeCanvas.SetActive(true);

        Color color = instance.fadeOutImage.color;
        color.a = fadeIn ? 0f : 1f;
        instance.fadeOutImage.color = color;

        // Determine target alpha and duration
        float targetAlpha = fadeIn ? 1f : 0f;
        float duration = fadeIn ? instance._fadeInTime : instance._fadeOutTime;

        // Tween the alpha
        instance.fadeOutImage.DOFade(targetAlpha, duration)
            .SetEase(Ease.OutCubic);
        yield return new WaitForSeconds(duration);
    }

    #endregion Save & Transition

    #region Utility Methods

    public void TradeItemPoolGenerate()
    {
        int poolSize = 50;
        tradeItemPool.Clear();
        for (int i = 0; i < poolSize; i++)
        {
            GameObject itemObj = Instantiate(tradeItemPrefab, tradeItemsHolder);
            itemObj.SetActive(false);
            tradeItemPool.Add(itemObj);
        }
    }

    #endregion Utility Methods
}