using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(DamageFlash))]
public class ShooterOnHIt : IDamagable
{
    private SpriteRenderer spriteRenderer;
    private DamageFlash flash;
    public int maxHealth;
    public int currentHealth;
    public bool _requirePerfect;
    public bool _requireHeartSwordAttack;
    public UnityEvent Die;

    public bool canRevive = false;
    public float reviveTime = 5f;

    // Start is called before the first frame update
    private void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        flash = GetComponent<DamageFlash>();
    }

    public override int Damage(int damageAmount, Transform sender, float stunDuration = 0)
    {
        if (_requirePerfect && !sender.GetComponent<IProjectile>().isPerfect) { return 0; }
        if (_requireHeartSwordAttack && sender != null) { return 0; }
        flash.OnDamageFlash();
        currentHealth -= damageAmount;
        if (currentHealth <= 0)
        {
            Die?.Invoke();
            if (canRevive)
            {
                Invoke("Revive", reviveTime);
                GetComponent<Shooter>().canShoot = false;
                GetComponent<SpriteRenderer>().color = new Color(0.4f, 0.1f, 0.14f);
            }
        }
        return 0;
    }

    public void Revive()
    {
        currentHealth = maxHealth;
        GetComponent<Shooter>().canShoot = true;
        GetComponent<SpriteRenderer>().color = new Color(0.35f, 0.75f, 0.5f);
    }
}