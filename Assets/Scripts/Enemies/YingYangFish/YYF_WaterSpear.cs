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
    private Spear secondSpear;
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
        if (!shooted && secondSpear != null) { Destroy(secondSpear); }
    }

    public override IEnumerator Act_coroutine()
    {
        yield return StartCoroutine(bossAI.SprintStartPoint());
        shooted = false;

        int possiblity = 5;

        if (bossAI.initialAction == bossAI.waterSpear)
        {
            if (playerEnergy.currentEnergy <= 5)
            {
                possiblity += 2;
            }
        }

        int i = UnityEngine.Random.Range(0, 10);
        bool second = false;
        if (i < possiblity) { second = true; }

        if (!bossAI.white_idling)
        {
            bossAI.whiteAnim.Play("spear_pre");
            bossAI.whiteAnim.SetBool("secondSpear", second);
            spear = pooler.SpawnFromPool("water_Spear", waterSpearPos_white.position).GetComponent<Spear>();
        }
        else
        {
            bossAI.blackAnim.Play("spear_pre");
            bossAI.blackAnim.SetBool("secondSpear", second);
            spear = pooler.SpawnFromPool("water_Spear", waterSpearPos_black.position).GetComponent<Spear>();
        }

        spear.SetUp(transform.right, this.gameObject, 0, _followTarget: true, _target: playerIDamagable, false, spearDamage, 0);
        spear.collisionActive = false;

        yield return new WaitForSeconds(1.8f);
        if (second)
        {
            if (!bossAI.white_idling) { secondSpear = pooler.SpawnFromPool("water_Spear", waterSpearPos_white.position).GetComponent<Spear>(); }
            else { secondSpear = pooler.SpawnFromPool("water_Spear", waterSpearPos_black.position).GetComponent<Spear>(); }

            secondSpear.SetUp(transform.right, this.gameObject, 0, _followTarget: true, _target: playerIDamagable, false, spearDamage, 0);
            secondSpear.collisionActive = false;
        }
        yield return new WaitForSeconds(0.8f);

        ShootSpear(spear);

        if (second)
        {
            yield return new WaitForSeconds(1.3f);
            ShootSpear(secondSpear);
        }
        //launch waterspear

        yield return new WaitForSeconds(0.5f);
        spear = null;
        secondSpear = null;

        if (bossAI.initialAction == bossAI.waterSpear)
        {
            if (bossAI.distanceToPlayer <= bossAI.swing.swingRange + 1)
            {
                yield return StartCoroutine(bossAI.SprintBackEqual());
                bossAI.InsertAction(bossAI.swing);
            }
            else if (Possibility(60))
            {
                //moving
                bossAI.InsertAction(bossAI.swing);
                bossAI.InsertAction(bossAI.dive);
            }
        }
        else
        {
            if (bossAI.actionList.Count < 2 || (bossAI.actionList[1] != this && bossAI.actionList[1] != bossAI.splash))
            {
                yield return StartCoroutine(bossAI.SprintBackEqual());
            }
            bossAI.white_idling = true;
            bossAI.black_idling = true;
        }

        bossAI.EndAction();
        yield return null;
    }

    public void ShootSpear(Spear spear)
    {
        spear.collisionActive = true;
        spear.SetUp(transform.right, this.gameObject, 0, _followTarget: false, _target: playerIDamagable, true, spearDamage, spearSpeed);
        spear.stunDuration = spear_stunDuration;
        spear.facingRight = bossAI.isFacingRight;
        shooted = true;
    }
}