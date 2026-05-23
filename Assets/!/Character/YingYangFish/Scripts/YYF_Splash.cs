using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.PlayerSettings;
using static UnityEngine.UI.Image;

public class YYF_Splash : IEnemyAction
{
    private YingYangFish_AI bossAI;
    public Transform splashEffect_1;
    public Transform splashEffect_2;
    public float[] shootPositionX;
    public float bulletSpeed = 150f;
    public int damage = 2;
    public float gravityScale = 5f;
    private Coroutine co_facePlayer;
    public float hitRange = 3f;
    public MeleeAttack splashAttack;

    public override void Start()
    {
        base.Start();
        bossAI = GetComponent<YingYangFish_AI>();
    }

    public override void CancelAct()
    {
        base.CancelAct();
        if (co_facePlayer != null) StopCoroutine(co_facePlayer);
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
            origin.position = new Vector3(origin.position.x, bossAI.waterLevel.position.y - 7f, origin.position.z);

            //before jump out
            if (toLeft) { origin.DOMove(new Vector3(target.x + 2, bossAI.waterLevel.position.y - 6, 0), 0.3f); }
            else { origin.DOMove(new Vector3(target.x - 2, bossAI.waterLevel.position.y - 6, 0), 0.3f); }

            //reset to initial
            origin.eulerAngles = Vector3.zero;
            fish.localPosition = new Vector3(0, 0, 0);
            bossAI.ResetFishGFX(factor);
            yield return new WaitForSeconds(0.3f);

            //jump out
            float temp_x = toLeft ? player.transform.position.x + 2 : player.transform.position.x - 2;
            if (isBlack) temp_x = toLeft ? player.transform.position.x - 2 : player.transform.position.x + 2;
            origin.DOMove(new Vector3(temp_x, bossAI.waterLevel.position.y + 10f, 0), 0.5f).SetEase(Ease.OutSine);
            anim.Play("splash");

            //keep rotating fishGFX
            Tween rotating = fishGFX.DOLocalRotate(new Vector3(0, 0, -360f), 0.2f, RotateMode.FastBeyond360)
              .SetEase(Ease.Linear)
              .SetLoops(-1, LoopType.Restart);
            Transform splashEffect = isBlack ? splashEffect_2 : splashEffect_1;

            yield return new WaitForSeconds(0.5f);

            //set splash effect position to fish GFX position
            splashEffect.position = fish.position;
            splashEffect.localPosition = new Vector3(1, 0, 0);
            co_facePlayer = StartCoroutine(IEFaceSplashAtPlayer(splashEffect, fish, isBlack));
            if (!isBlack) splashEffect.eulerAngles = new Vector3(0, 0, -90);

            //fade in to show splash effect
            splashEffect.gameObject.SetActive(true);
            splashEffect.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0);
            splashEffect.GetComponent<SpriteRenderer>().DOFade(1, 0.5f);

            //be ready;
            yield return new WaitForSeconds(0.5f);

            //blackfish: be ready and dash to player.
            if (isBlack)
            {
                origin.DOMove(new Vector3(player.transform.position.x, bossAI.waterLevel.position.y, 0), bossAI.fastSwimSpeed)
                    .SetEase(Ease.OutSine)
                    .SetSpeedBased();
            }
            //white fish: be ready dash to ground
            else
            {
                origin.DOMove(new Vector3(origin.position.x, bossAI.waterLevel.position.y, 0), bossAI.fastSwimSpeed)
                    .SetEase(Ease.OutSine)
                    .SetSpeedBased();
            }

            StartCoroutine(ApplyAttackInCircle(1f, hitRange, origin, splashAttack));

            yield return new WaitForSeconds(0.3f);

            //spawn bullets

            StartCoroutine(IESpawnBullet(bossAI.CreateWaterLevelYAxis(origin.position), isBlack));

            splashEffect.gameObject.SetActive(false);

            //move origin down water
            StopCoroutine(co_facePlayer);
            rotating.Kill();
            origin.DOKill();
            origin.DOMove(new Vector3(origin.position.x, bossAI.waterLevel.position.y - 7f, 0f), 0.05f).SetEase(Ease.Linear);
        }
        else if (factor == 2)//both fish
        {
            Vector3 target = player.transform.position + new Vector3(0, 3, 0);
            Transform origin = bossAI.fish_origin;
            bool toLeft = target.x < origin.position.x;

            bossAI.ResetFish();
            bossAI.ResetFishGFX();
            bossAI.ResetFishOrigin();

            origin.position = new Vector3(origin.position.x, bossAI.waterLevel.position.y - 7f, origin.position.z);

            //before jump out
            if (toLeft) { origin.DOMove(new Vector3(target.x + 2, bossAI.waterLevel.position.y - 6, 0), 0.3f); }
            else { origin.DOMove(new Vector3(target.x - 2, bossAI.waterLevel.position.y - 6, 0), 0.3f); }

            bossAI.whiteOrigin.Rotate(bossAI.Dir, -90);
            bossAI.blackOrigin.Rotate(bossAI.Dir, 90);
            //reset to initial
            origin.eulerAngles = Vector3.zero;
            bossAI.blackFish.localPosition = new Vector3(0, 0.3f, 0);
            bossAI.whiteFish.localPosition = new Vector3(0, 0.3f, 0);
            yield return new WaitForSeconds(0.3f);

            //jump out
            float temp_x = toLeft ? player.transform.position.x + 2 : player.transform.position.x - 2;
            origin.DOMove(new Vector3(temp_x, bossAI.waterLevel.position.y + 10f, 0), 0.5f).SetEase(Ease.OutSine);
            bossAI.blackAnim.Play("splash");
            bossAI.whiteAnim.Play("splash");

            //keep rotating fishGFX
            Tween rotating = origin.DOLocalRotate(new Vector3(0, 0, -360f), 0.2f, RotateMode.FastBeyond360)
              .SetEase(Ease.Linear)
              .SetLoops(-1, LoopType.Restart);

            //set splash effect position to fish GFX position
            splashEffect_1.position = origin.position;
            //splashEffect.SetParent(origin);
            splashEffect_1.localPosition = new Vector3(2f, 0, 0);

            co_facePlayer = StartCoroutine(IEFaceSplashAtPlayer(splashEffect_1, origin, true));

            //fade in to show splash effect
            splashEffect_1.gameObject.SetActive(true);
            splashEffect_1.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0);
            splashEffect_1.GetComponent<SpriteRenderer>().DOFade(1, 0.8f);

            //be ready;
            yield return new WaitForSeconds(1f);

            //blackfish: be ready and dash to player.

            origin.DOMove(new Vector3(player.transform.position.x, bossAI.waterLevel.position.y, 0), bossAI.fastSwimSpeed)
                .SetEase(Ease.OutSine)
                .SetSpeedBased();

            StartCoroutine(ApplyAttackInCircle(1f, hitRange, origin, splashAttack));
            yield return new WaitForSeconds(0.3f);
            //spawn bullets
            StartCoroutine(IESpawnBullet(bossAI.CreateWaterLevelYAxis(origin.position), false));
            StartCoroutine(IESpawnBullet(bossAI.CreateWaterLevelYAxis(origin.position), true, 0.1f));
            splashEffect_1.gameObject.SetActive(false);

            //move origin down water
            StopCoroutine(co_facePlayer);
            rotating.Kill();
            origin.DOKill();
            origin.DOMove(new Vector3(origin.position.x, bossAI.waterLevel.position.y - 7f, 0f), 0.05f).SetEase(Ease.Linear);
        }

        yield return new WaitForSeconds(0.1f);
        anim.Play("swim_up");
        splashEffect_1.gameObject.SetActive(false);
        yield return null;
    }

    private IEnumerator IEFaceSplashAtPlayer(Transform effect, Transform transform, bool face)
    {
        float elapsedTime = 0f;
        while (elapsedTime < 2f)
        {
            effect.position = transform.position;
            if (face && elapsedTime <= 1f) effect.eulerAngles = CalculateWantedEuler(player.transform.position, effect.position);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    public IEnumerator IESpawnBullet(Vector3 pos, bool isBlack, float delay = 0)
    {
        yield return new WaitForSeconds(delay);
        for (int i = 0; i < shootPositionX.Count() - (isBlack ? 1 : 0); i++)
        {
            if (isBlack) SpawnIceThorn(pos, i);
            else SpawnWaterBullet(pos, i);
            yield return new WaitForSeconds(0.2f);
        }
    }

    public void SpawnWaterBullet(Vector3 pos, int index)
    {
        IProjectile bullet = bossAI.selfPooler.SpawnFromPool("water_bullet",
            new Vector3(pos.x + shootPositionX[index],
            bossAI.waterLevel.position.y + 1.5f, 0)).
            GetComponent<IProjectile>();

        GameObject bulletEffect = bossAI.selfPooler.SpawnFromPool("water_bullet_hit_effect",
            new Vector3(pos.x + shootPositionX[index],
            bossAI.waterLevel.position.y, 0));
        bulletEffect.transform.rotation = Quaternion.Euler(0, 0, 90);

        bullet.SetUp(new Vector3(0, 0, 90),
            this.gameObject,
            _damage: damage,
            _speed: bulletSpeed,
            gravityScale: gravityScale,
            _stunValue: stunValue);

        IProjectile bullet2 = bossAI.selfPooler.SpawnFromPool("water_bullet",
           new Vector3(pos.x - shootPositionX[index],
           bossAI.waterLevel.position.y + 1.5f, 0)).
           GetComponent<IProjectile>();

        GameObject bulletEffect2 = bossAI.selfPooler.SpawnFromPool("water_bullet_hit_effect",
            new Vector3(pos.x - shootPositionX[index],
            bossAI.waterLevel.position.y, 0));
        bulletEffect2.transform.rotation = Quaternion.Euler(0, 0, 90);

        bullet2.SetUp(new Vector3(0, 0, 90),
            this.gameObject,
            _damage: damage,
            _speed: bulletSpeed,
            gravityScale: gravityScale,
            _stunValue: stunValue);
    }

    public void SpawnIceThorn(Vector3 pos, int index)
    {
        IceThorn thorn = bossAI.selfPooler.SpawnFromPool("ice_thorn",
            new Vector3(pos.x + shootPositionX[index],
            bossAI.waterLevel.position.y, 0)).
            GetComponent<IceThorn>();
        StartCoroutine(thorn.Action(new Vector3(pos.x + shootPositionX[index] - 2.5f,
            bossAI.waterLevel.position.y, 0)));
        IceThorn thorn2 = bossAI.selfPooler.SpawnFromPool("ice_thorn",
           new Vector3(pos.x - shootPositionX[index],
           bossAI.waterLevel.position.y, 0)).
           GetComponent<IceThorn>();
        StartCoroutine(thorn2.Action(new Vector3(pos.x - shootPositionX[index] + 2.5f,
            bossAI.waterLevel.position.y, 0)));
        return;
    }

    public void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, hitRange);
    }
}