using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EditorAttributes;
using System.Text.RegularExpressions;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.Windows;
using System.Threading;
using UnityEngine.U2D;

public class WaterBossAI : IEnemyController
{
    public bool Actions;

    [ShowField(nameof(Actions))][SerializeField, ButtonField("test_range_attack_long", "range_attack_long")] private Void void1;
    [ShowField(nameof(Actions)), ButtonField("test_range_attack_short", "range_attack_short"), SerializeField] private Void void2;
    [ShowField(nameof(Actions))][SerializeField, ButtonField("test_melee_attack_long", "melee_attack_long")] private Void void3;
    [ShowField(nameof(Actions))][SerializeField, ButtonField("test_melee_attack_short", "melee_attack_short")] private Void void4;

    [SerializeField, ShowField(nameof(Actions))] public IEnemyAction range_attack_long;
    [SerializeField, ShowField(nameof(Actions))] public IEnemyAction range_attack_short;
    [SerializeField, ShowField(nameof(Actions))] public IEnemyAction melee_attack_long;
    [SerializeField, ShowField(nameof(Actions))] public IEnemyAction melee_attack_short;
    [SerializeField, ShowField(nameof(Actions))] public IEnemyAction idle;
    [SerializeField, ShowField(nameof(Actions))] public IEnemyAction stun;
    [SerializeField, ShowField(nameof(Actions))] public IEnemyAction unstuckCorner;

    [HideInInspector] public Transform target;
    [HideInInspector] public Vector3 targetPos;

    [Header("stuck corner")] public Transform leftCorner;
    public Transform rightCorner;
    public Transform mid_of_room;
    public float cornerTimer = 0f;
    public float maxCornerTime = 6f;

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
        target = PlayerAttack.instance.gameObject.transform;
        flash = GetComponent<DamageFlash>();
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    private void Update()
    {
        if (DEAD) { return; }
        distanceToPlayer = Vector3.Distance(transform.position, GameManager.instance.Player.transform.position);
        if (target != null) targetPos = target.position;
        CheckCornerStuck();
        if (cornerTimer >= maxCornerTime) { nextAction = unstuckCorner; }
        if (IN_COMBAT)
        {
            if (canFlip)
            {
                if (targetPos.x >= this.transform.position.x && !isFacingRight) { Flip(); }//face right
                else if (targetPos.x < this.transform.position.x && isFacingRight) { Flip(); } //face left
            }

            Move();
        }
    }

    public void CheckCornerStuck()
    {
        float to_left = Vector2.Distance(leftCorner.position, transform.position);
        float to_right = Vector2.Distance(rightCorner.position, transform.position);
        float to_middle = Vector2.Distance(mid_of_room.position, transform.position);
        float to_player = Vector2.Distance(GameManager.instance.Player.transform.position, transform.position);

        if (to_middle <= 2f)
        {
            if (target != GameManager.instance.Player.transform)
            {
                canMove = false;
                target = GameManager.instance.Player.transform;
                inAct = false;
                NextAction(idle);
            }
        }
        else if (to_player < 6f)
        {
            if (to_left <= 3f || to_right <= 3f)
            {
                cornerTimer += Time.deltaTime;
            }
            else
            {
                if (cornerTimer > 0)
                {
                    cornerTimer -= Time.deltaTime;
                }
            }
        }
    }

    public void Move()
    {
        Vector2 targetVelocity = Vector2.zero;
        if (canMove)
        {
            targetVelocity = new Vector2(speed, rb.velocity.y) * transform.right;
        }
        anim.SetBool("isMoving", canMove);
        rb.velocity = Vector2.SmoothDamp(rb.velocity, targetVelocity, ref m_Velocity, 0.1f);
    }

    public void SetCombat(bool isCombat)
    {
        if (DEAD) { return; }
        HealthUI.SetActive(isCombat);
        if (!IN_COMBAT && AIActivate)
        {
            idle.Act();
        }
        IN_COMBAT = isCombat;
    }

    public void Stun(int stunAmount)
    {
        currentStun += stunAmount;
        if (currentStun >= maxStun)
        {
            currentStun = 0;
            CancelAllActions();
            inAct = true;
            stun.Act();
        }
    }

    public override int Damage(int damageAmount, Transform sender, float stunDuration = 0)
    {
        if (DEAD) { return 0; }
        Stun(1);
        flash.OnDamageFlash();
        currentHealth -= damageAmount;
        healthBar.fillAmount = (float)currentHealth / (float)maxHealth;
        if (currentHealth <= 0)
        {
            Invoke("Death", 3f);
            DEAD = true;
            rb.velocity = Vector3.zero;
            rb.gravityScale = 0f;
            rb.isKinematic = true;
            GetComponent<BoxCollider2D>().enabled = false;
            HealthUI.SetActive(false);
            gameObject.layer = 0;
            CancelAllActions();

            anim.Play("death");
        }
        return 0;
    }

    private void Death()
    {
        Health.instance.DEATH?.Invoke();
    }

    public void Flip()
    {
        isFacingRight = !isFacingRight;
        transform.Rotate(new Vector3(0, 1, 0), 180);
    }

    public override void NextAction(IEnemyAction previousAciton)
    {
        if (inAct) { return; }
        inAct = true;
        if (nextAction != null)
        {
            nextAction.Act();
            nextAction = null;

            return;
        }
        if (distanceToPlayer <= distanceThresholdForRangeAttack)//melee attack
        {
            if (previousAciton == melee_attack_long) { melee_attack_short.Act(); }
            else if (previousAciton == melee_attack_short) { melee_attack_long.Act(); }
            else
            {
                bool b = RandomIn_One_Hunderd(70);
                if (b) { melee_attack_short.Act(); }
                else { melee_attack_long.Act(); }
            }
        }
        else//range attack
        {
            bool b = RandomIn_One_Hunderd(50);
            if (previousAciton == range_attack_long) { if (b) { range_attack_short.Act(); } else { melee_attack_long.Act(); } }
            else if (previousAciton == range_attack_short) { if (b) { range_attack_long.Act(); } else { melee_attack_long.Act(); } }
            else
            {
                int i = Random.Range(0, 3);
                if (i == 0) { range_attack_short.Act(); }
                else if (i == 1) { range_attack_long.Act(); }
                else { melee_attack_long.Act(); }
            }
        }
    }

    public void CancelAllActions()
    {
        inAct = false;
        canFlip = true;
        sprite.material.SetFloat("_OutLine", 0);
        range_attack_long.CancelAct();
        range_attack_short.CancelAct();
        melee_attack_long.CancelAct();
        melee_attack_short.CancelAct();
        idle.CancelAct();
        stun.CancelAct();
        sprite.material.SetFloat("_OutLine", 0);
    }

    public bool RandomIn_One_Hunderd(int probablity)
    {
        int i = Random.Range(0, 101);
        return i <= probablity;
    }

    public void test_range_attack_long()
    {
        nextAction = range_attack_long;
    }

    public void test_range_attack_short()
    {
        nextAction = range_attack_short;
    }

    public void test_melee_attack_long()
    {
        nextAction = melee_attack_long;
    }

    public void test_melee_attack_short()
    {
        nextAction = melee_attack_short;
    }
}