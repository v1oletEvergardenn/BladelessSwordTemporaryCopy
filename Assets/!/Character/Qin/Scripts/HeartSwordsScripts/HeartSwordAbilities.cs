using DG.Tweening;
using EditorAttributes;
using Microlight.MicroBar;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Void = EditorAttributes.Void;
using Cinemachine;

public class HeartSwordAbilities : MonoBehaviour
{
    public static HeartSwordAbilities instance;
    [FoldoutGroup("reference", nameof(selfPooler), nameof(rb), nameof(anim), nameof(hsUpgradeCam))] public Void referenceVoid;
    [SerializeField, HideProperty] public InternalObjectPooler selfPooler;
    [SerializeField, HideProperty] public Rigidbody2D rb;
    [SerializeField, HideProperty] public Animator anim;
    [SerializeField, HideProperty] public CameraRegister hsUpgradeCam;

    [FoldoutGroup("HeartSword References", nameof(hs_CriticalSlash), nameof(hs_CounterAttack), nameof(hs_SlashWave))]
    public Void heartSwordReferencesVoid;

    [SerializeField, HideProperty] public HS_CriticalSlash hs_CriticalSlash;
    [SerializeField, HideProperty] public HS_CounterAttack hs_CounterAttack;
    [SerializeField, HideProperty] public HS_SlashWave hs_SlashWave;
    [HideProperty] public List<IHeartSwordAbility> allAbilities = new List<IHeartSwordAbility>();

    [GUIColor(GUIColor.Lime)]
    [FoldoutGroup("HeartSword Abilities", nameof(maxHS_point),
         nameof(HS_points), nameof(abilityWest), nameof(abilityEast),
         nameof(abilityNorth), nameof(currentActivatedAbility))]
    public Void heartSwordVoid;

    [SerializeField, HideProperty] private float maxHS_point = 3;
    [SerializeField, HideProperty] public List<HeartSwordUIPoint> HS_points = new List<HeartSwordUIPoint>();
    [SerializeField, HideProperty] private HSEnum abilityWest;
    [SerializeField, HideProperty] private HSEnum abilityNorth;
    [SerializeField, HideProperty] private HSEnum abilityEast;
    [SerializeField, HideProperty] private HSEnum currentActivatedAbility;
    [SerializeField] public float currentHS_point { get; private set; } = 0;

    public void Awake()
    {
        instance = this;
        InitializeAbilityList();
    }

    private void InitializeAbilityList()
    {
        allAbilities.Clear();
        allAbilities.Add(hs_CounterAttack);
        allAbilities.Add(hs_CriticalSlash);
        allAbilities.Add(hs_SlashWave);
    }

    private void Start()
    {
        if (GetWestAbility() != null) GetWestAbility().EquipAbility();
        if (GetNorthAbility() != null) GetNorthAbility().EquipAbility();
        if (GetEastAbility() != null) GetEastAbility().EquipAbility();
        InitializeHS_UI();
    }

    private void Update()
    {
        currentHS_point = Mathf.Clamp(currentHS_point, 0, maxHS_point);
        UILitEffect();
    }

    public Sprite GetInputSpriteOnAbility(IHeartSwordAbility ability)
    {
        if (ability == GetWestAbility())
        {
            return InputMaster.instance.icons.gamePadicons.GetSprite(InputKeyType.AbilityWest_key);
        }
        else if (ability == GetNorthAbility())
        {
            return InputMaster.instance.icons.gamePadicons.GetSprite(InputKeyType.AbilityNorth_key);
        }
        else if (ability == GetEastAbility())
        {
            return InputMaster.instance.icons.gamePadicons.GetSprite(InputKeyType.AbilityEast_key);
        }
        else return null;
    }

    public List<IHeartSwordAbility> GetUnlockedAbilities()
    {
        List<IHeartSwordAbility> unlockedAbilities = new List<IHeartSwordAbility>();
        foreach (var ability in allAbilities)
        {
            if (ability.IsUnlocked()) unlockedAbilities.Add(ability);
        }
        return unlockedAbilities;
    }

    public IHeartSwordAbility GetWestAbility() => GetAbilityByEnum(abilityWest);

    public IHeartSwordAbility GetNorthAbility() => GetAbilityByEnum(abilityNorth);

    public IHeartSwordAbility GetEastAbility() => GetAbilityByEnum(abilityEast);

    public IHeartSwordAbility GetCurrentActivatedAbility() => GetAbilityByEnum(currentActivatedAbility);

    public void UnequipAbility(IHeartSwordAbility ability)
    {
        if (ability == GetWestAbility()) abilityWest = HSEnum.None;
        if (ability == GetEastAbility()) abilityEast = HSEnum.None;
        if (ability == GetNorthAbility()) abilityNorth = HSEnum.None;
        ability.UnequipAbility();
    }

    public void EquipAbility(IHeartSwordAbility ability, AbilitySlot slot, bool unequipOldAbility)
    {
        switch (slot)
        {
            case AbilitySlot.West:
                if (unequipOldAbility && GetWestAbility() != null) GetWestAbility().UnequipAbility();
                abilityWest = GetAbilityIndex(ability);
                if (ability != null) ability.EquipAbility();
                break;

            case AbilitySlot.North:
                if (unequipOldAbility && GetNorthAbility() != null) GetNorthAbility().UnequipAbility();
                abilityNorth = GetAbilityIndex(ability);
                if (ability != null) ability.EquipAbility();
                break;

            case AbilitySlot.East:
                if (unequipOldAbility && GetEastAbility() != null) GetEastAbility().UnequipAbility();
                abilityEast = GetAbilityIndex(ability);
                if (ability != null) ability.EquipAbility();
                break;
        }
    }

    public void CancelAllAbilities()
    {
        if (GetWestAbility() != null && GetWestAbility().GetCurrentAttribute().canBeStopped) GetWestAbility().CancelAction();
        if (GetNorthAbility() != null && GetNorthAbility().GetCurrentAttribute().canBeStopped) GetNorthAbility().CancelAction();
        if (GetEastAbility() != null && GetEastAbility().GetCurrentAttribute().canBeStopped) GetEastAbility().CancelAction();
    }

    public void ActivateAbility(IHeartSwordAbility ability)
    {
        Deactivateability(GetEastAbility());
        Deactivateability(GetNorthAbility());
        Deactivateability(GetWestAbility());
        if (ability.ActivateAbility())
        {
            currentActivatedAbility = GetAbilityIndex(ability);
        }
    }

    public bool CheckAnyPerformingAbility()
    {
        if (GetEastAbility() != null && GetEastAbility().isPerforming) return true;
        if (GetWestAbility() != null && GetWestAbility().isPerforming) return true;
        if (GetNorthAbility() != null && GetNorthAbility().isPerforming) return true;
        return false;
    }

    public void Deactivateability(IHeartSwordAbility ability)
    {
        if (ability == null) return;
        if (currentActivatedAbility == GetAbilityIndex(ability))
        {
            currentActivatedAbility = HSEnum.None;
        }
        ability.DeactivateAbility();
    }

    public void ModifyHSPoint(float amount)
    {
        currentHS_point += amount;
        currentHS_point = Mathf.Clamp(currentHS_point, 0, maxHS_point);
        RefreshHS_UI();
    }

    private float alpha = 0;
    private bool isIncreasing = true;

    public void UILitEffect()
    {
        if (isIncreasing == true)
        {
            alpha += Time.deltaTime * 1f;
            if (alpha >= 1f)
            {
                isIncreasing = false;
            }
        }
        else
        {
            alpha -= Time.deltaTime * 1f;
            if (alpha <= 0.3f)
            {
                isIncreasing = true;
            }
        }

        alpha = Mathf.Clamp(alpha, 0.3f, 1f);
        for (int i = 0; i < HS_points.Count; i++)
        {
            HS_points[i].lit.color = new Color(1, 1, 1, alpha);
        }
    }

    private Coroutine co_refreshHS_UI;

    public float GetMaxHSpoint()
    {
        return maxHS_point;
    }

    public float GetCurrentHSpoint()
    {
        return currentHS_point;
    }

    public void SetMaxHSPoint(float point)
    {
        maxHS_point = point;
        RefreshHS_UI();
    }

    public void SetCurrentHSPoint(float point)
    {
        currentHS_point = point;
        RefreshHS_UI();
    }

    public void InitializeHS_UI()
    {
        for (int i = 0; i < HS_points.Count; i++)
        {
            HS_points[i].border.gameObject.SetActive(true);
            HS_points[i].lit.gameObject.SetActive(false);
            HS_points[i].fill.fillAmount = 0;
        }
    }

    private float hs_point_before_refresh = 0;

    public void RefreshHS_UI()
    {
        if (co_refreshHS_UI != null) { StopCoroutine(co_refreshHS_UI); }
        StopAllCoroutines();

        for (int i = 0; i < HS_points.Count; i++)
        {
            if (i < (int)maxHS_point) HS_points[i].gameObject.SetActive(true);
            else HS_points[i].gameObject.SetActive(false);
        }

        co_refreshHS_UI = StartCoroutine(Refresh_HS_UI_state());
    }

    public IEnumerator Refresh_HS_UI_state()
    {
        bool is_Minus = hs_point_before_refresh > currentHS_point;
        hs_point_before_refresh = currentHS_point;
        if (is_Minus)
        {
            for (int i = (int)maxHS_point - 1; i >= 0; i--)
            {
                yield return StartCoroutine(Fill_HS_UI(HS_points[i], 0.05f, Mathf.Clamp01(currentHS_point - i)));
            }
        }
        else
        {
            for (int i = 0; i < (int)maxHS_point; i++)
            {
                yield return StartCoroutine(Fill_HS_UI(HS_points[i], 0.05f, Mathf.Clamp01(currentHS_point - i)));
            }
        }

        yield return null;
    }

    public IEnumerator Fill_HS_UI(HeartSwordUIPoint point, float duration, float fillAmount)
    {
        float time = duration * Mathf.Abs(fillAmount - point.fill.fillAmount);
        float elapsedTime = 0f;
        if (point.fill.fillAmount != fillAmount)
        {
            while (elapsedTime <= time)
            {
                elapsedTime += Time.unscaledDeltaTime;
                point.fill.fillAmount = Mathf.Lerp(point.fill.fillAmount, fillAmount, elapsedTime / time);
                yield return null;
            }
            point.fill.fillAmount = fillAmount;
        }

        if (point.fill.fillAmount == 1)
        {
            point.border.gameObject.SetActive(false);
            point.lit.gameObject.SetActive(true);
        }
        else
        {
            point.border.gameObject.SetActive(true);
            point.lit.gameObject.SetActive(false);
        }
    }

    public void Save(ref HeartSwordSaveData data)
    {
        data.westAbilityIndex = abilityWest;
        data.eastAbilityIndex = abilityEast;
        data.northAbilityIndex = abilityNorth;
        data.hsAbilities = new List<HSAblitySaveData>();
        foreach (var ability in allAbilities)
        {
            HSAblitySaveData abilityData = new HSAblitySaveData
            {
                branchIndex = ability.GetBranchIndex(),
                unlocked = ability.IsUnlocked(),
                toggleToActivate = ability.toggleToActivate,
                branches = new List<HSAblityBranchSaveData>()
            };
            data.hsAbilities.Add(abilityData);
        }
    }

    public void Load(HeartSwordSaveData data)
    {
        abilityEast = data.eastAbilityIndex;
        EquipAbility(GetEastAbility(), AbilitySlot.East, true);
        abilityNorth = data.northAbilityIndex;
        EquipAbility(GetNorthAbility(), AbilitySlot.North, true);
        abilityWest = data.westAbilityIndex;
        EquipAbility(GetWestAbility(), AbilitySlot.West, true);
        for (int i = 0; i < allAbilities.Count; i++)
        {
            //allAbilities[i].ChangeBranch(data.hsAbilities[i].branchIndex);
            allAbilities[i].SetLockedStates(data.hsAbilities[i].unlocked);
            allAbilities[i].toggleToActivate = data.hsAbilities[i].toggleToActivate;
            //for (int j = 0; j < allAbilities[i].GetBranches().Count; j++)
            //{
            //allAbilities[i].GetBranches()[j].learned = data.hsAbilities[i].branches[j].learned;
            //}
        }
    }

    public void SwitchToHSUpgradeCamera()
    {
        hsUpgradeCam.SwitchThisCam();
    }

    public IHeartSwordAbility GetAbilityByEnum(HSEnum abilityEnum)
    {
        switch (abilityEnum)
        {
            case HSEnum.HSCounterAttack:
                return hs_CounterAttack;

            case HSEnum.CriticalSlash:
                return hs_CriticalSlash;

            case HSEnum.SlashWave:
                return hs_SlashWave;

            case HSEnum.None:
            default:
                return null;
        }
    }

    public HSEnum GetAbilityIndex(IHeartSwordAbility ability)
    {
        if (ability == hs_CounterAttack) return HSEnum.HSCounterAttack;
        if (ability == hs_CriticalSlash) return HSEnum.CriticalSlash;
        if (ability == hs_SlashWave) return HSEnum.SlashWave;
        return HSEnum.None;
    }
}

public enum HSEnum
{
    None,
    HSCounterAttack,
    CriticalSlash,
    SlashWave,
}