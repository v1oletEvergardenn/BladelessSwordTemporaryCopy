using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HS_CounterAttack : IHeartSwordAbility
{
    [SerializeField, Range(0f, 5f)] public float HS_attack_radius = 2.3f;
    public Transform counterAttackPos;

    private float timer = 0f;

    public override void Start()
    {
        base.Start();
    }

    public void Update()
    {
        timer += Time.deltaTime;
        if (!isEquipped) return;
        if (!isActive) return;
    }

    public override bool OriginalAbilityPerformance(bool isLeft)
    {
        if (!playerAttack.canAttack) return false;
        if (controller.isFloating) return false;
        if (health.stunned) return false;
        if (CheckAnyPerformingAbility()) return false;
        if (playerAttack.attackTimer < playerAttack.attackGap) return false;
        if (hSAbilityManager.currentHS_point < GetCurrentAttribute().HS_Cost) { return false; }
        if (controller.FacingRight == isLeft) { controller.Flip(); }

        hSAbilityManager.ModifyHSPoint(-GetCurrentAttribute().HS_Cost);
        isPerforming = true;
        playerAttack.InitializeAttack(isLeft);
        hsHitEffectPlayed = false;
        anim.SetBool("isCombat", true);
        hsHitTargets.Clear();
        anim.Play(GetAttackAnimName());

        CancelAction();
        co_ability = StartCoroutine(Act());
        return true;

        string GetAttackAnimName()
        {
            string indexStr = playerAttack.attackIndex.ToString();
            if (controller.isJumping)
                return $"HS_attack_jump_{indexStr}";
            if (controller.isFalling)
                return $"HS_attack_fall_{indexStr}";
            if (controller.isRunning)
                return $"HS_attack_run_{indexStr}";
            return $"HS_attack_idle_{indexStr}";
        }
    }

    public override IEnumerator Act()
    {
        timer = 0f;
        playerAttack.isCounterAttacking = true;

        yield return null;
        while (timer < playerAttack.counterAttackCheckDuration)
        {
            timer += VFXManager.isInBulletTime ? Time.unscaledDeltaTime : Time.deltaTime;
            CheckHSCounterAttack(null);
            yield return null;
        }
        isPerforming = false;
    }

    public override void CheckHSCounterAttack(Collider2D col)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(playerAttack.counterAttackPoint.position, HS_attack_radius + 5);
        float hsCheckDistance = HS_attack_radius + playerAttack.counterAttackPoint.localPosition.x;
        // Gather projectiles and damagables, and find closest of each
        foreach (Collider2D collider in colliders)
        {
            if (collider.gameObject == this.gameObject) continue;

            if (collider.TryGetComponent<IProjectile>(out IProjectile proj))
            {
                if (proj.isHostileToPlayer && !proj.collided)
                {
                    if (!IsInCounterDirection(proj.GetPivot())) continue;
                    float dist = Vector2.Distance(proj.GetPivot(), health.GetHitPos());
                    if (dist <= hsCheckDistance) HS_counterAttack(proj);//if hs attack
                }
            }

            if (collider.TryGetComponent<IDamagable>(out IDamagable dmg))
            {
                if (!IsInCounterDirection(dmg.GetHitPos())) continue;
                float dist = Vector3.Distance(dmg.GetHitPos(), health.GetHitPos());
                if (dist <= hsCheckDistance) HS_meleeAttack(dmg);
            }
        }
        // Helper: checks if a target is in the correct direction for counter
        bool IsInCounterDirection(Vector3 targetPos)
        {
            if (controller.FacingRight && targetPos.x <= transform.position.x) return false;
            if (!controller.FacingRight && targetPos.x >= transform.position.x) return false;
            return true;
        }
    }

    public override void HS_meleeAttack(IDamagable damagable)
    {
        if (damagable == health) return;
        if (hsHitTargets.Contains(damagable)) return; // Already hit this target in this attack
        IDamagable parentDamagble = damagable;
        if (damagable is SubDamageable sub) { parentDamagble = sub.ParentDamageable; }
        hsHitTargets.Add(parentDamagble);
        foreach (IDamagable i in parentDamagble.subDamagables) { hsHitTargets.Add(i); }
        MeleeAttack attackEffect = GetCurrentAttribute().HS_attack_effect;
        if (!hsHitEffectPlayed)
        {
            vfx.MeleeAttackEffect(attackEffect,
            damagable,
            damagable.GetHitPos().x < health.GetHitPos().x ? true : false);
            hsHitEffectPlayed = true;
        }
        else damagable.Repel(attackEffect.repel, damagable.GetHitPos().x < health.GetHitPos().x ? true : false);

        vfx.SpawnHeartSwordHitEffect(damagable.GetHitPos());

        damagable.Damage(attackEffect.damage, this.transform, 0, stunValue: attackEffect.stun);
        playerAttack.canDefend = true;
    }

    public override void HS_counterAttack(IProjectile projectile)
    {
        //if (isAimingRightStick) { projectile.transform.position = pointerPos.position; }
        playerAttack.attackTimer = playerAttack.attackGap + 0.5f;
        playerAttack.canDefend = true;

        energy.ChangeEnergy(-energy.attack_energy_consumption);
        //projectile.SetUp(playerAttack.pointerDirection, this.gameObject, 100, _isHostileToPlayer: false, _damage: projectile.damage * playerAttack.basicAttackDamage);
        //projectile.PerfectCounterAttack();
        projectile.HitByHSAttack();
        SoundManager.PlaySound("perfect_attack");
    }

    public void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(counterAttackPos.position, HS_attack_radius);
    }
}