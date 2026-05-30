using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergySword : IProjectile
{
    //private Animator anim;
    //public bool isRed;
    //public bool facingRight;

    //// Start is called before the first frame update
    //public override void Start()
    //{
    //    base.Start();
    //    anim = GetComponent<Animator>();
    //}

    //public override void OnTriggerEnter2D(Collider2D collision)
    //{
    //    IDamagable target = collision.gameObject.GetComponent<IDamagable>();
    //    if (target != null && collision.gameObject != owner && !collided)
    //    {
    //        if (collision.gameObject == gameManager.player)
    //        {
    //            if (collision.gameObject.layer == 14) { return; }
    //            vfx.RumblePulse(hitEffectSettings.frequency_norm, hitEffectSettings.rumbleDuration);
    //            if (isRed)
    //            {
    //                if (gameManager.player.GetComponent<PlayerAttack>().isCounterAttacking && gameManager.player.GetComponent<CharacterController2D>().FacingRight != facingRight)
    //                {
    //                    target.Repel(50f, this.transform.right.x < 0 ? true : false);
    //                    gameManager.player.GetComponent<PlayerAttack>().CounterAttack(this, true);
    //                }
    //                else
    //                {
    //                    vfx.RumblePulse(hitEffectSettings.frequency_norm, hitEffectSettings.rumbleDuration);
    //                    vfx.CameraShake(hitEffectSettings.cameraShakeForce.y);
    //                    vfx.SlowTimeForSeconds(0.1f, 0f);
    //                    vfx.SpawnSlashEffect(GetPivot(), true);
    //                    target.Damage(damage, transform, stunDuration);
    //                    target.Repel(50f, this.transform.right.x < 0 ? true : false);
    //                    Die();
    //                }
    //            }
    //            else
    //            {
    //                //shakeManager.CameraShake(gameManager.impulseSource, cameraShakeForce.y);
    //                rb.velocity = Vector3.zero;
    //                speed = 0f;
    //                collided = true;
    //                target.Damage(damage, transform, stunDuration);
    //                Die();
    //            }
    //        }
    //        else
    //        {
    //            target.Damage(damage, transform, stunDuration);
    //            Die();
    //        }
    //    }
    //    else if (collision.gameObject != owner && (stopLayer.value & (1 << collision.gameObject.layer)) > 0)
    //    {
    //        rb.velocity = Vector3.zero;
    //        speed = 0f;
    //        collided = true;
    //        Die();
    //    }
    //}

    //public override void Die()
    //{
    //    if (isRed) { vfx.SpawnSlashEffect(GetPivot(), true); }
    //    else { vfx.SpawnSlashEffect(GetPivot()); }
    //    base.Die();
    //}

    //public override void HitByHSAttack()
    //{
    //    Die();
    //}

    //public override void HitByMeleeAttack()
    //{
    //    Die();
    //}

    //public override void Hit()
    //{
    //    Die();
    //}
    public override void Hit()
    {
        throw new System.NotImplementedException();
    }

    public override void HitByHSAttack()
    {
        throw new System.NotImplementedException();
    }

    public override void HitByMeleeAttack()
    {
        throw new System.NotImplementedException();
    }
}