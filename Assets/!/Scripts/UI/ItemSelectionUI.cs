using EditorAttributes;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Codice.Client.BaseCommands.Import.Commit;

public class ItemSelectionUI : UIChildNavigate
{
    public Transform holder;
    public GameObject itemPrefab;
    public TextMeshProUGUI itemName;
    public TextMeshProUGUI itemDescription;

    public int itemsPerColumn = 11;
    private List<GameObject> itemPool = new List<GameObject>();
    private bool initialized = false;

    [HideProperty] public ItemLevel currentLevel;
    [HideProperty] public ItemMenuSlots currentSlot;

    // Start is called before the first frame update
    private void Start()
    {
        Initialize();
    }

    // Update is called once per frame
    private void Update()
    {
    }

    public void Initialize()
    {
        if (initialized) return;
        //generate item pools from itemPrefab 2 * itemsPerColumn, which default is 22 items
        int totalItems = 2 * itemsPerColumn;
        itemPool.Clear();
        selectables.Clear();

        for (int i = 0; i < totalItems; i++)
        {
            GameObject go = Instantiate(itemPrefab, holder);
            go.name = $"Item_{i + 1}";
            var selectable = go.GetComponent<Selectable>();
            if (selectable != null)
            {
                selectables.Add(selectable);
                AddSelectionListeners(selectable);
            }
            itemPool.Add(go);
            go.GetComponent<ItemSelectionUI_item>().itemUI = this;
        }

        for (int i = 0; i < selectables.Count; i++)
        {
            var nav = new Navigation
            {
                mode = Navigation.Mode.Explicit
            };

            // Set right to next in row, if not at end of row
            if ((i + 1) % itemsPerColumn != 0 && i + 1 < selectables.Count)
                nav.selectOnRight = selectables[i + 1];

            // Set left to previous in row, if not at start of row
            if (i % itemsPerColumn != 0)
                nav.selectOnLeft = selectables[i - 1];

            // Set down to the item in the next column (same row), if exists
            if (i < itemsPerColumn && i + itemsPerColumn < selectables.Count)
                nav.selectOnDown = selectables[i + itemsPerColumn];

            // Set up to the item in the previous column (same row), if exists
            if (i >= itemsPerColumn)
                nav.selectOnUp = selectables[i - itemsPerColumn];

            selectables[i].navigation = nav;
        }
        initialized = true;
    }

    public bool OpenItemSelectionMenu(ItemMenuSlots itemMenuSlot, ItemLevel itemLevel)
    {
        Initialize();

        List<Item> filteredItems = ItemManager.GetItemListByItemLevel(itemLevel);

        if (filteredItems.Count <= 0) { return false; }

        currentSlot = itemMenuSlot;
        currentLevel = itemLevel;
        int totalItems = 2 * itemsPerColumn;

        // Hide all itemPool objects first
        foreach (var go in itemPool)
            go.SetActive(false);

        // Show and populate only the filtered items (up to totalItems)
        for (int i = 0; i < totalItems; i++)
        {
            if (i < filteredItems.Count)
            {
                var go = itemPool[i];
                go.SetActive(true);

                // Set item data
                var itemSlot = go.GetComponent<ItemSelectionUI_item>();
                if (itemSlot != null)
                {
                    // Optionally set image/icon if available
                    itemSlot.item = filteredItems[i];
                    itemSlot.GetComponent<Image>().sprite = itemSlot.item.itemIcon;
                }
            }
        }
        EventSystemExtension.SetSelectObject(itemPool[0]);
        return true;
    }

    public void EquipItem(Item item)
    {
        ItemManager.EquipItem(currentSlot.slotIndex, item);
        MenuManager.UpdateItemSlotUI();
    }

    public void CloseItemSelectionMenu()
    {
        MenuManager.instance.CloseItemSelectionMenu();
    }

    public override void OnSelect(BaseEventData eventData)
    {
        // update item name and description based on selected item
        var selected = EventSystem.current.currentSelectedGameObject;
        if (selected != null)
        {
            // Example: If your itemPrefab has a script with name/description, fetch and display here
            var itemData = selected.GetComponent<ItemSelectionUI_item>();
            if (itemData != null)
            {
                itemData.OnSelect();
                itemName.text = itemData.item.itemName;
                itemDescription.text = itemData.item.itemDescription;
            }
        }
    }

    public override void OnDeselect(BaseEventData eventData)
    {
        var selected = EventSystem.current.currentSelectedGameObject;
        if (selected != null)
        {
            // Example: If your itemPrefab has a script with name/description, fetch and display here
            var itemData = selected.GetComponent<ItemSelectionUI_item>();
            if (itemData != null)
            {
                itemData.OnDeselect();
            }
        }
    }
}