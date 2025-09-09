using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YYF_Gatling : IEnemyAction
{
    private YingYangFish_AI bossAI;

    public Transform gatlingPos_black;
    public Transform gatlingPos_white;

    public float trackingSpeed = 20f;
    public float shootInterval = 0.2f;
    public float shootDuration = 5f;
    public float bulletSpeed = 100f;
    public float bulletDamage = 10;

    public float damageCooldown = 0.2f;
    private float damageTimer = 0f;
    private float selfDamageTimer = 0f;

    public override void Start()
    {
        base.Start();
        bossAI = GetComponent<YingYangFish_AI>();
    }

    private void Update()
    {
        damageTimer += Time.deltaTime;
        selfDamageTimer += Time.deltaTime;
    }

    public override void CancelAct()
    {
        base.CancelAct();
        StartCoroutine(CenterEnd());
    }

    public override bool CanAct()
    {
        if (bossAI.isWhiteBusy && bossAI.isBlackBusy) return false;
        else return true;
    }

    public IEnumerator CenterEnd()
    {
        if (gatlingPos_black.gameObject.activeInHierarchy) gatlingPos_black.GetComponent<Animator>().Play("end");
        if (gatlingPos_white.gameObject.activeInHierarchy) gatlingPos_white.GetComponent<Animator>().Play("end");
        yield return new WaitForSeconds(0.5f);
        gatlingPos_black.gameObject.SetActive(false);
        gatlingPos_white.gameObject.SetActive(false);
    }

    public override IEnumerator Act_coroutine(float factor = 0)
    {
        proceedCall = false;
        bool isBlack = false;
        if (!bossAI.isWhiteBusy && !bossAI.isBlackBusy) { isBlack = bossAI.CheckCloserFish() == bossAI.blackFish; }
        if (bossAI.isWhiteBusy && !bossAI.isBlackBusy) { isBlack = true; }
        bossAI.SetBusy(isBlack);

        yield return bossAI.co_IEcloseSwim = StartCoroutine(bossAI.IECloseSwim(false));
        yield return bossAI.co_sprintStartPoint = StartCoroutine(bossAI.IESprintStartPoint(isBlack ? "black" : "white"));

        Animator anim = isBlack ? bossAI.blackAnim : bossAI.whiteAnim;
        Transform fish = isBlack ? bossAI.blackFish : bossAI.whiteFish;
        Transform origin = isBlack ? bossAI.blackOrigin : bossAI.whiteOrigin;
        Transform fishGFX = isBlack ? bossAI.blackFishGFX : bossAI.whiteFishGFX;
        Transform gatlingSpawnPos = isBlack ? gatlingPos_black : gatlingPos_white;

        anim.Play("gatling_pre");
        gatlingSpawnPos.gameObject.SetActive(true);

        if (factor == 1) { yield return new WaitForSeconds(5f); }
        // --- Gatling shooting logic ---
        float elapsed = 0f;

        float shootTimer = 0f;
        bool proceeded = false;
        yield return new WaitForSeconds(0.5f);
        // Initial direction to player
        Quaternion aimDirection = CalculateWantedRotation(playerIDamagable.GetHitPos(), gatlingSpawnPos.position);
        while (elapsed < shootDuration)
        {
            elapsed += Time.deltaTime;
            shootTimer += Time.deltaTime;

            aimDirection = Quaternion.RotateTowards(aimDirection,
                CalculateWantedRotation(playerIDamagable.GetHitPos(), gatlingSpawnPos.position),
                trackingSpeed * Time.deltaTime);

            while (shootTimer >= shootInterval)
            {
                // Add random Vector2 offset to the spawn position
                Vector2 randomOffset = Random.insideUnitCircle * 1f; // adjust 0.3f as needed
                Vector3 spawnPosWithOffset = gatlingSpawnPos.position + new Vector3(randomOffset.x, randomOffset.y, 0f);
                ShootBulletAtPosition(spawnPosWithOffset, aimDirection.eulerAngles);
                shootTimer -= shootInterval;
            }

            if (bossAI.initialAction == bossAI.waterSpear && !proceeded && shootDuration - elapsed <= 1.5f)
            { bossAI.waterSpear.OnProceedCall(); proceeded = true; }
            yield return null;
        }

        //end
        anim.SetTrigger("gatling_end");
        gatlingSpawnPos.GetComponent<Animator>().Play("end");

        yield return new WaitForSeconds(0.5f);

        if (bossAI.initialAction = bossAI.gatling)
        {
            //压缩泡泡光线->翻腾 / 双摆尾
            //条件：距离小于一定值。
            if (bossAI.distanceToPlayer <= bossAI.close_distance_threshhold)
            {
                //压缩泡泡光线->潜水->水凝枪 = 泡泡牢笼 / 单摆尾。
                //条件：在释放完压缩泡泡光线后，距离小于一定值时（远离）
                if (Possibility(50))
                {
                    bossAI.movingTarget = bossAI.GetBoundaryFarOfPlayer(); bossAI.AddAction(bossAI.dive);
                    bossAI.AddAction(bossAI.waterSpear);
                    bossAI.AddAction(RandomPick<IEnemyAction>(bossAI.bubbleTrap, bossAI.singleSwing));
                }
                else
                {
                    bossAI.AddAction(RandomPick<IEnemyAction>(bossAI.splash, bossAI.swing));
                }
            }
            //压缩泡泡光线->潜水->翻腾 ？翻腾
            //条件：在释放完水凝枪后，距离大于一定值时（靠近）
            //？：释放后的2秒内Boss受到伤害时有50 %。
            else if (bossAI.distanceToPlayer >= bossAI.far_distance_threshhold)
            {
                bossAI.movingTarget = bossAI.player;
                bossAI.AddAction(bossAI.dive);
                bossAI.AddAction(bossAI.splash);
            }
        }

        gatlingSpawnPos.gameObject.SetActive(false);

        bossAI.SetNotBusy(isBlack);
        bossAI.SetFishTargetRotateSpeed(isBlack, bossAI.idleRotateSpeed);
        bossAI.AddActionBreak(actionBreakAmount);
        bossAI.EndAction();
    }

    private string[] bulletTags = { "gatling_bullet_1", "gatling_bullet_2", "gatling_bullet_3" };

    private void ShootBulletAtPosition(Vector3 position, Vector3 dir)
    {
        string selectedTag = bulletTags[Random.Range(0, bulletTags.Length)];
        IProjectile bullet = bossAI.selfPooler.SpawnFromPool(selectedTag, position, false).GetComponent<IProjectile>();

        bullet.SetUp(dir, this.transform.gameObject, _speed: bulletSpeed, _damage: bulletDamage);
    }

    public void Hit(Gatling_bubbles bubble)
    {
        bool dealDamage = damageTimer >= damageCooldown;
        GameManager.instance.playerhealth.Damage(
            dealDamage ? bubble.damage : 0,
            transform,
            bubble.stunDuration,
            stunValue: bubble.stunValue);
        if (dealDamage) damageTimer = 0f;
    }

    public void HitSelf(Gatling_bubbles bubble)
    {
        bool dealDamage = selfDamageTimer >= damageCooldown;
        GameManager.instance.playerhealth.Damage(
            dealDamage ? bubble.damage : 0,
            transform,
            bubble.stunDuration,
            stunValue: dealDamage ? bubble.stunValue : 0);
        if (dealDamage) selfDamageTimer = 0f;
    }
}