using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QX_add_teleport_up_attack : IEnemyAction
{
    private QianXiaoAI bossAI;
    public Transform shootPos;
    public int bullet_damage = 3;
    public float bullet_speed = 150f;
    public IEnemyAction actionSender;

    public override void Start()
    {
        base.Start();
        bossAI = GetComponent<QianXiaoAI>();
    }

    public override void Act()
    {
        StartCoroutine(Act_coroutine());
    }

    public override IEnumerator Act_coroutine()
    {
        isThisActing = true;
        bossAI.canFlip = false;
        if (bossAI.isStage2) { anim.Play("S2_add_teleport_attack"); }
        else { anim.Play("S1_add_teleport_attack"); }
        yield return new WaitForSeconds(0.25f);
        transform.position = new Vector3(playerIDamagable.GetHitPos().x + 0.1f, bossAI.leftCorner.position.y);
        yield return new WaitForSeconds(0.5f);
        vfx.SpawnEffectWithEnum(Hit_Effect.slash, transform.position + new Vector3(0, 0.5f, 0f));
        yield return new WaitForSeconds(action_time);
        isThisActing = false;
        bossAI.canFlip = true;
        bossAI.inAct = false;
        if (actionSender != null)
        {
            bossAI.NextAction();
            actionSender = null;
        }
        else
        {
            bossAI.NextAction();
        }
    }

    public void ShootBullet()
    {
        IProjectile proj = ObjectPooler.instance.SpawnFromPool("sword_bullet", shootPos.position).GetComponent<IProjectile>();
        proj.SetUp(new Vector3(0, 0, 90f), this.gameObject, _damage: bullet_damage, _speed: bullet_speed);
    }
}