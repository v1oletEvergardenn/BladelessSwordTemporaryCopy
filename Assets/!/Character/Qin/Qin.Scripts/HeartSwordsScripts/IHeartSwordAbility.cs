using EditorAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using Void = EditorAttributes.Void;

/// <summary>
/// Abstract base class for Heart Sword abilities. Handles ability attributes, state, and provides
/// core logic for equipping, activating, and performing abilities. Derived classes should implement
/// specific ability behavior.
/// </summary>
public abstract class IHeartSwordAbility : MonoBehaviour
{
    #region Inspector Fields & Attributes

    public bool DebugMode = false;
    public SO_HeartSwordAttribute commonAttribute;
    public SO_HeartSwordAttribute branch1Attribute;
    public SO_HeartSwordAttribute branch2Attribute;

    protected bool unlocked = false;
    public bool isActive = false;

    protected bool unlockedBranch1 = false;
    protected bool unlockedBranch2 = false;
    protected int branchIndex = 0;
    protected int upgradedLevel = 0;

    [Space(20)] public Void spaceholder;
    protected int abilityAttributeIndex;

    [HideProperty] public bool isPerforming = false;
    [HideProperty] protected bool isEquipped = false;
    [HideProperty] public HashSet<IDamagable> hsHitTargets = new HashSet<IDamagable>();
    [HideProperty] public bool hsHitEffectPlayed = false;
    [HideProperty] public bool toggleToActivate = false;
    [HideProperty] public Coroutine co_ability;

    #endregion Inspector Fields & Attributes

    #region References

    [HideProperty] public CharacterController2D controller;
    [HideProperty] public VFXManager vfx;
    [HideProperty] public GameManager gameManager;
    [HideProperty] public Energy energy;
    [HideProperty] public Rigidbody2D rb;
    [HideProperty] public InputPlayer inputPlayer;
    [HideProperty] public InputMaster inputMaster;
    [HideProperty] public Health health;
    [HideProperty] public PlayerAttack playerAttack;
    [HideProperty] public InternalObjectPooler selfPooler;
    [HideProperty] public HeartSwordAbilities hSAbilityManager;
    [HideProperty] public Animator anim;

    #endregion References

    public virtual void Start()
    {
        vfx = VFXManager.instance;
        gameManager = GameManager.instance;
        controller = CharacterController2D.instance;
        inputPlayer = InputPlayer.instance;
        inputMaster = InputMaster.instance;
        playerAttack = PlayerAttack.instance;
        health = Health.instance;
        energy = Energy.instance;
        hSAbilityManager = HeartSwordAbilities.instance;
        selfPooler = hSAbilityManager.selfPooler;
        rb = hSAbilityManager.rb;
        anim = hSAbilityManager.anim;
    }

    #region Ability Performance

    /// <summary>
    /// Checks and performs the ability, using the equipped branch if available.
    /// </summary>
    /// <param name="isLeft">Direction or context for the ability.</param>
    /// <returns>True if performed successfully, otherwise false.</returns>
    public virtual bool CheckPerformAbility(bool isLeft)
    {
        if (branchIndex == 1)
        {
            return FirstBranchAbilityPerformance(isLeft);
        }
        else if (branchIndex == 2)
        {
            return SecondBranchAbilityPerformance(isLeft);
        }
        else
        {
            return OriginalAbilityPerformance(isLeft);
        }
    }

    /// <summary>
    /// Executes the original ability logic. Must be implemented by derived classes.
    /// </summary>
    /// <param name="isLeft">Direction or context for the ability (true = left, false = right).</param>
    /// <returns>True if performed successfully, otherwise false.</returns>
    public abstract bool OriginalAbilityPerformance(bool isLeft);

    public abstract bool FirstBranchAbilityPerformance(bool isLeft);

    public abstract bool SecondBranchAbilityPerformance(bool isLeft);

    /// <summary>
    /// Coroutine for the ability's main action. Must be implemented by derived classes.
    /// </summary>
    public abstract IEnumerator OriginalAct();

    public abstract IEnumerator FirstBranchAct();

    public abstract IEnumerator SecondBranchAct();

    #endregion Ability Performance

    #region Branch Management

    public int GetBranchIndex() => branchIndex;

    public bool IsBranch1Unlocked() => unlockedBranch1;

    public bool IsBranch2Unlocked() => unlockedBranch2;

    public int GetUpgradedLevel() => upgradedLevel;

    public bool IsUnlocked() => unlocked;

    public void SetUpgradeLevel(int level) => upgradedLevel = level;

    public void SetBranch1LockedStates(bool unlocked) => unlockedBranch1 = unlocked;

    public void SetBranch2LockedStates(bool unlocked) => unlockedBranch2 = unlocked;

    public void SetLockedStates(bool unlocked) => this.unlocked = unlocked;

    public void Unlock()
    {
        SetAttribute(0);
        unlocked = true;
    }

    /// <summary>
    /// Changes the currently equipped ability branch and updates attributes.
    /// </summary>
    /// <param name="branch">The branch to equip.</param>
    ///
    public void ChangeBranch(int branchIndex)
    {
        if (branchIndex == 1) { SetAttribute(branch2Attribute); this.branchIndex = branchIndex; return; }
        else if (branchIndex == 2) { SetAttribute(branch1Attribute); this.branchIndex = branchIndex; return; }
        else { SetNoneBranch(); this.branchIndex = 0; return; }
    }

    public void SetNoneBranch()
    { branchIndex = 0; SetAttribute(commonAttribute); }

    public void SetAttribute(SO_HeartSwordAttribute attribute)
    {
        if (attribute == commonAttribute) { SetAttribute(0); }
        else if (attribute == branch1Attribute) { SetAttribute(1); }
        else if (attribute == branch2Attribute) { SetAttribute(2); }
    }

    public void SetAttribute(int index)
    {
        abilityAttributeIndex = index;
    }

    public SO_HeartSwordAttribute GetCurrentAttribute()
    {
        if (abilityAttributeIndex == 0) { return commonAttribute; }
        else if (abilityAttributeIndex == 1) { return branch1Attribute; }
        else if (abilityAttributeIndex == 2) { return branch2Attribute; }
        else return commonAttribute;
    }

    #endregion Branch Management

    #region Equip/Unequip & Activation

    public bool IsEquipped() => isEquipped;

    /// <summary>
    /// Marks this ability as equipped.
    /// </summary>
    public virtual void EquipAbility() => isEquipped = true;

    /// <summary>
    /// Deactivates and ends the ability action.
    /// </summary>
    public virtual void UnequipAbility()
    {
        isEquipped = false;
        DeactivateAbility();
        EndAction();
    }

    /// <summary>
    /// Activates the ability if possible (not stunned and enough points).
    /// </summary>
    /// <returns>True if activation succeeded, otherwise false.</returns>
    public virtual bool ActivateAbility()
    {
        if (health.stunned) return false;
        if (!CheckEnoughHeartSwordPoints()) return false;
        isActive = true;
        vfx.RumblePulse(0.2f, 0.3f, 0.1f);
        return true;
    }

    /// <summary>
    /// Deactivates the ability.
    /// </summary>
    public virtual void DeactivateAbility()
    {
        isActive = false;
    }

    #endregion Equip/Unequip & Activation

    #region Ability State & Utility

    /// <summary>
    /// Checks if any Heart Sword ability is currently being performed.
    /// </summary>
    /// <returns>True if any ability is performing, otherwise false.</returns>
    public bool CheckAnyPerformingAbility()
    {
        return hSAbilityManager.CheckAnyPerformingAbility();
    }

    /// <summary>
    /// Checks if there are enough Heart Sword points to use this ability.
    /// </summary>
    /// <returns>True if enough points, otherwise false.</returns>
    public bool CheckEnoughHeartSwordPoints()
    {
        return GetCurrentAttribute().HS_Cost <= hSAbilityManager.currentHS_point;
    }

    /// <summary>
    /// Gets the input action associated with this ability's slot.
    /// </summary>
    /// <returns>The corresponding InputAction, or null if not assigned.</returns>
    public InputAction GetInputAction()
    {
        if (this == hSAbilityManager.GetEastAbility()) return inputMaster._AbilityB;
        else if (this == hSAbilityManager.GetNorthAbility()) return inputMaster._AbilityY;
        else if (this == hSAbilityManager.GetWestAbility()) return inputMaster._AbilityX;
        else return null;
    }

    public bool Upgrade(int index)
    {
        if (!unlocked) return false;
        if (upgradedLevel + 1 != index) return false;
        upgradedLevel++;
        return true;
    }

    public bool UnlockBranch(bool firstBranch)
    {
        if (!unlocked) return false;
        if (upgradedLevel < 3) return false;
        if (firstBranch) unlockedBranch1 = true;
        else unlockedBranch2 = true;
        return true;
    }

    #endregion Ability State & Utility

    #region Action Control

    /// <summary>
    /// Ends the current ability action. Can be overridden for cleanup.
    /// </summary>
    public virtual void EndAction()
    {
    }

    /// <summary>
    /// Cancels the current ability action and stops any running coroutine.
    /// </summary>
    public virtual void CancelAction()
    {
        if (co_ability != null)
        {
            StopCoroutine(co_ability);
        }
        EndAction();
    }

    #endregion Action Control

    #region Combat & Counter Logic

    /// <summary>
    /// Checks for counter-attack opportunities on colliders overlapping the given collider.
    /// </summary>
    /// <param name="col">The collider to check for overlaps.</param>
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
                    if (!IsInCounterDirection(proj.GetHitPos())) continue;
                    HS_counterAttack(proj);
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

    /// <summary>
    /// Performs a melee attack on the specified damagable target.
    /// </summary>
    /// <param name="damagable">The target to attack.</param>
    public virtual void HS_meleeAttack(IDamagable damagable)
    {
        if (damagable == health) return;
        if (hsHitTargets.Contains(damagable)) return; // Already hit this target in this attack
        IDamagable parentDamagble = damagable;
        if (damagable is SubDamageable sub) { parentDamagble = sub.ParentDamageable; }
        hsHitTargets.Add(parentDamagble);
        foreach (IDamagable i in parentDamagble.subDamagables) { hsHitTargets.Add(i); }
        MeleeAttack attackEffect = GetCurrentAttribute().HS_attack_effect;

        // Visual effect
        if (!hsHitEffectPlayed)
        {
            vfx.MeleeAttackEffect(attackEffect,
                damagable,
                damagable.GetHitPos().x < health.GetHitPos().x ? true : false);
            hsHitEffectPlayed = true;
        }
        else
        {
            damagable.Repel(attackEffect.repel, damagable.GetHitPos().x < health.GetHitPos().x ? true : false);
        }
        vfx.SpawnHeartSwordHitEffect(damagable.GetHitPos());

        // Apply damage
        damagable.Damage(attackEffect.damage, this.transform, 0, bossBreakValue: attackEffect.breakAmount);
        HitTarget();
    }

    public abstract void HitTarget();

    /// <summary>
    /// Performs a counter-attack on the specified projectile.
    /// </summary>
    /// <param name="projectile">The projectile to counter.</param>
    public virtual void HS_counterAttack(IProjectile projectile)
    {
        energy.ChangeEnergy(-energy.attack_energy_consumption);
        projectile.HitByHSAttack();
        SoundManager.PlaySound("perfect_attack");
        HitTarget();
    }

    #endregion Combat & Counter Logic
}