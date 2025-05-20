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
        vfx.RumblePulse(rumbleFrequncy_perfect.x, rumbleFrequncy_perfect.y, rumbleDuration_perfect);
        vfx.CameraShake(cameraShakeForce.y);
        vfx.SlowTimeForSeconds(0.3f, 0f);
        isPerfect = true;
        vfx.SpawnHitEffect(true, gameManager.Player.GetComponent<PlayerAttack>().counterAttackPoint.position);
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
            if (collision.gameObject == gameManager.Player)
            {
                if (collision.gameObject.layer == 14) { return; }
                if (gameManager.Player.GetComponent<PlayerAttack>().isAttacking && gameManager.Player.GetComponent<CharacterController2D>().FacingRight != facingRight)
                {
                    target.Repel(50f, this.transform.right);
                    gameManager.Player.GetComponent<PlayerAttack>().CounterAttack(this, true);
                }
                else
                {
                    vfx.RumblePulse(rumbleFrequncy_normal.x, rumbleFrequncy_normal.y, rumbleDuration_normal);
                    vfx.CameraShake(cameraShakeForce.y);
                    vfx.SlowTimeForSeconds(0.1f, 0f);
                    vfx.SpawnHitEffect(false, GetPivot());
                    target.Damage(damage, transform, stunDuration);
                    target.Repel(150f, this.transform.right);
                    Die();
                }
            }
            else
            {
                vfx.SpawnHitEffect(false, GetPivot());
                target.Damage(damage, transform, stunDuration);
                Die();
            }
        }
        else if (collision.gameObject != owner && (stopLayer.value & (1 << collision.gameObject.layer)) > 0)
        {
            rb.velocity = Vector3.zero;
            rb.isKinematic = true;
        }
    }

    public override void Die()
    {
        GetComponent<SpriteRenderer>().sprite = null;
        GetComponent<Animator>().Play("spear_hit");
        rb.velocity = Vector3.zero;
        rb.isKinematic = true;
        collided = true;
        Invoke("SetFalseActive", 0.3f);
    }

    public void SetFalseActive()
    {
        gameObject.SetActive(false);
    }
}