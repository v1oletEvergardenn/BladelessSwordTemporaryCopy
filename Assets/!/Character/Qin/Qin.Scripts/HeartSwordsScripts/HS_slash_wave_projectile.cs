using System.Collections.Generic;
using UnityEngine;

public class HS_slash_wave_projectile : IProjectile
{
    public string anim_after_hit = "hit";
    public float death_delay_time_after_hit = 0f;

    public Collider2D stopCollider;
    public Collider2D damageCollider;
    private Animator anim;
    private ContactFilter2D stopFilter = new ContactFilter2D();

    private float attackCD = 0.5f;
    private float attackTimer = 0f;

    public bool showBox = false;
    public Vector2 boxSize = new Vector2(1f, 1f); // Set to your desired size
    public Vector2 boxOffset = Vector2.zero; // Offset from the projectile's position

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
        stopFilter.SetLayerMask(stopLayer);
        anim = GetComponent<Animator>();
    }

    public override void Update()
    {
        base.Update();
        attackTimer += Time.deltaTime;
        CheckStopLayerCollision();
    }

    public void CheckStopLayerCollision()
    {
        if (collided) return;

        // Define the box's center and size (adjust as needed)
        Vector2 boxCenter = transform.position + (Vector3)boxOffset;
        Vector2 boxsize = boxSize; // Set to your desired size
        float boxAngle = 0f; // Rotation in degrees

        // Use the stopLayer mask from your projectile
        Collider2D[] results = Physics2D.OverlapBoxAll(boxCenter, boxsize, boxAngle, stopLayer);

        if (results.Length > 0)
        {
            if (anim != null) anim.Play(anim_after_hit);
            collided = true;
            Invoke("Die", death_delay_time_after_hit);
        }
    }

    public override void HitByMeleeAttack()
    {
        vfx.SpawnEffectWithEnum(Hit_Effect.hs_hit, transform.position);
        collided = true;
        Invoke("Die", death_delay_time_after_hit);
    }

    public override void HitByHSAttack()
    {
        vfx.SpawnEffectWithEnum(Hit_Effect.hs_hit, transform.position);
        collided = true;
        Invoke("Die", death_delay_time_after_hit);
    }

    public override void Hit()
    {
    }

    public override void OnTriggerEnter2D(Collider2D col)
    {
        IDamagable target = col.gameObject.GetComponent<IDamagable>();
        IProjectile proj = col.gameObject.GetComponent<IProjectile>();

        if (proj != null && proj.isHostileToPlayer && !collided)
        {
            proj.HitByHSAttack();
        }

        if (target != null && col.gameObject != owner && !collided)
        {
            if (isHostileToPlayer && col.gameObject.layer == 13) { return; }

            if (col.gameObject == gameManager.player)
            {
                if (col.gameObject.layer == 14) { return; }
                if (!isHostileToPlayer) { return; }

                vfx.RumblePulse(hitEffectSettings.frequency_norm, hitEffectSettings.rumbleDuration);
                vfx.SlowTimeForSeconds(hitEffectSettings.freezeTime, hitEffectSettings.Time_scale);
                gameManager.playerhealth.Repel(hitEffectSettings.repelForce, transform.right.x < 0 ? true : false);
            }

            if (attackTimer >= attackCD)
            {
                vfx.SpawnEffectWithEnum(Hit_Effect.hs_hit, target.GetHitPos());
                target.Damage(attribute, transform);
                attackTimer = 0f;
            }
        }
    }

    public override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        Gizmos.color = Color.cyan;
        // Draw the box (no rotation support in Gizmos.DrawWireCube)
        Gizmos.DrawWireCube(transform.position + (Vector3)boxOffset, boxSize);
    }
}