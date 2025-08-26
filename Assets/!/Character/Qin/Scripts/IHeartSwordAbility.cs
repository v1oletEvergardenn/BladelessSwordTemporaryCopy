using EditorAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IHeartSwordAbility : MonoBehaviour
{
    public string abilityName;
    public string abilityDescription;
    public Sprite abilityIcon;
    [Range(0, 10)] public int HS_Cost;
    public bool isTriggeredByAttackKey = true;

    public MeleeAttack HS_attack_effect = new MeleeAttack(2, 0.5f, 0.05f, new Vector2(1, 1.4f), 0.1f, 20f, 0.1f);

    [HideInInspector] public bool isPerforming = false;
    [HideInInspector] public bool isEquipped = false;
    [HideInInspector] public CharacterController2D controller;
    [HideInInspector] public VFXManager vfx;
    [HideInInspector] public GameManager gameManager;
    [HideInInspector] public Energy energy;
    [HideInInspector] public Rigidbody2D rb;
    [HideInInspector] public AnimSetBool animSet;
    [HideInInspector] public InputPlayer inputPlayer;
    [HideInInspector] public InputMaster inputMaster;
    [HideInInspector] public IDamagable health;
    [HideInInspector] public PlayerAttack playerAttack;
    [HideInInspector] public InternalObjectPooler selfPooler;
    [HideInInspector] public HeartSwordAbilities hSAbilityManager;
    [HideInInspector] public Animator anim;

    [HideInInspector] public HashSet<IDamagable> hsHitTargets = new HashSet<IDamagable>();
    [HideInInspector] public bool hsHitEffectPlayed = false;

    public bool isActive = false;

    public Coroutine co_ability;

    public virtual void Start()
    {
        vfx = VFXManager.instance;
        gameManager = GameManager.instance;
        controller = CharacterController2D.instance;
        inputPlayer = InputPlayer.instance;
        playerAttack = PlayerAttack.instance;
        animSet = AnimSetBool.instance;
        health = Health.instance;
        energy = Energy.instance;
        hSAbilityManager = HeartSwordAbilities.instance;
        selfPooler = hSAbilityManager.selfPooler;
        rb = hSAbilityManager.rb;
        anim = hSAbilityManager.anim;
    }

    public virtual bool PerformAbility(bool isLeft)
    {
        // This method should be overridden in derived classes to implement specific ability behavior
        Debug.Log("PerformAbility called in IHeartSwordAbility, but should be overridden in derived classes.");
        return false;
    }

    public virtual IEnumerator Act()
    {
        yield return null;
    }

    public virtual void EquipAbility()
    {
        isEquipped = true;
        Debug.Log($"{abilityName} equipped.");
    }

    public virtual void ActivateAbility()
    {
        if (!CheckEnoughHeartSwordPoints()) return;
        isActive = true;
        vfx.RumblePulse(0.2f, 0.3f, 0.1f);
    }

    public virtual void DeactivateAbility()
    {
        isActive = false;
    }

    public virtual void UnequipAbility()
    {
        DeactivateAbility();
        UnequipAbility();
        EndAction();
    }

    public bool CheckEnoughHeartSwordPoints()
    {
        if (HS_Cost <= hSAbilityManager.currentHS_point) return true;
        return false;
    }

    public virtual void EndAction()
    {
    }

    public virtual void CancelAction()
    {
        if (co_ability != null)
        {
            StopCoroutine(co_ability);
        }
    }

    public virtual void CheckHSCounterAttack(Collider2D col)

    {
        List<Collider2D> colliders = new List<Collider2D>();
        ContactFilter2D filter = new ContactFilter2D();
        filter.NoFilter();
        col.OverlapCollider(filter, colliders);

        // Gather projectiles and damagables
        foreach (Collider2D collider in colliders)
        {
            if (collider.gameObject == this.gameObject) continue; // Skip self
            if (collider.TryGetComponent<IProjectile>(out IProjectile proj))
            {
                if (proj.isHostileToPlayer && !proj.collided)
                {
                    if (!IsInCounterDirection(proj.GetPivot())) continue;
                    HS_counterAttack(proj);//if hs attack
                }
            }

            if (collider.TryGetComponent<IDamagable>(out IDamagable dmg))
            {
                if (!IsInCounterDirection(dmg.GetHitPos())) continue;
                HS_meleeAttack(dmg);
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

    public virtual void HS_meleeAttack(IDamagable damagable)
    {
        if (damagable == health) return;
        if (hsHitTargets.Contains(damagable)) return; // Already hit this target in this attack
        IDamagable parentDamagble = damagable;
        if (damagable is SubDamageable sub) { parentDamagble = sub.ParentDamageable; }
        hsHitTargets.Add(parentDamagble);
        foreach (IDamagable i in parentDamagble.subDamagables) { hsHitTargets.Add(i); }

        //effect
        if (!hsHitEffectPlayed)
        {
            vfx.MeleeAttackEffect(HS_attack_effect,
            damagable,
            damagable.GetHitPos().x < health.GetHitPos().x ? true : false);
            hsHitEffectPlayed = true;
        }
        else damagable.Repel(HS_attack_effect.repel, damagable.GetHitPos().x < health.GetHitPos().x ? true : false);
        vfx.SpawnHeartSwordHitEffect(damagable.GetHitPos());

        //damage
        damagable.Damage(HS_attack_effect.damage, this.transform, 0, stunValue: 10);
    }

    public virtual void HS_counterAttack(IProjectile projectile)
    {
        energy.ChangeEnergy(-energy.attack_energy_consumption);
        projectile.SetUp(playerAttack.pointerDirection, this.gameObject, 100, _isHostileToPlayer: false, _damage: projectile.damage * playerAttack.basicAttackDamage);
        projectile.PerfectCounterAttack();
        SoundManager.PlaySound("perfect_attack");
    }
}