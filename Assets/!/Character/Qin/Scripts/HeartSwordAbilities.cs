using EditorAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeartSwordAbilities : MonoBehaviour
{
    public static HeartSwordAbilities instance;

    public InternalObjectPooler selfPooler;
    public Rigidbody2D rb;
    public Animator anim;

    public float maxHS_point = 3;
    public float currentHS_point = 0;

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
    }

    private void Update()
    {
        currentHS_point = Mathf.Clamp(currentHS_point, 0, maxHS_point);
    }

    public void ActivateAbility(IHeartSwordAbility ability)
    {
        ability.ActivateAbility();
        currentActivatedAbility = ability;
    }

    public void ModifyHSPoint(float i)
    {
        currentHS_point += i;
        currentHS_point = Mathf.Clamp(currentHS_point, 0, maxHS_point);
    }

    public void UpdateHS()
    {
        //for (int i = 0; i < HS_points.Count; i++)
        //{
        //    //if (i < activatedHS_point)
        //    //{
        //    //    HS_points[i].SetActive(true);
        //    //    HS_points[i].GetComponent<Animator>().Play("activate");
        //    //}
        //    if (i < currentHS_point)
        //    {
        //        HS_points[i].SetActive(true);
        //        HS_points[i].GetComponent<Animator>().Play("idle");
        //    }
        //    else
        //    {
        //        HS_points[i].SetActive(false);
        //    }
        //}
    }
}