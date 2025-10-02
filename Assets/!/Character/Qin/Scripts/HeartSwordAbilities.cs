using DG.Tweening;
using EditorAttributes;
using Microlight.MicroBar;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Profiling.Memory.Experimental;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Void = EditorAttributes.Void;

public class HeartSwordAbilities : MonoBehaviour
{
    public static HeartSwordAbilities instance;
    [FoldoutGroup("reference", nameof(allAbilities_ref), nameof(selfPooler), nameof(rb), nameof(anim))] public Void referenceVoid;
    [SerializeField, HideProperty] public InternalObjectPooler selfPooler;
    [SerializeField, HideProperty] public Rigidbody2D rb;
    [SerializeField, HideProperty] public Animator anim;
    [SerializeField, HideProperty] public List<GameObject> allAbilities_ref = new List<GameObject>();

    [GUIColor(GUIColor.Lime)]
    [FoldoutGroup("HeartSword Abilities", nameof(maxHS_point),
         nameof(HS_points), nameof(abilityWest), nameof(abilityEast),
         nameof(abilityNorth), nameof(currentActivatedAbility))]
    public Void heartSwordVoid;

    [HideProperty] public List<IHeartSwordAbility> allAbilities = new List<IHeartSwordAbility>();
    [SerializeField, HideProperty] private float maxHS_point = 3;
    [SerializeField, HideProperty] public List<HeartSwordUIPoint> HS_points = new List<HeartSwordUIPoint>();
    [SerializeField, HideProperty] public IHeartSwordAbility abilityWest;
    [SerializeField, HideProperty] public IHeartSwordAbility abilityNorth;
    [SerializeField, HideProperty] public IHeartSwordAbility abilityEast;
    [SerializeField, HideProperty] public IHeartSwordAbility currentActivatedAbility = null;
    [SerializeField] public float currentHS_point { get; private set; } = 0;

    public void Awake()
    {
        instance = this;
        InitializeAbilityList();
    }

    private void InitializeAbilityList()
    {
        allAbilities.Clear();
        foreach (var reference in allAbilities_ref)
        {
            if (reference != null)
            {
                var ability = reference.GetComponentInChildren<IHeartSwordAbility>();
                if (ability != null)
                {
                    allAbilities.Add(ability);
                }
            }
        }
    }

    private void Start()
    {
        if (abilityWest != null) abilityWest.EquipAbility();
        if (abilityNorth != null) abilityNorth.EquipAbility();
        if (abilityEast != null) abilityEast.EquipAbility();
        InitializeHS_UI();
    }

    private void Update()
    {
        currentHS_point = Mathf.Clamp(currentHS_point, 0, maxHS_point);
        UILitEffect();
    }

    public Sprite GetInputSpriteOnAbility(IHeartSwordAbility ability)
    {
        if (ability == abilityWest)
        {
            return InputMaster.instance.icons.gamePadicons.GetSprite(InputKeyType.AbilityWest_key);
        }
        else if (ability == abilityNorth)
        {
            return InputMaster.instance.icons.gamePadicons.GetSprite(InputKeyType.AbilityNorth_key);
        }
        else if (ability == abilityEast)
        {
            return InputMaster.instance.icons.gamePadicons.GetSprite(InputKeyType.AbilityEast_key);
        }
        else return null;
    }

    public void UnequipAbility(IHeartSwordAbility ability)
    {
        if (ability == abilityWest) abilityWest = null;
        if (ability == abilityEast) abilityEast = null;
        if (ability == abilityNorth) abilityNorth = null;
        ability.UnequipAbility();
    }

    public void EquipAbility(IHeartSwordAbility ability, AbilitySlot slot, bool unequipOldAbility)
    {
        switch (slot)
        {
            case AbilitySlot.West:
                if (unequipOldAbility && abilityWest != null) abilityWest.UnequipAbility();
                abilityWest = ability;
                ability.EquipAbility();
                break;

            case AbilitySlot.North:
                if (unequipOldAbility && abilityNorth != null) abilityNorth.UnequipAbility();
                abilityNorth = ability;
                ability.EquipAbility();
                break;

            case AbilitySlot.East:
                if (unequipOldAbility && abilityEast != null) abilityEast.UnequipAbility();
                abilityEast = ability;
                ability.EquipAbility();
                break;
        }
    }

    public void CancelAllAbilities()
    {
        if (abilityWest != null && abilityWest.canBeStopped) abilityWest.CancelAction();
        if (abilityNorth != null && abilityNorth.canBeStopped) abilityNorth.CancelAction();
        if (abilityEast != null && abilityEast.canBeStopped) abilityEast.CancelAction();
    }

    public void ActivateAbility(IHeartSwordAbility ability)
    {
        Deactivateability(abilityEast);
        Deactivateability(abilityNorth);
        Deactivateability(abilityWest);
        if (ability.ActivateAbility())
        {
            currentActivatedAbility = ability;
        }
    }

    public bool CheckAnyPerformingAbility()
    {
        if (abilityEast != null && abilityEast.isPerforming) return true;
        if (abilityWest != null && abilityWest.isPerforming) return true;
        if (abilityNorth != null && abilityNorth.isPerforming) return true;
        return false;
    }

    public void Deactivateability(IHeartSwordAbility ability)
    {
        if (ability == null) return;
        if (currentActivatedAbility == ability)
        {
            currentActivatedAbility = null;
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
}