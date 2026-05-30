using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class LargeEnergySword : IProjectile
{
    //private Animator anim;
    //public bool facingRight;
    //[HideInInspector] public QX_throw_sword QX_throw_sword;
    //public bool isS2 = false;
    //[HideInInspector] public QianXiaoAI qianXiaoAI;
    //private Transform player;
    //public float explodeRange = 4f;
    //public float counterTime = 1.5f;
    //public float counterTimer = 0f;
    //private LayerMask qianxiaoLayer = 1 << 13;

    //// Start is called before the first frame update
    //public override void Start()
    //{
    //    base.Start();
    //    anim = GetComponent<Animator>();
    //    player = GameManager.instance.player.transform;
    //    qianXiaoAI = QianXiaoAI.instance;
    //    QX_throw_sword = qianXiaoAI.GetComponent<QX_throw_sword>();
    //}

    //public override void NormalCounterAttack()
    //{
    //    base.NormalCounterAttack();
    //    speed += 20;
    //    counterTimer = 0;
    //}

    //public override void PerfectCounterAttack()
    //{
    //    base.PerfectCounterAttack();
    //    speed += 20;
    //    counterTimer = 0;
    //}

    //public override void Update()
    //{
    //    if (QX_throw_sword == null) { QX_throw_sword = qianXiaoAI.GetComponent<QX_throw_sword>(); }
    //    if (!transform.gameObject.activeInHierarchy) { return; }
    //    if (!isS2 && transform.position.x - player.position.x <= 0.4f && player.position.y > transform.position.y)
    //    {
    //        QX_throw_sword.TeleportAttack();
    //        Die();
    //    }

    //    if (isS2 && !collided && !isHostileToPlayer)
    //    {
    //        if (rb.velocity != Vector2.zero)
    //        {
    //            counterTimer += Time.deltaTime;
    //            if (counterTimer >= counterTime)
    //            {
    //                counterTimer = 0f;
    //                StartCoroutine(CounterAttack());
    //            }
    //        }
    //    }

    //    if (collided) { return; }
    //    lifeTimer += Time.deltaTime;
    //    if (lifeTimer >= lifeTime)
    //    {
    //        Die();
    //        QX_throw_sword.EndAction();
    //    }
    //    if (target != null)
    //    {
    //        if (followTarget)
    //        {
    //            transform.rotation = Quaternion.RotateTowards(transform.rotation, CalculateWantedRotation(target.GetHitPos()), rotationSpeed * Time.deltaTime);
    //        }//follow target
    //    }
    //    rb.velocity = transform.right * speed / 10;
    //}

    //public IEnumerator CounterAttack()
    //{
    //    RaycastHit2D[] hit = Physics2D.RaycastAll(transform.position, transform.right, 40f, qianxiaoLayer);
    //    Debug.DrawLine(transform.position, transform.right * 40f + transform.position, Color.blue, 10f);
    //    bool _t = false;
    //    foreach (RaycastHit2D i in hit)
    //    {
    //        if (i.collider != null && i.collider.gameObject == qianXiaoAI.gameObject)
    //        {
    //            _t = true;
    //        }
    //    }
    //    if (!_t)
    //    {
    //        isHostileToPlayer = true;
    //        qianXiaoAI.anim.Play("S2_teleport");
    //        yield return new WaitForSeconds(0.2f);
    //        QX_throw_sword.teleportAirCounterAttack(qianXiaoAI.transform.position = transform.position + transform.right * speed / 10 * Time.deltaTime);
    //        yield return null;
    //    }
    //    yield return null;
    //}

    //public override void OnTriggerEnter2D(Collider2D collision)
    //{
    //    IDamagable target = collision.gameObject.GetComponent<IDamagable>();
    //    if (target != null && !collided)
    //    {
    //        if (isHostileToPlayer && collision.gameObject == gameManager.player)
    //        {
    //            QX_throw_sword.TeleportThrust();
    //            Die();
    //        }
    //        else if (!isHostileToPlayer && collision.gameObject == qianXiaoAI.gameObject)
    //        {
    //            vfx.SpawnSlashEffect(GetPivot());
    //            QX_throw_sword.CheckCounterAttack();
    //        }
    //    }
    //    else if (collision.gameObject != owner && (stopLayer.value & (1 << collision.gameObject.layer)) > 0)
    //    {
    //        rb.velocity = Vector3.zero;
    //        speed = 0f;
    //        collided = true;
    //        anim.Play("explode");
    //        QX_throw_sword.EndAction();
    //        Invoke("Die", 0.67f);
    //    }
    //}

    //public void Explode()
    //{
    //    Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, explodeRange);
    //    foreach (Collider2D collider in colliders)
    //    {
    //        if (collider.gameObject.layer == 6)
    //        {
    //            vfx.SpawnSlashEffect(player.GetComponent<Health>().GetHitPos(), true);
    //            GameManager.instance.playerhealth.Damage(damage, transform, stunDuration);
    //        }
    //    }
    //}

    //public override void HitByHSAttack()
    //{
    //    Die();
    //}

    //public override void HitByMeleeAttack()
    //{
    //    Die();
    //}

    //public override void Hit()
    //{
    //    Die();
    //}
    public override void Hit()
    {
        throw new System.NotImplementedException();
    }

    public override void HitByHSAttack()
    {
        throw new System.NotImplementedException();
    }

    public override void HitByMeleeAttack()
    {
        throw new System.NotImplementedException();
    }
}