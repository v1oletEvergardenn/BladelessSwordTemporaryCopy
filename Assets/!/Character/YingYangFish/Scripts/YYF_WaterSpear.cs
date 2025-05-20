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
    private bool secondShooted = false;

    public override void Start()
    {
        base.Start();
        bossAI = GetComponent<YingYangFish_AI>();
    }

    public override void CancelAct()
    {
        if (act_routine != null) { StopCoroutine(act_routine); }

        if (!shooted && spear != null) { spear.SetFalseActive(); }
        if (!secondShooted && secondSpear != null) { secondSpear.SetFalseActive(); }
    }

    public override IEnumerator Act_coroutine(float factor = 0)
    {
        bool isBlack = false;
        if (factor == 0)
        {
            yield return bossAI.co_IEcloseSwim = StartCoroutine(bossAI.IECloseSwim(false));
            yield return bossAI.co_sprintStartPoint = StartCoroutine(bossAI.SprintStartPoint());
            isBlack = bossAI.closerFish_Black;
        }

        shooted = false;
        secondShooted = false;
        Spear _spear;
        Spear _secondSpear = null;

        bool second = false;
        int possiblity = 5;

        if (factor == 0)
        {
            if (bossAI.initialAction == bossAI.waterSpear)
            {
                if (playerEnergy.currentEnergy <= 5)
                {
                    possiblity += 2;
                }
            }
            int i = UnityEngine.Random.Range(0, 10);
            second = false;
            if (i < possiblity) { second = true; }
        }
        else if (factor != 5 && factor != 6)
        {
            second = true;
        }

        if (factor == 1 || factor == 3 || factor == 5)
        {
            while (!bossAI.blackPositioned) { yield return null; }
            yield return StartCoroutine(bossAI.SprintStartPoint("black"));
            if (factor == 1 || factor == 3) { yield return new WaitForSeconds(0.6f); }

            isBlack = true;
        }//black fish
        else if (factor == 2 || factor == 4 || factor == 6)
        {
            while (!bossAI.whitePositioned) { yield return null; }
            yield return StartCoroutine(bossAI.SprintStartPoint("white"));
            isBlack = false;
        }//white fish

        if (!isBlack)
        {
            bossAI.whiteAnim.Play("spear_pre");
            bossAI.whiteAnim.SetBool("secondSpear", second);
            _spear = bossAI.selfPooler.SpawnFromPool("water_Spear", waterSpearPos_white.position).GetComponent<Spear>();
        }
        else
        {
            bossAI.blackAnim.Play("spear_pre");
            bossAI.blackAnim.SetBool("secondSpear", second);
            _spear = bossAI.selfPooler.SpawnFromPool("water_Spear", waterSpearPos_black.position).GetComponent<Spear>();
        }
        if (factor == 0) { spear = _spear; }
        _spear.SetUp(transform.right, this.gameObject, 0, _followTarget: true, _target: playerIDamagable, false, spearDamage, 0);
        _spear.collisionActive = false;

        yield return new WaitForSeconds(1.8f);
        if (second)
        {
            bossAI.AddActionBreak(actionBreakAmount);
            if (!isBlack) { _secondSpear = bossAI.selfPooler.SpawnFromPool("water_Spear", waterSpearPos_white.position).GetComponent<Spear>(); }
            else { _secondSpear = bossAI.selfPooler.SpawnFromPool("water_Spear", waterSpearPos_black.position).GetComponent<Spear>(); }

            _secondSpear.SetUp(transform.right, this.gameObject, 0, _followTarget: true, _target: playerIDamagable, false, spearDamage, 0);
            _secondSpear.collisionActive = false;
            secondSpear = _secondSpear;
        }
        yield return new WaitForSeconds(0.8f);

        ShootSpear(_spear);
        shooted = true;

        if (second)
        {
            yield return new WaitForSeconds(1.3f);
            ShootSpear(_secondSpear);
            secondShooted = true;
        }

        yield return new WaitForSeconds(0.5f);
        spear = null;
        secondSpear = null;

        if (factor == 0)
        {
            if (bossAI.initialAction == bossAI.waterSpear)
            {
                if (bossAI.distanceToPlayer <= bossAI.swing.swingRange + 1)
                {
                    yield return bossAI.co_sprintBackEqual = StartCoroutine(bossAI.SprintBackEqual());
                    bossAI.InsertAction(bossAI.swing);
                }
                else if (Possibility(60))
                {
                    //moving
                    bossAI.InsertAction(bossAI.swing);
                    bossAI.InsertAction(bossAI.dive);
                }
            }

            bossAI.SetNormalRotateSpeed();
            bossAI.AddActionBreak(actionBreakAmount);
            bossAI.EndAction();
        }
        else if (factor == 1)
        {
            bossAI.SetBlackTargetRotateSpeed(bossAI.sprintRotateSpeed);
            //StartCoroutine(bossAI.IE_SwimAway(bossAI.waterSpearPos_black2.position, 10, true));
            //StartCoroutine(Act_coroutine(3));
            StartCoroutine(bossAI.IE_SwimAway(player.transform.position + new Vector3(-15, 1), 10, true));
        }
        else if (factor == 2)
        {
            bossAI.SetWhiteTargetRotateSpeed(bossAI.sprintRotateSpeed);
            //StartCoroutine(bossAI.IE_SwimAway(bossAI.waterSpearPos_white2.position, 10, false));
            //StartCoroutine(Act_coroutine(4));

            StartCoroutine(bossAI.IE_SwimAway(player.transform.position + new Vector3(15, 1), 10, false));

            while (!bossAI.blackPositioned || !bossAI.whitePositioned) { yield return null; }
            yield return StartCoroutine(bossAI.SprintSamePos());
            StartCoroutine(Act_coroutine(3));
            yield return StartCoroutine(Act_coroutine(4));
        }
        else if (factor == 4)
        {
            StartCoroutine(Act_coroutine(5));
            StartCoroutine(Act_coroutine(6));
        }
        else if (factor == 6)
        {
            bossAI.SetWhiteTargetRotateSpeed(bossAI.sprintRotateSpeed);
            bossAI.SetBlackTargetRotateSpeed(bossAI.sprintRotateSpeed);
            StartCoroutine(bossAI.IE_SwimAway(bossAI.center.position, 10, false));
            StartCoroutine(bossAI.IE_SwimAway(bossAI.center.position, 10, true));
            while (!bossAI.blackPositioned || !bossAI.whitePositioned) { yield return null; }
            yield return bossAI.co_sprintBackEqual = StartCoroutine(bossAI.SprintBackEqual());
            bossAI.SetWhiteTargetRotateSpeed(bossAI.sprintRotateSpeed);
            bossAI.SetBlackTargetRotateSpeed(bossAI.sprintRotateSpeed);
            bossAI.finishedWaterSpearUltimate = true;
        }
        yield return null;
    }

    public void ShootSpear(Spear spear)
    {
        spear.collisionActive = true;
        spear.SetUp(transform.right, this.gameObject, 0, _followTarget: false, _target: playerIDamagable, true, spearDamage, spearSpeed);
        spear.stunDuration = spear_stunDuration;
        shooted = true;
    }
}