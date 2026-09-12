using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TJJAction_Charge : TJJBaseAction
{
    public Transform attackPos;

    [Range(0f, 2f)] public float windupDuration = 0.3f;
    [Range(0f, 2f)] public float counterAttackedRecoveryDuration = 0.5f;
    [Range(0f, 5f)] public float hitWallRecoveryDuration = 0.5f;
    [Range(0f, 2f)] public float timeExcceedRecoveryDuration = 0.5f;

    [Range(0f, 5f)] public float chargeDuration = 1.2f;
    [Range(0f, 50f)] public float chargeSpeed = 16f;
    [Range(0.01f, 0.5f)] public float hitTickInterval = 0.08f;
    [Range(0f, 1f)] public float chargeAttackDuration = 0.08f;
    [Range(0f, 5f)] public float chargeRange = 1.4f;
    public MeleeAttack chargeAttack = new MeleeAttack(1, 0.5f, 0.05f, new Vector2(0.15f, 0.25f), 0.08f, 10f, 0.05f);

    private bool wasCountered;
    private bool hitBoundary;
    private bool isCharging;

    public override IEnumerator Act_coroutine(float factor = 0, Transform _target = null)
    {
        // short windup, holding the sword
        yield return WaitForEnemy(windupDuration);

        // charge forward, keep attacking while moving.
        wasCountered = false;
        hitBoundary = false;
        isCharging = true;
        float elapsed = 0f;
        float attackCD = 0f;
        //moving forward

        bossAi.Move(IsPlayerLeft() ? bossAi.leftBoundary : bossAi.rightBoundary, chargeSpeed);

        while (elapsed < chargeDuration && !bossAi.DEAD && !wasCountered && !hitBoundary)
        {
            elapsed += TimeScaleManager.EnemyDt;
            attackCD += TimeScaleManager.EnemyDt;
            if (attackCD >= hitTickInterval)
            {
                attackCD = 0f;
                StartCoroutine(ApplyAttackInCircle(chargeAttackDuration, chargeRange, attackPos, chargeAttack));
            }
            yield return null;
        }

        isCharging = false;
        bossAi.StopCurrentMove();

        if (wasCountered)
        {
            //counter attacked by player, end moving and attack.
            yield return WaitForEnemy(counterAttackedRecoveryDuration);
        }
        else if (hitBoundary)
        {
            // if hit the boundary, stun for a period of time and end the moving and attack.
            yield return WaitForEnemy(hitWallRecoveryDuration);
        }
        else
        {
            // when exceeds a certain time, brake animation and end the moving and attack.
            yield return WaitForEnemy(timeExcceedRecoveryDuration);
        }

        OnActionEnd();
        yield return null;
    }

    public override void HitPlayer(MeleeAttack melee, Transform sourceAttackPos, Vector3 offset = default)
    {
        MeleeAttackResult dealtDamage = playerIDamagable.DamageFromMeleeAttack(sourceAttackPos, melee);
        bool left = playerIDamagable.GetHitPos().x < transform.position.x;

        if (dealtDamage == MeleeAttackResult.Countered)
        {
            // when counterattacked by player, end moving and attack.
            wasCountered = true;
            vfx.MeleeAttackEffect(melee, playerIDamagable, left);
            bossAi.Repel(2, !left);
            return;
        }

        if (dealtDamage == MeleeAttackResult.Defended || dealtDamage == MeleeAttackResult.DamagedSuccessfully)
        {
            vfx.MeleeAttackEffect(melee, playerIDamagable, left);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isCharging || collision == null)
        {
            return;
        }
        if (collision.gameObject.layer == 18)//wall layer
        {
            hitBoundary = true;
            bossAi.StopCurrentMove();
            return;
        }
    }

    protected override void DrawEditModeAttackDebugGizmos()
    {
        if (attackPos == null)
        {
            return;
        }

        Color cachedColor = Gizmos.color;
        Gizmos.color = new Color(0.95f, 0.55f, 0.15f, 0.9f);
        Gizmos.DrawWireSphere(attackPos.position, chargeRange);
        Gizmos.color = cachedColor;
    }
}