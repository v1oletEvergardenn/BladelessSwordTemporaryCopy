using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TradeItemBtn : MonoBehaviour
{
    public Store store;
    public int itemIndex;
    public int remaining;
    public bool soldOut;

    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI itemPriceText;
    public TextMeshProUGUI itemRemainingText;
    public Image itemIcon;
    public Image soldOutOverLay;

    /// <summary>
    /// Sets up the button UI with the store and item index.
    /// </summary>
    public void SetItem(Store _store, int _itemIndex)
    {
        store = _store;
        itemIndex = _itemIndex;
        remaining = store.GetRemainingQuantity(itemIndex);
        soldOut = store.IsSoldOut(itemIndex);

        // Get item data
        TradeItemSO itemData = store.GetItem(itemIndex);

        // Update UI
        itemNameText.text = itemData.itemName;
        itemPriceText.text = itemData.price.ToString();
        itemRemainingText.text = soldOut ? "Sold Out" : $"x{remaining}";
        itemIcon.sprite = itemData.icon;

        if (soldOut)
        {
            soldOutOverLay.gameObject.SetActive(true);
        }
        else
        {
            soldOutOverLay.gameObject.SetActive(false);
        }

        // Set price text color based on currency
        if (CurrencyManager.instance != null && CurrencyManager.instance.Currency < itemData.price)
        {
            itemPriceText.color = Color.red;
        }
        else
        {
            itemPriceText.color = Color.white;
        }
    }

    /// <summary>
    /// Called when the buy button is clicked.
    /// </summary>
    public void OnClick_BuyItem()
    {
        bool success = store.BuyItem(itemIndex);
        if (soldOut) success = false;
        if (success)
        {
            // Update UI after purchase
            MenuManager.instance.UpdateStoreUI(store);
        }
        else
        {
            // Shake the button horizontally if purchase fails
            var rect = GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.DOComplete(); // Stop any previous tweens
                rect.DOShakeAnchorPos(0.1f, new Vector2(40f, 0f), 10, 20, false, true)
                    .OnComplete(() =>
                    {
                        rect.DOShakeAnchorPos(0.1f, new Vector2(20f, 0f), 10, 20, false, true);
                    });
            }
        }
    }
}