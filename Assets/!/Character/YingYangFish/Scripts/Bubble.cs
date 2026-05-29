using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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

        if (target != null &&
            collision.gameObject != owner &&
            !collided)// collision with Idamagble Objects
        {
            //ignore enemy if ishostile to player
            if (collision.gameObject.layer == 13 && isHostileToPlayer) { return; }

            //if hit player
            if (collision.gameObject.layer == 14) { return; }//if is dashing, ignore

            StartCoroutine(Explode(target));
        }
        else if (collision.gameObject != owner &&
            (stopLayer.value & (1 << collision.gameObject.layer)) > 0 &&
            !collided)// collision with walls and grounds
        {
            StartCoroutine(Explode(null));
        }
        else if (collision.gameObject.layer == 8 && !collision.TryGetComponent<Bubble>(out Bubble i))// collision with other projectiles excpet this
        {
            StartCoroutine(Explode(null));
        }
    }

    public override void Die()
    {
        StartCoroutine(Explode(null));
    }

    public IEnumerator Explode(IDamagable dmg)
    {
        if (collided) { yield break; }
        anim.Play("explode");
        rb.velocity = Vector3.zero;
        rb.gravityScale = 0;
        speed = 0f;
        collided = true;

        yield return new WaitForSeconds(0.05f);

        if (dmg != null)
        {
            dmg.Damage(damage, transform, stunDuration, stunValue: stunValue);
        }
        yield return new WaitForSeconds(0.2f);
        gameObject.SetActive(false);
    }

    public override void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, explodeRange);
    }

    public override void HitByHSAttack()
    {
    }

    public override void HitByMeleeAttack()
    {
    }

    public override void Hit()
    {
    }
}