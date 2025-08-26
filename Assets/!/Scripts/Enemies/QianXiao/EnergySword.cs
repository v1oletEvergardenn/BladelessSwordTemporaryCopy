using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergySword : IProjectile
{
    private Animator anim;
    public bool isRed;
    public bool facingRight;

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
        anim = GetComponent<Animator>();
    }

    public override void OnTriggerEnter2D(Collider2D collision)
    {
        IDamagable target = collision.gameObject.GetComponent<IDamagable>();
        if (target != null && collision.gameObject != owner && !collided)
        {
            if (collision.gameObject == gameManager.Player)
            {
                if (collision.gameObject.layer == 14) { return; }
                vfx.RumblePulse(rumbleFrequncy_normal.x, rumbleFrequncy_normal.y, rumbleDuration_normal);
                if (isRed)
                {
                    if (gameManager.Player.GetComponent<PlayerAttack>().isCounterAttacking && gameManager.Player.GetComponent<CharacterController2D>().FacingRight != facingRight)
                    {
                        target.Repel(50f, this.transform.right.x < 0 ? true : false);
                        gameManager.Player.GetComponent<PlayerAttack>().CounterAttack(this, true);
                    }
                    else
                    {
                        vfx.RumblePulse(rumbleFrequncy_normal.x, rumbleFrequncy_normal.y, rumbleDuration_normal);
                        vfx.CameraShake(cameraShakeForce.y);
                        vfx.SlowTimeForSeconds(0.1f, 0f);
                        vfx.SpawnSlashEffect(GetPivot(), true);
                        target.Damage(damage, transform, stunDuration);
                        target.Repel(50f, this.transform.right.x < 0 ? true : false);
                        Die();
                    }
                }
                else
                {
                    //shakeManager.CameraShake(gameManager.impulseSource, cameraShakeForce.y);
                    rb.velocity = Vector3.zero;
                    speed = 0f;
                    collided = true;
                    target.Damage(damage, transform, stunDuration);
                    Die();
                }
            }
            else
            {
                target.Damage(damage, transform, stunDuration);
                Die();
            }
        }
        else if (collision.gameObject != owner && (stopLayer.value & (1 << collision.gameObject.layer)) > 0)
        {
            rb.velocity = Vector3.zero;
            speed = 0f;
            collided = true;
            Die();
        }
    }

    public override void Die()
    {
        if (isRed) { vfx.SpawnSlashEffect(GetPivot(), true); }
        else { vfx.SpawnSlashEffect(GetPivot()); }
        base.Die();
    }
}