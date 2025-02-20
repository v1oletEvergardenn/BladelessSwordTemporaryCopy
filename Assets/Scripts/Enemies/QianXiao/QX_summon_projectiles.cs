using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QX_summon_projectiles : IEnemyAction
{
    private QianXiaoAI bossAI;
    public AdvancedShooter crow1;
    public AdvancedShooter crow2;

    public override void Start()
    {
        base.Start();
        bossAI = GetComponent<QianXiaoAI>();
    }

    private void Update()
    {
        if (isThisActing && !bossAI.isStunning)
        {
            if (player.GetComponent<CharacterController2D>().isJumping)
            {
                isThisActing = false;
                bossAI.CancelAllActions();
                bossAI.add_teleport_up_attack.Act();
                bossAI.add_teleport_up_attack.GetComponent<QX_add_teleport_up_attack>().actionSender = this;
            }
        }
    }

    public override void CancelAct()
    {
        base.CancelAct();
        isThisActing = false;
        crow1.Deactivate();
        crow2.Deactivate();
    }

    public override IEnumerator Act_coroutine()
    {
        isThisActing = true;
        crow1.Activate();
        crow2.Activate();
        crow1.CDtimer = crow1.roundCD;
        crow2.CDtimer = crow2.roundCD;
        yield return new WaitForSeconds(action_time);
        isThisActing = false;
        if (bossAI.inAct)
        {
            bossAI.inAct = false;
            bossAI.NextAction(this);
        }
        yield return null;
    }
}