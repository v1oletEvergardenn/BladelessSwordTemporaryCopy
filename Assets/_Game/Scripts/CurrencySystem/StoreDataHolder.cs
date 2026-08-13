using EditorAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[System.Serializable]
//public struct StoreItem
//{
//    public TradeItemSO item;
//    //[Range(-1, 50)] public int quantity; // Set in Inspector: -1 means infinite, otherwise finite
//}

public class StoreDataHolder : MonoBehaviour
{
    [SerializeField] public TradeItemSO[] itemsForSale;
    [SerializeField][Range(-1, 100)] public int[] remainingQuantities;

    private void Awake()
    {
        //remainingQuantities = new int[itemsForSale.Length];
        //for (int i = 0; i < itemsForSale.Length; i++)
        //{
        //    remainingQuantities[i] = itemsForSale[i].quantity;
        //}
    }
}