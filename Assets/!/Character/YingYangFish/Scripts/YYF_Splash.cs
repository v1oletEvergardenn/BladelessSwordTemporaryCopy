using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.UI.Image;

public class YYF_Splash : IEnemyAction
{
    private YingYangFish_AI bossAI;
    public float[] shootPositionX;
    public float bulletSpeed = 150f;
    public int damage = 2;
    public float gravityScale = 5f;
    public float stunValue = 5f;

    public override void Start()
    {
        base.Start();
        bossAI = GetComponent<YingYangFish_AI>();
    }

    public override bool CanAct()
    {
        return (!bossAI.isWhiteBusy && !bossAI.isBlackBusy);
    }

    public override IEnumerator Act_coroutine(float factor = 0)
    {
        proceedCall = false;

        //black fish or white fish
        if (factor == 0 || factor == 1)
        {
            bool isBlack = factor == 0 ? true : false;

            Animator anim = isBlack ? bossAI.blackAnim : bossAI.whiteAnim;
            Transform fish = isBlack ? bossAI.blackFish : bossAI.whiteFish;
            Transform origin = isBlack ? bossAI.blackOrigin : bossAI.whiteOrigin;
            Transform fishGFX = isBlack ? bossAI.blackFishGFX : bossAI.whiteFishGFX;

            Vector3 target = player.transform.position + new Vector3(0, 3, 0);
            bool toLeft = target.x < origin.position.x;

            //before jump out
            if (toLeft) { origin.DOMove(new Vector3(target.x + 2, bossAI.waterLevel.position.y - 6, 0), 1f); }
            else { origin.DOMove(new Vector3(target.x - 2, bossAI.waterLevel.position.y - 6, 0), 1f); }

            //reset to initial
            origin.eulerAngles = Vector3.zero;
            fish.localPosition = new Vector3(0, 1, 0);
            fishGFX.DOLocalRotate(new Vector3(0, 0, 0), 0.1f);
            fishGFX.DOLocalMove(new Vector3(0, 0, 0), 0.1f);
            yield return new WaitForSeconds(1f);

            //jump out
            float temp_x = toLeft ? player.transform.position.x + 2 : player.transform.position.x - 2;
            if (isBlack) temp_x = toLeft ? player.transform.position.x - 2 : player.transform.position.x + 2;
            origin.DOMove(new Vector3(temp_x, bossAI.waterLevel.position.y + 6f, 0), 0.5f).SetEase(Ease.OutSine);
            anim.Play("swing", 0, 0.5f);

            //be ready;
            yield return new WaitForSeconds(0.7f);

            //blackfish: be ready and dash to player.
            if (isBlack)
            {
                origin.DOMove(new Vector3(player.transform.position.x, bossAI.waterLevel.position.y, 0), 0.3f).SetEase(Ease.OutSine);
            }
            //white fish: be ready dash to ground
            else
            {
                origin.DOMove(new Vector3(origin.position.x, bossAI.waterLevel.position.y, 0), 0.3f).SetEase(Ease.OutSine);
            }
            yield return new WaitForSeconds(0.15f);
            anim.Play("swing_attack");
            yield return new WaitForSeconds(0.15f);
            //spawn bullets
            for (int i = 0; i < shootPositionX.Count(); i++)
            {
                SpawnWaterBullet(origin, i);
                yield return new WaitForSeconds(0.2f);
            }
            yield return bossAI.co_return_singleFishDive = StartCoroutine(bossAI.IESingleFishDive(isBlack, !toLeft));
        }
        else if (factor == 2)//both fish
        {
            for (int i = 0; i < shootPositionX.Count(); i++)
            {
                SpawnWaterBullet(bossAI.fish_origin, i);
                yield return new WaitForSeconds(0.2f);
            }
        }
        //bossAI.SetWhiteBusy();
        //bossAI.SetBlackBusy();
        //yield return bossAI.co_IEcloseSwim = StartCoroutine(bossAI.IECloseSwim(true));
        //yield return bossAI.co_sprintToAngle = StartCoroutine(bossAI.IESprintToAngle(false, -180));
        //yield return bossAI.co_sprintBackEqual = StartCoroutine(bossAI.IESprintBackEqual());
        //bossAI.SetFishTargetRotateSpeed(false, 0);
        //bossAI.SetFishTargetRotateSpeed(true, 0);
        //bossAI.co_circling = StartCoroutine(bossAI.Circling(1.2f));
        //transform.DOMoveY(bossAI.waterLevel.position.y + 8f, 1f).SetEase(Ease.InOutSine).OnComplete(() =>
        //{
        //    transform.DOMoveY(bossAI.waterLevel.position.y + 1f, 0.3f).SetEase(Ease.InSine);
        //});

        //yield return new WaitForSeconds(1.3f);
        //transform.DOMoveY(bossAI.waterLevel.position.y + 4.5f, 1f).SetEase(Ease.OutSine);
        //bossAI.SetBlackTargetRotateSpeed(bossAI.idleRotateSpeed);
        //bossAI.SetWhiteTargetRotateSpeed(bossAI.idleRotateSpeed);
        //yield return new WaitForSeconds(0.5f);

        //if (bossAI.initialAction = bossAI.splash)
        //{
        //    if (bossAI.distanceToPlayer <= bossAI.close_distance_threshhold)
        //    {
        //        //翻腾->水凝枪 / 压缩泡泡光线
        //        //条件：距离大于一定值。
        //        if (Possibility(70)) bossAI.AddAction(bossAI.swing);
        //        else { bossAI.movingTarget = bossAI.GetBoundaryFarOfPlayer(); bossAI.AddAction(bossAI.dive); }
        //        //翻腾->潜水
        //        //条件: 距离大于一定值时（靠近） or 距离小于一定值时（远离)。
        //    }
        //    else if (bossAI.distanceToPlayer >= bossAI.far_distance_threshhold)
        //    {
        //        //翻腾->双摆尾
        //        //条件：距离小于一定值时。
        //        if (Possibility(70)) bossAI.AddAction(RandomPick<IEnemyAction>(bossAI.waterSpear, bossAI.gatling));
        //        else { bossAI.movingTarget = bossAI.player; bossAI.AddAction(bossAI.dive); }
        //        //翻腾->潜水
        //        //条件: 距离大于一定值时（靠近） or 距离小于一定值时（远离）。
        //    }
        //}

        //bossAI.SetNormalRotateSpeed();
        //bossAI.SetWhiteNotBusy();
        //bossAI.SetBlackNotBusy();

        //bossAI.EndAction();
        bossAI.AddActionBreak(actionBreakAmount);
        yield return null;
    }

    public void SpawnWaterBullet(Transform origin, int index)
    {
        IProjectile bullet = bossAI.selfPooler.SpawnFromPool("water_bullet",
            new Vector3(origin.position.x + shootPositionX[index],
            bossAI.waterLevel.position.y + 1.5f, 0)).
            GetComponent<IProjectile>();

        GameObject bulletEffect = bossAI.selfPooler.SpawnFromPool("water_bullet_hit_effect",
            new Vector3(origin.position.x + shootPositionX[index],
            bossAI.waterLevel.position.y, 0));
        bulletEffect.transform.rotation = Quaternion.Euler(0, 0, 90);

        bullet.SetUp(new Vector3(0, 0, 90),
            this.gameObject,
            _damage: damage,
            _speed: bulletSpeed,
            gravityScale: gravityScale,
            _stunValue: stunValue);

        IProjectile bullet2 = bossAI.selfPooler.SpawnFromPool("water_bullet",
           new Vector3(origin.position.x - shootPositionX[index],
           bossAI.waterLevel.position.y + 1.5f, 0)).
           GetComponent<IProjectile>();

        GameObject bulletEffect2 = bossAI.selfPooler.SpawnFromPool("water_bullet_hit_effect",
            new Vector3(origin.position.x - shootPositionX[index],
            bossAI.waterLevel.position.y, 0));
        bulletEffect2.transform.rotation = Quaternion.Euler(0, 0, 90);

        bullet2.SetUp(new Vector3(0, 0, 90),
            this.gameObject,
            _damage: damage,
            _speed: bulletSpeed,
            gravityScale: gravityScale,
            _stunValue: stunValue);
    }
}