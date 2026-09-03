using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TJJAction_GoldShards : TJJBaseAction
{
    [Header("Timing")]
    public float windupTime = 0.45f;

    public float recoverTime = 0.5f;
    public float shotInterval = 0.12f;

    public float bulletDelay = 0.2f;

    [Header("Count")]
    public int minShardCount = 2;

    public int maxShardCount = 5;

    [Header("Spawn")]
    public string goldShardPoolTag = "gold_shard";

    public Transform firePoint;
    public float spawnSpreadY = 0.3f;

    [Header("fan spread")]
    public float startAngle = -15f;

    public float panAngle = 10f;

    [Header("Projectile")]
    public IProjectileBasicAttributes shardAttribute;

    public ProjectileHit shardHitEffect;

    public override IEnumerator Act_coroutine(float factor = 0, Transform _target = null)
    {
        Transform trueTarget = _target != null ? _target : player != null ? player.transform : null;
        bool fromHoldHighEnough = bossAi != null && bossAi.lastAction is TJJAction_HoldHighEnough;

        bossAi.FaceTarget(trueTarget);
        yield return WaitForEnemy(windupTime);

        bossAi.FaceTarget(trueTarget);
        yield return WaitForEnemy(windupTime);

        int shardCount = ResolveShardCount(factor);

        Vector3 baseSpawnPos = firePoint != null ? firePoint.position : transform.position;
        Vector3 lockedAimPos = ResolveTargetHitPos(trueTarget);

        for (int i = 0; i < shardCount; i++)
        {
            Vector3 spawnPos = GetSpawnPos(baseSpawnPos, i, shardCount);

            IProjectile projectile = bossAi.selfPooler.SpawnFromPool(goldShardPoolTag, spawnPos).GetComponent<IProjectile>();

            projectile.collisionEnabled = true;

            if (fromHoldHighEnough)
            {
                float z = (startAngle + panAngle * i);
                float fireAngle = IsPlayerLeft() ? 180 - z : z;
                projectile.SetUp(new Vector3(0, 0, fireAngle), gameObject)
                     .SetAttributes(shardAttribute)
                     .SetHitEffect(shardHitEffect)
                     .SetHostileToPlayer(true)
                     .SetDelay(bulletDelay, false);
            }
            else
            {
                Vector3 currentAimPos = ResolveTargetHitPos(trueTarget);
                Vector3 fireEuler = CalculateWantedEuler(currentAimPos, projectile.transform.position);
                projectile.SetUp(fireEuler, gameObject)
                         .SetAttributes(shardAttribute)
                         .SetHitEffect(shardHitEffect)
                         .SetHostileToPlayer(true)
                         .SetTarget(playerIDamagable)
                         .SetDelay(bulletDelay, false, true);
                yield return WaitForEnemy(shotInterval);
            }
        }
        yield return WaitForEnemy(recoverTime);

        OnActionEnd();
    }

    private int ResolveShardCount(float factor)
    {
        int min = Mathf.Max(1, minShardCount);
        int max = Mathf.Max(min, maxShardCount);

        if (factor > 0f)
        {
            int forced = Mathf.RoundToInt(factor);
            return Mathf.Clamp(forced, min, max);
        }

        return Random.Range(min, max + 1);
    }

    private Vector3 GetSpawnPos(Vector3 basePos, int index, int total)
    {
        if (total <= 1)
        {
            return basePos;
        }

        float half = (total - 1) * 0.5f;
        float offsetY = (index - half) * spawnSpreadY;
        return basePos + new Vector3(0f, offsetY, 0f);
    }

    private Vector3 ResolveTargetHitPos(Transform target)
    {
        if (target == null)
        {
            return transform.position + transform.right;
        }

        if (target.TryGetComponent<IDamagable>(out IDamagable damagable))
        {
            return damagable.GetHitPos();
        }

        return target.position;
    }
}