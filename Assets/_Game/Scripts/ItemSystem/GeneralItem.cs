using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "NewItem", menuName = "Items/GeneralItem")]
public class GeneralItem : Item
{
    public override void OnEquip()
    {
        Debug.Log("Equipped " + itemName);
        ApplyEffects(onEquipEffects);
    }

    public override void OnUnequip()
    {
        Debug.Log("Unequipped " + itemName);
        ApplyEffects(onUnequipEffects);
    }
}