using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class YYF_splash_white : IEnemyAction
{
    private YingYangFish_AI bossAI;
    public Transform shootPos;
    public Vector2[] shootDirecitons;

    public int damage = 2;
    public float gravityScale = 5f;

    public override void Start()
    {
        base.Start();
        bossAI = GetComponent<YingYangFish_AI>();
    }

    //low and far
    public override IEnumerator Act_coroutine(float factor = 0)
    {
        if (factor == 0)
        {
            yield return bossAI.co_IEcloseSwim = StartCoroutine(bossAI.IECloseSwim(false));

            bool finished = false;
            bossAI.white_targetRotateSpeed = 0;
            bossAI.whiteAnim.Play("sprint");
            bossAI.whiteOrigin.DORotate(new Vector3(0, 0, -180), bossAI.sprintRotateSpeed, RotateMode.FastBeyond360).SetEase(Ease.OutSine).SetSpeedBased(true).OnComplete(() => { finished = true; });
            while (!finished) { yield return null; }
        }
        else if (factor == 1)
        {
            bossAI.white_targetRotateSpeed = bossAI.sprintRotateSpeed;
            bossAI.whiteAnim.Play("sprint");
            while (Mathf.Abs(180 - bossAI.whiteFish.eulerAngles.z) >= 10)
            {
                yield return null;
            }
            bossAI.white_targetRotateSpeed = 0;
        }

        bossAI.whiteAnim.Play("splash");

        yield return new WaitForSeconds(1f);

        for (int i = 0; i < shootDirecitons.Count(); i++)
        {
            SpawnWaterBullet(i);
        }

        yield return new WaitForSeconds(0.5f);

        if (bossAI.initialAction == this)
        {
            if (!playerController.isGrounded || bossAI.distanceToPlayer <= bossAI.swing.swingRange)
            {
                bossAI.InsertAction(bossAI.splash_black);
            }
            if (Possibility(50)) { bossAI.InsertAction(bossAI.waterSpear); }
        }

        if (factor == 0)
        {
            yield return bossAI.co_sprintBackEqual = StartCoroutine(bossAI.SprintBackEqual());
            bossAI.SetNormalRotateSpeed();
        }
        else
        {
            bossAI.white_targetRotateSpeed = bossAI.sprintRotateSpeed;
        }

        bossAI.AddActionBreak(actionBreakAmount);
        bossAI.EndAction();

        yield return null;
    }

    public void SpawnWaterBullet(int index)
    {
        IProjectile bullet = pooler.SpawnFromPool("water_bullet", shootPos.position).GetComponent<IProjectile>();
        if (player.transform.position.x <= shootPos.position.x)
        {
            bullet.SetUp(new Vector3(0, 0, 90 + shootDirecitons[index].x), this.gameObject, _damage: damage, _speed: shootDirecitons[index].y, gravityScale: gravityScale);
        }
        else
        {
            bullet.SetUp(new Vector3(0, 0, 90 - shootDirecitons[index].x), this.gameObject, _damage: damage, _speed: shootDirecitons[index].y, gravityScale: gravityScale);
        }
    }
}