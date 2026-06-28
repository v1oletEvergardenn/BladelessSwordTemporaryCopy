using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Store : MonoBehaviour
{
    public StoreEnum store;
    public int[] remainingQuantities => StoreManager.GetStore(store).remainingQuantities;
    public TradeItemSO[] itemsForSale => StoreManager.GetStore(store).itemsForSale;

    // Call this to display store UI and available items
    public void OpenStore()
    {
        InputMaster.SwitchToUIAction();
        CharacterController2D.instance.RunToPosition(transform.position + new Vector3(1, 0, 0),
            false,
            () =>
            {
                CharacterController2D.instance.FaceTarget(transform);
                MenuManager.instance.OpenStoreCanvas(this);
            });
    }

    // Call this when the player selects an item to buy
    public void BuyItem(int itemIndex, TradeItemBtn btn)
    {
        if (itemIndex < 0 || itemIndex >= itemsForSale.Length)
        {
            Debug.LogWarning("Invalid item index.");
            return;
        }
        TradeItemSO item = itemsForSale[itemIndex];
        if (IsSoldOut(itemIndex) || !CurrencyManager.instance.hasEnoughCurrency(item.price))
        {
            btn.Fail();
            Debug.LogWarning("Not enough currency or item sold out.");
            return;
        }
        WarningSystem.ShowWarning($"Are you sure you want to buy {itemsForSale[itemIndex].name}?",
            () => CurrencyManager.instance.TryBuyItem(item, btn, this));
    }

    private int FindNextAvailableItem(int itemIndex)
    {
        // Collect all active item indices
        List<int> activeIndices = new List<int>();
        for (int i = 0; i < MenuManager.instance.tradeItemPool.Count; i++)
        {
            if (MenuManager.instance.tradeItemPool[i].activeInHierarchy)
            {
                activeIndices.Add(i);
            }
        }

        if (activeIndices.Count == 0)
            return -1; // No active items

        // Find the position of itemIndex in the active list
        int pos = activeIndices.IndexOf(itemIndex);

        if (pos == -1)
            return activeIndices[0]; // If itemIndex is not active, return first active

        // If itemIndex is the last active, return previous active
        if (pos == activeIndices.Count - 1)
            return activeIndices[Math.Max(0, pos - 1)];

        // Otherwise, return next active
        return activeIndices[pos + 1];
    }

    public bool SellItem(int itemIndex)
    {
        TradeItemSO item = itemsForSale[itemIndex];

        // Only decrement if not infinite
        if (remainingQuantities[itemIndex] != -1)
        {
            remainingQuantities[itemIndex]--;
            if (remainingQuantities[itemIndex] <= 0)
            {
                remainingQuantities[itemIndex] = 0;
                // TODO: Update store UI to reflect sold out state
                Debug.Log($"Player bought {item.itemName}. Now sold out.");

                EventSystem.current.SetSelectedGameObject(MenuManager.instance.tradeItemPool[FindNextAvailableItem(itemIndex)]);
            }
            else
            {
                // TODO: Update store UI to reflect new quantity
                Debug.Log($"Player bought {item.itemName}. {remainingQuantities[itemIndex]} left.");
            }
        }
        else
        {
            // Infinite stock, no decrement
            // TODO: Update store UI if needed
            Debug.Log($"Player bought {item.itemName}. (Infinite stock)");
        }
        return true;
    }

    // Optional: Call this to close the store UI
    public void CloseStore()
    {
        // TODO: Hide store UI
        Debug.Log("Store closed.");
    }

    // Helper for UI: check if an item is sold out
    public bool IsSoldOut(int itemIndex)
    {
        if (itemIndex < 0 || itemIndex >= remainingQuantities.Length) return false;
        // Infinite stock is never sold out
        if (remainingQuantities[itemIndex] == -1) return false;
        return remainingQuantities[itemIndex] <= 0;
    }

    // Helper for UI: get remaining quantity
    public int GetRemainingQuantity(int itemIndex)
    {
        if (itemIndex < 0 || itemIndex >= remainingQuantities.Length) return 0;
        return remainingQuantities[itemIndex];
    }

    // Helper for UI: get item info
    public TradeItemSO GetItem(int itemIndex)
    {
        if (itemIndex < 0 || itemIndex >= itemsForSale.Length) return null;
        return itemsForSale[itemIndex];
    }
}