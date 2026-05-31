using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YYF_Gatling : IEnemyAction
{
    private YingYangFish_AI bossAI;

    public Transform gatlingPos;

    public IProjectileBasicAttributes bubbleAttributes;
    public float trackingSpeed = 20f;
    public float shootInterval = 0.2f;
    public float shootDuration = 5f;

    [Header("Heart Spread Settings")]
    [Tooltip("Max spread angle from center (in degrees). Bullets spread between -this and +this")]
    public float maxSpreadAngle = 50f;

    [Tooltip("How fast bullets turn toward player (degrees/sec)")]
    public float bulletTurnSpeed = 120f;

    [Tooltip("How fast turn speed ramps up (for smooth curves)")]
    public float bulletTurnAcceleration = 180f;

    [Tooltip("Angle threshold to stop turning and shoot straight")]
    public float aimThreshold = 5f;

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

    public IEnumerator CenterEnd()
    {
        if (gatlingPos.gameObject.activeInHierarchy) gatlingPos.GetComponent<Animator>().Play("end");
        yield return new WaitForSeconds(0.5f);
        gatlingPos.gameObject.SetActive(false);
    }

    public override IEnumerator Act_coroutine(float factor = 0)
    {
        Animator animator = gatlingPos.GetComponent<Animator>();
        animator.Play("gatling_pre");
        gatlingPos.gameObject.SetActive(true);

        float elapsed = 0f;
        float shootTimer = 0f;
        yield return new WaitForSeconds(0.5f);

        while (elapsed < shootDuration)
        {
            elapsed += Time.deltaTime;
            shootTimer += Time.deltaTime;

            while (shootTimer >= shootInterval)
            {
                // Spawn position with slight random offset
                Vector2 randomOffset = Random.insideUnitCircle * 0.3f;
                Vector3 spawnPos = gatlingPos.position + new Vector3(randomOffset.x, randomOffset.y, 0f);

                // Random spread angle - positive = left side, negative = right side
                // This creates the heart shape spread
                float spreadAngle = Random.Range(-maxSpreadAngle, maxSpreadAngle);

                ShootBulletHeartSpread(spawnPos, spreadAngle);
                shootTimer -= shootInterval;
            }
            yield return null;
        }
        animator.Play("end");
        gatlingPos.gameObject.SetActive(false);
    }

    private string[] bulletTags = { "gatling_bullet_1", "gatling_bullet_2", "gatling_bullet_3" };

    private void ShootBulletHeartSpread(Vector3 spawnPos, float spreadAngle)
    {
        string selectedTag = bulletTags[Random.Range(0, bulletTags.Length)];
        Gatling_bubbles bullet = bossAI.selfPooler.SpawnFromPool(selectedTag, spawnPos, false).GetComponent<Gatling_bubbles>();

        // Configure bullet settings
        bullet.turnAcceleration = bulletTurnAcceleration;
        bullet.aimThreshold = aimThreshold;

        // Setup heart spread - bullets launch backward and curve toward player
        bullet.SetUpHeartSpread(
            spawnPos,
            spreadAngle,
            playerIDamagable,
            this.transform.gameObject,
            bulletTurnSpeed,
            bubbleAttributes
        );
    }

    public void Hit(Gatling_bubbles bubble)
    {
        bool dealDamage = damageTimer >= damageCooldown;
        GameManager.instance.playerhealth.Damage(
            dealDamage ? bubble.attribute.damage : 0,
            bubble.transform,
            bubble.attribute.stunDuration,
            stunValue: bubble.attribute.bossBreakValue);
        if (dealDamage) damageTimer = 0f;
    }

    public void HitSelf(Gatling_bubbles bubble)
    {
        bool dealDamage = selfDamageTimer >= damageCooldown;
        bossAI.Damage(
            dealDamage ? bubble.attribute.damage : 0,
            bubble.transform,
            bubble.attribute.stunDuration,
            stunValue: dealDamage ? bubble.attribute.bossBreakValue : 0);
        if (dealDamage) selfDamageTimer = 0f;
    }
}