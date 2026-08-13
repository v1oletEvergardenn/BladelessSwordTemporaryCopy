using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YYF_WaterSpear : IEnemyAction
{
    private YingYangFish_AI bossAI;

    //public Transform waterSpearPos_black;
    //public Transform waterSpearPos_white;

    public IProjectileBasicAttributes spearAttribute;
    public ProjectileHit spearHit;
    public IProjectileBasicAttributes smallSpearAttribute;
    public ProjectileHit smallSpearHit;
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
        //if (bossAI.isWhiteBusy && bossAI.isBlackBusy) return false;
        return true;
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
    /// <para>8 = combo of waterSpear => gatling </para>
    /// <para>9 = combo of waterSpear => singleSwing </para>
    /// </summary>
    /// <param name="factor"></param>
    /// <returns></returns>
    public override IEnumerator Act_coroutine(float factor = 0, Transform _target = null)
    {
        bossAI.SetCenterCameraFollow(true);
        Transform trueTarget = _target != null ? _target : player.transform;

        Spear _spear; Spear _smallSpear1 = null; Spear _smallSpear2 = null;
        bool addition = false;
        if (factor == 1) { addition = true; }
        Transform lauchPos = bossAI.center;

        //spawn spear
        _spear = bossAI.selfPooler.SpawnFromPool("water_Spear", lauchPos.position).GetComponent<Spear>();

        //set spear
        spear = _spear;
        SetUp(_spear, trueTarget);

        //wait for launch
        yield return WaitForEnemy(0.5f);

        //spawn small spears if need
        if (addition)
        {
            _smallSpear1 = bossAI.selfPooler.SpawnFromPool("small_water_spear", lauchPos.position + new Vector3(0, 1, 0)).GetComponent<Spear>();
            _smallSpear2 = bossAI.selfPooler.SpawnFromPool("small_water_spear", lauchPos.position + new Vector3(0, -1, 0)).GetComponent<Spear>();
            _smallSpear1.transform.localScale = Vector3.one; _smallSpear2.transform.localScale = new Vector3(1, -1, 1);
            SetUp(_smallSpear1, trueTarget); SetUp(_smallSpear2, trueTarget);
            _smallSpear1.transform.SetParent(_spear.transform, true);
            _smallSpear2.transform.SetParent(_spear.transform, true);
            smallSpear1 = _smallSpear1; smallSpear2 = _smallSpear2;
        }

        yield return WaitForEnemy(1.55f);

        if (addition)
        {
            bossAI.selfPooler.SpawnFromPool("ringEffect", bossAI.center.position, true);
            yield return WaitForEnemy(0.2f);
            bossAI.selfPooler.SpawnFromPool("ringEffect", bossAI.center.position, true);
            yield return WaitForEnemy(0.5f);
            bossAI.selfPooler.SpawnFromPool("ringEffect", bossAI.center.position, true);
            yield return WaitForEnemy(0.05f);
        }
        else
        {
            bossAI.selfPooler.SpawnFromPool("ringEffect", bossAI.center.position, true);
            yield return WaitForEnemy(0.75f);
        }

        //launch spear
        if (addition)
        {
            ShootSmallSpear(_smallSpear1, trueTarget); smallSpear1 = null;
            yield return WaitForEnemy(0.2f);
            ShootSmallSpear(_smallSpear2, trueTarget); smallSpear2 = null;
            yield return WaitForEnemy(0.5f);
        }

        ShootSpear(_spear, trueTarget); spear = null;

        if (GameManager.instance.isInPerformingState) bossAI.SetCenterCameraFollow(false);
        yield return null;

        void SetUp(Spear spear, Transform target)
        {
            spear.SetUp(transform.right, this.gameObject).
                SetSpeed(0).
                SetDamage(0).
                SetFollowTarget(target);
            spear.collisionEnabled = false;
        }
    }

    public void ShootSpear(Spear spear, Transform target)
    {
        spear.collisionEnabled = true;

        spear.SetUp(transform.right, this.gameObject).
                SetAttributes(spearAttribute).
                SetFollowTarget(target).
                SetHitEffect(spearHit);

        GameObject burst = bossAI.selfPooler.SpawnFromPool("burst", bossAI.center.position);
        burst.transform.eulerAngles = spear.transform.eulerAngles;
    }

    public void ShootSmallSpear(Spear spear, Transform target)
    {
        spear.transform.SetParent(null);
        spear.collisionEnabled = true;
        spear.SetUp(transform.right, this.gameObject).
               SetAttributes(smallSpearAttribute).
               SetFollowTarget(target).
               SetHitEffect(smallSpearHit);
    }
}