using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QX_stun : IEnemyAction
{
    private QianXiaoAI bossAI;
    public float S1_anim_getUp_time = 0.5f;
    public float S2_anim_getUp_time = 1.5f;
    private bool falling = false;

    public override void Start()
    {
        base.Start();
        bossAI = GetComponent<QianXiaoAI>();
    }

    public void Update()
    {
        if (bossAI.isStage2 && falling)
        {
            if (bossAI.isGrounded)
            {
                anim.Play("S2_land_on_ground");
                falling = false;
            }
        }
    }

    public override void Act()
    {
        bossAI.canFlip = false;
        bossAI.isStunning = true;
        bossAI.canMove = false;
        if (!bossAI.isStage2) { StartCoroutine(S1_Act()); }
        else { StartCoroutine(S2_Act()); }
    }

    public IEnumerator S1_Act()
    {
        anim.Play("S1_Stun");
        yield return new WaitForSeconds(action_time);
        anim.Play("S1_Stun_finish");
        yield return new WaitForSeconds(S1_anim_getUp_time + 0.5f);//end
        bossAI.canFlip = true;
        bossAI.isStunning = false;
        bossAI.NextAction(this);
        yield return null;
    }

    public IEnumerator S2_Act()
    {
        anim.Play("S2_Lose_power");
        falling = true;
        yield return new WaitForSeconds(action_time);
        anim.Play("S2_standingUp");
        yield return new WaitForSeconds(S2_anim_getUp_time + 0.5f);//end
        bossAI.canFlip = true;
        bossAI.SetFlyEngine(true);
        bossAI.isStunning = false;
        bossAI.NextAction(this);
        yield return null;
    }
}