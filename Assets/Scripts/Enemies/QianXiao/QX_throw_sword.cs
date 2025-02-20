using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static ObjectPooler;

public class QX_throw_sword : IEnemyAction
{
    private QianXiaoAI bossAI;
    public Transform pos;
    public int damage;
    public float speed;

    public LargeEnergySword proj;
    private int teleportAirCounterTime = 0;
    private int counterAttackTime = 0;

    public override void Start()
    {
        base.Start();
        bossAI = GetComponent<QianXiaoAI>();
        pooler = ObjectPooler.instance;
    }

    public override void Act()
    {
        if (!bossAI.isStage2) { StartCoroutine(S1_Act()); }
        else { StartCoroutine(S2_Act()); }
    }

    public override void CancelAct()
    {
        base.CancelAct();
        if (proj != null)
        {
            proj.Die();
        }
    }

    public IEnumerator S1_Act()
    {
        teleportAirCounterTime = 0;
        counterAttackTime = 0;
        isThisActing = true;
        anim.Play("S1_throw_sword 1");
        yield return new WaitForSeconds(2.2f);
        StartCoroutine(S1Shoot());
        yield return null;
    }

    public IEnumerator S2_Act()
    {
        teleportAirCounterTime = 0;
        counterAttackTime = 0;
        isThisActing = true;
        bossAI.targetPos = transform.position + new Vector3(0, 5, 0);
        yield return new WaitForSeconds(0.4f);
        anim.Play("S2_throw_sword 1");
        yield return new WaitForSeconds(2.2f);
        StartCoroutine(S2Shoot());
        yield return null;
    }

    public IEnumerator S2Shoot()
    {
        LargeEnergySword i = pooler.SpawnFromPool("large_energy_sword", pos.position, Quaternion.identity).GetComponent<LargeEnergySword>();
        proj = i;
        i.QX_throw_sword = this;
        i.isS2 = true;
        i.qianXiaoAI = bossAI;
        vfx.SpawnLargeSlashEffect(i.transform.position, true);
        i.SetUp(transform.right, this.gameObject, 0, _followTarget: true, _target: playerIDamagable, false, 0, 0);
        yield return new WaitForSeconds(2f);
        vfx.SpawnSlashEffect(i.transform.position, true);
        i.facingRight = bossAI.isFacingRight;
        yield return new WaitForSeconds(0.2f);
        i.SetUp(transform.right, this.gameObject, 0, _followTarget: false, _target: playerIDamagable, true, damage, speed);
        yield return null;
    }

    public IEnumerator S1Shoot()
    {
        LargeEnergySword i = pooler.SpawnFromPool("large_energy_sword", pos.position, Quaternion.identity).GetComponent<LargeEnergySword>();
        proj = i;
        i.QX_throw_sword = this;
        i.qianXiaoAI = bossAI;
        i.isS2 = false;
        i.qianXiaoAI = bossAI;
        vfx.SpawnLargeSlashEffect(i.transform.position, true);
        i.SetUp(transform.eulerAngles, this.gameObject, _damage: 0, _speed: 0);
        yield return new WaitForSeconds(2f);
        vfx.SpawnSlashEffect(i.transform.position, true);
        i.facingRight = bossAI.isFacingRight;
        yield return new WaitForSeconds(0.2f);
        i.SetUp(transform.eulerAngles, this.gameObject, _damage: damage, _speed: speed);
        yield return null;
    }

    public void TeleportThrust()
    {
        isThisActing = false;
        bossAI.CancelAllActions();
        bossAI.add_thrust.Act();
        bossAI.add_thrust.GetComponent<QX_add_thrust>().actionSender = this;
    }

    public void TeleportAttack()
    {
        isThisActing = false;
        bossAI.CancelAllActions();
        bossAI.add_teleport_up_attack.Act();
        bossAI.add_teleport_up_attack.GetComponent<QX_add_teleport_up_attack>().actionSender = this;
    }

    public IEnumerator CounterAttack()
    {
        if (proj.gameObject.activeInHierarchy)
        {
            proj.GetComponent<Animator>().Play("rotating");

            float s = proj.speed;
            proj.SetUp(transform.eulerAngles, this.gameObject, _damage: 0, _speed: 0);
            yield return new WaitForSeconds(0.6f);
            vfx.SpawnSlashEffect(proj.transform.position, true);
            yield return new WaitForSeconds(0.2f);
            proj.GetComponent<Animator>().Play("normal");
            proj.SetUp(transform.right, this.gameObject, 10, _followTarget: false, _target: playerIDamagable, true, damage, s += 20);
            proj.counterTimer = 0;
            //proj.counterTime -= 0.15f;
        }
    }

    public void teleportAirCounterAttack(Vector3 vector3)
    {
        if (teleportAirCounterTime >= 4)
        {
            proj.Die();
            bossAI.Teleport(vector3);
            isThisActing = false;
            bossAI.CancelAllActions();
            bossAI.add_land_attack.Act();
            bossAI.add_land_attack.GetComponent<QX_add_land_attack>().actionSender = this;
        }
        else
        {
            teleportAirCounterTime++;
            bossAI.Teleport(vector3);
            StartCoroutine(CounterAttack());
        }
    }

    public void CheckCounterAttack()
    {
        bool result = false;
        if (isThisActing)
        {
            if (bossAI.isStage2)
            {
                if (counterAttackTime == 0)
                {
                    result = true;
                }
                else if (counterAttackTime == 1)
                {
                    int i = Random.Range(0, 11);
                    if (i <= 7)
                    {
                        result = true;
                    }
                }
                else if (counterAttackTime == 2)
                {
                    int i = Random.Range(0, 2);
                    if (i == 0) { result = true; }
                }
            }
            else
            {
                if (counterAttackTime == 0)
                {
                    int i = Random.Range(0, 11);
                    if (i <= 5)
                    {
                        result = true;
                    }
                }
                else if (counterAttackTime == 1)
                {
                    int i = Random.Range(0, 11);
                    if (i <= 3)
                    {
                        result = true;
                    }
                }
            }
        }

        if (result)
        {
            StartCoroutine(CounterAttack());
            counterAttackTime++;
        }
        else
        {
            bossAI.Stun(5);
            proj.Die();
            EndAction();
        }
    }

    public void EndAction()
    {
        isThisActing = false;
        bossAI.CancelAllActions();
        bossAI.add_land_attack.GetComponent<QX_add_land_attack>().actionSender = this;
        bossAI.add_land_attack.Act();
    }
}