using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class YYF_splash : IEnemyAction
{
    private YingYangFish_AI bossAI;
    public Transform shootPos;
    public Vector2[] shootDirecitons_close;
    public Vector2[] shootDirecitons_far;

    public int damage = 2;
    public float gravityScale = 5f;

    public override void Start()
    {
        base.Start();
        bossAI = GetComponent<YingYangFish_AI>();
    }

    public override IEnumerator Act_coroutine()
    {
        yield return bossAI.co_sprintStartPoint = StartCoroutine(bossAI.SprintStartPoint());

        if (bossAI.closerFish_Black)
        {
            bossAI.blackAnim.Play("splash");
        }
        else
        {
            bossAI.whiteAnim.Play("splash");
        }

        yield return new WaitForSeconds(1f);

        for (int i = 0; i < shootDirecitons_close.Count(); i++)
        {
            SpawnWaterBullet(i);
        }

        yield return new WaitForSeconds(0.5f);

        if (bossAI.initialAction == this)
        {
            if (bossAI.distanceToPlayer <= bossAI.swing.swingRange) { bossAI.InsertAction(bossAI.swing); }
            else if (Possibility(50))
            {
                bossAI.InsertAction(bossAI.waterSpear);
            }
            else
            {
                if (bossAI.closerFish_Black)
                {
                    bossAI.blackAnim.Play("splash");
                }
                else
                {
                    bossAI.whiteAnim.Play("splash");
                }

                yield return new WaitForSeconds(1f);

                for (int i = 0; i < shootDirecitons_close.Count(); i++)
                {
                    SpawnWaterBullet(i);
                }
            }
        }

        yield return bossAI.co_sprintBackEqual = StartCoroutine(bossAI.SprintBackEqual());

        bossAI.SetNormalRotateSpeed();
        bossAI.AddActionBreak(actionBreakAmount);
        bossAI.EndAction();

        yield return null;
    }

    public void SpawnWaterBullet(int index)
    {
        IProjectile bullet = pooler.SpawnFromPool("water_bullet", shootPos.position).GetComponent<IProjectile>();
        if (player.transform.position.x <= shootPos.position.x)
        {
            if (Mathf.Abs(shootPos.position.x - player.transform.position.x) <= 6)
            {
                bullet.SetUp(new Vector3(0, 0, 90 + shootDirecitons_close[index].x), this.gameObject, _damage: damage, _speed: shootDirecitons_close[index].y, gravityScale: gravityScale);
            }
            else
            {
                bullet.SetUp(new Vector3(0, 0, 90 + shootDirecitons_far[index].x), this.gameObject, _damage: damage, _speed: shootDirecitons_far[index].y, gravityScale: gravityScale);
            }
        }
        else
        {
            if (Mathf.Abs(shootPos.position.x - player.transform.position.x) <= 6)
            {
                bullet.SetUp(new Vector3(0, 0, 90 - shootDirecitons_close[index].x), this.gameObject, _damage: damage, _speed: shootDirecitons_close[index].y, gravityScale: gravityScale);
            }
            else
            {
                bullet.SetUp(new Vector3(0, 0, 90 - shootDirecitons_far[index].x), this.gameObject, _damage: damage, _speed: shootDirecitons_far[index].y, gravityScale: gravityScale);
            }
        }
    }

    public void OnDrawGizmos()
    {
    }
}