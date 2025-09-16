using EditorAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Void = EditorAttributes.Void;

public abstract class IHeartSwordAbility : MonoBehaviour
{
    [GUIColor(GUIColor.Lime)]
    [FoldoutGroup("Attributes", nameof(abilityName), nameof(abilityDescription),
        nameof(abilityIcon), nameof(HS_Cost), nameof(isTriggeredByAttackKey),
        nameof(canBeStopped), nameof(isActive), nameof(toggleToActivate))]
    public Void abilityVoid1;

    [SerializeField, HideProperty] public string abilityName;
    [SerializeField, HideProperty] public string abilityDescription;
    [SerializeField, HideProperty] public Sprite abilityIcon;
    [SerializeField, HideProperty][Range(0, 10)] public int HS_Cost;
    [SerializeField, HideProperty] public bool isTriggeredByAttackKey = true;
    [SerializeField, HideProperty] public bool canBeStopped = true;
    [SerializeField, HideProperty] public bool isActive = false;
    [SerializeField, HideProperty] public bool toggleToActivate = false;
    [GUIColor(144f, 151f, 222f)] public MeleeAttack HS_attack_effect = new MeleeAttack(2, 0.5f, 0.05f, new Vector2(1, 1.4f), 0.1f, 20f, 0.1f);

    [GUIColor(GUIColor.Default)]
    [HideProperty] public bool isPerforming = false;

    [HideProperty] public bool isEquipped = false;
    [HideProperty] public CharacterController2D controller;
    [HideProperty] public VFXManager vfx;
    [HideProperty] public GameManager gameManager;
    [HideProperty] public Energy energy;
    [HideProperty] public Rigidbody2D rb;
    [HideProperty] public AnimSetBool animSet;
    [HideProperty] public InputPlayer inputPlayer;
    [HideProperty] public InputMaster inputMaster;
    [HideProperty] public Health health;
    [HideProperty] public PlayerAttack playerAttack;
    [HideProperty] public InternalObjectPooler selfPooler;
    [HideProperty] public HeartSwordAbilities hSAbilityManager;
    [HideProperty] public Animator anim;
    [HideProperty] public HashSet<IDamagable> hsHitTargets = new HashSet<IDamagable>();
    [HideProperty] public bool hsHitEffectPlayed = false;
    public Coroutine co_ability;

    public virtual void Start()
    {
        vfx = VFXManager.instance;
        gameManager = GameManager.instance;
        controller = CharacterController2D.instance;
        inputPlayer = InputPlayer.instance;
        inputMaster = InputMaster.instance;
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
    }

    public bool CheckAnyPerformingAbility()
    {
        return hSAbilityManager.CheckAnyPerformingAbility();
    }

    public virtual bool ActivateAbility()
    {
        if (health.stunned) return false;
        if (!CheckEnoughHeartSwordPoints()) return false;
        isActive = true;
        vfx.RumblePulse(0.2f, 0.3f, 0.1f);
        return true;
    }

    public virtual void DeactivateAbility()
    {
        isActive = false;
    }

    public virtual void UnequipAbility()
    {
        DeactivateAbility();
        EndAction();
    }

    public bool CheckEnoughHeartSwordPoints()
    {
        if (HS_Cost <= hSAbilityManager.currentHS_point) return true;
        return false;
    }

    public InputAction GetInputAction()
    {
        if (this == hSAbilityManager.abilityB) { return inputMaster._AbilityB; }
        else if (this == hSAbilityManager.abilityY) { return inputMaster._AbilityY; }
        else if (this == hSAbilityManager.abilityX) { return inputMaster._AbilityX; }
        else { return null; }
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
        EndAction();
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
        damagable.Damage(HS_attack_effect.damage, this.transform, 0, stunValue: HS_attack_effect.stun);
    }

    public virtual void HS_counterAttack(IProjectile projectile)
    {
        energy.ChangeEnergy(-energy.attack_energy_consumption);
        //projectile.SetUp(playerAttack.pointerDirection, this.gameObject, 100, _isHostileToPlayer: false, _damage: projectile.damage * playerAttack.basicAttackDamage);
        //projectile.PerfectCounterAttack();
        projectile.HitByHSAttack();
        SoundManager.PlaySound("perfect_attack");
    }
}