using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterBossAnimTrigger : MonoBehaviour
{
    public GameObject owner;
    private WB_range_attack_short range_attack_Short;
    private WB_melee_attack_long melee_attack_Long;
    private WB_melee_attack_short melee_attack_Short;
    private WaterBossAI bossAI;
    public GameObject melee_attack_long_hit_box;

    private void Start()
    {
        bossAI = owner.GetComponent<WaterBossAI>();
        range_attack_Short = owner.GetComponent<WB_range_attack_short>();
        melee_attack_Long = owner.GetComponent<WB_melee_attack_long>();
        melee_attack_Short = owner.GetComponent<WB_melee_attack_short>();
    }

    public void ShootWaterBall()
    {
        range_attack_Short.ShootWaterBall();
    }

    public void MeleeAttack_long_hit_box_activate(int i)
    {
        if (i == 1)
        {
            melee_attack_long_hit_box.SetActive(true);
        }
        else
        {
            melee_attack_long_hit_box.SetActive(false);
        }
    }

    public void Outline_MeleeActivate(int i)
    {
        bossAI.OutLine_Activate(i);
    }

    public void Outline_Flash()
    {
        bossAI.OutLineFlash();
    }

    public void MeleeAttack_short_activate()
    {
        melee_attack_Short.ApplyAttack();
    }
}