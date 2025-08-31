using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneralProjectile : IProjectile
{
    public string anim_after_hit = "after_hit";
    public float death_delay_time_after_hit = 0f;

    private Animator anim;
    public Hit_Effect hitEffect;
    public bool isRed;

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
            vfx.SpawnEffectWithEnum(hitEffect, transform.position, isRed);
            //shakeManager.CameraShake(gameManager.impulseSource, cameraShakeForce.y);
            rb.velocity = Vector3.zero;
            rb.gravityScale = 0;
            speed = 0f;
            collided = true;
            target.Damage(damage, transform, stunDuration, stunValue: stunValue);
            Invoke("Die", death_delay_time_after_hit);
        }
        else if (collision.gameObject != owner && (stopLayer.value & (1 << collision.gameObject.layer)) > 0 && !collided)
        {
            if (anim != null) anim.Play(anim_after_hit);
            vfx.SpawnEffectWithEnum(hitEffect, transform.position, isRed);
            rb.velocity = Vector3.zero;
            rb.gravityScale = 0;
            speed = 0f;
            collided = true;
            Invoke("Die", death_delay_time_after_hit);
        }
    }
}