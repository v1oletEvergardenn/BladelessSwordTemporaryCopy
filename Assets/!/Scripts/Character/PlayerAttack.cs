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
    private CharacterController2D controller;
    public static PlayerAttack instance;
    private VFXManager vfx;
    private GameManager gameManager;
    private Energy energy;
    private Rigidbody2D rb;
    private AnimSetBool animSet;
    private InputPlayer inputPlayer;
    [SerializeField] public Animator anim;
    private IDamagable health;
    [SerializeField] private Transform pointerPos;
    [HideInInspector] public bool isAimingRightStick;
    [HideInInspector] public bool isInCombat;
    public GameObject BounceUI;

    #region ATTACK VARIABLES

    [FoldoutGroup("Attack Variables", nameof(CounterAttackRadius), nameof(jumpCounterAttackRadius), nameof(jumpAttackPoint), nameof(counterAttackPoint), nameof(counterAttackCheckDuration), nameof(perfectCounterAttackCheckDuration), nameof(attackGap))]
    [SerializeField] private Void attackGroupHold;

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
    [HideInInspector] public bool canAttack;
    [HideInInspector] public bool isAttacking;
    [HideInInspector] public bool isInAttackAnim = false;
    private float attackAnimTimer = 0f;
    private float attackAnimDuration = 0.36f;//duration of an attack animation
    [HideInInspector] public int attackIndex = 2;
    [HideInInspector] public float combatTimer;
    private float comboTimer;
    public bool isAttackingLeft = true;

    [FoldoutGroup("Heart Sword Variables", nameof(HS_attack_radius), nameof(HS_attack_damage), nameof(maxHS_point), nameof(currentHS_point), nameof(activatedHS_point), nameof(HS_points))]
    [SerializeField] private Void HSGroupHold;

    [SerializeField, HideInInspector] public int maxHS_point = 3;
    [SerializeField, HideInInspector] public int currentHS_point = 0;
    [SerializeField, HideInInspector] public int activatedHS_point = 0;
    [SerializeField, HideInInspector, Range(0f, 5f)] public float HS_attack_radius = 2.3f;
    [SerializeField, HideInInspector, Range(0f, 10f)] public int HS_attack_damage = 5;
    public List<GameObject> HS_points;

    #endregion ATTACK VARIABLES

    #region BOOMERANG VARIABLES

    [FoldoutGroup("Boomerang Variables", nameof(_boomerang), nameof(launchPosition), nameof(boomerange_speed), nameof(boomerange_startDecreaseTime), nameof(boomerange_decreaseSpeed), nameof(teleportCheckLayer), nameof(boomerange_rotationSpeed))]
    [SerializeField] private Void boomerangGroupHold;

    [SerializeField, HideInInspector] public Boomerang _boomerang;
    [SerializeField, HideInInspector] private Transform launchPosition;
    [SerializeField, HideInInspector, Range(0f, 20f)] private float boomerange_speed = 10f;
    [SerializeField, HideInInspector, Range(0f, 1f)] private float boomerange_startDecreaseTime = 1f;
    [SerializeField, HideInInspector, Range(0f, 20f)] private float boomerange_decreaseSpeed = 30f;
    [SerializeField, HideInInspector, Range(0f, 300f)] private float boomerange_rotationSpeed = 100f;
    [SerializeField, HideInInspector] public LayerMask teleportCheckLayer;

    [HideInInspector] public bool canLaunchBoomerang = true;
    [HideInInspector] public bool isBoomeranging = false;
    [HideInInspector] public bool isHanging = false;

    #endregion BOOMERANG VARIABLES

    #region STORM VARIABLES

    [FoldoutGroup("Barrier Variables", nameof(storm), nameof(prepareStormTime), nameof(stormDuration), nameof(storm_radius), nameof(repelLayer), nameof(repelForce), nameof(stormEffectPos))]
    [SerializeField] private Void barrierGroupHold;

    [SerializeField, HideInInspector] private GameObject storm;
    [SerializeField, HideInInspector, Range(0f, 2f)] private float prepareStormTime = 1f;
    [SerializeField, HideInInspector, Range(0f, 2f)] private float stormDuration = 0.5f;
    [SerializeField, HideInInspector, Range(0f, 2f)] public float storm_radius = 1.1f;
    [SerializeField, HideInInspector] private LayerMask repelLayer;
    [SerializeField, HideInInspector, Range(0f, 300f)] private float repelForce = 100;
    [HideInInspector] public bool canStorm = true;
    [HideInInspector] public bool isOnStorm = false;
    [HideInInspector] public bool isPreparingStorm = false;
    private bool stormReady = false;
    [HideInInspector] public float prepareStormTimer = 0f;
    private Vector3 originalStormPos;
    public Transform stormEffectPos;

    #endregion STORM VARIABLES

    [HideInInspector] public bool isDefending;
    [HideInInspector] public bool canDefend = true;

    public bool showCounterAttackRange;
    public bool showJumpAttackRange;
    public bool showHSAttackRange;
    public bool showBarrierRange;

    private void Awake()
    {
        if (instance == null) { instance = this; }
    }

    // Start is called before the first frame update
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
        attackTimer += Time.deltaTime;
        counterAttackCheckTimer += Time.deltaTime;
        combatTimer -= Time.deltaTime;
        comboTimer += Time.deltaTime;
        attackAnimTimer += Time.deltaTime;
        UpdateHS();
        if (isPreparingStorm) { prepareStormTimer += Time.deltaTime; }
        else { prepareStormTimer = 0f; stormReady = false; }
        if (isPreparingStorm && prepareStormTimer >= prepareStormTime && !stormReady) { stormReady = true; SoundManager.PlaySound("defend_block2"); vfx.SpawnSlashEffect(stormEffectPos.position); }
        if (combatTimer <= 0) { anim.SetBool("isCombat", false); isInCombat = false; combatTimer = 0; }
        if (comboTimer >= 0.9f) { attackIndex = 2; }
        if (attackAnimTimer <= attackAnimDuration) { isInAttackAnim = true; }
        else { isInAttackAnim = false; }
        if (counterAttackCheckTimer <= counterAttackCheckDuration)
        {
            CheckCounterAttack();
        }
        else
        {
            isAttacking = false;
        }
    }

    #region Attack

    private bool isHSattack = false;

    public void Attack(bool attackLeft)
    {
        if (canAttack && !controller.isFloating && attackTimer >= attackGap)
        {
            isAttackingLeft = attackLeft;
            isInAttackAnim = false;
            isInCombat = true;
            isAttacking = true;
            attackTimer = 0f;
            attackAnimTimer = 0f;
            counterAttackCheckTimer = 0f;
            if (activatedHS_point > 0)
            {
                if (!energy.AttackConsume()) { return; }
            }
            isHSattack = false;

            attackIndex++;
            if (attackIndex > 2) { attackIndex = 1; }
            combatTimer = 2f;
            comboTimer = 0f;

            bool backAttack = false;
            if (controller.FacingRight == attackLeft) { controller.Flip(true); backAttack = true; }

            if (controller.isJumping) { anim.Play("attack_jump_" + attackIndex); }
            else if (controller.isFalling) { anim.Play("attack_fall_" + attackIndex); }
            else if (controller.isRunning) { if (backAttack) { anim.Play("attack_back"); } else { anim.Play("attack_run_" + attackIndex); } }
            else { anim.Play("attack_idle_" + attackIndex); }

            anim.SetBool("isCombat", true);

            EndHanging();
        }
    }

    public void CheckCounterAttack()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(counterAttackPoint.position, HS_attack_radius + 5);

        float dist = Mathf.Infinity;
        float dist2 = Mathf.Infinity;
        IProjectile closeTarget_proj = null;
        IDamagable closeTarget_obj = null;
        if (activatedHS_point > 0 && !isHSattack)
        {
            activatedHS_point -= 1;
            currentHS_point -= 1;
            isHSattack = true;
        }
        foreach (Collider2D collider in colliders)
        {
            if (collider.gameObject != this.gameObject)
            {
                if (collider.TryGetComponent<IProjectile>(out IProjectile i))
                {
                    float v = Vector3.Distance(i.GetPivot(), health.GetHitPos());
                    if (dist > v) { dist = v; closeTarget_proj = i; }
                }//close iprojectile
                if (collider.TryGetComponent<IDamagable>(out IDamagable d) && isHSattack)
                {
                    float v = Vector3.Distance(d.GetHitPos(), health.GetHitPos());
                    if (dist2 > v) { dist2 = v; closeTarget_obj = d; }
                } // close idamagable
                if (collider.TryGetComponent<NonHSAttackHitTrigger>(out NonHSAttackHitTrigger n))
                {
                    n.Trigger();
                }
            }
        }//search for close target
        if (closeTarget_proj != null && closeTarget_proj.isHostileToPlayer && !closeTarget_proj.collided)
        {
            if (controller.FacingRight && closeTarget_proj.GetPivot().x <= transform.position.x) { return; }
            if (!controller.FacingRight && closeTarget_proj.GetPivot().x >= transform.position.x) { return; }
            float distance = Vector2.Distance(closeTarget_proj.GetPivot(), health.GetHitPos());
            if (isHSattack)
            {
                if (distance <= HS_attack_radius + counterAttackPoint.localPosition.x) { CounterAttack(closeTarget_proj, true); return; }
            }
            if (distance <= CounterAttackRadius + counterAttackPoint.localPosition.x)
            {
                if (counterAttackCheckTimer <= perfectCounterAttackCheckDuration) { CounterAttack(closeTarget_proj, true); return; }
                if (counterAttackCheckTimer <= counterAttackCheckDuration) { CounterAttack(closeTarget_proj, false); return; }
            }//HS counter attack projectiles all in range
            if (closeTarget_obj != null && isHSattack)
            {
                if (controller.FacingRight && closeTarget_obj.GetHitPos().x <= transform.position.x) { return; }
                if (!controller.FacingRight && closeTarget_obj.GetHitPos().x >= transform.position.x) { return; }
                float v = Vector3.Distance(closeTarget_obj.GetHitPos(), health.GetHitPos());
                if (v <= HS_attack_radius + counterAttackPoint.localPosition.x) { HSAttack(closeTarget_obj); return; }//deal damage to them
            }//HS counter attack deals damage in range
        }
    }

    public void CounterAttack(IProjectile projectile, bool isPerfect)
    {
        if (isAimingRightStick) { projectile.transform.position = pointerPos.position; }
        isAttacking = false;
        canDefend = true;
        attackTimer = attackGap;
        counterAttackCheckTimer = counterAttackCheckDuration + 0.5f;
        if (isPerfect)
        {
            energy.PerfectCounterAttackRestore();
            projectile.SetUp(direction, this.gameObject, 100, _isHostileToPlayer: false);
            projectile.PerfectCounterAttack();
            SoundManager.PlaySound("perfect_attack");
        }
        else
        {
            projectile.SetUp(direction, this.gameObject, 30, _isHostileToPlayer: false);
            projectile.NormalCounterAttack();
            SoundManager.PlaySound("normal_counter_attack");
        }
    }

    public void CounterMeleeAttack()
    {
        currentHS_point = maxHS_point;
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

    public void HSAttack(IDamagable damagable)
    {
        damagable.Damage(HS_attack_damage, null, 0);
        isAttacking = false;
        canDefend = true;
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
        for (int i = activatedHS_point; i <= currentHS_point; i++)
        {
            vfx.SpawnSlashEffect(health.GetHitPos());
            SoundManager.PlaySound("HS_activate");
            vfx.RumblePulse(0.2f, 0.3f, 0.1f);
            yield return new WaitForSeconds(0.1f);
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

    #endregion Attack

    #region Defend

    public void OnDefend()
    {
        if (canDefend && !controller.isFloating)
        {
            if (!energy.DefendConsume()) { return; }
            RetreiveBoomerang();
            isDefending = true;
            animSet.Anim_Defend(0);
            anim.Play("defend");
            EndHanging();
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

    #endregion Defend

    #region Storm

    public void OnStorm()
    {
        if (isBoomeranging) { storm.transform.SetParent(_boomerang.transform, false); storm.transform.localPosition = Vector3.zero; }
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
            combatTimer = 2;
            stormReady = false;
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

    #endregion Storm

    #region Boomerang

    public void CheckBoomerang()
    {
        if (canLaunchBoomerang && !isAttacking && !isBoomeranging && !controller.isFloating)
        {
            if (!energy.BoomerangConsume()) return;
            LaunchBoomerang();
            canLaunchBoomerang = false;
            isBoomeranging = true;
        }
        else if (isBoomeranging && !_boomerang.isQTE)
        {
            if (_boomerang.stickedInToWall)
            {
                //enter hanging
                isHanging = true;
                rb.velocity = Vector3.zero;
                controller.canMove = false;
                controller.canDoubleJump = true;
                controller.canJump = true;
                controller.isGrounded = true;
                controller.isJumping = false;
                rb.isKinematic = true;
                //anim.setBool
                anim.SetBool("isHanging", true);
                if (_boomerang.facingRight == controller.FacingRight)
                {
                    controller.Flip();
                }
            }
            else
            {
                anim.SetTrigger("teleport");
            }
            _boomerang._End();
            EndBoomerang();
        }
    }

    public void TeleportToSword()
    {
        RaycastHit2D hit = Physics2D.Raycast(_boomerang.transform.position, Vector2.down, 2.2f, teleportCheckLayer);
        RaycastHit2D hit_horizontal = Physics2D.Raycast(_boomerang.transform.position, _boomerang.transform.right, 0.7f, teleportCheckLayer);
        float offset_y = 0f;
        float offset_x = 0f;
        if (hit.collider != null)
        {
            offset_y = 2.2f - hit.distance;
        }
        if (hit_horizontal.collider != null)
        {
            offset_x = hit_horizontal.distance + 0.1f;
            if (!controller.FacingRight)
            {
                offset_x = -hit_horizontal.distance - 0.1f;
            }
        }
        rb.velocity = Vector3.zero;
        transform.position = _boomerang.transform.position + new Vector3(offset_x, offset_y - 2.2f, 0);
    }

    public void EndHanging()
    {
        if (isHanging)
        {
            anim.SetBool("isHanging", false);
            rb.isKinematic = false;
            isHanging = false;
            rb.isKinematic = false;
            controller.canMove = true;
        }
    }

    public void RetreiveBoomerang()
    {
        if (_boomerang.isActiveAndEnabled)
        {
            _boomerang.BounceQTEUI.SetActive(false);
            if (CameraManager.IsActiveCamera(_boomerang.cam))
            {
                if (CameraManager.beforeActiveCam == _boomerang.cam | CameraManager.beforeActiveCam == null)
                {
                    CameraManager.instance.SwtichToNormalCam();
                }
                else
                {
                    CameraManager.SwitchBounceQTECamera(CameraManager.beforeActiveCam);
                }
            }
            storm.transform.SetParent(this.transform, false);
            storm.transform.localPosition = originalStormPos;
            _boomerang._End();
            EndBoomerang();
        }
    }

    public void LaunchBoomerang()
    {
        EndHanging();
        if (_boomerang.isActiveAndEnabled && _boomerang.stickedInToWall) { RetreiveBoomerang(); }
        if (isOnStorm)
        {
            storm.transform.SetParent(_boomerang.transform, false); storm.transform.localPosition = Vector3.zero;
        }
        rb.isKinematic = false;
        controller.canMove = true;
        SoundManager.PlaySound("sword_launch");
        anim.SetBool("isBoomerang", true);

        float offset = 0f;
        RaycastHit2D hit_horizontal = Physics2D.Raycast(launchPosition.position, transform.right, 1.3f, teleportCheckLayer);
        if (hit_horizontal.collider != null)
        {
            offset = 1.3f - hit_horizontal.distance;
        }
        _boomerang.transform.position = launchPosition.position - transform.right * offset;
        _boomerang.transform.rotation = Quaternion.identity;
        _boomerang.gameObject.SetActive(true);
        _boomerang.flyingTimer = 0f;
        _boomerang.SetUp(boomerange_speed, boomerange_startDecreaseTime, boomerange_decreaseSpeed, boomerange_rotationSpeed);
        if (!controller.FacingRight)
        {
            _boomerang.transform.rotation = Quaternion.Euler(0f, 0f, 180f);
        }
        _boomerang.facingRight = controller.FacingRight;
    }

    public void EndBoomerang()
    {
        anim.SetBool("isBoomerang", false);
        storm.transform.SetParent(this.transform, false);
        storm.transform.localPosition = originalStormPos;
        canLaunchBoomerang = true;
        isBoomeranging = false;
    }

    #endregion Boomerang

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
}