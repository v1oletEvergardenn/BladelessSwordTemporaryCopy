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
    public float stunValue = 25f;

    public int small_spearDamage = 5;
    public float small_spearSpeed = 100f;
    public float small_spear_stunDuration = 0.5f;
    public float small_spear_stunValue = 10f;
    private Spear spear;
    private Spear smallSpear1;
    private Spear smallSpear2;

    public override void Start()
    {
        base.Start();
        bossAI = GetComponent<YingYangFish_AI>();
    }

    public override bool CanAct()
    {
        if (bossAI.isWhiteBusy && bossAI.isBlackBusy) return false;
        else return true;
    }

    public override void CancelAct()
    {
        if (act_routine != null) { StopCoroutine(act_routine); }

        if (spear != null) { spear.Die(); }
        if (smallSpear1 != null) { smallSpear1.Die(); }
        if (smallSpear2 != null) { smallSpear2.Die(); }
    }

    /// <summary>
    /// 1,3,5 = black fish
    /// <para>2,4,6 = white fish</para>
    /// <para>7 = combo from dive, if close to player, add swing</para>
    /// </summary>
    /// <param name="factor"></param>
    /// <returns></returns>
    public override IEnumerator Act_coroutine(float factor = 0)
    {
        bool isBlack = false;
        if (!bossAI.isWhiteBusy && !bossAI.isBlackBusy) { isBlack = bossAI.CheckCloserFish() == bossAI.blackFish; }
        if (bossAI.isWhiteBusy && !bossAI.isBlackBusy) { isBlack = true; }
        bossAI.SetBusy(isBlack);
        if (factor == 0)
        {
            yield return bossAI.co_IEcloseSwim = StartCoroutine(bossAI.IECloseSwim(false));
            yield return bossAI.co_sprintStartPoint = StartCoroutine(bossAI.IESprintStartPoint(isBlack ? "black" : "white"));
        }

        //initial setup
        bossAI.SetFishTargetRotateSpeed(isBlack, 0);
        Spear _spear; Spear _smallSpear1 = null; Spear _smallSpear2 = null;
        bool addition = false;

        if (factor == 0)
        {
            int possiblity = 5;
            if (bossAI.initialAction == bossAI.waterSpear && (playerEnergy.currentEnergy <= 5)) possiblity += 2;
            int i = UnityEngine.Random.Range(0, 10);
            addition = false; if (i < possiblity) { addition = true; }
        }
        if (factor == 1 || factor == 3 || factor == 5)
        {
            bossAI.SetBlackBusy();
            while (!bossAI.blackPositioned) { yield return null; }
            yield return StartCoroutine(bossAI.IESprintStartPoint("black"));
            if (factor == 1 || factor == 3) { yield return new WaitForSeconds(0.6f); }

            isBlack = true;
        }//black fish
        else if (factor == 2 || factor == 4 || factor == 6)
        {
            bossAI.SetWhiteBusy();
            while (!bossAI.whitePositioned) { yield return null; }
            yield return StartCoroutine(bossAI.IESprintStartPoint("white"));
            isBlack = false;
        }//white fish

        //references
        Animator anim = isBlack ? bossAI.blackAnim : bossAI.whiteAnim;
        Transform fish = isBlack ? bossAI.blackFish : bossAI.whiteFish;
        Transform origin = isBlack ? bossAI.blackOrigin : bossAI.whiteOrigin;
        Transform fishGFX = isBlack ? bossAI.blackFishGFX : bossAI.whiteFishGFX;
        Transform waterSpearSpawnPos = isBlack ? waterSpearPos_black : waterSpearPos_white;

        //spawn spear
        anim.Play("spear_pre");
        _spear = bossAI.selfPooler.SpawnFromPool("water_Spear", waterSpearSpawnPos.position).GetComponent<Spear>();

        //set spear
        if (factor == 0) { spear = _spear; }
        SetUp(_spear);

        //wait for launch
        yield return new WaitForSeconds(0.5f);

        //spawn small spears if need
        if (addition)
        {
            _smallSpear1 = bossAI.selfPooler.SpawnFromPool("small_water_spear", waterSpearSpawnPos.position + new Vector3(0, 1, 0)).GetComponent<Spear>();
            _smallSpear2 = bossAI.selfPooler.SpawnFromPool("small_water_spear", waterSpearSpawnPos.position + new Vector3(0, -1, 0)).GetComponent<Spear>();
            _smallSpear1.transform.localScale = Vector3.one; _smallSpear2.transform.localScale = new Vector3(1, -1, 1);
            SetUp(_smallSpear1); SetUp(_smallSpear2);
            smallSpear1 = _smallSpear1; smallSpear2 = _smallSpear2;
        }

        yield return new WaitForSeconds(2.3f);

        //launch spear
        if (addition)
        {
            ShootSpear(_smallSpear1); smallSpear1 = null;
            yield return new WaitForSeconds(0.2f);
            ShootSpear(_smallSpear2); smallSpear2 = null;
            yield return new WaitForSeconds(0.5f);
        }

        ShootSpear(_spear); spear = null;
        yield return new WaitForSeconds(0.5f);
        //normal state
        if (factor == 7)
        {
            if (bossAI.distanceToPlayer <= bossAI.close_distance_threshhold) bossAI.AddAction(bossAI.swing);
        }
        if (factor == 0)
        {
            if (bossAI.initialAction == bossAI.waterSpear)
            {
                if (bossAI.distanceToPlayer <= bossAI.close_distance_threshhold)
                {
                    //水凝枪 -> 翻腾/双摆尾
                    //条件：在释放完水凝枪后，距离小于一定值。
                    if (Possibility(50))
                    {
                        if (Possibility(50)) bossAI.AddAction(bossAI.swing);
                        else bossAI.AddAction(bossAI.splash);
                    }
                    //水凝枪 -> 潜水 -> 压缩泡泡光线 = 泡泡牢笼/单摆尾。
                    //条件：在释放完水凝枪后，距离小于一定值时（远离）
                    else
                    {
                        bossAI.movingTarget = bossAI.GetBoundaryFarOfPlayer();
                        bossAI.AddAction(bossAI.dive);

                        bossAI.AddAction(bossAI.gatling);
                        if (Possibility(50)) bossAI.AddAction(bossAI.singleSwing);
                        else bossAI.AddAction(bossAI.bubbleTrap);
                    }
                }

                //水凝枪 -> 潜水 -> 双摆尾 ？双摆尾
                //条件：在释放完水凝枪后，距离大于一定值时（靠近）
                //？：成功弹反时有50 %。
                if (bossAI.distanceToPlayer >= bossAI.far_distance_threshhold)
                {
                    bossAI.movingTarget = player.transform;
                    bossAI.AddAction(bossAI.dive);
                    bossAI.AddAction(bossAI.swing, 4);
                }
            }

            bossAI.SetNotBusy(isBlack);
            bossAI.SetFishTargetRotateSpeed(isBlack, bossAI.idleRotateSpeed);
            bossAI.AddActionBreak(actionBreakAmount);
            bossAI.EndAction();
        }
        else if (factor == 1)
        {
            bossAI.SetBlackTargetRotateSpeed(bossAI.sprintRotateSpeed);
            StartCoroutine(bossAI.IESwimAway(player.transform.position + new Vector3(-15, 1), 10, true));
        }
        else if (factor == 2)
        {
            bossAI.SetWhiteTargetRotateSpeed(bossAI.sprintRotateSpeed);
            StartCoroutine(bossAI.IESwimAway(player.transform.position + new Vector3(15, 1), 10, false));

            while (!bossAI.blackPositioned || !bossAI.whitePositioned) { yield return null; }
            yield return StartCoroutine(bossAI.IESprintSamePos());
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
            StartCoroutine(bossAI.IESwimAway(bossAI.center.position, 10, false));
            StartCoroutine(bossAI.IESwimAway(bossAI.center.position, 10, true));
            while (!bossAI.blackPositioned || !bossAI.whitePositioned) { yield return null; }
            yield return bossAI.co_sprintBackEqual = StartCoroutine(bossAI.IESprintBackEqual());
            bossAI.SetWhiteTargetRotateSpeed(bossAI.sprintRotateSpeed);
            bossAI.SetBlackTargetRotateSpeed(bossAI.sprintRotateSpeed);
            bossAI.finishedWaterSpearUltimate = true;
        }

        yield return null;

        void SetUp(Spear spear)
        {
            spear.SetUp(transform.right, this.gameObject, 0, _followTarget: true, _target: playerIDamagable, false, spearDamage, 0);
            spear.collisionActive = false;
        }
    }

    public void ShootSpear(Spear spear)
    {
        spear.collisionActive = true;
        spear.SetUp(transform.right, this.gameObject, 0, _followTarget: false, _target: playerIDamagable, true, spearDamage, spearSpeed, _stunValue: stunValue);
        spear.stunDuration = spear_stunDuration;
    }

    public void ShootSmallSpear(Spear spear)
    {
        spear.collisionActive = true;
        spear.SetUp(transform.right, this.gameObject, 0, _followTarget: false, _target: playerIDamagable, true, spearDamage, spearSpeed, _stunValue: stunValue);
        spear.stunDuration = spear_stunDuration;
    }
}