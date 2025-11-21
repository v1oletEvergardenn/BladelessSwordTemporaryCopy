using UnityEngine;

[System.Serializable]
public struct StoreItem
{
    public TradeItemSO item;
    [Range(-1, 50)] public int quantity; // Set in Inspector: -1 means infinite, otherwise finite
}

public class Store : MonoBehaviour
{
    [SerializeField] public StoreItem[] itemsForSale;
    [HideInInspector] public int[] remainingQuantities;

    private void Awake()
    {
        remainingQuantities = new int[itemsForSale.Length];
        for (int i = 0; i < itemsForSale.Length; i++)
        {
            // If quantity is -1, treat as infinite (store as -1)
            remainingQuantities[i] = itemsForSale[i].quantity;
        }
    }

    // Call this to display store UI and available items
    public void OpenStore()
    {
        MenuManager.instance.OpenStoreCanvas(this);
    }

    // Call this when the player selects an item to buy
    public bool BuyItem(int itemIndex)
    {
        if (itemIndex < 0 || itemIndex >= itemsForSale.Length)
        {
            Debug.LogWarning("Invalid item index.");
            return false;
        }

        if (IsSoldOut(itemIndex))
        {
            Debug.Log("Item is sold out.");
            // TODO: Show sold out UI feedback
            return false;
        }

        TradeItemSO item = itemsForSale[itemIndex].item;
        bool success = CurrencyManager.instance.TryBuyItem(item);

        if (success)
        {
            // Only decrement if not infinite
            if (remainingQuantities[itemIndex] != -1)
            {
                remainingQuantities[itemIndex]--;
                if (remainingQuantities[itemIndex] <= 0)
                {
                    remainingQuantities[itemIndex] = 0;
                    // TODO: Update store UI to reflect sold out state
                    Debug.Log($"Player bought {item.itemName}. Now sold out.");
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
        else
        {
            // TODO: Show not enough currency UI feedback
            Debug.Log("Purchase failed.");
            return false;
        }
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
        return itemsForSale[itemIndex].item;
    }
}