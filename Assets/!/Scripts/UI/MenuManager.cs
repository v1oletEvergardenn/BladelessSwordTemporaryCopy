using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using EditorAttributes;
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
        PauseGameCanvas.SetActive(false);
        SavePointCanvas.SetActive(false);
        SavePointMenu.SetActive(false);
        HSAbilitySwapMenu.SetActive(false);
        HSAbilityUpgradeMenu.SetActive(false);
        StoreCanvas.SetActive(false);
    }

    #endregion Singleton

    #region Fields & Inspector

    #region Map Menu

    public GameObject MapCanvas;

    #endregion Map Menu

    #region PauseGame Menu

    public GameObject PauseGameCanvas;
    public Transform pauseMenuTabHolder;
    public bool canChangeTab = true;
    public bool canCloseMenu = true;

    private List<GameObject> PauseMenuTabs = new List<GameObject>();
    public int currentIndexTab = 0;

    #endregion PauseGame Menu

    #region SavePoint Menu

    public GameObject SavePointCanvas;
    public GameObject SavePointMenu;
    public GameObject HSAbilitySwapMenu;
    public GameObject HSAbilityUpgradeMenu;
    public Image backgroundImage1;
    public Image backgroundImage2;
    public Transform hsUpgrade_rotator;
    public List<HSAbilityUpgradeUI> hsUpgradeUIs = new List<HSAbilityUpgradeUI>();

    #endregion SavePoint Menu

    #region End and Fade Menu

    public GameObject EndGameCanvas;

    public GameObject FadeCanvas;
    public Image fadeOutImage;
    [Range(0.1f, 10f)] public float _fadeOutTime = 1f;
    [Range(0.1f, 10f)] public float _fadeInTime = 1f;

    #endregion End and Fade Menu

    #region Store Menu

    public GameObject tradeItemPrefab;
    public GameObject StoreCanvas;
    public ScrollRect storeScrollRect;
    public Transform tradeItemsHolder;
    public Transform storeMenuTabHolder;
    public TextMeshProUGUI currencyText;
    public TextMeshProUGUI itemDetail_name;
    public TextMeshProUGUI itemDetail_description;
    public Image itemDetail_icon;
    private List<GameObject> StoreMenuTabs = new List<GameObject>();
    [HideInInspector] public List<GameObject> tradeItemPool = new List<GameObject>();
    private TradeItemType tradeItemType;
    [HideInInspector] public Store currentStore;

    #endregion Store Menu

    #region Quest UI

    public Transform questHolder;
    public Transform border;
    public GameObject questUIPrefab;
    public GameObject objectiveUIPrefab;
    [HideInInspector] public List<UI_Objective> ui_objs = new List<UI_Objective>();

    #endregion Quest UI

    #region Item Menu

    public ItemMenuSlots[] itemMenuSlots;
    public GameObject itemMenuCanvas;
    public GameObject itemSelectionCanvas;

    #endregion Item Menu

    #endregion Fields & Inspector

    #region Unity Methods

    private void Start()
    {
        // Initialize menu states

        InputMaster.instance.uiActions.FlipPage_LB.performed += ctx => PreviousTab();
        InputMaster.instance.uiActions.FlipPage_RB.performed += ctx => NextTab();
        InputMaster.instance._MenuOpenAction.performed += ctx => OpenPauseGameCanvas();
        InputMaster.instance.uiActions.MenuClose.performed += ctx => CloseMenu();
        InputMaster.instance.uiActions.Uninstall.performed += ctx => CloseMenu();
        InputMaster.instance.uiActions.Cancel.performed += ctx => BackToLastMenu();
        InputMaster.instance._OpenMapAction.performed += ctx => OpenMapCanvas();
        InputMaster.instance._CloseMapAction.performed += ctx => CloseMapCanvas();
        // Register input events for tab navigation and menu open/close

        UpdateQuestUI();
        TradeItemPoolGenerate();

        StoreMenuTabs.Clear();
        foreach (Transform child in storeMenuTabHolder)
        {
            StoreMenuTabs.Add(child.gameObject);
        }

        PauseMenuTabs.Clear();
        foreach (Transform child in pauseMenuTabHolder)
        {
            PauseMenuTabs.Add(child.gameObject);
        }
    }

    #endregion Unity Methods

    /// <summary>
    /// Closes the currently open menu (pause or save point).
    /// </summary>
    public void CloseMenu()
    {
        if (PauseGameCanvas.activeInHierarchy)
        {
            ClosePauseGameCanvas();
            currentIndexTab = -1;
            NextTab();
        }
        else if (MapCanvas.activeInHierarchy)
        {
            CloseMapCanvas();
            currentIndexTab = -1;
            NextTab();
        }
        else if (HSAbilityUpgradeMenu.activeInHierarchy)
        {
            CloseHSUpgradeMenu(false);
        }
        else if (StoreCanvas.activeInHierarchy)
        {
            CloseStoreCanvas();
            currentIndexTab = -1;
            NextTab();
        }
        else if (SavePointCanvas.activeInHierarchy)
        {
            CloseSavePointCanvas();
            currentIndexTab = -1;
            NextTab();
            CameraManager.instance.SwitchToNormalCam();
        }
        else if (itemMenuCanvas.activeInHierarchy)
        {
            CloseItemMenuCanvas();
        }
        canChangeTab = true;
        Health.instance.SetCharacterUI(true);
    }

    public void BackToLastMenu()
    {
        EventSystem.current.currentSelectedGameObject?.GetComponent<IBackToLastMenu>()?.GoBack();
    }

    /// <summary>
    /// Opens the end game canvas.
    /// </summary>
    public void EndCanvas()
    {
        GameManager.instance.PauseGame();
        EndGameCanvas.SetActive(true);
        EventSystemExtension.SetSelectObject(EndGameCanvas.GetComponent<FirstSelectObjectSerializer>().Selected());
    }

    #region Map Menu

    public void OpenMapCanvas()
    {
        MapManager.instance.OpenMap();
    }

    public void CloseMapCanvas()
    {
        MapManager.instance.CloseMap();
    }

    #endregion Map Menu

    #region PauseGame Menu

    /// <summary>
    /// Opens the pause game canvas and sets up tab navigation.
    /// </summary>
    public void OpenPauseGameCanvas()
    {
        GameManager.instance.PauseGame();
        PauseGameCanvas.SetActive(true);
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

    #endregion PauseGame Menu

    #region SavePointMenu

    /// <summary>
    /// Opens the save point canvas and sets up the UI.
    /// </summary>
    public void OpenSavePointCanvas()
    {
        Health.instance.SetCharacterUI(false);
        currentIndexTab = -1;
        NextTab();
        SavePointCanvas.SetActive(true);
        SavePointMenu.SetActive(true);
        HSAbilitySwapMenu.SetActive(false);
        EventSystemExtension.SetSelectObject(SavePointMenu.GetComponent<FirstSelectObjectSerializer>().Selected());
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

    public void OpenHSUpgradeMenu()
    {
        previousHSUpgradeTabIndex = 0;
        currentIndexTab = 0;
        HeartSwordAbilities.instance.SwitchToHSUpgradeCamera();
        Health.instance.SetCharacterUI(false);

        InputMaster.SwitchToUIAction();

        HSAbilityUpgradeMenu.SetActive(true);

        backgroundImage1.color = new Color(backgroundImage1.color.r, backgroundImage1.color.g, backgroundImage1.color.b, 0f);
        backgroundImage1.DOFade(1f, 1f).SetEase(Ease.OutCubic);

        backgroundImage2.color = new Color(backgroundImage2.color.r, backgroundImage2.color.g, backgroundImage2.color.b, 0f);
        backgroundImage2.DOFade(1f, 1f).SetEase(Ease.OutCubic);

        foreach (var ui in hsUpgradeUIs)
        {
            ui.ShowOrHide(true);
        }
    }

    public void CloseHSUpgradeMenu(bool openSavePointMenu)
    {
        StartCoroutine(CloseHSUpgradeMenuCoroutine(openSavePointMenu));
    }

    public IEnumerator CloseHSUpgradeMenuCoroutine(bool openSavePointMenu)
    {
        foreach (var ui in hsUpgradeUIs)
        {
            ui.ShowOrHide(false);
        }
        backgroundImage1.color = new Color(backgroundImage1.color.r, backgroundImage1.color.g, backgroundImage1.color.b, 1f);
        backgroundImage1.DOFade(0f, 1f).SetEase(Ease.OutCubic);

        backgroundImage2.color = new Color(backgroundImage2.color.r, backgroundImage2.color.g, backgroundImage2.color.b, 1f);
        backgroundImage2.DOFade(0f, 1f).SetEase(Ease.OutCubic);

        CameraManager.instance.SwitchToNormalCam();
        yield return new WaitForSeconds(0.7f);
        HSAbilityUpgradeMenu.SetActive(false);
        if (openSavePointMenu)
        {
            OpenSavePointCanvas();
        }
        else
        {
            SavePointCanvas.SetActive(false);
            yield return new WaitForSeconds(0.3f);
            InputMaster.SwitchToGameplayAction();
        }
        currentIndexTab = 0;
        hsUpgrade_rotator.localRotation = Quaternion.Euler(0, 0, 0);
    }

    #endregion SavePointMenu

    #region StoreMenu

    public void CloseStoreCanvas()
    {
        StoreCanvas.SetActive(false);
        currentStore = null;
        CameraManager.instance.SwitchToNormalCam();
        InputMaster.SwitchToGameplayAction();
        Health.instance.SetCharacterUI(true);
    }

    /// <summary>
    /// Opens the store canvas for player interaction.
    /// </summary>
    /// <param name="store">The store to open.</param>
    public void OpenStoreCanvas(Store store)
    {
        Health.instance.SetCharacterUI(false);
        EventSystemExtension.SetSelectObject(tradeItemPool[0]);
        StoreCanvas.SetActive(true);

        InputMaster.SwitchToUIAction();
        tradeItemType = TradeItemType.All;
        currentStore = store;
        Health.instance.SetCharacterUI(false);
        ShowStoreTab();
    }

    public void UpdateStoreItemDetail(int index)
    {
        if (currentStore == null || currentStore.itemsForSale == null) return;
        itemDetail_name.text = currentStore.GetItem(index).itemName;
        itemDetail_description.text = currentStore.GetItem(index).description;
        itemDetail_icon.sprite = currentStore.GetItem(index).icon;
    }

    public void UpdateStoreUI()
    {
        if (currentStore == null || currentStore.itemsForSale == null)
        {
            foreach (var itemObj in tradeItemPool)
                itemObj.SetActive(false);
            return;
        }
        currencyText.text = "$" + CurrencyManager.instance.Currency.ToString();
        int itemCount = currentStore.itemsForSale.Length;

        for (int i = 0; i < tradeItemPool.Count; i++)
        {
            GameObject itemObj = tradeItemPool[i];

            if (i >= itemCount)
            {
                itemObj.SetActive(false);
                continue;
            }

            if (currentStore.remainingQuantities[i] <= 0)
            {
                itemObj.SetActive(false);
                continue;
            }

            var itemData = currentStore.GetItem(i);
            bool showItem = tradeItemType == TradeItemType.All || itemData.itemType == tradeItemType;
            itemObj.SetActive(showItem);

            if (showItem)
            {
                var btn = itemObj.GetComponent<TradeItemBtn>();
                btn?.SetItem(currentStore, i);
            }
        }
    }

    #endregion StoreMenu

    #region Item Menu

    public void OpenItemMenu()
    {
        itemMenuCanvas.SetActive(true);
        ItemManager itemManager = ItemManager.instance;
        //according to the current equipped itemModule, update the itemMenuSlots, a
        if (itemManager.currentItemModule != null)
        {
            OpenItemModule(itemManager.currentItemModule);
            currentIndexTab = itemManager.availableItemModules.IndexOf(itemManager.currentItemModule);
        }
        else
        {
            if (itemManager.availableItemModules.Count > 0)
            {
                OpenItemModule(itemManager.availableItemModules[0]);
                currentIndexTab = 0;
            }
        }
    }

    public void ShowItemMenuTab()
    {
        currentIndexTab = currentIndexTab % ItemManager.instance.availableItemModules.Count;
        if (currentIndexTab < 0) { currentIndexTab = ItemManager.instance.availableItemModules.Count - 1; }
        SwitchItemModule(ItemManager.instance.availableItemModules[currentIndexTab]);
    }

    public void OpenItemModule(ItemModule module)
    {
        CloseItemSelectionMenu();
        foreach (ItemMenuSlots slot in itemMenuSlots)
        {
            slot.gameObject.SetActive(false);
        }

        List<ItemSlot> itemSlots = new List<ItemSlot>
        {
            module.item1,
            module.item2,
            module.item3,
            module.item4,
            module.item5,
            module.item6
        };

        for (int i = 0; i < itemSlots.Count; i++)
        {
            if (itemSlots[i].slotLevel != ItemLevel.None)
            {
                itemMenuSlots[i].gameObject.SetActive(true);
                itemMenuSlots[i].slotLevel = itemSlots[i].slotLevel;
            }
            else
            {
                itemMenuSlots[i].gameObject.SetActive(false);
            }
        }

        ItemManager.SetCurrentItemModule(module);
        UpdateItemSlotUI();
        SelectFirstAvailableItemSlot();
    }

    public void SwitchItemModule(ItemModule module)
    {
        CloseItemSelectionMenu();
        List<ItemSlot> itemSlots = new List<ItemSlot>
        {
            module.item1,
            module.item2,
            module.item3,
            module.item4,
            module.item5,
            module.item6
        };

        for (int i = 0; i < itemSlots.Count; i++)
        {
            if (itemSlots[i].slotLevel != ItemLevel.None)
            {
                itemMenuSlots[i].gameObject.SetActive(true);
                itemMenuSlots[i].slotLevel = itemSlots[i].slotLevel;
                itemMenuSlots[i].FinishFadeTween();
            }
            else
            {
                itemMenuSlots[i].gameObject.SetActive(false);
            }
        }

        ItemManager.SetCurrentItemModule(module);
        UpdateItemSlotUI();
        SelectFirstAvailableItemSlot();
    }

    public static void UpdateItemSlotUI()
    {
        foreach (ItemMenuSlots slot in instance.itemMenuSlots)
        {
            slot.UpdateUI();
        }
    }

    public void SelectFirstAvailableItemSlot()
    {
        foreach (ItemMenuSlots slot in itemMenuSlots)
        {
            if (slot.gameObject.activeInHierarchy)
            {
                EventSystemExtension.SetSelectObject(slot.gameObject);
                break;
            }
        }
    }

    public void CloseItemMenuCanvas()
    {
        itemMenuCanvas.SetActive(false);
        OpenSavePointCanvas();
    }

    public void OpenItemSelectionMenu(ItemMenuSlots itemMenuSlot, ItemLevel itemLevel)
    {
        if (itemSelectionCanvas.GetComponent<ItemSelectionUI>().OpenItemSelectionMenu(itemMenuSlot, itemLevel))
        {
            itemSelectionCanvas.SetActive(true);
        }
    }

    public void CloseItemSelectionMenu()
    {
        if (itemSelectionCanvas.activeInHierarchy)
        {
            GameObject obj = itemSelectionCanvas.GetComponent<ItemSelectionUI>().currentSlot.gameObject;
            if (obj != null) EventSystemExtension.SetSelectObject(obj);
            else SelectFirstAvailableItemSlot();
            itemSelectionCanvas.SetActive(false);
        }
    }

    #endregion Item Menu

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
        if (PauseGameCanvas.activeInHierarchy)
        {
            ShowPauseGameTab();
        }
        else if (StoreCanvas.activeInHierarchy)
        {
            ShowStoreTab();
        }
        else if (HSAbilityUpgradeMenu.activeInHierarchy)
        {
            ShowHSAbilityUpgradeTab();
        }
        else if (itemMenuCanvas.activeInHierarchy)
        {
            ShowItemMenuTab();
        }
    }

    public void ShowStoreTab()
    {
        currentIndexTab = currentIndexTab % StoreMenuTabs.Count;
        if (currentIndexTab < 0) { currentIndexTab = StoreMenuTabs.Count - 1; }
        foreach (GameObject obj in StoreMenuTabs)
        {
            obj.GetComponent<Image>().color = Color.white;
        }
        if (StoreMenuTabs[currentIndexTab] != null)
        {
            StoreMenuTabs[currentIndexTab].GetComponent<Image>().color = Color.black;
            if (currentIndexTab == 0) tradeItemType = TradeItemType.All;
            else if (currentIndexTab == 1) tradeItemType = TradeItemType.Consumable;
            else if (currentIndexTab == 2) tradeItemType = TradeItemType.Equipment;
            else if (currentIndexTab == 3) tradeItemType = TradeItemType.Material;
            else if (currentIndexTab == 4) tradeItemType = TradeItemType.QuestItem;
        }
        UpdateStoreUI();
        var firstActive = tradeItemPool.FirstOrDefault(obj => obj.activeSelf);
        if (firstActive != null)
            EventSystemExtension.SetSelectObject(firstActive);
        storeScrollRect.GetComponent<ScrollRectAutoScroll>().ScrollToSelected(false);
    }

    public void ShowPauseGameTab()
    {
        currentIndexTab = currentIndexTab % PauseMenuTabs.Count;
        if (currentIndexTab < 0) { currentIndexTab = PauseMenuTabs.Count - 1; }
        foreach (GameObject obj in PauseMenuTabs)
        {
            if (obj != null)
            {
                obj.SetActive(false);
                obj.GetComponent<FirstSelectObjectSerializer>().DisSelected();
            }
        }
        if (PauseMenuTabs[currentIndexTab] != null)
        {
            PauseMenuTabs[currentIndexTab].SetActive(true);
            EventSystemExtension.SetSelectObject(PauseMenuTabs[currentIndexTab].GetComponent<FirstSelectObjectSerializer>().Selected());
        }
    }

    private int previousHSUpgradeTabIndex = 0;
    private Tween hsUpgradeRotatorTween;

    public void ShowHSAbilityUpgradeTab()
    {
        int tabCount = 4;
        currentIndexTab = currentIndexTab % tabCount;
        if (currentIndexTab < 0) { currentIndexTab = tabCount - 1; }

        // Calculate target angle based on tab index
        float targetAngle = 90f * currentIndexTab;

        // Kill previous tween if active
        if (hsUpgradeRotatorTween != null && hsUpgradeRotatorTween.IsActive())
            hsUpgradeRotatorTween.Kill();

        // Tween to the target angle (shortest path)
        hsUpgradeRotatorTween = hsUpgrade_rotator.DOLocalRotate(
            new Vector3(0, 0, targetAngle),
            0.3f,
            RotateMode.Fast
        ).SetEase(Ease.OutSine);

        previousHSUpgradeTabIndex = currentIndexTab;

        EventSystemExtension.SetSelectObject(
            hsUpgradeUIs[currentIndexTab * 2].gameObject
        );
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
            storeScrollRect.GetComponent<StoreItemBtnNavigates>().selectables.Add(itemObj.GetComponent<Selectable>());
        }
        storeScrollRect.GetComponent<StoreItemBtnNavigates>().InitializeSlots();
    }

    #endregion Utility Methods
}