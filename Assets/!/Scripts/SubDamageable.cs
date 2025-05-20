using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubDamageable : IDamagable
{
    public IDamagable ParentDamageable;
    public DamageFlash flash;

    // Start is called before the first frame update
    public override int Damage(int damageAmount, Transform sender = null, float stunDuration = 0)
    {
        flash.OnDamageFlash();
        ParentDamageable.SubObjectDamage(damageAmount, sender, stunDuration);
        return 0;
    }
}