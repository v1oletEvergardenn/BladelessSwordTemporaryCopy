using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyBubble : IProjectile
{
    private Animator anim;

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
            if (collision.gameObject == gameManager.player) { return; }
            End();
            vfx.SpawnSlashEffect(transform.position, true);
            rb.velocity = Vector3.zero;
            speed = 0f;
            collided = true;
            target.Damage(damage, transform, stunDuration);
            //Invoke("Die", death_delay_time_after_hit);
        }
        else if (collision.gameObject != owner && (stopLayer.value & (1 << collision.gameObject.layer)) > 0)
        {
            End();
            vfx.SpawnSlashEffect(transform.position, true);
            rb.velocity = Vector3.zero;
            speed = 0f;
            collided = true;
            //Invoke("Die", death_delay_time_after_hit);
        }
    }

    public void End()
    {
        if (gameObject.activeInHierarchy)
        {
            anim.Play("end");
            Invoke("Die", 1f);
        }
    }

    public override void HitByHSAttack()
    {
        Die();
    }

    public override void HitByMeleeAttack()
    {
        Die();
    }

    public override void Hit()
    {
        Die();
    }
}