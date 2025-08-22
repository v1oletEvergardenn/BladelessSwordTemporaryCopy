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
    [SerializeField] public Animator anim;
    [SerializeField] private Transform pointerPos;

    #endregion Singleton & References

    #region State Flags

    [HideInInspector] public bool isAimingRightStick;
    [HideInInspector] public bool isInCombat;
    [HideInInspector] public bool isDefending;
    [HideInInspector] public bool canDefend = true;
    [HideInInspector] public bool canAttack;
    [HideInInspector] public bool isAttacking;
    [HideInInspector] public bool isInAttackAnim = false;
    [HideInInspector] public bool isAttackingLeft = true;
    [HideInInspector] public bool isHS_attack = false;
    [HideInInspector] public bool canStorm = true;
    [HideInInspector] public bool isOnStorm = false;
    [HideInInspector] public bool isPreparingStorm = false;

    #endregion State Flags

    #region Debug/Editor Flags

    public bool showCounterAttackRange;
    public bool showJumpAttackRange;
    public bool showHSAttackRange;
    public bool showBarrierRange;

    #endregion Debug/Editor Flags

    #region Attack Variables

    [FoldoutGroup("Attack Variables", nameof(basicAttackDamage), nameof(CounterAttackRadius), nameof(jumpCounterAttackRadius),
        nameof(jumpAttackPoint), nameof(counterAttackPoint), nameof(counterAttackCheckDuration),
        nameof(perfectCounterAttackCheckDuration), nameof(attackGap))]
    [SerializeField] private Void attackGroupHold;

    [SerializeField, HideInInspector] private int basicAttackDamage = 1;
    [SerializeField, HideInInspector, Range(0f, 3f)] private float CounterAttackRadius;
    [SerializeField, HideInInspector, Range(0f, 2f)] private float jumpCounterAttackRadius;
    [SerializeField, HideInInspector] public Transform jumpAttackPoint;
    [SerializeField, HideInInspector] public Transform counterAttackPoint;

    [SerializeField, HideInInspector, Range(0f, 0.3f)] public float counterAttackCheckDuration;
    [SerializeField, HideInInspector, Range(0f, 0.2f)] public float perfectCounterAttackCheckDuration;
    [SerializeField, HideInInspector, Range(0f, 1f)] public float attackGap;
    private float attackTimer = 0f;
    private float counterAttackCheckTimer = 0f;

    [HideInInspector] public Vector3 direction;
    private float attackAnimTimer = 0f;
    private float attackAnimDuration = 0.36f;//duration of an attack animation
    [HideInInspector] public int attackIndex = 2;
    [HideInInspector] public float combatTimer;
    private float comboTimer;
    private HashSet<IDamagable> hsHitTargets = new HashSet<IDamagable>();

    #endregion Attack Variables

    #region Heart Sword Variables

    [FoldoutGroup("Heart Sword Variables", nameof(HS_attack_radius), nameof(HS_attack_damage),
        nameof(maxHS_point), nameof(currentHS_point), nameof(activatedHS_point))]
    [SerializeField] private Void HSGroupHold;

    [SerializeField, HideInInspector] public float maxHS_point = 3;

    [SerializeField, HideInInspector] public float currentHS_point = 0;
    [SerializeField, HideInInspector] public float activatedHS_point = 0;
    [SerializeField, HideInInspector, Range(0f, 5f)] public float HS_attack_radius = 2.3f;
    [SerializeField, HideInInspector, Range(0f, 10f)] public int HS_attack_damage = 5;
    public MeleeAttack HS_attack_effect;
    public List<GameObject> HS_points;
    private bool hsHitEffectPlayed = false;

    #endregion Heart Sword Variables

    #region Storm Variables

    [FoldoutGroup("Barrier Variables", nameof(storm), nameof(prepareStormTime), nameof(stormDuration),
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
        originalStormPos = storm.transform.localPosition;
        activatedHS_point = 0;
        attackIndex = 2;//max index, so that next loop will start from initial
    }

    private void Update()
    {
        currentHS_point = Mathf.Clamp(currentHS_point, 0, maxHS_point);
        activatedHS_point = Mathf.Clamp(activatedHS_point, 0, currentHS_point);
        bool isInBulletTime = VFXManager.isInBulletTime;
        attackTimer += isInBulletTime ? Time.unscaledDeltaTime : Time.deltaTime;
        counterAttackCheckTimer += isInBulletTime ? Time.unscaledDeltaTime : Time.deltaTime;
        combatTimer -= isInBulletTime ? Time.unscaledDeltaTime : Time.deltaTime;
        comboTimer += isInBulletTime ? Time.unscaledDeltaTime : Time.deltaTime;
        attackAnimTimer += isInBulletTime ? Time.unscaledDeltaTime : Time.deltaTime;

        UpdateHS();

        if (isPreparingStorm)
        {
            prepareStormTimer += isInBulletTime ? Time.unscaledDeltaTime : Time.deltaTime;
            if (prepareStormTimer >= 0.2f) { anim.SetBool("storm", true); }
        }
        else { prepareStormTimer = 0f; stormReady = false; }

        if (isPreparingStorm && prepareStormTimer >= prepareStormTime && !stormReady) { stormReady = true; SoundManager.PlaySound("defend_block2"); vfx.SpawnSlashEffect(stormEffectPos.position); }
        if (combatTimer <= 0) { anim.SetBool("isCombat", false); isInCombat = false; combatTimer = 0; }
        if (comboTimer >= 0.67f) { attackIndex = 2; }

        if (attackAnimTimer <= attackAnimDuration) { isInAttackAnim = true; } else { isInAttackAnim = false; }

        //if (attackTimer > attackGap) { isAttacking = false; isHS_attack = false; }

        if (counterAttackCheckTimer <= counterAttackCheckDuration) { CheckCounterAttack(); }
        else { isAttacking = false; isHS_attack = false; }

        if (isOnStorm) StormCounterAttack();
    }

    #endregion Unity Lifecycle

    #region Attack Methods

    public void Attack(bool attackLeft)
    {
        // Guard clauses for attack eligibility

        if (attackTimer < attackGap) return;
        if (!canAttack) return;
        if (controller.isFloating) return;
        if (activatedHS_point < 1 && !energy.AttackConsume()) { return; }

        // Set attack state
        isAttackingLeft = attackLeft;
        isInAttackAnim = false;
        isInCombat = true;
        isAttacking = true;
        attackTimer = 0f;
        attackAnimTimer = 0f;
        counterAttackCheckTimer = 0f;
        hsHitEffectPlayed = false;
        hsHitTargets.Clear();

        // Heart Sword attack logic
        isHS_attack = false;
        if (activatedHS_point >= 1)
        {
            activatedHS_point -= 1;
            currentHS_point -= 1;
            isHS_attack = true;
        }

        // Combo logic
        attackIndex++;
        if (attackIndex > 2) attackIndex = 1;
        combatTimer = 2f;
        comboTimer = 0f;

        // Determine if this is a back attack and handle flipping
        bool backAttack = false;
        if (InputMaster.instance._moveAction.IsPressed())
        {
            if (inputPlayer.leftPointLeft != attackLeft)
            {
                controller.Flip(true);
                backAttack = true;
            }
        }
        else if (controller.FacingRight == attackLeft)
        {
            controller.Flip(true);
            backAttack = true;
        }

        // Play the appropriate attack animation
        anim.Play(GetAttackAnimName(backAttack));
        anim.SetBool("isCombat", true);

        // Helper method to select the correct animation name
        string GetAttackAnimName(bool backAttack)
        {
            string prefix = isHS_attack ? "HS_" : "";
            string indexStr = attackIndex.ToString();

            if (controller.isJumping)
                return $"{prefix}attack_jump_{indexStr}";
            if (controller.isFalling)
                return $"{prefix}attack_fall_{indexStr}";
            if (controller.isRunning)
                return backAttack
                    ? $"{prefix}attack_back_{indexStr}"
                    : $"{prefix}attack_run_{indexStr}";
            return $"{prefix}attack_idle_{indexStr}";
        }
    }

    public void CheckCounterAttack()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(counterAttackPoint.position, HS_attack_radius + 5);

        //IProjectile closestProjectile = null;
        //IDamagable closestDamagable = null;
        //float minProjDist = float.PositiveInfinity;
        //float minDmgDist = float.PositiveInfinity;

        //var inRangeProjectiles = new List<IProjectile>();
        //var inRangeDamagables = new List<IDamagable>();
        float counterRadius = CounterAttackRadius + counterAttackPoint.localPosition.x;
        float hsCheckDistance = HS_attack_radius + counterAttackPoint.localPosition.x;
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
                    if (isHS_attack && dist <= hsCheckDistance) HS_CounterAttack(proj);//if hs attack
                    else if (dist <= counterRadius)// if normal attack
                    {
                        if (counterAttackCheckTimer <= perfectCounterAttackCheckDuration) CounterAttack(proj, true);
                        if (counterAttackCheckTimer <= counterAttackCheckDuration) CounterAttack(proj, false);
                    }
                }
            }

            if (isHS_attack && collider.TryGetComponent<IDamagable>(out IDamagable dmg))
            {
                if (!IsInCounterDirection(dmg.GetHitPos())) continue;
                float dist = Vector3.Distance(dmg.GetHitPos(), health.GetHitPos());
                if (dist <= hsCheckDistance) HSAttack(dmg);
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
            ModifyHSPoint(1);
            energy.ChangeEnergy(-energy.attack_energy_consumption);

            projectile.SetUp(direction, this.gameObject, 100, _isHostileToPlayer: false, _damage: projectile.damage * basicAttackDamage);
            projectile.PerfectCounterAttack();
            SoundManager.PlaySound("perfect_attack");
        }
        else
        {
            projectile.SetUp(direction, this.gameObject, 30, _isHostileToPlayer: false, _damage: projectile.damage * basicAttackDamage);
            projectile.NormalCounterAttack();
            SoundManager.PlaySound("normal_counter_attack");
        }
    }

    public void HS_CounterAttack(IProjectile projectile)
    {
        //if (isAimingRightStick) { projectile.transform.position = pointerPos.position; }
        attackTimer = attackGap + 0.5f;
        canDefend = true;

        energy.ChangeEnergy(-energy.attack_energy_consumption);

        projectile.SetUp(direction, this.gameObject, 100, _isHostileToPlayer: false, _damage: projectile.damage * basicAttackDamage);
        projectile.PerfectCounterAttack();
        SoundManager.PlaySound("perfect_attack");
    }

    public void HSAttack(IDamagable damagable)
    {
        if (hsHitTargets.Contains(damagable)) return; // Already hit this target in this attack
        IDamagable parentDamagble = damagable;
        if (damagable is SubDamageable sub) { parentDamagble = sub.ParentDamageable; }
        hsHitTargets.Add(parentDamagble);
        foreach (IDamagable i in parentDamagble.subDamagables) { hsHitTargets.Add(i); }

        if (!hsHitEffectPlayed)
        {
            vfx.MeleeAttackEffect(HS_attack_effect,
            damagable,
            damagable.GetHitPos().x < health.GetHitPos().x ? true : false);
            hsHitEffectPlayed = true;
        }
        else
        {
            vfx.SpawnHitEffect(true, damagable.GetHitPos());
            damagable.Repel(HS_attack_effect.repel, damagable.GetHitPos().x < health.GetHitPos().x ? true : false);
        }

        damagable.Damage(HS_attack_damage, this.transform, 0, stunValue: 10);
        canDefend = true;
    }

    public void ModifyHSPoint(float i)
    {
        currentHS_point += i;
        currentHS_point = Mathf.Clamp(currentHS_point, 0, maxHS_point);
    }

    public void CounterMeleeAttack()
    {
        ModifyHSPoint(1);
        energy.ChangeEnergy(-2);
        int i = Random.Range(1, 3);
        SoundManager.PlaySound("metal_hit" + i);
        energy.PerfectCounterAttackRestore();
        isAttacking = false;
        canDefend = true;
        attackTimer = 3f;
    }

    public void EndAttack()
    {
        isAttacking = false;
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
        Vector3 v = new Vector3(0, 0, -90);
        SoundManager.PlaySound("normal_counter_attack");
        projectile.SetUp(v, this.gameObject, 100, _isHostileToPlayer: false);
        energy.ChangeEnergy(4);
        projectile.PerfectCounterAttack();
    }

    public void ActivateHS()
    {
        if (activatedHS_point != currentHS_point)
        {
            activatedHS_point += 1;
            SoundManager.PlaySound("HS_activate");
            vfx.RumblePulse(0.2f, 0.3f, 0.1f);
            vfx.SpawnSlashEffect(health.GetHitPos());
        }
    }

    public void DeactivateHS()
    {
        activatedHS_point -= 1;
    }

    public void CancelHS()
    {
        activatedHS_point = 0;
    }

    public void FullHS()
    {
        if (activatedHS_point != currentHS_point)
        {
            StartCoroutine(IE_activateHS());
            activatedHS_point = currentHS_point;
        }
    }

    private IEnumerator IE_activateHS()
    {
        for (float i = activatedHS_point; i <= currentHS_point; i++)
        {
            vfx.SpawnSlashEffect(health.GetHitPos());
            SoundManager.PlaySound("HS_activate");
            vfx.RumblePulse(0.2f, 0.3f, 0.1f);
            yield return new WaitForSecondsRealtime(0.1f);
        }

        yield return null;
    }

    public void UpdateHS()
    {
        for (int i = 0; i < HS_points.Count; i++)
        {
            if (i < activatedHS_point)
            {
                HS_points[i].SetActive(true);
                HS_points[i].GetComponent<Animator>().Play("activate");
            }
            else if (i < currentHS_point)
            {
                HS_points[i].SetActive(true);
                HS_points[i].GetComponent<Animator>().Play("idle");
            }
            else
            {
                HS_points[i].SetActive(false);
            }
        }
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
        if (showHSAttackRange)
        {
            Gizmos.DrawWireSphere(counterAttackPoint.position, HS_attack_radius);
        }
        if (showBarrierRange)
        {
            Gizmos.DrawWireSphere(storm.transform.position, storm_radius);
        }
    }

    #endregion Gizmos
}