using System.Collections;
using UnityEngine;

public class TJJAction_FoxFire : TJJBaseAction
{
    [Header("Jump (Force)")]
    public float jumpHorizontalForce = 6f;

    public float jumpVerticalForce = 9f;
    public float jumpingTime = 1.5f;
    public float landingTime = 1.5f;

    [Header("Fox Fire")]
    public string foxFirePoolTag = "foxfire";

    public Transform firePoint;
    public IProjectileBasicAttributes foxFireAttribute;
    public ProjectileHit foxFireHitEffect;
    public float centerFireAngle = -90f;
    public float fanAngle = 18f;
    private float originalGravity;

    public override void Start()
    {
        base.Start();
        originalGravity = rb.gravityScale;
    }

    public override void CancelAct()
    {
        base.CancelAct();
        rb.gravityScale = originalGravity;
    }

    public override IEnumerator Act_coroutine(float factor = 0, Transform _target = null)
    {
        Transform trueTarget = _target != null ? _target : player != null ? player.transform : null;
        float awayFromTargetSign = ResolveJumpDirectionSign(trueTarget);
        rb.velocity = Vector2.zero;
        rb.gravityScale = originalGravity;
        float elapsedAirTime = 0f;

        // Jump right => face left, jump left => face right.
        bool shouldFaceRight = awayFromTargetSign < 0f;
        bossAi.Face(shouldFaceRight);
        rb.AddForce(new Vector2(jumpHorizontalForce * awayFromTargetSign, jumpVerticalForce), ForceMode2D.Impulse);
        // Wait until apex (vertical speed turns downward), with timeout safety.
        while (elapsedAirTime < jumpingTime)
        {
            elapsedAirTime += TimeScaleManager.EnemyDt;
            if (rb.velocity.y <= 0f)
            {
                break; // reached peak / started falling
            }
            yield return null;
        }
        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;
        FireFoxBolts(spawnPos);

        // Start landing.
        rb.gravityScale = originalGravity;

        // Wait until grounded or timeout safety.
        elapsedAirTime = 0f;
        while (elapsedAirTime < landingTime)
        {
            elapsedAirTime += TimeScaleManager.EnemyDt;
            if (bossAi != null && bossAi.IsGrounded())
            {
                break;
            }

            yield return null;
        }

        OnActionEnd();
    }

    private float ResolveJumpDirectionSign(Transform target)
    {
        float awayFromTargetSign = 0f;

        if (target != null)
        {
            awayFromTargetSign = Mathf.Sign(transform.position.x - target.position.x);
        }

        if (Mathf.Approximately(awayFromTargetSign, 0f))
        {
            awayFromTargetSign = IsPlayerLeft() ? 1f : -1f;
        }

        return awayFromTargetSign;
    }

    private void FireFoxBolts(Vector3 spawnPos)
    {
        float[] angles = { centerFireAngle + fanAngle, centerFireAngle, centerFireAngle - fanAngle };

        for (int i = 0; i < angles.Length; i++)
        {
            IProjectile projectile = bossAi.selfPooler.SpawnFromPool(foxFirePoolTag, spawnPos).GetComponent<IProjectile>();
            projectile.SetUp(new Vector3(0f, 0f, ToFacingAngle(angles[i])), gameObject)
                      .SetAttributes(foxFireAttribute)
                      .SetHitEffect(foxFireHitEffect)
                      .SetHostileToPlayer(true);
        }
    }

    private float ToFacingAngle(float angle)
    {
        if (bossAi != null && !bossAi.isFacingRight)
        {
            return 180f - angle;
        }

        return angle;
    }
}