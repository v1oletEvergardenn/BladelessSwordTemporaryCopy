using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Cinemachine;
using static Cinemachine.CinemachineOrbitalTransposer;
using EditorAttributes;
using UnityEngine.UI;
using UnityEngine.Playables;

public class PlayerAttack : MonoBehaviour
{
    #region Singleton & References

    public static PlayerAttack instance;
    private CharacterController2D controller;
    private VFXManager vfx;
    private GameManager gameManager;
    private Energy energy;
    private Rigidbody2D rb;
    private AnimSetBool animSet;
    private InputPlayer inputPlayer;
    private IDamagable health;
    private InternalObjectPooler selfPooler;
    private HeartSwordAbilities hSAbilitiesManager;
    [SerializeField] public Animator anim;
    [SerializeField] private Transform pointerPos;

    #endregion Singleton & References

    #region State Flags

    [HideInInspector] public bool isAimingRightStick;
    [HideInInspector] public bool isInCombat;
    [HideInInspector] public bool isDefending;
    [HideInInspector] public bool canDefend = true;
    [HideInInspector] public bool canAttack;
    [HideInInspector] public bool isCounterAttacking;
    [HideInInspector] public bool isAttackingLeft = true;
    [HideInInspector] public bool canStorm = true;
    [HideInInspector] public bool isOnStorm = false;
    [HideInInspector] public bool isPreparingStorm = false;

    #endregion State Flags

    #region Debug/Editor Flags

    public bool showCounterAttackRange;
    public bool showJumpAttackRange;
    public bool showBarrierRange;

    #endregion Debug/Editor Flags

    #region Attack Variables

    [FoldoutGroup("Attack Variables", nameof(basicAttackDamage), nameof(CounterAttackRadius), nameof(jumpCounterAttackRadius),
        nameof(jumpAttackPoint), nameof(counterAttackPoint), nameof(counterAttackCheckDuration),
        nameof(perfectCounterAttackCheckDuration), nameof(attackGap))]
    [SerializeField] private Void attackGroupHold;

    [SerializeField, HideInInspector] public int basicAttackDamage = 1;
    [SerializeField, HideInInspector, Range(0f, 3f)] private float CounterAttackRadius;
    [SerializeField, HideInInspector, Range(0f, 2f)] private float jumpCounterAttackRadius;
    [SerializeField, HideInInspector] public Transform jumpAttackPoint;
    [SerializeField, HideInInspector] public Transform counterAttackPoint;
    [SerializeField, HideInInspector, Range(0f, 0.3f)] public float counterAttackCheckDuration;
    [SerializeField, HideInInspector, Range(0f, 0.2f)] public float perfectCounterAttackCheckDuration;
    [SerializeField, HideInInspector, Range(0f, 1f)] public float attackGap; //CD of attack
    [HideInInspector] public float attackTimer = 0f;//CD timer of attack
    [HideInInspector] public float counterAttackCheckTimer = 0f;
    [HideInInspector] public float attackAnimationTime = 0.35f;

    [HideInInspector] public Vector3 pointerDirection;
    public int attackIndex = 2;
    [HideInInspector] public float combatTimer;
    private float comboTimer;

    #endregion Attack Variables

    #region Storm Variables

    [FoldoutGroup("Storm Variables", nameof(storm), nameof(prepareStormTime), nameof(stormDuration),
        nameof(storm_radius), nameof(repelLayer), nameof(repelForce), nameof(stormEffectPos))]
    [SerializeField] private Void barrierGroupHold;

    [SerializeField, HideInInspector] private GameObject storm;
    [SerializeField, HideInInspector, Range(0f, 2f)] private float prepareStormTime = 1f;
    [SerializeField, HideInInspector, Range(0f, 2f)] private float stormDuration = 0.5f;
    [SerializeField, HideInInspector, Range(0f, 2f)] public float storm_radius = 1.1f;
    [SerializeField, HideInInspector] private LayerMask repelLayer;
    [SerializeField, HideInInspector, Range(0f, 300f)] private float repelForce = 100;
    [HideInInspector] public bool stormReady = false;
    [HideInInspector] public float prepareStormTimer = 0f;
    private Vector3 originalStormPos;
    public Transform stormEffectPos;

    #endregion Storm Variables

    #region Unity Lifecycle

    private void Awake()
    {
        if (instance == null) { instance = this; }
    }

    private void Start()
    {
        controller = GetComponent<CharacterController2D>();
        inputPlayer = GetComponent<InputPlayer>();
        rb = GetComponent<Rigidbody2D>();
        vfx = VFXManager.instance;
        gameManager = GameManager.instance;
        animSet = GetComponentInChildren<AnimSetBool>();
        health = GetComponent<Health>();
        energy = GetComponent<Energy>();
        selfPooler = GetComponentInChildren<InternalObjectPooler>();
        hSAbilitiesManager = HeartSwordAbilities.instance;
        originalStormPos = storm.transform.localPosition;

        attackIndex = 2;//max index, so that next loop will start from initial
    }

    private void Update()
    {
        bool isInBulletTime = VFXManager.isInBulletTime;
        attackTimer += isInBulletTime ? Time.unscaledDeltaTime : Time.deltaTime;
        counterAttackCheckTimer += isInBulletTime ? Time.unscaledDeltaTime : Time.deltaTime;
        combatTimer -= isInBulletTime ? Time.unscaledDeltaTime : Time.deltaTime;
        comboTimer += isInBulletTime ? Time.unscaledDeltaTime : Time.deltaTime;

        if (isPreparingStorm)
        {
            prepareStormTimer += isInBulletTime ? Time.unscaledDeltaTime : Time.deltaTime;
            if (prepareStormTimer >= 0.2f) { anim.SetBool("storm", true); }
        }
        else { prepareStormTimer = 0f; stormReady = false; }

        if (isPreparingStorm && prepareStormTimer >= prepareStormTime && !stormReady) { stormReady = true; SoundManager.PlaySound("defend_block2"); vfx.SpawnSlashEffect(stormEffectPos.position); }
        if (combatTimer <= 0) { anim.SetBool("isCombat", false); isInCombat = false; combatTimer = 0; }
        if (comboTimer >= 0.67f) { attackIndex = 2; }

        //if (attackTimer > attackGap) { isAttacking = false; }
        if (counterAttackCheckTimer <= counterAttackCheckDuration) { CheckCounterAttack(); }
        else { isCounterAttacking = false; }

        if (isOnStorm) StormCounterAttack();
    }

    #endregion Unity Lifecycle

    #region Attack Methods

    public bool Attack(bool attackLeft, bool consumeEnergy = true)
    {
        // Guard clauses for attack eligibility

        if (!canAttack) return false;
        if (controller.isFloating) return false;
        if (attackTimer < attackGap) return false;
        if (consumeEnergy) if (!energy.AttackConsume()) { return false; }

        InitializeAttack(attackLeft);

        // Play the appropriate attack animation
        anim.Play(GetAttackAnimName());
        anim.SetBool("isCombat", true);

        return true;
    }

    public void InitializeAttack(bool attackLeft)
    {
        isAttackingLeft = attackLeft;
        isInCombat = true;
        isCounterAttacking = true;
        attackTimer = 0f;
        counterAttackCheckTimer = 0f;
        attackIndex++;
        if (attackIndex > 2) attackIndex = 1;
        combatTimer = 2f;
        comboTimer = 0f;
    }

    private string GetAttackAnimName()
    {
        string indexStr = attackIndex.ToString();
        if (controller.isJumping)
            return $"attack_jump_{indexStr}";
        if (controller.isFalling)
            return $"attack_fall_{indexStr}";
        if (controller.isRunning)
            return $"attack_run_{indexStr}";
        return $"attack_idle_{indexStr}";
    }

    public void CheckCounterAttack()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(counterAttackPoint.position, CounterAttackRadius + 10);
        float counterRadius = CounterAttackRadius + counterAttackPoint.localPosition.x;
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
                    if (dist <= counterRadius)// if normal attack
                    {
                        if (counterAttackCheckTimer <= perfectCounterAttackCheckDuration) CounterAttack(proj, true);
                        if (counterAttackCheckTimer <= counterAttackCheckDuration) CounterAttack(proj, false);
                    }
                }
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

    public void CounterAttack(IProjectile projectile, bool isPerfect)
    {
        //if (isAimingRightStick) { projectile.transform.position = pointerPos.position; }
        attackTimer = attackGap + 0.5f;
        canDefend = true;
        if (isPerfect)
        {
            hSAbilitiesManager.ModifyHSPoint(0.5f);
            energy.ChangeEnergy(-energy.attack_energy_consumption);

            projectile.SetUp(pointerDirection, this.gameObject, 100, _isHostileToPlayer: false, _damage: projectile.damage * basicAttackDamage);
            projectile.PerfectCounterAttack();
            SoundManager.PlaySound("perfect_attack");
        }
        else
        {
            hSAbilitiesManager.ModifyHSPoint(0.2f);
            projectile.SetUp(pointerDirection, this.gameObject, 30, _isHostileToPlayer: false, _damage: projectile.damage * basicAttackDamage);
            projectile.NormalCounterAttack();
            SoundManager.PlaySound("normal_counter_attack");
        }
    }

    public void CounterMeleeAttack()
    {
        hSAbilitiesManager.ModifyHSPoint(1);
        energy.ChangeEnergy(-2);
        int i = Random.Range(1, 3);
        SoundManager.PlaySound("metal_hit" + i);
        energy.PerfectCounterAttackRestore();
        isCounterAttacking = false;
        canDefend = true;
        attackTimer = 3f;
    }

    public void EndAttack()
    {
        isCounterAttacking = false;
    }

    public bool JumpAttack()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(jumpAttackPoint.position, jumpCounterAttackRadius + 5);
        bool hit = false;
        foreach (Collider2D collider in colliders)
        {
            if (collider.TryGetComponent<IProjectile>(out IProjectile i))
            {
                if (i != null && i.isHostileToPlayer && !i.collided)
                {
                    float distance = Vector3.Distance(i.GetPivot(), jumpAttackPoint.position);
                    if (distance <= jumpCounterAttackRadius) { JumpCounterAttack(i); hit = true; }
                }
            }
        }
        return hit;
    }

    private void JumpCounterAttack(IProjectile projectile)
    {
        hSAbilitiesManager.ModifyHSPoint(0.5f);
        Vector3 v = new Vector3(0, 0, -90);
        SoundManager.PlaySound("normal_counter_attack");
        projectile.SetUp(v, this.gameObject, 100, _isHostileToPlayer: false);
        energy.ChangeEnergy(4);
        projectile.PerfectCounterAttack();
    }

    #endregion Attack Methods

    #region Defend Methods

    public void OnDefend()
    {
        if (canDefend && !controller.isFloating)
        {
            if (!energy.DefendConsume()) { return; }
            isDefending = true;
            animSet.Anim_Defend(0);
            anim.Play("defend");
        }
    }

    public void EndDefend()
    {
        if (!isDefending) return;
        animSet.Anim_Defend(1);
        anim.SetBool("isCombat", true);
        combatTimer = 2;
        if (controller.isFalling) { anim.Play("fall_combat"); }
        else if (controller.isJumping) { anim.Play("jump_combat"); }
        else { anim.Play("idle_combat"); }
        isDefending = false;
    }

    #endregion Defend Methods

    #region Storm Methods

    public void OnStorm()
    {
        if (canStorm && !isOnStorm)
        {
            if (stormReady)
            {
                attackIndex = 1;
                combatTimer = 2f;
                comboTimer = 0f;

                if (controller.isJumping) { anim.Play("attack_jump_" + attackIndex); }
                else if (controller.isFalling) { anim.Play("attack_fall_" + attackIndex); }
                else if (controller.isRunning) { anim.Play("attack_run_" + attackIndex); }
                else { anim.Play("attack_idle_" + attackIndex); }

                anim.SetBool("isCombat", true);
                isInCombat = true;
                isOnStorm = true;
                SoundManager.PlaySound("storm");
                storm.SetActive(true);
                Invoke("EndStorm", stormDuration);
                StormCounterAttack();
            }
            combatTimer = 2;
            stormReady = false;
        }
    }

    public void StormCounterAttack()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(storm.transform.position, storm_radius, repelLayer);
        foreach (Collider2D collider in colliders)
        {
            if (collider.TryGetComponent<Rigidbody2D>(out Rigidbody2D rb))
            {
                bool goingLeft = storm.transform.position.x > collider.transform.position.x;
                Vector2 dir = goingLeft ? new Vector2(-1, 0) : new Vector2(1, 0);
                rb.AddForce(repelForce * dir, ForceMode2D.Impulse);
            }
        }
    }

    public void EndStorm()
    {
        if (controller.isJumping)
        {
            rb.isKinematic = false;
            controller.canMove = true;
        }
        storm.transform.SetParent(this.transform, false);
        storm.transform.localPosition = originalStormPos;
        storm.SetActive(false);
        isOnStorm = false;
    }

    #endregion Storm Methods

    #region Gizmos

    private void OnDrawGizmos()
    {
        if (showCounterAttackRange)
        {
            Gizmos.DrawWireSphere(counterAttackPoint.position, CounterAttackRadius);
        }
        if (showJumpAttackRange)
        {
            Gizmos.DrawWireSphere(jumpAttackPoint.position, jumpCounterAttackRadius);
        }
        if (showBarrierRange)
        {
            Gizmos.DrawWireSphere(storm.transform.position, storm_radius);
        }
    }

    #endregion Gizmos
}