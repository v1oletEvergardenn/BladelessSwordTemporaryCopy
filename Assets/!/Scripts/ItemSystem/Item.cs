using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public enum EffectType
{
    Heal,
    IncreaseDamage,
    RestoreEnergy,
    IncreaseMaxEnergy,
    RestoreHSpoint
}

public enum ItemLevel
{
    LevelOne,
    LevelTwo,
    LevelThree,
    None
}

[System.Serializable]
public struct ItemEffect
{
    public EffectType effectType;

    public float factor;

    private Color GetColor
    {
        get
        {
            switch (effectType)
            {
                case EffectType.Heal: return Color.green;
                case EffectType.IncreaseDamage: return Color.red;
                case EffectType.RestoreEnergy: return Color.magenta;
                case EffectType.IncreaseMaxEnergy: return Color.yellow;
                case EffectType.RestoreHSpoint: return new Color(1f, 0.5f, 0f); // Orange
                default: return Color.white;
            }
        }
    }

    public void ApplyEffect()
    {
        // Example effect application logic
        switch (effectType)
        {
            case EffectType.Heal:
                // Apply healing logic
                Debug.Log($"Healing for {factor} points.");
                break;

            case EffectType.IncreaseDamage:
                // Apply damage logic
                Debug.Log($"Damaging for {factor} points.");
                break;

            case EffectType.RestoreEnergy:
                // Apply energy restoration logic
                Debug.Log($"Restoring {factor} energy.");
                break;

            case EffectType.IncreaseMaxEnergy:
                // Apply max energy increase logic
                Debug.Log($"Increasing max energy by {factor}.");
                break;

            case EffectType.RestoreHSpoint:
                // Apply health point restoration logic
                Debug.Log($"Restoring {factor} health points.");
                break;

            default:
                Debug.Log("Unknown effect.");
                break;
        }
    }
}

[System.Serializable]
public abstract class Item : ScriptableObject
{
    public string itemName;
    public Sprite itemIcon;
    public string itemDescription;
    public int itemID;
    public ItemLevel itemLevel;

    public float duration = 0f; // 0 means permanent
    public bool isStackable = false;
    public int maxStack = 1;
    public int currentStack = 1;
    public float cooldown = 0f;
    public bool isActive = true;

    public List<ItemEffect> onEquipEffects;
    public List<ItemEffect> onUnequipEffects;
    public List<ItemEffect> onUpdateEffects;
    public List<ItemEffect> onDeathEffects;
    public List<ItemEffect> onAttackEffects;

    public virtual void OnEquip()
    { ApplyEffects(onEquipEffects); }

    public virtual void OnUnequip()
    { ApplyEffects(onUnequipEffects); }

    public virtual void OnUpdate()
    { ApplyEffects(onUpdateEffects); }

    public virtual void OnDeath()
    { ApplyEffects(onDeathEffects); }

    public virtual void OnAttack()
    {
        ApplyEffects(onAttackEffects);
    }

    #region Utilities

#if UNITY_EDITOR

    private void OnValidate()
    {
        if (itemID == 0)
        {
            ResetID();
        }
    }

#endif

    public void ResetID()
    {
        const string LastItemIDKey = "ItemSystem_LastItemID";
        int lastID = EditorPrefs.GetInt(LastItemIDKey, 1);
        itemID = lastID++;
        EditorPrefs.SetInt(LastItemIDKey, lastID);
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
    }

    public void ApplyEffects(List<ItemEffect> effects)
    {
        foreach (var effect in effects) effect.ApplyEffect();
    }

    #endregion Utilities
}