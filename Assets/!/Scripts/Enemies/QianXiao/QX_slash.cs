using EditorAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QX_slash : IEnemyAction
{
    private QianXiaoAI bossAI;
    public int damage = 4;
    public float stunDuration = 0.2f;

    [Header("effects")][SerializeField] public float rumbleDuration = 0.1f;
    [SerializeField, MinMaxSlider(0, 3f)] public Vector2 rumbleFrequncy = new Vector2(0.25f, 0.4f);
    public float freezeTimeDuration = 0.2f;
    public float RepelForce = 20f;

    public float minDistance_attack = 10f;

    //private bool reachedInRange = false;
    //private bool startedIEAttack = false;
    public GameObject hitBox;

    public override void Start()
    {
        base.Start();
        bossAI = GetComponent<QianXiaoAI>();
    }

    //private void Update()
    //{
    //    if (!reachedInRange && isThisActing)
    //    {
    //        Vector3 v = player.transform.position - transform.position;
    //        float d = v.magnitude;
    //        if (d <= minDistance_attack)//enough ditance
    //        {
    //            //stop check moving
    //            bossAI.canMove = false;
    //            if (!startedIEAttack)
    //            {
    //                if (!bossAI.isStage2) { StartCoroutine(S1_Act()); }
    //                else { StartCoroutine(S2_Act()); }
    //                reachedInRange = true;
    //                startedIEAttack = true;
    //                bossAI.canFlip = false;
    //                bossAI.canMove = false;
    //            }
    //        }
    //    }
    //    if (isThisActing)
    //    {
    //        if ((transform.position.x > player.transform.position.x && bossAI.isFacingRight) | (transform.position.x < player.transform.position.x && !bossAI.isFacingRight))
    //        {
    //            CancelAct();
    //            isThisActing = false;
    //            bossAI.CancelAllActions();
    //            bossAI.targetPos = bossAI.nullTargetPos;
    //            bossAI.canMove = false;
    //            bossAI.add_repel.Act();
    //            bossAI.add_repel.GetComponent<QX_add_repel>().actionSender = this;
    //        }
    //    }
    //}

    //public override void CancelAct()
    //{
    //    base.CancelAct();
    //    hitBox.SetActive(false);
    //    bossAI.OutLine_Activate(0);
    //}

    //public override void Act()
    //{
    //    isThisActing = true;
    //    bossAI.canMove = true;
    //    reachedInRange = false;
    //    startedIEAttack = false;
    //}

    //public IEnumerator S1_Act()
    //{
    //    anim.Play("S1_slash");
    //    yield return new WaitForSeconds(action_time);
    //    isThisActing = false;
    //    bossAI.canFlip = true;
    //    if (bossAI.inAct)
    //    {
    //        bossAI.inAct = false;
    //        bossAI.NextAction();
    //    }
    //    yield return null;
    //}

    //public IEnumerator S2_Act()
    //{
    //    anim.Play("S2_slash");
    //    yield return new WaitForSeconds(action_time);
    //    isThisActing = false;
    //    bossAI.canFlip = true;
    //    if (bossAI.inAct)
    //    {
    //        bossAI.inAct = false;
    //        bossAI.NextAction();
    //    }
    //    yield return null;
    //}

    public void Damage()
    {
        int dealtDamage = playerIDamagable.DamageFromMeleeAttack(transform, damage, stunDuration);

        if (dealtDamage == 2)//counter attack
        {
            //counter attack effect
            vfx.SpawnHitEffect(true, playerIDamagable.hitEffectPosition.position);

            playerIDamagable.Repel(RepelForce, transform.right.x < 0 ? true : false);
            vfx.CameraShake(0.2f);
            vfx.RumblePulse(rumbleFrequncy.x * 2, rumbleFrequncy.y * 2, rumbleDuration * 2);
            vfx.SlowTimeForSeconds(freezeTimeDuration, 0f);
        }
        else if (dealtDamage == 1)//defended
        {
            vfx.SpawnSlashEffect(playerIDamagable.GetHitPos());
            playerIDamagable.Repel(RepelForce, transform.right.x < 0 ? true : false);
            vfx.CameraShake(0.2f);
            vfx.RumblePulse(rumbleFrequncy.x * 2, rumbleFrequncy.y * 2, rumbleDuration * 2);
            vfx.SlowTimeForSeconds(freezeTimeDuration, 0f);
        }
        else if (dealtDamage == 0)//dealt damage
        {
            vfx.SpawnSlashEffect(playerIDamagable.GetHitPos());
            vfx.RumblePulse(rumbleFrequncy.x, rumbleFrequncy.y, rumbleDuration);
            playerIDamagable.Repel(RepelForce * 2, transform.right.x < 0 ? true : false);
            vfx.SlowTimeForSeconds(freezeTimeDuration, 0f);
        }
    }
}