using UnityEngine;

[System.Serializable]
public struct CurrencySaveData
{
    public int currency;
}

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager instance;

    [SerializeField]
    private int currency = 0;

    public int Currency => currency;

    public void Save(ref CurrencySaveData saveData)
    {
        saveData.currency = currency;
    }

    public void Load(CurrencySaveData saveData)
    {
        currency = saveData.currency;
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddCurrency(int amount)
    {
        currency += amount;
        if (currency < 0) currency = 0;
        // TODO: Update currency UI here
    }

    public bool SpendCurrency(int amount)
    {
        if (currency >= amount)
        {
            currency -= amount;
            // TODO: Update currency UI here
            return true;
        }
        return false;
    }

    public bool hasEnoughCurrency(int amount)
    {
        return currency >= amount;
    }

    /// <summary>
    /// Attempts to buy an item. Returns true if successful.
    /// </summary>
    public bool TryBuyItem(TradeItemSO item, TradeItemBtn btn, Store store)
    {
        if (item == null) return false;
        if (SpendCurrency(item.price))
        {
            // TODO: Grant the item to the player (e.g., add to inventory)
            Debug.Log($"Purchased {item.itemName} for {item.price} currency.");
            // TODO: Update inventory UI here
            store.SellItem(btn.itemIndex);
            btn.Success();
            return true;
        }
        else
        {
            Debug.Log("Not enough currency.");
            // TODO: Show insufficient funds UI feedback
            return false;
        }
    }
}