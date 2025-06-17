using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bubble : IProjectile
{
    private Animator anim;
    public Hit_Effect hitEffect;
    public bool isRed;

    public float explodeRange = 3f;

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
            //ignore enemy if ishostile to player
            if (isHostileToPlayer && collision.gameObject.layer == 13) { return; }

            //if hit player
            if (collision.gameObject.layer == 14) { return; }//if is dashing, ignore

            StartCoroutine(Explode());
        }
        else if (collision.gameObject != owner && (stopLayer.value & (1 << collision.gameObject.layer)) > 0 && !collided)
        {
            // Estimate the collision normal
            Vector2 normal = ((Vector2)transform.position - collision.ClosestPoint(transform.position)).normalized;
            if (normal == Vector2.zero)
            {
                normal = -rb.velocity.normalized;
            }

            // Reflect the velocity based on the normal
            rb.velocity = Vector2.Reflect(rb.velocity, normal);
        }
    }

    public override void FixedUpdate()
    {
    }

    public IEnumerator Explode()
    {
        anim.Play("explode");
        rb.velocity = Vector3.zero;
        rb.gravityScale = 0;
        speed = 0f;
        collided = true;

        yield return new WaitForSeconds(0.05f);

        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, explodeRange);
        foreach (Collider2D collider in colliders)
        {
            if (collider.TryGetComponent<IDamagable>(out IDamagable dmg))
            {
                if (dmg.gameObject == gameManager.Player)
                {
                    if (dmg.gameObject.layer == 14) { continue; }// if is dashing, ignore
                    vfx.RumblePulse(rumbleFrequncy_normal.x, rumbleFrequncy_normal.y, rumbleDuration_normal);
                    vfx.SlowTimeForSeconds(freezeTimeDuration, slowTimeScale);
                    gameManager.player_Idamagable.Repel(repelForce, transform.right.x < 0 ? true : false);
                }
                dmg.Damage(damage, transform, stunDuration, stunValue: stunValue);
            }
        }
        yield return new WaitForSeconds(0.2f);
        gameObject.SetActive(false);
    }

    public override void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, explodeRange);
    }
}