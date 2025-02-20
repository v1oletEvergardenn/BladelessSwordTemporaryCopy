using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class WB_unstuckCorner : IEnemyAction
{
    private WaterBossAI bossAI;

    public override void Start()
    {
        base.Start();
        bossAI = GetComponent<WaterBossAI>();
    }

    public override void Act()
    {
        anim.GetComponent<Animator>().Play("move"); bossAI.cornerTimer = 0f; bossAI.target = bossAI.mid_of_room; bossAI.canMove = true;
    }
}