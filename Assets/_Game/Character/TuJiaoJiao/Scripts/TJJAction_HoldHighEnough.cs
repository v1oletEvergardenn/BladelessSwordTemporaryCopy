using System.Collections;
using UnityEngine;

public class TJJAction_HoldHighEnough : TJJBaseAction
{
    [Header("References")]
    public Transform attackPos;

    [Range(0f, 2f)] public float slashMovementDistance = 0.5f;
    [Range(0f, 2f)] public float slashStartup = 0.08f;
    [Range(0f, 2f)] public float twoSmallSlashGap = 0.12f;
    [Range(0f, 2f)] public float holdWindupDuration = 0.35f;
    [Range(0f, 2f)] public float heavyStartup = 0.15f;
    [Range(0f, 2f)] public float recoverDuration = 1.1f;

    [Header("Double Slash")] public float slashHitDuration = 0.12f;
    public float slashRange = 1.8f;
    public MeleeAttack slashAttack = new MeleeAttack(1, 0.3f, 0.05f, new Vector2(0.15f, 0.25f), 0.08f, 8f, 0.05f);

    [Header("Hold + Heavy Slash")]
    public float heavySlashDuration = 0.2f;

    public float heavyRange = 2.4f;

    public MeleeAttack heavyAttack = new MeleeAttack(2, 0.8f, 0.08f, new Vector2(0.2f, 0.4f), 0.12f, 16f, 0.1f);

    public override IEnumerator Act_coroutine(float factor = 0, Transform _target = null)
    {
        yield return StartCoroutine(bossAi.IEFollow());

        // melee attack twice
        bossAi.FaceTarget();
        bossAi.MoveCurrentDirection(slashMovementDistance);
        yield return StartCoroutine(ApplyAttackInCircle(slashHitDuration, slashRange, attackPos, slashAttack, slashStartup));
        yield return WaitForEnemy(twoSmallSlashGap);
        bossAi.MoveCurrentDirection(slashMovementDistance);
        yield return StartCoroutine(ApplyAttackInCircle(slashHitDuration, slashRange, attackPos, slashAttack, slashStartup));

        // short windup, holding the sword
        yield return WaitForEnemy(holdWindupDuration);

        bossAi.FaceTarget();
        bossAi.MoveCurrentDirection(slashMovementDistance);
        // large melee attack
        yield return StartCoroutine(ApplyAttackInCircle(heavySlashDuration, heavyRange, attackPos, heavyAttack));

        // skip recover if next action is gold shards
        if (!ShouldSkipRecoverForGoldShards())
        {
            yield return WaitForEnemy(recoverDuration);
        }

        OnActionEnd();

        yield return null;
    }

    private bool ShouldSkipRecoverForGoldShards()
    {
        IEnemyAction next = bossAi.NextAction(this);
        return next is TJJAction_GoldShards;
    }

    protected override void DrawEditModeAttackDebugGizmos()
    {
        if (attackPos == null) return;

        Vector3 center = attackPos.position;

        Color cachedColor = Gizmos.color;

        Gizmos.color = new Color(1f, 0.92f, 0.16f, 0.9f); // slash
        Gizmos.DrawWireSphere(center, slashRange);

        Gizmos.color = new Color(1f, 0.35f, 0.35f, 0.9f); // heavy
        Gizmos.DrawWireSphere(center, heavyRange);

        Gizmos.color = cachedColor;
    }
}