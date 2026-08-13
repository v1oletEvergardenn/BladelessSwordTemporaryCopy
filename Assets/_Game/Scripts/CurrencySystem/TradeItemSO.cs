using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum TradeItemType
{
    All,
    Consumable,
    Equipment,
    Material,
    QuestItem,
}

[CreateAssetMenu(fileName = "TradeItem", menuName = "CurrencySystem/Trade Item")]
public class TradeItemSO : ScriptableObject
{
    public string itemName;
    public TradeItemType itemType;
    public Sprite icon;
    [Min(0)] public int price;
    [TextArea(4, 10)] public string description;
}