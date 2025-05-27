using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubDamageable : IDamagable
{
    public IDamagable ParentDamageable;
    public DamageFlash flash;
    public bool takeRepel = false;

    private void Start()
    {
        ParentDamageable.subDamagables.Add(this);
    }

    public override int Damage(float damageAmount, Transform sender = null, float stunDuration = 0, bool damageFlash = true, float stunValue = 0)
    {
        flash.OnDamageFlash();
        ParentDamageable.SubObjectDamage(damageAmount, sender, stunDuration, stunValue: stunValue);
        return 0;
    }

    public override void Repel(float force, bool left)
    {
        if (takeRepel) { base.Repel(force, left); }
    }
}