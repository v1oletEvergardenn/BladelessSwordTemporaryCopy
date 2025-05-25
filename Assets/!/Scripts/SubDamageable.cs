using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubDamageable : IDamagable
{
    public IDamagable ParentDamageable;
    public DamageFlash flash;

    private void Start()
    {
        ParentDamageable.subDamagables.Add(this);
    }

    public override int Damage(int damageAmount, Transform sender = null, float stunDuration = 0)
    {
        flash.OnDamageFlash();
        ParentDamageable.SubObjectDamage(damageAmount, sender, stunDuration);
        return 0;
    }
}