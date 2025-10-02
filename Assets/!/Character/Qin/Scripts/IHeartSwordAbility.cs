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

    [GUIColor(GUIColor.Lime)]
    [FoldoutGroup("Attributes", nameof(abilityAttributes), nameof(equippedBranch),
        nameof(HS_Cost), nameof(isTriggeredByAttackKey),
        nameof(canBeStopped), nameof(learned), nameof(isActive), nameof(toggleToActivate))]
    public Void abilityVoid1;

    [SerializeField, HideProperty] public SO_HeartSwordAttribute abilityAttributes;
    [SerializeField, HideProperty] public IHeartSwordAbilityBranch equippedBranch;
    [SerializeField, HideProperty][Range(0, 10)] public int HS_Cost;
    [SerializeField, HideProperty] public bool isTriggeredByAttackKey = true; // If false, triggered by ability key
    [SerializeField, HideProperty] public bool canBeStopped = true;
    [SerializeField, HideProperty] public bool learned = false;
    [SerializeField, HideProperty] public bool isActive = false;
    [SerializeField, HideProperty] public bool toggleToActivate = false;
    [GUIColor(144f, 151f, 222f)] public MeleeAttack HS_attack_effect = new MeleeAttack(2, 0.5f, 0.05f, new Vector2(1, 1.4f), 0.1f, 20f, 0.1f);

    private SO_HeartSwordAttribute original_abiltyAttributes;
    private int original_HS_Cost;
    private bool original_isTriggeredByAttackKey;
    private bool original_canBeStopped;
    private bool original_learned;
    private bool original_toggleToActivate;
    private MeleeAttack original_HS_attack_effect;
    private IHeartSwordAbilityBranch origianl_branch;

    [GUIColor(GUIColor.Default)]
    [HideProperty] public bool isPerforming = false;

    [HideProperty] public bool isEquipped = false;

    #endregion Inspector Fields & Attributes

    #region References

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

    #endregion References

    #region State & Helper Collections

    [HideProperty] public HashSet<IDamagable> hsHitTargets = new HashSet<IDamagable>();
    [HideProperty] public List<IHeartSwordAbilityBranch> allBranches = new List<IHeartSwordAbilityBranch>();
    [HideProperty] public bool hsHitEffectPlayed = false;
    public Coroutine co_ability;

    #endregion State & Helper Collections

    #region Unity Lifecycle

    private void Awake()
    {
        SaveOriginalAttributes();
    }

    /// <summary>
    /// Initializes references and updates all ability branches.
    /// </summary>
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
        allBranches = GetComponents<IHeartSwordAbilityBranch>().ToList<IHeartSwordAbilityBranch>();
    }

    #endregion Unity Lifecycle

    #region Ability Performance

    /// <summary>
    /// Executes the original ability logic. Must be implemented by derived classes.
    /// </summary>
    /// <param name="isLeft">Direction or context for the ability (true = left, false = right).</param>
    /// <returns>True if performed successfully, otherwise false.</returns>
    public abstract bool OriginalAbilityPerformance(bool isLeft);

    /// <summary>
    /// Checks and performs the ability, using the equipped branch if available.
    /// </summary>
    /// <param name="isLeft">Direction or context for the ability.</param>
    /// <returns>True if performed successfully, otherwise false.</returns>
    public virtual bool CheckPerformAbility(bool isLeft)
    {
        if (equippedBranch != null)
            return equippedBranch.BranchAbilityPerformance(isLeft);
        else
            return OriginalAbilityPerformance(isLeft);
    }

    /// <summary>
    /// Coroutine for the ability's main action. Must be implemented by derived classes.
    /// </summary>
    public abstract IEnumerator Act();

    #endregion Ability Performance

    #region Branch Management

    /// <summary>
    /// Changes the currently equipped ability branch and updates attributes.
    /// </summary>
    /// <param name="branch">The branch to equip.</param>
    public virtual void ChangeBranch(IHeartSwordAbilityBranch branch)
    {
        if (branch == null) { RestoreAttributes(); return; }
        abilityAttributes = branch.abilityAttributes;
        HS_Cost = branch.HS_Cost;
        isTriggeredByAttackKey = branch.isTriggeredByAttackKey;
        canBeStopped = branch.canBeStopped;
        learned = branch.learned;
        toggleToActivate = branch.toggleToActivate;
        HS_attack_effect = branch.HS_attack_effect;
        equippedBranch = branch;
    }

    public void RestoreAttributes()
    {
        abilityAttributes = original_abiltyAttributes;
        HS_Cost = original_HS_Cost;
        isTriggeredByAttackKey = original_isTriggeredByAttackKey;
        canBeStopped = original_canBeStopped;
        learned = original_learned;
        toggleToActivate = original_toggleToActivate;
        HS_attack_effect = original_HS_attack_effect;
        equippedBranch = origianl_branch;
    }

    public void SaveOriginalAttributes()
    {
        original_abiltyAttributes = abilityAttributes;
        original_HS_Cost = HS_Cost;
        original_isTriggeredByAttackKey = isTriggeredByAttackKey;
        original_canBeStopped = canBeStopped;
        original_learned = learned;
        original_toggleToActivate = toggleToActivate;
        original_HS_attack_effect = HS_attack_effect;
        origianl_branch = equippedBranch;
    }

    /// <summary>
    /// Returns a list of all learned (available) branches.
    /// </summary>
    public List<IHeartSwordAbilityBranch> GetAvailableBranches()
    {
        List<IHeartSwordAbilityBranch> branches = new List<IHeartSwordAbilityBranch>();
        foreach (IHeartSwordAbilityBranch branch in allBranches)
        {
            if (branch.learned) branches.Add(branch);
        }
        return branches;
    }

    #endregion Branch Management

    #region Equip/Unequip & Activation

    /// <summary>
    /// Marks this ability as equipped.
    /// </summary>
    public virtual void EquipAbility()
    {
        isEquipped = true;
    }

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
        return HS_Cost <= hSAbilityManager.currentHS_point;
    }

    /// <summary>
    /// Gets the input action associated with this ability's slot.
    /// </summary>
    /// <returns>The corresponding InputAction, or null if not assigned.</returns>
    public InputAction GetInputAction()
    {
        if (this == hSAbilityManager.abilityEast) return inputMaster._AbilityB;
        else if (this == hSAbilityManager.abilityNorth) return inputMaster._AbilityY;
        else if (this == hSAbilityManager.abilityWest) return inputMaster._AbilityX;
        else return null;
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
                    if (!IsInCounterDirection(proj.GetPivot())) continue;
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

        // Visual effect
        if (!hsHitEffectPlayed)
        {
            vfx.MeleeAttackEffect(HS_attack_effect,
                damagable,
                damagable.GetHitPos().x < health.GetHitPos().x ? true : false);
            hsHitEffectPlayed = true;
        }
        else
        {
            damagable.Repel(HS_attack_effect.repel, damagable.GetHitPos().x < health.GetHitPos().x ? true : false);
        }
        vfx.SpawnHeartSwordHitEffect(damagable.GetHitPos());

        // Apply damage
        damagable.Damage(HS_attack_effect.damage, this.transform, 0, stunValue: HS_attack_effect.stun);
    }

    /// <summary>
    /// Performs a counter-attack on the specified projectile.
    /// </summary>
    /// <param name="projectile">The projectile to counter.</param>
    public virtual void HS_counterAttack(IProjectile projectile)
    {
        energy.ChangeEnergy(-energy.attack_energy_consumption);
        projectile.HitByHSAttack();
        SoundManager.PlaySound("perfect_attack");
    }

    #endregion Combat & Counter Logic
}