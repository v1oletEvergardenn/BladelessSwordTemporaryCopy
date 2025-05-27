using EditorAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QX_add_repel : IEnemyAction
{
    private QianXiaoAI bossAI;
    public IEnemyAction actionSender;
    public Transform repelPosition;
    public float repelRange = 5f;

    public float repelForce = 100f;

    public int damageAmount = 3;
    public float stunDuration = 0.5f;
    public float freezeTimeDuration;
    [SerializeField, MinMaxSlider(0, 3f)] public Vector2 rumbleFrequncy = new Vector2(0.25f, 0.4f);
    public float rumbleDuration = 0.2f;

    public override void Start()
    {
        base.Start();
        bossAI = GetComponent<QianXiaoAI>();
    }

    //public override void Act()
    //{
    //    bossAI.canFlip = false;
    //    isThisActing = true;
    //    if (!bossAI.isStage2) { StartCoroutine(S1_Act()); }
    //    else { StartCoroutine(S2_Act()); }
    //}

    //public IEnumerator S1_Act()
    //{
    //    anim.Play("S1_add_repel");
    //    yield return new WaitForSeconds(0.35f);
    //    hit();

    //    yield return new WaitForSeconds(action_time);
    //    bossAI.canFlip = true;
    //    isThisActing = false;
    //    bossAI.targetPos = bossAI.nullTargetPos;
    //    bossAI.inAct = false;
    //    yield return new WaitForSeconds(0.2f);
    //    if (actionSender != null)
    //    {
    //        bossAI.NextAction();
    //        actionSender = null;
    //    }
    //    else
    //    {
    //        bossAI.NextAction();
    //    }

    //    yield return null;
    //}

    //public IEnumerator S2_Act()
    //{
    //    anim.Play("S2_add_repel");
    //    yield return new WaitForSeconds(0.35f);
    //    hit();
    //    yield return new WaitForSeconds(action_time);
    //    bossAI.canFlip = true;
    //    bossAI.targetPos = bossAI.nullTargetPos;
    //    isThisActing = false;
    //    bossAI.inAct = false;
    //    yield return new WaitForSeconds(0.2f);
    //    if (actionSender != null)
    //    {
    //        bossAI.NextAction();
    //        actionSender = null;
    //    }
    //    else
    //    {
    //        bossAI.NextAction();
    //    }

    //    yield return null;
    //}

    public void hit()
    {
        float d = Vector3.Distance(playerIDamagable.GetHitPos(), repelPosition.position);
        if (d <= repelRange)
        {
            int dealtDamage = playerIDamagable.DamageFromMeleeAttack(transform, damageAmount, stunDuration);

            if (dealtDamage == 2)//counter attack
            {
                //counter attack effect
                vfx.SpawnHitEffect(true, playerIDamagable.hitEffectPosition.position);

                playerIDamagable.Repel(repelForce, -transform.right.x < 0 ? true : false);
                vfx.CameraShake(0.1f);
                vfx.RumblePulse(rumbleFrequncy.x * 2, rumbleFrequncy.y * 2, rumbleDuration * 2);
                vfx.SlowTimeForSeconds(freezeTimeDuration, 0f);
            }
            else if (dealtDamage == 1)//defend
            {
                vfx.SpawnHitEffect(true, playerIDamagable.GetHitPos());
                playerIDamagable.Repel(repelForce, -transform.right.x < 0 ? true : false);
                vfx.CameraShake(0.1f);
                vfx.RumblePulse(rumbleFrequncy.x * 2, rumbleFrequncy.y * 2, rumbleDuration * 2);
                vfx.SlowTimeForSeconds(freezeTimeDuration, 0f);
            }
            else if (dealtDamage == 0)//dealtDamage
            {
                vfx.SpawnHitEffect(true, playerIDamagable.GetHitPos());
                vfx.RumblePulse(rumbleFrequncy.x, rumbleFrequncy.y, rumbleDuration);
                playerIDamagable.Repel(repelForce * 2, -transform.right.x < 0 ? true : false);
                vfx.SlowTimeForSeconds(freezeTimeDuration, 0f);
            }
        }
    }
}