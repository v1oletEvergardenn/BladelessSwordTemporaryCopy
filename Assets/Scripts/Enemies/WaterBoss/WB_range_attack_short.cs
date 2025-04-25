using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WB_range_attack_short : IEnemyAction
{
    public Transform shootPos;
    private WaterBossAI bossAI;
    public int damage = 5;
    public float speed = 100f;
    private bool acting_this = false;

    public override void Start()
    {
        base.Start();
        pooler = ObjectPooler.instance;
        bossAI = GetComponent<WaterBossAI>();// new WaterBossAI();
    }

    private void Update()
    {
        if (acting_this)
        {
            //if (bossAI.distanceToPlayer <= bossAI.distanceThresholdForRangeAttack)
            //{
            //    CancelAct();
            //    acting_this = false;
            //    bossAI.inAct = false;
            //    bossAI.nextAction = bossAI.melee_attack_short;
            //    bossAI.NextAction();
            //}
        }
    }

    public override void CancelAct()
    {
        base.CancelAct();
        acting_this = false;
    }

    public void ShootWaterBall()
    {
        IProjectile waterball = pooler.SpawnFromPool("water_ball", shootPos.position, Quaternion.identity).GetComponent<IProjectile>();
        waterball.SetUp(transform.right, this.gameObject, 0, false, playerIDamagable, true, damage, speed);
    }

    public override IEnumerator Act_coroutine(float factor = 0)
    {
        acting_this = true;
        anim.Play("short_range_attack");
        yield return new WaitForSeconds(5f);
        acting_this = false;
        if (bossAI.inAct)
        {
            bossAI.inAct = false;
            bossAI.NextAction();
        }
        yield return null;
    }
}