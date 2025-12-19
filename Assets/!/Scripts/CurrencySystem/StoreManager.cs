using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StoreEnum
{
    None,
    YingYangfishStore
}

[System.Serializable]
public struct StoreSaveData
{
    public QuantityData[] remainingQuantities;
}

[System.Serializable]
public struct QuantityData
{
    public int[] quantity;
}

public class StoreManager : MonoBehaviour
{
    public static StoreManager instance;

    public StoreDataHolder yinYangFishStore;

    private List<StoreDataHolder> allStores = new List<StoreDataHolder>();

    private void Awake()
    {
        instance = this;
        allStores = GetAllStores();
    }

    public void Save(ref StoreSaveData saveData)
    {
        saveData.remainingQuantities = new QuantityData[allStores.Count];
        for (int i = 0; i < allStores.Count; i++)
        {
            saveData.remainingQuantities[i].quantity = new int[allStores[i].remainingQuantities.Length];
            for (int j = 0; j < allStores[i].remainingQuantities.Length; j++)
            {
                saveData.remainingQuantities[i].quantity[j] = allStores[i].remainingQuantities[j];
            }
        }
    }

    public void Load(StoreSaveData saveData)
    {
        for (int i = 0; i < allStores.Count; i++)
        {
            for (int j = 0; j < saveData.remainingQuantities[i].quantity.Length; j++)
            {
                allStores[i].remainingQuantities[j] = saveData.remainingQuantities[i].quantity[j];
            }
        }
    }

    public List<StoreDataHolder> GetAllStores()
    {
        List<StoreDataHolder> stores = new List<StoreDataHolder>();
        foreach (StoreEnum storeEnum in Enum.GetValues(typeof(StoreEnum)))
        {
            if (storeEnum == StoreEnum.None)
                continue; // Skip None if you don't want it

            StoreDataHolder store = GetStore(storeEnum);
            if (store != null)
                stores.Add(store);
        }
        return stores;
    }

    public static StoreDataHolder GetStore(StoreEnum store)
    {
        switch (store)
        {
            case StoreEnum.YingYangfishStore:
                return instance.yinYangFishStore;

            case StoreEnum.None:
                Debug.LogWarning("Didn't set Store.");
                return null;

            default:
                Debug.LogWarning("Store not found.");
                return null;
        }
    }
}