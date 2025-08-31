using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gatling_bubbles : IProjectile
{
    public string anim_after_hit = "after_hit";
    public float death_delay_time_after_hit = 0f;
    private Animator anim;
    public Hit_Effect hitEffect;

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
            if (isHostileToPlayer && collision.gameObject.layer == 13) { return; }

            if (collision.gameObject == gameManager.player)
            {
                if (collision.gameObject.layer == 14) { return; }
                if (!isHostileToPlayer) { return; }

                vfx.RumblePulse(hitEffectSettings.frequency_norm, hitEffectSettings.rumbleDuration);
                vfx.SlowTimeForSeconds(hitEffectSettings.freezeTime, hitEffectSettings.Time_scale);
                gameManager.playerhealth.Repel(hitEffectSettings.repelForce, transform.right.x < 0 ? true : false);
            }
            if (anim != null) anim.Play(anim_after_hit);
            vfx.SpawnEffectWithEnum(hitEffect, transform.position);
            Stop();
            Invoke("Die", death_delay_time_after_hit);

            if (collision.gameObject == gameManager.player) YingYangFish_AI.instance.gatling.Hit(this);
            else target.Damage(damage, transform, stunDuration, stunValue: stunValue);
        }
        else if (collision.gameObject != owner && (stopLayer.value & (1 << collision.gameObject.layer)) > 0 && !collided)
        {
            if (anim != null) anim.Play(anim_after_hit);
            vfx.SpawnEffectWithEnum(hitEffect, transform.position);
            Stop();
            Invoke("Die", death_delay_time_after_hit);
        }
    }

    public void Stop()
    {
        rb.velocity = Vector3.zero;
        rb.gravityScale = 0;
        speed = 0f;
        collided = true;
    }
}