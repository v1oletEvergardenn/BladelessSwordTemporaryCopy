using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterBall : IProjectile
{
    public string anim_after_hit = "after_hit";

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
            if (collision.gameObject == gameManager.Player)
            {
                if (collision.gameObject.layer == 14) { return; }
                vfx.RumblePulse(rumbleFrequncy_normal.x, rumbleFrequncy_normal.y, rumbleDuration_normal);
            }

            anim.Play(anim_after_hit);
            //shakeManager.CameraShake(gameManager.impulseSource, cameraShakeForce.y);
            rb.velocity = Vector3.zero;
            speed = 0f;
            collided = true;
            target.Damage(damage, transform, stunDuration);
            Invoke("Die", 0.2f);
        }
        else if (collision.gameObject != owner && (stopLayer.value & (1 << collision.gameObject.layer)) > 0)
        {
            anim.Play(anim_after_hit);
            rb.velocity = Vector3.zero;
            speed = 0f;
            collided = true;
            Invoke("Die", 0.2f);
        }
    }
}