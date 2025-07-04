using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEditor.PlayerSettings;

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

    //low and far
    public override IEnumerator Act_coroutine(float factor = 0)
    {
        bossAI.SetWhiteBusy();
        bossAI.SetBlackBusy();
        //yield return bossAI.co_IEcloseSwim = StartCoroutine(bossAI.IECloseSwim(false));
        //yield return bossAI.co_sprintToAngle = StartCoroutine(bossAI.IESprintToAngle(false, -180));
        yield return bossAI.co_sprintBackEqual = StartCoroutine(bossAI.IESprintBackEqual());
        bossAI.SetFishTargetRotateSpeed(false, 0);
        bossAI.SetFishTargetRotateSpeed(true, 0);
        bossAI.co_circling = StartCoroutine(bossAI.Circling(1.2f));
        transform.DOMoveY(bossAI.waterLevel.position.y + 8f, 1f).SetEase(Ease.InOutSine).OnComplete(() =>
        {
            transform.DOMoveY(bossAI.waterLevel.position.y + 1f, 0.3f).SetEase(Ease.InSine);
        });

        yield return new WaitForSeconds(1.3f);

        for (int i = 0; i < shootPositionX.Count(); i++)
        {
            SpawnWaterBullet(i);
            yield return new WaitForSeconds(0.2f);
        }

        transform.DOMoveY(bossAI.waterLevel.position.y + 4.5f, 1f).SetEase(Ease.OutSine);
        bossAI.SetBlackTargetRotateSpeed(bossAI.idleRotateSpeed);
        bossAI.SetWhiteTargetRotateSpeed(bossAI.idleRotateSpeed);
        yield return new WaitForSeconds(0.5f);
        bossAI.SetNormalRotateSpeed();
        bossAI.SetWhiteNotBusy();
        bossAI.SetBlackNotBusy();
        bossAI.AddActionBreak(actionBreakAmount);
        bossAI.EndAction();

        yield return null;
    }

    public void SpawnWaterBullet(int index)
    {
        IProjectile bullet = bossAI.selfPooler.SpawnFromPool("water_bullet",
            new Vector3(transform.position.x + shootPositionX[index],
            bossAI.waterLevel.position.y + 1.5f, 0)).
            GetComponent<IProjectile>();

        GameObject bulletEffect = bossAI.selfPooler.SpawnFromPool("water_bullet_hit_effect",
            new Vector3(transform.position.x + shootPositionX[index],
            bossAI.waterLevel.position.y, 0));
        bulletEffect.transform.rotation = Quaternion.Euler(0, 0, 90);

        bullet.SetUp(new Vector3(0, 0, 90),
            this.gameObject,
            _damage: damage,
            _speed: bulletSpeed,
            gravityScale: gravityScale,
            _stunValue: stunValue);

        IProjectile bullet2 = bossAI.selfPooler.SpawnFromPool("water_bullet",
           new Vector3(transform.position.x - shootPositionX[index],
           bossAI.waterLevel.position.y + 1.5f, 0)).
           GetComponent<IProjectile>();

        GameObject bulletEffect2 = bossAI.selfPooler.SpawnFromPool("water_bullet_hit_effect",
            new Vector3(transform.position.x - shootPositionX[index],
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