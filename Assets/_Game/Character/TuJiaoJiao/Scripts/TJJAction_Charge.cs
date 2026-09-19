using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TJJAction_Charge : TJJBaseAction
{
    public Transform attackPos;

    [Range(0f, 2f)] public float windupDuration = 0.3f;
    [Range(0f, 5f)] public float counterAttackedRecoveryDuration = 0.5f;
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

    public override void CancelAct()
    {
        base.CancelAct();
        bossAi.OnDamaged -= OnStop1;
    }

    public override IEnumerator Act_coroutine(float factor = 0, Transform _target = null)
    {
        bossAi.OnDamaged += OnStop1;
        // short windup, holding the sword
        bossAi.FaceTarget();
        anim.Play("charge");
        yield return WaitForEnemy(windupDuration);

        // charge forward, keep attacking while moving.
        wasCountered = false;
        hitBoundary = false;
        isCharging = true;
        float elapsed = 0f;
        float attackCD = 0f;
        //moving forward

        float x = chargeSpeed * chargeDuration;
        bossAi.MoveByDuration(IsPlayerLeft() ? -x : x, chargeDuration + timeExcceedRecoveryDuration);

        yield return WaitForEnemy(0.1f);
        anim.Play("charge_loop_2");

        while (elapsed < chargeDuration + timeExcceedRecoveryDuration && !bossAi.DEAD && !wasCountered && !hitBoundary)
        {
            elapsed += TimeScaleManager.EnemyDt;
            attackCD += TimeScaleManager.EnemyDt;
            if (elapsed >= chargeDuration)
            {
                Stop2();
            }
            else if (attackCD >= hitTickInterval)
            {
                attackCD = 0f;
                StartCoroutine(ApplyAttackInCircle(chargeAttackDuration, chargeRange, attackPos, chargeAttack));
            }

            yield return null;
        }

        isCharging = false;

        if (wasCountered || hitBoundary)
        {
            bossAi.StopCurrentMove();
            float jumpingDuration = 0.96f;
            Stop1();
            //counter attacked by player, end moving and attack.
            yield return WaitForEnemy(0.36f);
            bossAi.Repel(0.7f, bossAi.isFacingRight);
            yield return WaitForEnemy(0.3f);
            bossAi.Repel(0.4f, bossAi.isFacingRight);
            yield return WaitForEnemy(0.3f);
            bossAi.Repel(0.2f, bossAi.isFacingRight);
            yield return WaitForEnemy(counterAttackedRecoveryDuration - jumpingDuration - 0.5f);
            anim.SetTrigger("stunning_end");
            yield return WaitForEnemy(0.5f);
        }
        else
        {
            // when exceeds a certain time, brake animation and end the moving and attack.
            anim.Play("charge_stop2_end");
            yield return WaitForEnemy(0.18f);
        }

        OnActionEnd();
        bossAi.OnDamaged -= OnStop1;
        yield return null;
    }

    public override void HitPlayer(MeleeAttack melee, Transform sourceAttackPos, Vector3 offset = default)
    {
        MeleeAttackResult dealtDamage = playerIDamagable.DamageFromMeleeAttack(sourceAttackPos, melee);
        bool left = playerIDamagable.GetHitPos().x < transform.position.x;

        if (dealtDamage == MeleeAttackResult.Countered)
        {
            // when counterattacked by player, end moving and attack.
            OnStop1();
            return;
        }

        if (dealtDamage == MeleeAttackResult.Defended || dealtDamage == MeleeAttackResult.DamagedSuccessfully)
        {
            vfx.MeleeAttackEffect(melee, playerIDamagable, left);
        }
    }

    public void OnStop1()
    {
        if (wasCountered || hitBoundary)
        {
            return;
        }
        wasCountered = true;
        bool left = playerIDamagable.GetHitPos().x < transform.position.x;
        vfx.MeleeAttackEffect(chargeAttack, playerIDamagable, left);
        bossAi.Repel(2, !left);
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
            OnStop1();
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

    public void Stop1()
    {
        anim.Play("charge_stop1");
    }

    public void Stop2()
    {
        anim.Play("charge_stop2");
    }
}