using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HS_slash_wave_projectile : IProjectile
{
    public string anim_after_hit = "hit";
    public float death_delay_time_after_hit = 0f;
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
            if (isHostileToPlayer && collision.gameObject.layer == 13) { return; }

            if (collision.gameObject == gameManager.Player)
            {
                if (collision.gameObject.layer == 14) { return; }
                if (!isHostileToPlayer) { return; }

                vfx.RumblePulse(rumbleFrequncy_normal.x, rumbleFrequncy_normal.y, rumbleDuration_normal);
                vfx.SlowTimeForSeconds(freezeTimeDuration, slowTimeScale);
                gameManager.player_Idamagable.Repel(repelForce, transform.right.x < 0 ? true : false);
            }

            vfx.SpawnEffectWithEnum(Hit_Effect.hs_hit, target.GetHitPos());
            target.Damage(damage, transform, stunDuration, stunValue: stunValue);
        }
        else if (collision.gameObject != owner && (stopLayer.value & (1 << collision.gameObject.layer)) > 0 && !collided)
        {
            if (anim != null) anim.Play(anim_after_hit);
            vfx.SpawnEffectWithEnum(Hit_Effect.hs_hit, target.GetHitPos());
            collided = true;
            Invoke("Die", death_delay_time_after_hit);
        }
    }

    public override void HitByMeleeAttack()
    {
        vfx.SpawnEffectWithEnum(Hit_Effect.hs_hit, target.GetHitPos());
        collided = true;
        Invoke("Die", death_delay_time_after_hit);
    }
}