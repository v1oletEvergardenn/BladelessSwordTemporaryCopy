using EditorAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YYF_Swing : IEnemyAction
{
    private YingYangFish_AI bossAI;

    public GameObject swingEffect;

    public MeleeAttack swingAttack = new MeleeAttack(2, 0.5f, 0.5f, new Vector2(0.25f, 0.4f), 0.2f, 10f);

    public override void Start()
    {
        base.Start();
        bossAI = GetComponent<YingYangFish_AI>();
    }

    public override IEnumerator Act_coroutine()
    {
        yield return StartCoroutine(bossAI.IECloseSwim(true));

        bossAI.whiteAnim.Play("swing");
        bossAI.blackAnim.Play("swing");
        bossAI.black_idling = false;
        bossAI.white_idling = false;

        yield return new WaitForSeconds(1f);

        swingEffect.transform.eulerAngles = bossAI.whiteFish.eulerAngles;
        swingEffect.SetActive(true);

        yield return new WaitForSeconds(.7f);

        swingEffect.SetActive(false);
        bossAI.black_idling = true;
        bossAI.white_idling = true;

        yield return StartCoroutine(bossAI.IECloseSwim(false));
        bossAI.EndAction();
    }

    public void Hit()
    {
        int dealtDamage = playerIDamagable.DamageFromMeleeAttack(bossAI.isFacingRight, swingAttack.damage, swingAttack.stun);

        if (dealtDamage == 2)//counter attack
        {
            //counter attack effect
            vfx.SpawnHitEffect(true, playerIDamagable.hitEffectPosition.position);
            playerIDamagable.Repel(swingAttack.repel, transform.right);
            vfx.CameraShake(0.1f);
            vfx.RumblePulse(swingAttack.rumble.x * 2, swingAttack.rumble.y * 2, swingAttack.rumbleDuration * 2);
            vfx.SlowTimeForSeconds(swingAttack.freezeTime, 0f);
        }
        else if (dealtDamage == 1)//defend
        {
            vfx.SpawnHitEffect(true, playerIDamagable.GetHitPos());
            playerIDamagable.Repel(swingAttack.repel, transform.right);
            vfx.CameraShake(0.1f);
            vfx.RumblePulse(swingAttack.rumble.x * 2, swingAttack.rumble.y * 2, swingAttack.rumbleDuration * 2);
            vfx.SlowTimeForSeconds(swingAttack.freezeTime, 0f);
        }
        else if (dealtDamage == 0)//dealtDamage
        {
            vfx.SpawnHitEffect(true, playerIDamagable.GetHitPos());
            vfx.RumblePulse(swingAttack.rumble.x, swingAttack.rumble.y, swingAttack.rumbleDuration);
            playerIDamagable.Repel(swingAttack.repel * 2, transform.right);
            vfx.SlowTimeForSeconds(swingAttack.freezeTime, 0f);
        }
    }
}