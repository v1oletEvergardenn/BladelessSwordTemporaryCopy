using EditorAttributes;
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerAttack : MonoBehaviour
{
    #region Singleton & References

    public static PlayerAttack instance;
    private CharacterController2D controller;
    private VFXManager vfx;
    private GameManager gameManager;
    private Energy energy;
    private Rigidbody2D rb;

    //private AnimSetBool animSet;
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
    [HideInInspector] public bool isCounterAttacking;
    [HideInInspector] public bool isAttackingLeft = true;
    [HideInInspector] public bool isOnStorm = false;
    [HideInInspector] public bool isPreparingStorm = false;

    public bool CanAttack()
    { return ActionLock.Can(Lock.Attack); }

    public bool CanDefend()
    { return ActionLock.Can(Lock.Defend); }

    public bool CanStorm()
    { return ActionLock.Can(Lock.Storm); }

    #endregion State Flags

    #region Attack Variables

    public bool DebugAttack;
    public int basicAttackDamage = 1;

    public Transform counterAttackPoint;
    public ProjectileHit NormalCounterAttackEffect;
    public ProjectileHit PerfectCounterAttackEffect;
    public Vector3 pointerDirection;
    public int attackIndex = 2;
    public Vector3 counterAttackPointOriginalLocalPos;
    [SerializeField, Range(0f, 3f)] private float CounterAttackRadius;

    [Range(0f, 0.3f)] public float counterAttackCheckDuration;
    [Range(0f, 0.2f)] public float perfectCounterAttackCheckDuration;
    [Range(0f, 1f)] public float attackGap; //CD of attack

    private CircleCollider2D counterAttackCollider;

    private Vector2 counterAttackColliderOriginalOffset;
    private readonly List<Collider2D> counterAttackResults = new List<Collider2D>();
    private float comboTimer;

    [HideInInspector] public float attackTimer = 0f;//CD timer of attack
    [HideInInspector] public float counterAttackCheckTimer = 0f;
    [HideInInspector] public float attackAnimationTime = 0.35f;
    [HideInInspector] public float combatTimer;

    #endregion Attack Variables

    #region JumpAttack Variables

    public bool DebugJumpAttack;
    public Transform jumpAttackPoint;
    [SerializeField, Range(0f, 2f)] private float jumpCounterAttackRadius;

    #endregion JumpAttack Variables

    #region Storm Variables

    public bool DebugStorm;
    public bool stormReady = false;
    public float prepareStormTimer = 0f;
    public Transform stormEffectPos;
    [Range(0f, 2f)] public float storm_radius = 1.1f;
    [SerializeField] private GameObject storm;
    [SerializeField, Range(0f, 2f)] private float prepareStormTime = 1f;
    [SerializeField, Range(0f, 2f)] private float stormDuration = 0.5f;
    [SerializeField] private LayerMask repelLayer;
    [SerializeField, Range(0f, 300f)] private float repelForce = 100;

    private Vector3 originalStormPos;

    #endregion Storm Variables

    #region Unity Lifecycle

    private PlayerQuestActionKey playerQuestActionKey;

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
        health = GetComponent<Health>();
        energy = GetComponent<Energy>();
        selfPooler = GetComponentInChildren<InternalObjectPooler>();
        hSAbilitiesManager = HeartSwordAbilities.instance;
        originalStormPos = storm.transform.localPosition;
        counterAttackCollider = GetCounterAttackPoint().GetComponent<CircleCollider2D>();
        attackIndex = 2;//max index, so that next loop will start from initial
        playerQuestActionKey = gameManager.playerQuestActionKey;
        if (counterAttackCollider != null)
        {
            counterAttackColliderOriginalOffset = counterAttackCollider.offset;
            counterAttackCollider.isTrigger = true;
            counterAttackCollider.enabled = false;
        }
    }

    private Transform GetCounterAttackPoint()
    {
        return counterAttackPoint;
    }

    private void Update()
    {
        attackTimer += TimeScaleManager.Delta(TimeChannel.Player);
        counterAttackCheckTimer += TimeScaleManager.Delta(TimeChannel.Player);
        combatTimer -= TimeScaleManager.Delta(TimeChannel.Player);
        comboTimer += TimeScaleManager.Delta(TimeChannel.Player);

        if (isPreparingStorm)
        {
            prepareStormTimer += TimeScaleManager.Delta(TimeChannel.Player);
            if (prepareStormTimer >= 0.2f) { anim.SetBool("storm", true); }
        }
        else { prepareStormTimer = 0f; stormReady = false; }

        if (isPreparingStorm && prepareStormTimer >= prepareStormTime && !stormReady)
        {
            stormReady = true; vfx.SpawnSlashEffect(stormEffectPos.position);
        }
        if (combatTimer <= 0) { anim.SetBool("isCombat", false); isInCombat = false; combatTimer = 0; }
        if (comboTimer >= 0.67f) { attackIndex = 2; }

        UpdateCounterAttackColliderState();

        if (counterAttackCheckTimer <= counterAttackCheckDuration) { CheckCounterAttack(); }
        else { isCounterAttacking = false; }

        if (isOnStorm) StormCounterAttack();
    }

    #endregion Unity Lifecycle

    #region Attack Methods

    public bool Attack(bool attackLeft, bool consumeEnergy = true)
    {
        if (!CanAttack()) return false;
        if (attackTimer < attackGap) return false;
        if (consumeEnergy) if (!energy.AttackConsume()) { return false; }
        InitializeAttack(attackLeft);
        if (attackLeft) { QuestManager.OnAction(playerQuestActionKey.leftCounterAttack); }
        else { QuestManager.OnAction(playerQuestActionKey.rightCounterAttack); }
        // Play the appropriate attack animation
        anim.Play(GetAttackAnimName());
        anim.SetBool("isCombat", true);
        int i = Random.Range(0, 2);
        if (i == 0)
        {
            SoundManager.PlaySound("attackempty", random: true);
        }
        else
        {
            SoundManager.PlaySound("quickAttEmpty", random: true);
        }

        return true;
    }

    public void InitializeAttack(bool attackLeft)
    {
        isAttackingLeft = attackLeft;
        counterAttackCheckTimer = 0f;
        isInCombat = true;
        isCounterAttacking = true;
        attackTimer = 0f;
        attackIndex++;
        if (attackIndex > 2) attackIndex = 1;
        combatTimer = 2f;
        comboTimer = 0f;
        hitIdamagables.Clear();
        UpdateAttackPointPosition();
        UpdateCounterAttackColliderState();
    }

    private string GetAttackAnimName()
    {
        string indexStr = attackIndex.ToString();
        if (controller.isJumping)
            return $"attack_jump_{indexStr}";
        if (controller.isFalling)
            return $"attack_fall_{indexStr}";
        if (controller.isRunning)
        {
            if (controller.FacingRight == isAttackingLeft)
            {
                return $"attack_back_{indexStr}";
            }
            else
            {
                return $"attack_run_{indexStr}";
            }
        }
        return $"attack_idle_{indexStr}";
    }

    private List<IDamagable> hitIdamagables = new List<IDamagable>();

    public void CheckCounterAttack()
    {
        UpdateAttackPointPosition();

        float counterRadius = CounterAttackRadius + Mathf.Abs(counterAttackPoint.localPosition.x);
        counterAttackResults.Clear();
        counterAttackCollider.radius = CounterAttackRadius;
        Physics2D.OverlapCollider(counterAttackCollider, new ContactFilter2D().NoFilter(), counterAttackResults);

        foreach (Collider2D collider in counterAttackResults)
        {
            if (collider == null) continue;
            if (collider.gameObject == this.gameObject || collider.transform.IsChildOf(transform)) continue;

            if (collider.TryGetComponent<IProjectile>(out IProjectile proj))
            {
                if (proj.isHostileToPlayer && !proj.collided)
                {
                    if (!IsInCounterDirection(proj.GetHitPos())) continue;
                    float dist = Vector2.Distance(proj.GetHitPos(), counterAttackPoint.position);
                    if (dist <= counterRadius)
                    {
                        if (counterAttackCheckTimer <= perfectCounterAttackCheckDuration) CounterAttack(proj, true);
                        else if (counterAttackCheckTimer <= counterAttackCheckDuration) CounterAttack(proj, false);
                    }
                }
            }

            if (collider.TryGetComponent<IDamagable>(out IDamagable dmg))
            {
                if (!hitIdamagables.Contains(dmg))
                {
                    if (dmg.canBeHitWithoutHSAttack)
                    {
                        float distance = Vector2.Distance(dmg.GetHitPos(), counterAttackPoint.position);
                        if (distance <= counterRadius)
                        {
                            dmg.Damage(basicAttackDamage, this.transform, 0);
                            hitIdamagables.Add(dmg);
                            HitEffect(dmg, true);
                        }
                    }
                }
            }
        }

        bool IsInCounterDirection(Vector3 targetPos)
        {
            if (isAttackingLeft && targetPos.x >= transform.position.x) return false;
            if (!isAttackingLeft && targetPos.x <= transform.position.x) return false;
            return true;
        }
    }

    private void UpdateAttackPointPosition()
    {
        float x = isAttackingLeft ? -counterAttackPointOriginalLocalPos.x : counterAttackPointOriginalLocalPos.x;
        float y = counterAttackPointOriginalLocalPos.y;
        counterAttackPoint.position = transform.position + new Vector3(x, y, 0);
    }

    private void UpdateCounterAttackColliderState()
    {
        if (counterAttackCollider == null) return;
        bool active = isCounterAttacking && counterAttackCheckTimer <= counterAttackCheckDuration;
        counterAttackCollider.enabled = active;
    }

    public void CounterAttack(IProjectile projectile, bool isPerfect)
    {
        if (!projectile.collisionEnabled) return;
        //if (isAimingRightStick) { projectile.transform.position = pointerPos.position; }
        attackTimer = attackGap;
        HitEffect(projectile, true);

        if (isPerfect)
        {
            QuestManager.OnAction(playerQuestActionKey.CT_Proj);
            QuestManager.OnAction(playerQuestActionKey.CT_perf_Proj);
            hSAbilitiesManager.ModifyHSPoint(0.5f);
            energy.ChangeEnergy(-energy.attack_energy_consumption);

            projectile.hitEffect.Repel(ProjectileHitResult.Perfect, projectile, health);
            projectile.SetUp(pointerDirection, this.gameObject).
                SetHostileToPlayer(false).
                SetDamage(projectile.attribute.damage * basicAttackDamage).
                SetAdditionalSpeed(100);
            projectile.PerfectCounterAttack();
            if (!projectile.muteHitSound)
            {
                SoundManager.PlaySound("PerfectParry", 0.7f);
                SoundManager.PlaySound("PerfectParryConfirm", 0.7f);
            }
        }
        else
        {
            QuestManager.OnAction(playerQuestActionKey.CT_Proj);
            QuestManager.OnAction(playerQuestActionKey.CT_norm_Proj);
            hSAbilitiesManager.ModifyHSPoint(0.2f);
            projectile.hitEffect.Repel(ProjectileHitResult.Normal, projectile, health);
            projectile.SetUp(pointerDirection, this.gameObject).
                SetHostileToPlayer(false).
                SetDamage(projectile.attribute.damage * basicAttackDamage).
                SetAdditionalSpeed(30);

            projectile.NormalCounterAttack();
        }
    }

    public void CounterMeleeAttack()
    {
        SoundManager.PlaySound("PerfectParry", 0.7f);
        SoundManager.PlaySound("PerfectParryConfirm", 0.7f);
        QuestManager.OnAction(playerQuestActionKey.CT_Melee);
        hSAbilitiesManager.ModifyHSPoint(1);
        energy.ChangeEnergy(-2);
        int i = Random.Range(1, 3);
        energy.PerfectCounterAttackRestore();
        isCounterAttacking = false;
        attackTimer = 3f;
        UpdateCounterAttackColliderState();
    }

    public void HitEffect(IDamagable dmg, bool isPerfect)
    {
        if (dmg.resetAttackCDOnHit) attackTimer = attackGap;
        if (!dmg.consumeEnergyOnHit) energy.PerfectCounterAttackRestore();
        if (isPerfect)
        {
            PerfectCounterAttackEffect.CameraShake(ProjectileHitResult.Perfect);
            PerfectCounterAttackEffect.RumblePulse(ProjectileHitResult.Perfect);
            vfx.SpawnHitEffect(true, dmg.GetHitPos());
        }
        else
        {
            PerfectCounterAttackEffect.CameraShake(ProjectileHitResult.Normal);
            PerfectCounterAttackEffect.RumblePulse(ProjectileHitResult.Normal);
        }
    }

    public void HitEffect(IProjectile projectile, bool isPerfect)
    {
        if (isPerfect)
        {
            PerfectCounterAttackEffect.CameraShake(ProjectileHitResult.Perfect);
            PerfectCounterAttackEffect.RumblePulse(ProjectileHitResult.Perfect);
            vfx.SpawnHitEffect(true, projectile.GetHitPos());
        }
        else
        {
            PerfectCounterAttackEffect.CameraShake(ProjectileHitResult.Normal);
            PerfectCounterAttackEffect.RumblePulse(ProjectileHitResult.Normal);
        }
    }

    public void EndAttack()
    {
        isCounterAttacking = false;
        UpdateCounterAttackColliderState();
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
                    float distance = Vector3.Distance(i.GetHitPos(), jumpAttackPoint.position);
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
        projectile.SetUp(v, this.gameObject).SetAdditionalSpeed(100).SetHostileToPlayer(false);
        energy.ChangeEnergy(4);
        projectile.PerfectCounterAttack();
    }

    #endregion Attack Methods

    #region Defend Methods

    public void OnDefend()
    {
        if (CanDefend())
        {
            if (!energy.DefendConsume()) { return; }
            if (!isDefending) { QuestManager.OnAction(playerQuestActionKey.defend); }
            isDefending = true;

            //animSet.Anim_Defend(0);
            ActionLock.Add("onDefend", Lock.Move | Lock.Attack | Lock.Jump | Lock.SwordTeleport | Lock.SwordJump);
            anim.Play("defend");
        }
    }

    public void DefendHit()
    {
        int i = Random.Range(1, 3);
        anim.Play("defend_hit");
        SoundManager.PlaySound("defend", random: true);
    }

    public void EndDefend()
    {
        if (!isDefending) return;
        //animSet.Anim_Defend(1);
        ActionLock.Remove("onDefend");
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
        if (CanStorm() && !isOnStorm)
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
        //if (controller.isJumping)
        //{
        //    rb.isKinematic = false;
        //    controller.canMove = true;
        //}
        storm.transform.SetParent(this.transform, false);
        storm.transform.localPosition = originalStormPos;
        storm.SetActive(false);
        isOnStorm = false;
    }

    #endregion Storm Methods

    #region Gizmos

    private void OnDrawGizmos()
    {
        if (DebugAttack)
        {
            Gizmos.DrawWireSphere(transform.position + counterAttackPointOriginalLocalPos, CounterAttackRadius);
        }
        if (DebugJumpAttack)
        {
            Gizmos.DrawWireSphere(jumpAttackPoint.position, jumpCounterAttackRadius);
        }
        if (DebugStorm)
        {
            Gizmos.DrawWireSphere(storm.transform.position, storm_radius);
        }
    }

    #endregion Gizmos
}