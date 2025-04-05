using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements.Experimental;

public class QX_jump_attack : IEnemyAction
{
    //private QianXiaoAI bossAI;
    //private Vector2 jumpDir = new Vector2(1, 1);
    //public float jumpForce;
    //private bool falling = false;

    //private bool flying = false;
    //private float flyingTimer;

    //private bool leftGround = false;
    public Transform shootPos;

    public int bulletDamage = 3;
    public float bulletSpeed = 200f;

    public override void Start()
    {
        base.Start();
        //bossAI = GetComponent<QianXiaoAI>();
    }

    //private void FixedUpdate()
    //{
    //    if (!bossAI.isGrounded && !leftGround) { leftGround = true; }
    //    if (flying)
    //    {
    //        flyingTimer += Time.deltaTime;
    //        if (bossAI.isFacingRight) { jumpDir = new Vector2(-2, 0.5f); }
    //        else { jumpDir = new Vector2(2, 0.5f); }
    //        rb.gravityScale = 0f;
    //        rb.velocity = new Vector2(jumpDir.x, (Mathf.Cos(flyingTimer * 2) * Mathf.Rad2Deg) / 45) * jumpForce * Time.deltaTime;
    //    }
    //    if (isThisActing && falling && leftGround)
    //    {
    //        if (bossAI.isGrounded)
    //        {
    //            rb.gravityScale = bossAI.oriGravity;
    //            isThisActing = false;
    //            StartCoroutine(Grounded());
    //        }
    //    }
    //}

    public override void CancelAct()
    {
        base.CancelAct();
        //flying = false;
        rb.velocity = Vector2.zero;
    }

    //public IEnumerator Grounded()
    //{
    //    yield return new WaitForSeconds(0.1f);
    //    flying = false;
    //    StopCoroutine(S1_Act());
    //    StopCoroutine(S2_Act());
    //    if (bossAI.isStage2) { anim.Play("S2_Idle"); bossAI.SetFlyEngine(true); }
    //    else { rb.gravityScale = bossAI.oriGravity; anim.Play("S1_Idle"); }
    //    falling = false;
    //    bossAI.canFlip = true;
    //    yield return new WaitForSeconds(2f);
    //    rb.velocity = Vector3.zero;
    //    if (bossAI.inAct)
    //    {
    //        bossAI.inAct = false;
    //        bossAI.NextAction();
    //    }
    //}

    //public override void Act()
    //{
    //    if (!bossAI.isStage2) { StartCoroutine(S1_Act()); }
    //    else { StartCoroutine(S2_Act()); }
    //}

    //public IEnumerator S1_Act()
    //{
    //    falling = true;
    //    leftGround = false;
    //    isThisActing = true;
    //    bossAI.canFlip = false;
    //    flyingTimer = 0f;
    //    anim.Play("S1_jumpAttack");
    //    flying = true;
    //    rb.gravityScale = 0f;
    //    yield return new WaitForSeconds(action_time);
    //    rb.gravityScale = bossAI.oriGravity;
    //    rb.velocity = Vector3.zero;
    //    flying = false;
    //}

    //public IEnumerator S2_Act()
    //{
    //    falling = true;
    //    leftGround = false;
    //    isThisActing = true;
    //    flyingTimer = 0f;
    //    anim.Play("S2_jumpAttack");
    //    bossAI.canFlip = false;
    //    rb.gravityScale = 0f;
    //    flying = true;
    //    yield return new WaitForSeconds(action_time);
    //    rb.velocity = Vector3.zero;
    //    flying = false;
    //}

    public void ShootSwordBullet()
    {
        IProjectile proj = ObjectPooler.instance.SpawnFromPool("sword_bullet", shootPos.position).GetComponent<IProjectile>();
        proj.SetUp(new Vector3(0, 0, 225), this.gameObject, _target: playerIDamagable, _followTarget: true, _isHostileToPlayer: false, _damage: 0, _speed: 0f);
        StartCoroutine(LaunchSword(proj));
    }

    public IEnumerator LaunchSword(IProjectile proj)
    {
        yield return new WaitForSeconds(1.5f);
        vfx.SpawnSlashEffect(proj.GetPivot());
        yield return new WaitForSeconds(0.2f);
        proj.SetUp(new Vector3(0, 0, 225), this.gameObject, _target: playerIDamagable, _isHostileToPlayer: true, _damage: bulletDamage, _speed: bulletSpeed);
    }
}