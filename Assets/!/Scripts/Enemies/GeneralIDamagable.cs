using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GeneralIDamagable : IDamagable
{
    [Header("General Damage Settings")]
    [SerializeField] private bool canTakeDamage = true;

    public bool enableAttacksToDie = false;
    [ShowIf(nameof(enableAttacksToDie))][SerializeField] private int attacksToDie = 1;

    [HideIf(nameof(enableAttacksToDie))] public float maxHealth = 15f;
    private float currentHealth;

    public UnityEvent DeathEvent;

    private int currentAttackCount = 0;
    private bool isDead = false;

    public override int Damage(float damageAmount, Transform sender = null, float stunDuration = 0f, bool damageFlash = true, float bossBreakValue = 0)
    {
        if (isDead || !canTakeDamage)
        {
            return 0;
        }

        float appliedDamage = damageAmount;

        if (sender.GetComponent<GeneralProjectile>() != null)
        {
            return 0;
        }
        if (appliedDamage <= 0f)
        {
            return 0;
        }

        currentAttackCount++;
        currentHealth -= damageAmount;

        if (enableAttacksToDie)
        {
            if (currentAttackCount >= Mathf.Max(1, attacksToDie))
            {
                Death();
            }
        }
        else
        {
            if (currentHealth <= 0f)
            {
                Death();
            }
        }

        return 0;
    }

    public override void Death()
    {
        if (isDead)
        {
            return;
        }

        isDead = true;
        canTakeDamage = false;

        base.Death();
        DeathEvent?.Invoke();
    }

    public void StartTakeDamage()
    {
        canTakeDamage = true;
    }

    public void StopTakeDamage()
    {
        canTakeDamage = false;
    }

    public void ResetDamageState()
    {
        currentAttackCount = 0;
        isDead = false;
    }
}