using EditorAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
public abstract class IHeartSwordAbilityBranch : MonoBehaviour
{
    public SO_HeartSwordAttribute abilityAttribute;
    public bool learned = false;

    [HideProperty] public IHeartSwordAbility parentAbility;
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

    //[FoldoutGroup("AbilityBranch")]public Void abilityBranchVoid;
    // Start is called before the first frame update
    public virtual void Start()
    {
        parentAbility = GetComponent<IHeartSwordAbility>();

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

    /// <summary>
    /// override parent ability's PerformAbility function
    /// </summary>
    /// <param name="isLeft"></param>
    /// <returns></returns>
    public abstract bool BranchAbilityPerformance(bool isLeft);

    public abstract IEnumerator Act();

    public bool CheckEnoughHeartSwordPoints()
    {
        if (abilityAttribute.HS_Cost <= hSAbilityManager.currentHS_point) return true;
        return false;
    }

    public InputAction GetInputAction()
    {
        if (this == hSAbilityManager.GetEastAbility()) { return inputMaster._AbilityB; }
        else if (this == hSAbilityManager.GetNorthAbility()) { return inputMaster._AbilityY; }
        else if (this == hSAbilityManager.GetWestAbility()) { return inputMaster._AbilityX; }
        else { return null; }
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
        MeleeAttack attackEffect = abilityAttribute.HS_attack_effect;
        //effect
        if (!hsHitEffectPlayed)
        {
            vfx.MeleeAttackEffect(attackEffect,
            damagable,
            damagable.GetHitPos().x < health.GetHitPos().x ? true : false);
            hsHitEffectPlayed = true;
        }
        else damagable.Repel(attackEffect.repel, damagable.GetHitPos().x < health.GetHitPos().x ? true : false);
        vfx.SpawnHeartSwordHitEffect(damagable.GetHitPos());

        //damage
        damagable.Damage(attackEffect.damage, this.transform, 0, stunValue: attackEffect.stun);
    }

    public virtual void HS_counterAttack(IProjectile projectile)
    {
        energy.ChangeEnergy(-energy.attack_energy_consumption);
        //projectile.SetUp(playerAttack.pointerDirection, this.gameObject, 100, _isHostileToPlayer: false, _damage: projectile.damage * playerAttack.basicAttackDamage);
        //projectile.PerfectCounterAttack();
        projectile.HitByHSAttack();
        SoundManager.PlaySound("perfect_attack");
    }

    public bool CheckAnyPerformingAbility()
    {
        return hSAbilityManager.CheckAnyPerformingAbility();
    }
}