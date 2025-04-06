using EditorAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YYF_Swing : IEnemyAction
{
    private YingYangFish_AI bossAI;

    public GameObject swingEffect;

    public MeleeAttack swingAttack = new MeleeAttack(2, 0.5f, 0.2f, new Vector2(0.25f, 0.4f), 0.2f, 20f, 0.1f);
    public float swingRange;
    public float swingAttackDuration;

    public override void Start()
    {
        base.Start();
        bossAI = GetComponent<YingYangFish_AI>();
    }

    public override IEnumerator Act_coroutine()
    {
        yield return bossAI.co_IEcloseSwim = StartCoroutine(bossAI.IECloseSwim(true));

        bossAI.whiteAnim.Play("swing");
        bossAI.blackAnim.Play("swing");
        bossAI.black_idling = false;
        bossAI.white_idling = false;

        yield return new WaitForSeconds(1f);

        swingEffect.transform.eulerAngles = bossAI.whiteFish.eulerAngles;
        swingEffect.SetActive(true);
        StartCoroutine(ApplyAttackInCircle(swingAttackDuration, swingRange, transform, swingAttack));
        yield return new WaitForSeconds(.7f);

        swingEffect.SetActive(false);
        bossAI.black_idling = true;
        bossAI.white_idling = true;

        if (bossAI.NextAction() != bossAI.dive)
        {
            yield return bossAI.co_IEcloseSwim = StartCoroutine(bossAI.IECloseSwim(false));
        }

        bossAI.AddActionBreak(actionBreakAmount);
        bossAI.EndAction();
    }

    public override void Hit(MeleeAttack melee, Transform attackPos)
    {
        int dealtDamage = playerIDamagable.DamageFromMeleeAttack(attackPos, melee.damage, melee.stun);
        Vector3 direction = new Vector3((playerIDamagable.GetHitPos() - attackPos.position).x, 0, 0).normalized;

        if (dealtDamage == 2)//counter attack
        {
            //counter attack effect
            vfx.SpawnHitEffect(true, playerIDamagable.hitEffectPosition.position);
            playerIDamagable.Repel(melee.repel, direction);
            vfx.CameraShake(melee.cameraShake);
            vfx.RumblePulse(melee.rumble.x * 2, melee.rumble.y * 2, melee.rumbleDuration * 2);
            vfx.SlowTimeForSeconds(melee.freezeTime, 0f);

            if (bossAI.actionList[0] == this)
            {
                bool combo = false;
                if (((float)bossAI.currentHealth / (float)bossAI.maxHealth) <= 0.5)
                {
                    combo = Possibility(70);
                }
                else { combo = Possibility(100); }

                if (combo && !bossAI.secondPhase)
                {
                    bossAI.movingTarget = bossAI.GetBoundaryFarOfPlayer();
                    bossAI.InsertAction(bossAI.splash);
                    bossAI.InsertAction(bossAI.waterSpear);
                    bossAI.InsertAction(bossAI.dive);
                }
            }
        }
        else if (dealtDamage == 1)//defend
        {
            vfx.SpawnHitEffect(true, playerIDamagable.GetHitPos());
            playerIDamagable.Repel(melee.repel, direction);
            vfx.CameraShake(melee.cameraShake);
            vfx.RumblePulse(melee.rumble.x * 2, melee.rumble.y * 2, melee.rumbleDuration * 2);
            vfx.SlowTimeForSeconds(melee.freezeTime, 0f);
        }
        else if (dealtDamage == 0)//dealtDamage
        {
            vfx.SpawnHitEffect(true, playerIDamagable.GetHitPos());
            playerIDamagable.Repel(melee.repel * 2, direction);
            vfx.RumblePulse(melee.rumble.x, melee.rumble.y, melee.rumbleDuration);
            vfx.SlowTimeForSeconds(melee.freezeTime, 0f);
        }
    }

    public void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, swingRange);
    }
}