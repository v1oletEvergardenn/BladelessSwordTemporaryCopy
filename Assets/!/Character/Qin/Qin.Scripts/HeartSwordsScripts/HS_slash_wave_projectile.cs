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
        CheckCollisionHS(col);
        CheckCollision(col);
    }

    public override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        Gizmos.color = Color.cyan;
        // Draw the box (no rotation support in Gizmos.DrawWireCube)
        Gizmos.DrawWireCube(transform.position + (Vector3)boxOffset, boxSize);
    }
}