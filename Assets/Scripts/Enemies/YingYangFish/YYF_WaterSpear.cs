using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YYF_WaterSpear : IEnemyAction
{
    private YingYangFish_AI bossAI;

    public Transform waterSpearPos_black;
    public Transform waterSpearPos_white;

    public int spearDamage = 10;
    public float spearSpeed = 100f;
    public float spear_stunDuration = 1f;

    private Spear spear;
    private bool shooted = false;

    public override void Start()
    {
        base.Start();
        bossAI = GetComponent<YingYangFish_AI>();
    }

    public override void CancelAct()
    {
        base.CancelAct();
        if (!shooted && spear != null) { Destroy(spear); }
    }

    public override IEnumerator Act_coroutine()
    {
        bool isWhiteActing = false;
        shooted = false;
        if (!bossAI.white_idling)
        {
            isWhiteActing = true;
            bossAI.whiteAnim.Play("white_spear");
            spear = pooler.SpawnFromPool("water_Spear", waterSpearPos_white.position).GetComponent<Spear>();
        }
        else
        {
            bossAI.blackAnim.Play("black_spear");
            spear = pooler.SpawnFromPool("water_Spear", waterSpearPos_black.position).GetComponent<Spear>();
        }

        spear.SetUp(transform.right, this.gameObject, 0, _followTarget: true, _target: playerIDamagable, false, spearDamage, 0);
        spear.collisionActive = false;

        yield return new WaitForSeconds(2.6f);

        //launch waterspear

        spear.collisionActive = true;
        spear.SetUp(transform.right, this.gameObject, 0, _followTarget: false, _target: playerIDamagable, true, spearDamage, spearSpeed);
        spear.stunDuration = spear_stunDuration;
        spear.facingRight = bossAI.isFacingRight;
        shooted = true;

        yield return new WaitForSeconds(0.5f);
        if (isWhiteActing) { bossAI.white_sprint_back = true; }
        else { bossAI.black_sprint_back = true; }

        yield return null;
    }
}