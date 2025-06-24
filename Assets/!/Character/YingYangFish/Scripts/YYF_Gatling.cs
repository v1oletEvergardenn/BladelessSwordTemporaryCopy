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
    public int bulletDamage = 10;

    public override void Start()
    {
        base.Start();
        bossAI = GetComponent<YingYangFish_AI>();
    }

    public override void CancelAct()
    {
        base.CancelAct();
        StartCoroutine(CenterEnd());
    }

    public IEnumerator CenterEnd()
    {
        gatlingPos_black.GetComponent<Animator>().Play("end");
        gatlingPos_white.GetComponent<Animator>().Play("end");
        yield return new WaitForSeconds(0.5f);
        gatlingPos_black.gameObject.SetActive(false);
        gatlingPos_white.gameObject.SetActive(false);
    }

    public override IEnumerator Act_coroutine(float factor = 0)
    {
        bool isBlack = false;

        yield return bossAI.co_IEcloseSwim = StartCoroutine(bossAI.IECloseSwim(false));
        yield return bossAI.co_sprintStartPoint = StartCoroutine(bossAI.IESprintStartPoint());
        isBlack = bossAI.closerFish_Black;

        Animator anim = isBlack ? bossAI.blackAnim : bossAI.whiteAnim;
        Transform fish = isBlack ? bossAI.blackFish : bossAI.whiteFish;
        Transform origin = isBlack ? bossAI.blackOrigin : bossAI.whiteOrigin;
        Transform fishGFX = isBlack ? bossAI.blackFishGFX : bossAI.whiteFishGFX;
        Transform gatlingSpawnPos = isBlack ? gatlingPos_black : gatlingPos_white;

        anim.Play("gatling_pre");
        gatlingSpawnPos.gameObject.SetActive(true);
        // --- Gatling shooting logic ---
        float elapsed = 0f;

        float shootTimer = 0f;

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

            yield return null;
        }

        //end
        anim.SetTrigger("gatling_end");
        gatlingSpawnPos.GetComponent<Animator>().Play("end");

        yield return new WaitForSeconds(0.5f);
        gatlingSpawnPos.gameObject.SetActive(false);

        if (isBlack) { bossAI.SetBlackNotBusy(); } else { bossAI.SetWhiteNotBusy(); }
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
}