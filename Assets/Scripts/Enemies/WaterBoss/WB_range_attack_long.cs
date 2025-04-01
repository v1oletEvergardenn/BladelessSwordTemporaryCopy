using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WB_range_attack_long : IEnemyAction
{
    public float holding_time = 3f;
    public GameObject Spear;
    public Transform shootPos;
    private Spear spear;
    public float spearSpeed = 400f;
    public int spearDamage = 10;
    public float spear_stunDuration = 1f;
    private WaterBossAI bossAI;

    private bool shooted = false;

    public override void Start()
    {
        base.Start();
        bossAI = GetComponent<WaterBossAI>();
    }

    public override void CancelAct()
    {
        base.CancelAct();
        if (!shooted && spear != null) { Destroy(spear); }
    }

    public override IEnumerator Act_coroutine()
    {
        //start
        anim.Play("range_attack_long_start");
        yield return new WaitForSeconds(0.18f);
        shooted = false;
        //aiming

        spear = Instantiate(Spear, shootPos.position, Quaternion.identity, shootPos).GetComponent<Spear>();
        spear.SetUp(transform.right, this.gameObject, 0, _followTarget: true, _target: playerIDamagable, false, spearDamage, 0);
        spear.gameObject.GetComponent<Animator>().SetFloat("holdingTime_multiplier", 1 / (holding_time / 1.33f));
        spear.collisionActive = false;
        yield return new WaitForSeconds(holding_time + 0.05f);
        //shoot the spear
        shooted = true;
        spear.collisionActive = true;
        spear.transform.SetParent(null, true);
        spear.SetUp(transform.right, this.gameObject, 0, _followTarget: false, _target: playerIDamagable, true, spearDamage, spearSpeed);
        spear.stunDuration = spear_stunDuration;
        spear.facingRight = bossAI.isFacingRight;
        anim.Play("range_attack_long_after");

        yield return new WaitForSeconds(1.5f);
        if (bossAI.inAct)
        {
            bossAI.inAct = false;
            bossAI.NextAction();
        }
        yield return null;
    }
}