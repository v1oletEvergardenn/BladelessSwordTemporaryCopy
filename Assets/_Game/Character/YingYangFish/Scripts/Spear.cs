using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spear : IProjectile
{
    [HideInInspector] public bool facingRight;

    public override void Update()
    {
        if (collided) { return; }
        lifeTimer += TimeScaleManager.ProjDt;
        if (lifeTimer >= lifeTime)
        {
            Die();
        }
        if (target != null)
        {
            if (followTarget)
            {
                transform.rotation =
                    Quaternion.RotateTowards
                    (transform.rotation,
                    CalculateWantedRotation(GetTargetHitPosition(target)),
                    rotationSpeed * TimeScaleManager.ProjDt);
            }//follow target
        }
        rb.velocity = transform.right * attribute.speed / 10 * TimeScaleManager.ProjScale;
        if (transform.right.x < 0) { facingRight = false; }
        else { facingRight = true; }
    }

    public override void PerfectCounterAttack()
    {
        hitEffect.AllEffects(ProjectileHitResult.Perfect);
        vfx.SpawnHitEffect(true, gameManager.player.GetComponent<PlayerAttack>().counterAttackPoint.position);
        isPerfect = true;
    }

    public override void NormalCounterAttack()
    {
        PerfectCounterAttack();
    }

    public override void Die()
    {
        gameObject.SetActive(false);
    }

    public override void HitByHSAttack()
    {
        Hit();
    }

    public override void HitByMeleeAttack()
    {
        Hit();
    }

    public override void Hit()
    {
        if (!collisionEnabled) return;
        GetComponent<SpriteRenderer>().sprite = null;
        GetComponent<Animator>().Play("spear_hit");
        rb.velocity = Vector3.zero;
        rb.isKinematic = true;
        collided = true;
        Invoke("Die", 0.3f);
    }
}