using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QX_energy_swords : IEnemyAction
{
    private QianXiaoAI bossAI;

    public Transform shoot_pos1;
    public Transform shoot_pos2;
    public Transform shoot_pos3;
    public Transform shoot_pos4;
    public Transform shoot_pos5;
    public Transform shoot_pos6;

    public int damage_white;
    public float speed_white;
    public int damage_red;
    public float speed_red;
    private List<IProjectile> projs = new List<IProjectile>();

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
        foreach (IProjectile i in projs)
        {
            i.Die();
        }
        projs.Clear();
    }

    public IEnumerator S1_Act()
    {
        anim.Play("S1_energy_swords");
        StartCoroutine(shootSword_White(shoot_pos1.position));
        yield return new WaitForSeconds(0.5f);
        StartCoroutine(shootSword_White(shoot_pos2.position));
        yield return new WaitForSeconds(0.25f);
        StartCoroutine(shootSword_White(shoot_pos3.position));
        yield return new WaitForSeconds(0.5f);
        StartCoroutine(shootSword_Red(shoot_pos4.position));
        yield return new WaitForSeconds(action_time);
        anim.Play("S1_Idle");
        if (bossAI.inAct)
        {
            bossAI.inAct = false;
            bossAI.NextAction(this);
        }
        yield return null;
    }

    public IEnumerator S2_Act()
    {
        bossAI.targetPos = transform.position + new Vector3(0, 5, 0);
        yield return new WaitForSeconds(0.2f);
        anim.Play("S2_energy_swords");
        StartCoroutine(shootSword_White(shoot_pos2.position));
        yield return new WaitForSeconds(0.5f);
        StartCoroutine(shootSword_White(shoot_pos3.position));
        yield return new WaitForSeconds(0.25f);
        StartCoroutine(shootSword_White(shoot_pos5.position));
        yield return new WaitForSeconds(0.25f);
        StartCoroutine(shootSword_White(shoot_pos6.position));
        yield return new WaitForSeconds(0.5f);
        StartCoroutine(shootSword_Red(shoot_pos1.position));
        yield return new WaitForSeconds(0.25f);
        StartCoroutine(shootSword_Red(shoot_pos4.position));
        yield return new WaitForSeconds(action_time);
        anim.Play("S2_Idle");
        isThisActing = false;
        bossAI.CancelAllActions();
        bossAI.add_land_attack.Act();
        bossAI.add_land_attack.GetComponent<QX_add_land_attack>().actionSender = this;
        yield return null;
    }

    public IEnumerator shootSword_White(Vector3 pos)
    {
        EnergySword i = pooler.SpawnFromPool("energy_sword", pos, Quaternion.identity).GetComponent<EnergySword>();
        i.GetComponent<SpriteRenderer>().color = Color.white;
        i.GetComponent<TrailRenderer>().startColor = Color.white;
        i.GetComponent<TrailRenderer>().endColor = Color.white;
        i.isRed = false;
        i.SetUp(transform.right, this.gameObject, 0, _followTarget: true, _target: playerIDamagable, false, damage_white, 0);
        projs.Add(i);
        yield return new WaitForSeconds(2f);
        i.GetComponent<Animator>().Play("energy_sword_shooting");
        i.facingRight = bossAI.isFacingRight;
        yield return new WaitForSeconds(0.2f);
        projs.Remove(i);
        SoundManager.PlaySound("qianxiao_throw_energy_sword_W");
        i.SetUp(transform.right, this.gameObject, 0, _followTarget: true, _target: playerIDamagable, true, damage_white, speed_white);
        yield return null;
    }

    public IEnumerator shootSword_Red(Vector3 pos)
    {
        EnergySword i = pooler.SpawnFromPool("energy_sword", pos, Quaternion.identity).GetComponent<EnergySword>();
        i.GetComponent<SpriteRenderer>().color = Color.red;
        i.GetComponent<TrailRenderer>().startColor = Color.red;
        i.GetComponent<TrailRenderer>().endColor = Color.red;
        i.isRed = true;
        i.SetUp(transform.right, this.gameObject, 0, _followTarget: true, _target: playerIDamagable, false, damage_white, 0);
        projs.Add(i);
        yield return new WaitForSeconds(3f);
        i.GetComponent<Animator>().Play("energy_swordred_shooting");
        yield return new WaitForSeconds(0.75f);
        vfx.SpawnSlashEffect(i.transform.position, true);
        i.facingRight = bossAI.isFacingRight;
        yield return new WaitForSeconds(0.2f);
        projs.Remove(i);
        SoundManager.PlaySound("qianxiao_throw_energy_sword_R");
        i.SetUp(transform.right, this.gameObject, 0, _followTarget: false, _target: playerIDamagable, true, damage_red, speed_red);
        yield return null;
    }
}