using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ItemSlot
{
    [EnumToggleButtons] public ItemLevel slotLevel;
    public int itemID;
    public Item item;

    public bool CheckValidate()
    {
        if (slotLevel == ItemLevel.None) return false;
        if (itemID == 0) return false;
        Item temp = ItemManager.GetItemByID(itemID);
        if (temp == null) return false;
        if (temp != item) item = temp;
        return true;
    }

    public void AssignItem(Item newItem)
    {
        Debug.Log("Assigning item: " + newItem);
        if (newItem != null) itemID = newItem.itemID;
        item = newItem;
    }

    public void AssignItem(int ItemID)
    {
        Item temp = ItemManager.GetItemByID(itemID);
        itemID = ItemID;
        item = temp;
    }

    public void UnEquipItem()
    {
        itemID = 0;
        item = null;
    }
}

[System.Serializable]
[CreateAssetMenu(fileName = "ItemModule", menuName = "ItemModule")]
public class ItemModule : ScriptableObject
{
    public string moduleName;

    public ItemSlot item1;
    public ItemSlot item2;
    public ItemSlot item3;
    public ItemSlot item4;
    public ItemSlot item5;
    public ItemSlot item6;

    public List<Item> GetEquippedItems()
    {
        List<Item> equippedItems = new List<Item>();
        if (item1.CheckValidate()) equippedItems.Add(item1.item);
        if (item2.CheckValidate()) equippedItems.Add(item2.item);
        if (item3.CheckValidate()) equippedItems.Add(item3.item);
        if (item4.CheckValidate()) equippedItems.Add(item4.item);
        if (item5.CheckValidate()) equippedItems.Add(item5.item);
        if (item6.CheckValidate()) equippedItems.Add(item6.item);
        return equippedItems;
    }

    public void EquipItem(int index, Item item)
    {
        List<ItemSlot> itemSlots = new List<ItemSlot> { item1, item2, item3, item4, item5, item6 };

        for (int i = 0; i < 6; i++)
        {
            if (i == index) continue;
            if (itemSlots[i].item == item)
            {
                // Swap items
                Item tempItem = itemSlots[index].item;
                itemSlots[index].AssignItem(item);
                itemSlots[i].AssignItem(tempItem);
                return;
            }
        }
        itemSlots[index].AssignItem(item); return;
    }

    public void UnequipItem(int index)
    {
        switch (index)
        {
            case 0:
                item1.UnEquipItem();
                break;

            case 1:
                item2.UnEquipItem();
                break;

            case 2:
                item3.UnEquipItem();
                break;

            case 3:
                item4.UnEquipItem();
                break;

            case 4:
                item5.UnEquipItem();
                break;

            case 5:
                item6.UnEquipItem();
                break;
        }
    }

    public ItemSlot GetItemSlot(int index)
    {
        return index switch
        {
            0 => item1,
            1 => item2,
            2 => item3,
            3 => item4,
            4 => item5,
            5 => item6,
            _ => throw new System.IndexOutOfRangeException("Invalid item slot index"),
        };
    }

    public void ClearAllSlots()
    {
        List<ItemSlot> itemSlots = new List<ItemSlot> { item1, item2, item3, item4, item5, item6 };

        for (int i = 0; i < 6; i++)
        {
            itemSlots[i].UnEquipItem();
        }
    }
}