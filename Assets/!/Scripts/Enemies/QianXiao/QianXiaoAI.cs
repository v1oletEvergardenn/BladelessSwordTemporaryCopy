using EditorAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class QianXiaoAI : IEnemyController
{
    public bool Actions;
    public static QianXiaoAI instance;

    //[ShowField(nameof(Actions)), ButtonField("NextAction", "NextAction"), SerializeField] private Void void13;
    [ShowField(nameof(Actions))][SerializeField, ButtonField("test_stun", "Stun")] private Void void1;

    [ShowField(nameof(Actions)), ButtonField("test_death", "death"), SerializeField] private Void void2;
    [ShowField(nameof(Actions)), ButtonField("test_S1_to_S2", "S1_to_S2"), SerializeField] private Void void3;
    [ShowField(nameof(Actions)), ButtonField("test_slash", "slash"), SerializeField] private Void void4;
    [ShowField(nameof(Actions)), ButtonField("test_energy_swords", "energy_swords"), SerializeField] private Void void5;
    [ShowField(nameof(Actions)), ButtonField("test_summon_projectile", "summon_projectile"), SerializeField] private Void void6;
    [ShowField(nameof(Actions)), ButtonField("test_jump_attack", "jump_attack"), SerializeField] private Void void7;
    [ShowField(nameof(Actions)), ButtonField("test_throw_sword", "throw_sword"), SerializeField] private Void void8;
    [ShowField(nameof(Actions)), ButtonField("test_add_repel", "add_repel"), SerializeField] private Void void9;
    [ShowField(nameof(Actions)), ButtonField("test_add_teleport_up_attack", "add_teleport_up_attack"), SerializeField] private Void void10;
    [ShowField(nameof(Actions)), ButtonField("test_add_land_attack", "add_land_attack"), SerializeField] private Void void11;
    [ShowField(nameof(Actions)), ButtonField("test_add_thrust", "add_thrust"), SerializeField] private Void void12;

    [SerializeField, ShowField(nameof(Actions))] public IEnemyAction idle;
    [SerializeField, ShowField(nameof(Actions))] public IEnemyAction stun;
    [SerializeField, ShowField(nameof(Actions))] public IEnemyAction stageTransfer;
    [SerializeField, ShowField(nameof(Actions))] public IEnemyAction slash;
    [SerializeField, ShowField(nameof(Actions))] public IEnemyAction energy_swords;
    [SerializeField, ShowField(nameof(Actions))] public IEnemyAction summon_projectile;
    [SerializeField, ShowField(nameof(Actions))] public IEnemyAction jump_attack;
    [SerializeField, ShowField(nameof(Actions))] public IEnemyAction throw_sword;
    [SerializeField, ShowField(nameof(Actions))] public IEnemyAction add_repel;
    [SerializeField, ShowField(nameof(Actions))] public IEnemyAction add_teleport_up_attack;
    [SerializeField, ShowField(nameof(Actions))] public IEnemyAction add_land_attack;
    [SerializeField, ShowField(nameof(Actions))] public IEnemyAction add_thrust;

    [HideInInspector] public Vector3 targetPos;
    [HideInInspector] public Vector3 nullTargetPos = new Vector3(-99, -99, -99);
    private bool deadAnimPlayed = false;
    public float stage2Speed = 25;
    public int stage2MaxHealth = 120;
    [HideInInspector] public float oriGravity;
    [HideInInspector] public bool isStage2 = false;
    [HideInInspector] public bool isStunning = false;
    public Transform leftCorner;
    public Transform rightCorner;

    public float slashRange = 10f;

    public override void Start()
    {
        base.Start();
        targetPos = nullTargetPos;
        oriGravity = rb.gravityScale;
        stunBar.UpdateBar(currentStun);
    }

    public void Awake()
    {
        if (instance == null) { instance = this; }
    }

    // Update is called once per frame
    private void Update()
    {
        if (!deadAnimPlayed && DEAD && isGrounded)
        {
            deadAnimPlayed = true;
            anim.Play("S2_death");
            Invoke("Death", 3f);
        }
        if (deadAnimPlayed) { return; }
        GroundCheck();
        distanceToPlayer = Vector3.Distance(transform.position, GameManager.instance.player.transform.position);

        if (IN_COMBAT)
        {
            if (canFlip)
            {
                float x = 0;
                if (targetPos == nullTargetPos) { x = player.position.x; }
                else { x = targetPos.x; }
                if (x >= this.transform.position.x && !isFacingRight) { Flip(); }//face right
                else if (x < this.transform.position.x && isFacingRight) { Flip(); } //face left
            }

            Move();
        }
    }

    public override IEnemyAction NextAction()
    {
        IEnemyAction previousAciton = null;

        if (DEAD) return null;
        inAct = true;
        canFlip = true;
        canMove = false;
        //if (nextAction != null) { nextAction.Act(); nextAction = null; return null; }
        if (previousAciton == slash)
        {
            if (distanceToPlayer <= slashRange) { jump_attack.Act(); return null; }
            else { energy_swords.Act(); return null; }
        }
        else if (previousAciton == jump_attack)
        {
            if (playerEnergy.currentEnergy <= 6) { summon_projectile.Act(); return null; }
            else
            {
                bool i = RandomIn_One_Hunderd(70);
                if (i) { energy_swords.Act(); return null; }
                else { slash.Act(); return null; }
            }
        }
        else if (previousAciton == energy_swords)
        {
            if (playerEnergy.currentEnergy <= 6) { summon_projectile.Act(); return null; }
            else
            {
                bool i = RandomIn_One_Hunderd(50);
                if (i) { slash.Act(); return null; }
                else { throw_sword.Act(); return null; }
            }
        }
        else if (previousAciton == summon_projectile)
        {
            if (distanceToPlayer <= slashRange) { slash.Act(); return null; }
            else { energy_swords.Act(); return null; }
        }
        else if (previousAciton == throw_sword)
        {
            if (distanceToPlayer <= slashRange) { slash.Act(); return null; }
            else { summon_projectile.Act(); return null; }
        }
        else
        {
            if (distanceToPlayer <= slashRange) { slash.Act(); return null; }
            else
            {
                bool i = RandomIn_One_Hunderd(70);
                if (i) { energy_swords.Act(); return null; }
                else { throw_sword.Act(); return null; }
            }
        }
        //next action
    }

    public override int Damage(float damageAmount, Transform sender, float stunDuration = 0, bool damageFlash = true, float stunValue = 0)
    {
        if (DEAD) { return 0; }
        Stun(1);
        flash.OnDamageFlash();
        currentHealth -= damageAmount;
        healthBar.UpdateBar(currentHealth);
        if (!isStage2)
        {
            if (currentHealth <= 0)
            {
                //nextAction = stageTransfer;
            }
        }
        else
        {
            if (currentHealth <= 0)
            {
                DEAD = true;
                rb.velocity = Vector3.zero;
                HealthUI.SetActive(false);
                gameObject.layer = 17;
                CancelAllActions();
                anim.Play("S2_Lose_power");
            }
        }
        return 0;
    }

    public void Death()
    {
        Health.instance.DEATH?.Invoke();
    }

    public void Stage2HealthChange()
    {
        maxHealth = stage2MaxHealth;
        currentHealth = maxHealth;
        healthBar.UpdateBar(currentHealth);
    }

    public void Move()
    {
        Vector2 targetVelocity = Vector2.zero;
        if (canMove)
        {
            if (targetPos == nullTargetPos)
            {
                if (isStage2)
                {
                    targetVelocity = new Vector2(stage2Speed, rb.velocity.y) * transform.right;
                }
                else
                {
                    targetVelocity = new Vector2(speed, rb.velocity.y) * transform.right;
                }
            }
        }
        else if (targetPos != nullTargetPos)
        {
            float f_speed = 0f;
            if (isStage2) f_speed = stage2Speed;
            else f_speed = speed;

            transform.position = Vector3.MoveTowards(transform.position, targetPos, f_speed * Time.deltaTime);
            if (Vector3.Distance(transform.position, targetPos) <= 1f)
            {
                targetPos = nullTargetPos;
            }
        }
        anim.SetBool("moving", canMove);
        //rb.velocity = Vector2.SmoothDamp(rb.velocity, targetVelocity, ref m_Velocity, 0.1f);
    }

    public void SetCombat(bool isCombat)
    {
        if (DEAD) { return; }
        HealthUI.SetActive(isCombat);

        if (!isCombat)
        {
            //nextAction = null;
            CancelAllActions();
        }
        IN_COMBAT = isCombat;
    }

    public void Teleport(Vector3 pos)
    {
        pos = new Vector3(Mathf.Clamp(pos.x, leftCorner.position.x, rightCorner.position.x), pos.y, pos.z);
        transform.position = pos;
    }

    public void Stun(int stunAmount)
    {
        currentStun += stunAmount;
        stunBar.UpdateBar(currentStun);
        if (currentStun >= maxStun)
        {
            currentStun = 0;
            CancelAllActions();
            inAct = true;
            stun.Act();
        }
    }

    public void CancelAllActions()
    {
        inAct = false;
        targetPos = nullTargetPos;
        sprite.material.SetFloat("_OutLine", 0);
        idle.CancelAct();
        stun.CancelAct();
        stageTransfer.CancelAct();
        slash.CancelAct();
        energy_swords.CancelAct();
        summon_projectile.CancelAct();
        jump_attack.CancelAct();
        throw_sword.CancelAct();
        add_repel.CancelAct();
        add_teleport_up_attack.CancelAct();
        add_land_attack.CancelAct();
    }

    public bool RandomIn_One_Hunderd(int probablity)
    {
        int i = Random.Range(0, 101);
        return i <= probablity;
    }

    public void Flip()
    {
        isFacingRight = !isFacingRight;
        transform.Rotate(new Vector3(0, 1, 0), 180);
    }

    public void SetFlyEngine(bool i)
    {
        if (i)
        {
            rb.gravityScale = 0f;
        }
        else
        {
            rb.gravityScale = oriGravity;
        }
    }

    #region ABILITIES TEST

    //public void test_slash()
    //{
    //    print("test_slash");
    //    if (slash != null) { nextAction = slash; }
    //}

    //public void test_energy_swords()
    //{
    //    print("test_energy_swords");
    //    if (energy_swords != null) { nextAction = nextAction = energy_swords; }
    //}

    //public void test_summon_projectile()
    //{
    //    print("test_summon_projectile");
    //    if (summon_projectile != null) { nextAction = nextAction = summon_projectile; }
    //}

    //public void test_jump_attack()
    //{
    //    print("test_jump_attack");
    //    if (jump_attack != null) { nextAction = nextAction = jump_attack; }
    //}

    //public void test_throw_sword()
    //{
    //    print("test_throw_sword");
    //    if (throw_sword != null) { nextAction = nextAction = throw_sword; }
    //}

    //public void test_add_repel()
    //{
    //    print("test_add_repel");
    //    if (add_repel != null) { nextAction = nextAction = add_repel; }
    //}

    //public void test_add_teleport_up_attack()
    //{
    //    print("test_add_teleport_up_attack");
    //    if (add_teleport_up_attack != null) { nextAction = nextAction = add_teleport_up_attack; }
    //}

    //public void test_add_land_attack()
    //{
    //    print("test_add_land_attack");
    //    if (add_land_attack != null) { nextAction = nextAction = add_land_attack; }
    //}

    //public void test_S1_to_S2()
    //{
    //    print("test_S1_to_S2");
    //    if (stageTransfer != null) { nextAction = nextAction = stageTransfer; }
    //}

    public void test_stun()
    {
        print("test_stun");
        Stun(1);
    }

    public void test_death()
    {
        print("test_death");
        Damage(maxHealth, null, 0);
    }

    public void test_add_thrust()
    {
        print("test_add_thrust");
        //nextAction = add_thrust;
    }

    #endregion ABILITIES TEST

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(GetHitPos(), slashRange);
    }
}