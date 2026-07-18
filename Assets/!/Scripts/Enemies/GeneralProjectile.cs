using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneralProjectile : IProjectile
{
    public string anim_after_hit = "after_hit";
    public float death_delay_time_after_hit = 0f;

    private Animator anim;
    public bool isRed;

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
        anim = GetComponent<Animator>();
    }

    public override void HitByHSAttack()
    {
        Hit();
    }

    public override void HitByMeleeAttack()
    {
        Hit();
    }

    public override void Hit()
    {
        if (anim != null) anim.Play(anim_after_hit);
        rb.velocity = Vector3.zero;
        rb.gravityScale = 0;
        attribute.speed = 0f;
        collided = true;
        Invoke("Die", death_delay_time_after_hit);
    }
}