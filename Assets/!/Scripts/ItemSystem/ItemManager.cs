using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class ItemManager : MonoBehaviour
{
    public static ItemManager instance;
    private Dictionary<int, Item> allItemDictionary;
    private List<Item> allItems = new List<Item>();

    public List<ItemModule> availableItemModules = new List<ItemModule>();

    public ItemModule currentItemModule { get; private set; }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;

            BuildItemDictionary();
            LoadAllItems();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        OnUpdate();
    }

    #region Entry Methods

    public static void OnEquip()
    {
        foreach (Item item in GetEquippedItems()) item.OnEquip();
    }

    public static void OnUnequip()
    {
        foreach (Item item in GetEquippedItems()) item.OnUnequip();
    }

    public static void OnUpdate()
    {
        foreach (Item item in GetEquippedItems()) item.OnUpdate();
    }

    public static void OnDeath()
    {
        foreach (Item item in GetEquippedItems()) item.OnDeath();
    }

    public static void OnAttack()
    {
        foreach (Item item in GetEquippedItems()) item.OnAttack();
    }

    #endregion Entry Methods

    #region Utilities

    public static Item GetItemByID(int id)
    {
        foreach (Item item in instance.allItems)
        {
            if (item.itemID == id) return item;
        }
        return null;
    }

    public static List<Item> GetItemListByItemLevel(ItemLevel itemLevel)
    {
        List<Item> itemList = new List<Item>();
        foreach (Item item in instance.allItems)
        {
            if (item.itemLevel == itemLevel)
            {
                itemList.Add(item);
            }
        }
        return itemList;
    }

    public static List<Item> GetEquippedItems()
    {
        return GetCurrentItemModule().GetEquippedItems();
    }

    private void BuildItemDictionary()
    {
        allItemDictionary = new Dictionary<int, Item>();
        foreach (var item in allItems)
        {
            allItemDictionary[item.itemID] = item;
        }
    }

    private void LoadAllItems()
    {
        allItems.Clear();
        // Loads all Item ScriptableObjects from Resources/Items/
        Item[] loadedItems = Resources.LoadAll<Item>("Items");
        allItems.AddRange(loadedItems);
    }

    public static Item GetItemByName(string name)
    {
        foreach (Item item in instance.allItems)
        {
            if (item.itemName == name)
            {
                return item;
            }
        }
        return null;
    }

    public static void EquipItem(int index, Item item)
    {
        instance.currentItemModule.EquipItem(index, item);
    }

    public static void UnequipItem(int index)
    {
        instance.currentItemModule.UnequipItem(index);
    }

    public static void GetItemSlotFromCurrentModule(int index, out ItemSlot itemSlot)
    {
        itemSlot = GetCurrentItemModule().GetItemSlot(index);
    }

    public static ItemModule GetCurrentItemModule()
    {
        if (instance.currentItemModule == null)
        {
            instance.currentItemModule = instance.availableItemModules[0];
        }
        return instance.currentItemModule;
    }

    public static void SetCurrentItemModule(ItemModule itemModule)
    {
        instance.currentItemModule = itemModule;
    }
}

#endregion Utilities