using DG.Tweening;
using System.Collections;
using System.Linq;
using UnityEngine;

public class YYF_Splash : IEnemyAction
{
    private YingYangFish_AI bossAI;
    public Transform splashEffect_1;
    public Transform splashEffect_2;
    public float[] shootPositionX;

    public float spawnDelay = 1f;

    public IProjectileBasicAttributes bulletAttribute;
    public float gravityScale = 5f;
    private Coroutine co_facePlayer;
    public float hitRange = 3f;
    public MeleeAttack splashAttack;
    public MeleeAttack iceAttack;

    private Coroutine co_spawnWaterBullet;

    private Coroutine co_spawnWaterBullet2;

    public override void Start()
    {
        base.Start();
        bossAI = GetComponent<YingYangFish_AI>();
    }

    public override void CancelAct()
    {
        base.CancelAct();
        if (co_facePlayer != null) StopCoroutine(co_facePlayer);
        TryStopCoroutine(co_spawnWaterBullet);
        TryStopCoroutine(co_spawnWaterBullet2);
    }

    public override IEnumerator Act_coroutine(float factor = 0, Transform _target = null)
    {
        Transform trueTarget = _target != null ? _target : player.transform;
        //factor = 3 & factor = 4: no spawning bullets and ice thorn, only splash attack
        //black fish or white fish
        if (factor == 0 || factor == 1 || factor == 3 || factor == 4)
        {
            bool isBlack = true;
            if (factor == 1 || factor == 4) { isBlack = false; }
            YYF_fish YYFFish = bossAI.SpawnFish(isBlack);
            Animator anim = YYFFish.anim;
            Transform fish = YYFFish.fish;
            Transform origin = YYFFish.transform;
            Transform fishGFX = YYFFish.fishGFX;

            bool spawningBullets = true;
            if (factor == 3 || factor == 4) spawningBullets = false;

            Vector3 target = trueTarget.position + new Vector3(0, 3, 0);
            bool toLeft = GameManager.instance.isInPerformingState ? !isBlack : GetAttackDirection();
            origin.position = new Vector3(origin.position.x, bossAI.waterLevel.position.y - 7f, origin.position.z);

            //before jump out
            origin.position = new Vector3(target.x + (toLeft ? 2f : -2f), bossAI.waterLevel.position.y - 7f, 0);

            //reset to initial
            origin.eulerAngles = Vector3.zero;
            fish.localPosition = new Vector3(0, 0, 0);
            bossAI.ResetFishGFX(YYFFish);

            //jump out
            float temp_x = toLeft ? trueTarget.position.x + 2 : trueTarget.position.x - 2;
            if (isBlack) temp_x = toLeft ? trueTarget.position.x - 2 : trueTarget.position.x + 2;
            origin.DOMove(new Vector3(temp_x, bossAI.waterLevel.position.y + 10f, 0), 0.5f).SetEase(Ease.OutSine).SetTimeDt(this, TimeChannel.Enemy);
            anim.Play("splash");

            //keep rotating fishGFX
            Tween rotating = fishGFX.DOLocalRotate(new Vector3(0, 0, -360f), 0.2f, RotateMode.FastBeyond360)
              .SetEase(Ease.Linear)
              .SetLoops(-1, LoopType.Restart)
              .SetTimeDt(this, TimeChannel.Enemy);
            Transform splashEffect = isBlack ? splashEffect_2 : splashEffect_1;

            yield return WaitForEnemy(0.5f);

            //set splash effect position to fish GFX position
            splashEffect.position = fish.position;
            splashEffect.localPosition = new Vector3(1, 0, 0);
            co_facePlayer = StartCoroutine(IEFaceSplashAtTarget(splashEffect, fish, isBlack, trueTarget));
            if (!isBlack) splashEffect.eulerAngles = new Vector3(0, 0, -90);

            //fade in to show splash effect
            splashEffect.gameObject.SetActive(true);
            splashEffect.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0);
            splashEffect.GetComponent<SpriteRenderer>().DOFade(1, 0.5f).SetTimeDt(this, TimeChannel.Enemy);

            //be ready;
            yield return WaitForEnemy(0.5f);

            //blackfish: be ready and dash to player.
            if (isBlack)
            {
                origin.DOMove(new Vector3(trueTarget.position.x, bossAI.waterLevel.position.y, 0), bossAI.fastSwimSpeed)
                    .SetEase(Ease.OutSine)
                    .SetSpeedBased()
                    .SetTimeDt(this, TimeChannel.Enemy);
            }
            //white fish: be ready dash to ground
            else
            {
                origin.DOMove(new Vector3(origin.position.x, bossAI.waterLevel.position.y, 0), bossAI.fastSwimSpeed)
                    .SetEase(Ease.OutSine)
                    .SetSpeedBased()
                    .SetTimeDt(this, TimeChannel.Enemy);
            }

            StartCoroutine(ApplyAttackInCircle(0.3f, hitRange, origin, splashAttack));

            yield return WaitForEnemy(0.3f);

            if (spawningBullets)
            {
                //spawn bullets

                if (isBlack)
                {
                    co_spawnWaterBullet2 = StartCoroutine(IESpawnBullet(bossAI.CreateWaterLevelYAxis(origin.position), isBlack));
                }
                else
                {
                    co_spawnWaterBullet = StartCoroutine(IESpawnBullet(bossAI.CreateWaterLevelYAxis(origin.position), isBlack));
                }
            }

            splashEffect.gameObject.SetActive(false);

            //move origin down water
            StopCoroutine(co_facePlayer);
            rotating.Kill();
            origin.DOKill();
            origin.DOMove(new Vector3(origin.position.x, bossAI.waterLevel.position.y - 7f, 0f), 0.05f).
                SetEase(Ease.Linear).
                SetTimeDt(this, TimeChannel.Enemy);
            YYFFish.gameObject.SetActive(false);
        }
        else if (factor == 2 || factor == 5)//both fish
        {
            bool spawningBullets = true;
            if (factor == 5) spawningBullets = false;
            Vector3 target = trueTarget.position + new Vector3(0, 3, 0);

            YYF_fish black_YYFFish = bossAI.SpawnFish(true);
            YYF_fish white_YYFFish = bossAI.SpawnFish(false);

            bool toLeft = GetAttackDirection();
            //origin.position = new Vector3(origin.position.x, bossAI.waterLevel.position.y - 7f, origin.position.z);

            //before jump out
            if (toLeft)
            {
                white_YYFFish.transform.position = new Vector3(target.x + 2, bossAI.waterLevel.position.y - 7, 0);
                black_YYFFish.transform.position = new Vector3(target.x + 2, bossAI.waterLevel.position.y - 7, 0);
            }
            else
            {
                white_YYFFish.transform.position = new Vector3(target.x - 2, bossAI.waterLevel.position.y - 7, 0);
                black_YYFFish.transform.position = new Vector3(target.x - 2, bossAI.waterLevel.position.y - 7, 0);
            }

            white_YYFFish.fish.transform.Rotate(bossAI.Dir, -90);
            black_YYFFish.fish.transform.Rotate(bossAI.Dir, 90);
            //reset to initial
            black_YYFFish.fish.localPosition = new Vector3(0, 0.3f, 0);
            white_YYFFish.fish.localPosition = new Vector3(0, 0.3f, 0);

            //jump out
            float temp_x = toLeft ? trueTarget.position.x + 2 : trueTarget.position.x - 2;
            white_YYFFish.transform.DOMove(new Vector3(temp_x, bossAI.waterLevel.position.y + 10f, 0), 0.5f).
                SetEase(Ease.OutSine).SetTimeDt(this, TimeChannel.Enemy);
            black_YYFFish.transform.DOMove(new Vector3(temp_x, bossAI.waterLevel.position.y + 10f, 0), 0.5f).
                SetEase(Ease.OutSine).SetTimeDt(this, TimeChannel.Enemy);
            black_YYFFish.anim.Play("splash");
            white_YYFFish.anim.Play("splash");

            //keep rotating fishGFX
            Tween rotating = white_YYFFish.transform.DOLocalRotate(new Vector3(0, 0, -360f), 0.2f, RotateMode.FastBeyond360)
              .SetEase(Ease.Linear)
              .SetLoops(-1, LoopType.Restart)
              .SetTimeDt(this, TimeChannel.Enemy);
            Tween rotating2 = black_YYFFish.transform.DOLocalRotate(new Vector3(0, 0, -360f), 0.2f, RotateMode.FastBeyond360)
              .SetEase(Ease.Linear)
              .SetLoops(-1, LoopType.Restart)
              .SetTimeDt(this, TimeChannel.Enemy);

            //set splash effect position to fish GFX position
            splashEffect_1.position = white_YYFFish.transform.position;
            //splashEffect.SetParent(origin);
            splashEffect_1.localPosition = new Vector3(2f, 0, 0);

            co_facePlayer = StartCoroutine(IEFaceSplashAtTarget(splashEffect_1, white_YYFFish.transform, true, trueTarget));

            //fade in to show splash effect
            splashEffect_1.gameObject.SetActive(true);
            splashEffect_1.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0);
            splashEffect_1.GetComponent<SpriteRenderer>().DOFade(1, 0.8f).SetTimeDt(this, TimeChannel.Enemy);

            //be ready;
            yield return WaitForEnemy(1f);

            //blackfish: be ready and dash to player.

            white_YYFFish.transform.DOMove(new Vector3(trueTarget.position.x, bossAI.waterLevel.position.y, 0), bossAI.fastSwimSpeed)
                .SetEase(Ease.OutSine)
                .SetSpeedBased()
                .SetTimeDt(this, TimeChannel.Enemy);
            black_YYFFish.transform.DOMove(new Vector3(trueTarget.position.x, bossAI.waterLevel.position.y, 0), bossAI.fastSwimSpeed)
                .SetEase(Ease.OutSine)
                .SetSpeedBased()
                .SetTimeDt(this, TimeChannel.Enemy);
            StartCoroutine(ApplyAttackInCircle(0.3f, hitRange, white_YYFFish.transform, splashAttack));
            yield return WaitForEnemy(0.3f);
            //spawn bullets
            if (spawningBullets)
            {
                co_spawnWaterBullet2 = StartCoroutine(IESpawnBullet(bossAI.CreateWaterLevelYAxis(white_YYFFish.transform.position), false));
                co_spawnWaterBullet = StartCoroutine(IESpawnBullet(bossAI.CreateWaterLevelYAxis(white_YYFFish.transform.position), true, 0.1f));
            }

            splashEffect_1.gameObject.SetActive(false);

            //move origin down water
            StopCoroutine(co_facePlayer);
            rotating.Kill();
            rotating2.Kill();
            white_YYFFish.transform.DOKill();
            black_YYFFish.transform.DOKill();
            white_YYFFish.transform.DOMove(new Vector3(white_YYFFish.transform.position.x, bossAI.waterLevel.position.y - 7f, 0f),
                0.05f).SetEase(Ease.Linear).SetTimeDt(this, TimeChannel.Enemy);
            black_YYFFish.transform.DOMove(new Vector3(black_YYFFish.transform.position.x, bossAI.waterLevel.position.y - 7f, 0f),
                0.05f).SetEase(Ease.Linear).SetTimeDt(this, TimeChannel.Enemy);
            white_YYFFish.gameObject.SetActive(false);
            black_YYFFish.gameObject.SetActive(false);
        }

        yield return WaitForEnemy(0.1f);
        splashEffect_1.gameObject.SetActive(false);
        attackDirectionSet = false;
        yield return null;
    }

    private IEnumerator IEFaceSplashAtTarget(Transform effect, Transform transform, bool face, Transform trueTarget)
    {
        float elapsedTime = 0f;
        while (elapsedTime < 2f)
        {
            effect.position = transform.position;
            if (face && elapsedTime <= 1f) effect.eulerAngles = CalculateWantedEuler(trueTarget.position, effect.position);
            elapsedTime += TimeScaleManager.EnemyDt;
            yield return null;
        }
    }

    public IEnumerator IESpawnBullet(Vector3 pos, bool isBlack, float delay = 0)
    {
        yield return WaitForEnemy(delay);
        for (int i = 0; i < shootPositionX.Count() - (isBlack ? 1 : 0); i++)
        {
            if (isBlack) SpawnIceThorn(pos, i);
            else StartCoroutine(SpawnWaterBullet(pos, i));
            yield return WaitForEnemy(0.2f);
        }
    }

    public IEnumerator SpawnWaterBullet(Vector3 pos, int index)
    {
        GameObject spawnEffect1 = bossAI.selfPooler.SpawnFromPool("water_bullet_spawn_effect",
            bossAI.CreateWaterLevelYAxis(pos.x + shootPositionX[index]));

        GameObject spawnEffect2 = bossAI.selfPooler.SpawnFromPool("water_bullet_spawn_effect",
           bossAI.CreateWaterLevelYAxis(pos.x - shootPositionX[index]));
        spawnEffect1.GetComponent<SelfDisactive>().SetNewDisActiveTime(spawnDelay);
        spawnEffect2.GetComponent<SelfDisactive>().SetNewDisActiveTime(spawnDelay);
        yield return WaitForEnemy(spawnDelay);
        IProjectile bullet = bossAI.selfPooler.SpawnFromPool("water_bullet",
            new Vector3(pos.x + shootPositionX[index],
            bossAI.waterLevel.position.y + 1.5f, 0)).
            GetComponent<IProjectile>();

        GameObject bulletEffect = bossAI.selfPooler.SpawnFromPool("water_bullet_hit_effect",
           bossAI.CreateWaterLevelYAxis(pos.x + shootPositionX[index]));
        bulletEffect.transform.rotation = Quaternion.Euler(0, 0, 90);

        bullet.SetUp(new Vector3(0, 0, 90), this.gameObject).
            SetAttributes(bulletAttribute).
            SetGravity(gravityScale);

        IProjectile bullet2 = bossAI.selfPooler.SpawnFromPool("water_bullet",
           new Vector3(pos.x - shootPositionX[index],
           bossAI.waterLevel.position.y + 1.5f, 0)).
           GetComponent<IProjectile>();

        GameObject bulletEffect2 = bossAI.selfPooler.SpawnFromPool("water_bullet_hit_effect",
           bossAI.CreateWaterLevelYAxis(pos.x - shootPositionX[index]));
        bulletEffect2.transform.rotation = Quaternion.Euler(0, 0, 90);

        bullet2.SetUp(new Vector3(0, 0, 90), this.gameObject).
             SetAttributes(bulletAttribute).
             SetGravity(gravityScale);

        yield return null;
    }

    public void SpawnIceThorn(Vector3 pos, int index)
    {
        IceThorn thorn = bossAI.selfPooler.SpawnFromPool("ice_thorn",
            new Vector3(pos.x + shootPositionX[index],
            bossAI.waterLevel.position.y, 0)).
            GetComponent<IceThorn>();
        thorn.iceAttack = iceAttack;
        StartCoroutine(thorn.Action(new Vector3(pos.x + shootPositionX[index] - 2.5f,
            bossAI.waterLevel.position.y, 0), spawnDelay));
        IceThorn thorn2 = bossAI.selfPooler.SpawnFromPool("ice_thorn",
           new Vector3(pos.x - shootPositionX[index],
           bossAI.waterLevel.position.y, 0)).
           GetComponent<IceThorn>();
        thorn2.iceAttack = iceAttack;
        StartCoroutine(thorn2.Action(new Vector3(pos.x - shootPositionX[index] + 2.5f,
            bossAI.waterLevel.position.y, 0), spawnDelay));
        return;
    }

    public void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, hitRange);
    }
}