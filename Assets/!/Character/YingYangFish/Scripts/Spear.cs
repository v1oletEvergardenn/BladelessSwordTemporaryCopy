using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spear : IProjectile
{
    [HideInInspector] public bool facingRight;
    [HideInInspector] public bool collisionActive = false;

    public override void Update()
    {
        if (collided) { return; }
        lifeTimer += Time.deltaTime;
        if (lifeTimer >= lifeTime)
        {
            Die();
        }
        if (target != null)
        {
            if (followTarget)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, CalculateWantedRotation(target.GetHitPos()), rotationSpeed * Time.deltaTime);
            }//follow target
        }
        rb.velocity = transform.right * speed / 10;
        if (transform.right.x < 0) { facingRight = false; }
        else { facingRight = true; }
    }

    public override void PerfectCounterAttack()
    {
        vfx.RumblePulse(hitEffectSettings.frequncy_perfect, hitEffectSettings.rumbleDuration);
        vfx.CameraShake(hitEffectSettings.cameraShakeForce.y);
        vfx.SlowTimeForSeconds(hitEffectSettings.freezeTime, hitEffectSettings.Time_scale);
        isPerfect = true;
        vfx.SpawnHitEffect(true, gameManager.player.GetComponent<PlayerAttack>().counterAttackPoint.position);
    }

    public override void NormalCounterAttack()
    {
        PerfectCounterAttack();
    }

    public override void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collisionActive) { return; }
        IDamagable target = collision.gameObject.GetComponent<IDamagable>();
        if (target != null && collision.gameObject != owner && !collided)
        {
            if (isHostileToPlayer && collision.gameObject.layer == 13) { return; }
            if (collision.gameObject == gameManager.player)
            {
                if (collision.gameObject.layer == 14) { return; }
                if ((gameManager.player.GetComponent<PlayerAttack>().isCounterAttacking &&
                    gameManager.player.GetComponent<CharacterController2D>().FacingRight != facingRight)
                    || gameManager.player.GetComponent<PlayerAttack>().isOnStorm)
                {
                    target.Repel(hitEffectSettings.repelForce, this.transform.right.x < 0 ? true : false);
                    gameManager.player.GetComponent<PlayerAttack>().CounterAttack(this, true);
                }
                else
                {
                    vfx.RumblePulse(hitEffectSettings.frequency_norm, hitEffectSettings.rumbleDuration);
                    vfx.CameraShake(hitEffectSettings.cameraShakeForce.y);
                    vfx.SlowTimeForSeconds(0.1f, 0f);
                    vfx.SpawnHitEffect(false, GetPivot());
                    target.Damage(damage, transform, stunDuration, stunValue: stunValue);
                    target.Repel(150f, this.transform.right.x < 0 ? true : false);
                    Hit();
                }
            }
            else
            {
                vfx.SpawnHitEffect(false, GetPivot());
                target.Damage(damage, transform, stunDuration, stunValue: stunValue);
                Hit();
            }
        }
        else if (collision.gameObject != owner &&
            (stopLayer.value & (1 << collision.gameObject.layer)) > 0)
        {
            Hit();
        }
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
        GetComponent<SpriteRenderer>().sprite = null;
        GetComponent<Animator>().Play("spear_hit");
        rb.velocity = Vector3.zero;
        rb.isKinematic = true;
        collided = true;
        Invoke("Die", 0.3f);
    }
}