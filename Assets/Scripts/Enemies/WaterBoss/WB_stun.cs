using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WB_stun : IEnemyAction
{
    private WaterBossAI bossAI;

    public override void Start()
    {
        base.Start();
        bossAI = GetComponent<WaterBossAI>();
    }

    public override IEnumerator Act_coroutine()
    {
        anim.Play("stun");
        bossAI.canFlip = false;
        yield return new WaitForSeconds(action_time);
        anim.Play("stun_after");
        yield return new WaitForSeconds(1.1f);
        bossAI.canFlip = true;
        if (bossAI.inAct)
        {
            bossAI.inAct = false;
            bossAI.NextAction();
        }
        yield return null;
    }
}