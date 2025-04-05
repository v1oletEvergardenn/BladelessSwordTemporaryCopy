using Cinemachine;
using EditorAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WB_melee_attack_long : IEnemyAction
{
    private WaterBossAI bossAI;

    [Space(10)] public int damageAmount = 10;

    public float stunDuration = 0.2f;

    [SerializeField] public float rumbleDuration = 0.1f;
    [SerializeField, MinMaxSlider(0, 3f)] public Vector2 rumbleFrequncy = new Vector2(0.25f, 0.4f);
    public float freezeTimeDuration = 0.2f;
    public float RepelForce = 20f;

    [Space(10)] public float minDistance_attack;
    public float time_after_reach_min_distance = 0.5f;
    private bool reachedInRange = false;
    private bool startedAct = false;
    private bool startedIEAttack = false;

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
        bossAI = GetComponent<WaterBossAI>();
    }

    private void Update()
    {
        if (!reachedInRange && startedAct)
        {
            Vector3 v = player.transform.position - transform.position;
            float d = v.magnitude;
            if (d <= minDistance_attack + 4f)//enough ditance
            {
                //stop check moving
                bossAI.canMove = false;
                if (!startedIEAttack)
                {
                    StartCoroutine(Act_coroutine());
                }
            }
            else
            {
                bossAI.canMove = true;
            }
        }
    }

    public override void Act()
    {
        bossAI.canMove = true;
        startedAct = true;
        reachedInRange = false;
        startedIEAttack = false;
    }

    public override IEnumerator Act_coroutine()
    {
        startedIEAttack = true;
        anim.Play("melee_long_attack");
        yield return new WaitForSeconds(time_after_reach_min_distance);
        //stop moving
        bossAI.canFlip = false;
        reachedInRange = true;
        bossAI.canMove = false;
        startedAct = false;
        yield return new WaitForSeconds(2f - time_after_reach_min_distance);
        bossAI.canFlip = true;
        if (bossAI.inAct)
        {
            bossAI.inAct = false;
            bossAI.NextAction();
        }
        yield return null;
    }

    public void Hit()
    {
        int dealtDamage = playerIDamagable.DamageFromMeleeAttack(transform, damageAmount, stunDuration);

        if (dealtDamage == 2)//counter attack
        {
            //counter attack effect
            vfx.SpawnHitEffect(true, playerIDamagable.hitEffectPosition.position);

            playerIDamagable.Repel(RepelForce, transform.right);
            vfx.CameraShake(0.2f);
            vfx.RumblePulse(rumbleFrequncy.x * 2, rumbleFrequncy.y * 2, rumbleDuration * 2);
            vfx.SlowTimeForSeconds(freezeTimeDuration, 0f);
        }
        else if (dealtDamage == 1)
        {
            vfx.SpawnSlashEffect(playerIDamagable.GetHitPos());
            playerIDamagable.Repel(RepelForce, transform.right);
            vfx.CameraShake(0.2f);
            vfx.RumblePulse(rumbleFrequncy.x * 2, rumbleFrequncy.y * 2, rumbleDuration * 2);
            vfx.SlowTimeForSeconds(freezeTimeDuration, 0f);
        }
        else if (dealtDamage == 0)
        {
            vfx.SpawnSlashEffect(playerIDamagable.GetHitPos());
            vfx.RumblePulse(rumbleFrequncy.x, rumbleFrequncy.y, rumbleDuration);
            playerIDamagable.Repel(RepelForce * 2, transform.right);
            vfx.SlowTimeForSeconds(freezeTimeDuration, 0f);
        }
    }
}