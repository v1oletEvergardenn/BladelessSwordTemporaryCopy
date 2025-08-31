using DG.Tweening;
using EditorAttributes;
using Microlight.MicroBar;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeartSwordAbilities : MonoBehaviour
{
    public static HeartSwordAbilities instance;

    public InternalObjectPooler selfPooler;
    public Rigidbody2D rb;
    public Animator anim;

    [SerializeField] private float maxHS_point = 3;
    [SerializeField] public float currentHS_point { get; private set; } = 0;
    public List<HeartSwordUIPoint> HS_points = new List<HeartSwordUIPoint>();

    public IHeartSwordAbility abilityX;

    public IHeartSwordAbility abilityY;
    public IHeartSwordAbility abilityB;

    public IHeartSwordAbility currentActivatedAbility = null;

    public void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        if (abilityX != null) abilityX.EquipAbility();
        if (abilityY != null) abilityY.EquipAbility();
        if (abilityB != null) abilityB.EquipAbility();
        InitializeHS_UI();
    }

    private void Update()
    {
        currentHS_point = Mathf.Clamp(currentHS_point, 0, maxHS_point);
        Lit();
    }

    public void ActivateAbility(IHeartSwordAbility ability)
    {
        ability.ActivateAbility();
        currentActivatedAbility = ability;
    }

    public void ModifyHSPoint(float amount)
    {
        currentHS_point += amount;
        currentHS_point = Mathf.Clamp(currentHS_point, 0, maxHS_point);
        RefreshHS_UI();
    }

    private float alpha = 0;
    private bool isIncreasing = true;

    public void Lit()
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