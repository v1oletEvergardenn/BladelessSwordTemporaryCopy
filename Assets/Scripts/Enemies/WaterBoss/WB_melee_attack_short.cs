using EditorAttributes;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WB_melee_attack_short : IEnemyAction
{
    public float repelForce_self = 400f;
    public float repelForce_player = 10f;
    public Transform repelPosition;
    public float repelRange;
    public GameObject Explosion_Prefab;
    private WaterBossAI bossAI;

    public int damageAmount = 3;
    public float stunDuration = 0.5f;
    public float freezeTimeDuration;
    [SerializeField, MinMaxSlider(0, 3f)] public Vector2 rumbleFrequncy = new Vector2(0.25f, 0.4f);
    public float rumbleDuration = 0.2f;

    public override void Start()
    {
        base.Start();
        bossAI = GetComponent<WaterBossAI>();
    }

    public override void Act()
    {
        anim.Play("melee_short_attack");
        StartCoroutine(Act_coroutine());
    }

    public override IEnumerator Act_coroutine(float factor = 0)
    {
        yield return new WaitForSeconds(0.3f);
        bossAI.canFlip = false;
        yield return new WaitForSeconds(1.7f);
        bossAI.canFlip = true;
        if (bossAI.inAct)
        {
            bossAI.inAct = false;
            bossAI.NextAction();
        }
        yield return null;
    }

    public void ApplyAttack()
    {
        //spwan effect
        GameObject i = Instantiate(Explosion_Prefab, repelPosition.position, transform.rotation) as GameObject;
        Destroy(i, 1f);
        rb.AddForce(-transform.right * repelForce_self);
        float d = Vector3.Distance(playerIDamagable.GetHitPos(), repelPosition.position);
        if (d <= repelRange)
        {
            Hit();
        }
    }

    public void Hit()
    {
        int dealtDamage = playerIDamagable.DamageFromMeleeAttack(transform, damageAmount, stunDuration);

        if (dealtDamage == 2)//counter attack
        {
            //counter attack effect
            vfx.SpawnHitEffect(true, playerIDamagable.hitEffectPosition.position);

            playerIDamagable.Repel(repelForce_player, transform.right);
            vfx.CameraShake(0.1f);
            vfx.RumblePulse(rumbleFrequncy.x * 2, rumbleFrequncy.y * 2, rumbleDuration * 2);
            vfx.SlowTimeForSeconds(freezeTimeDuration, 0f);
        }
        else if (dealtDamage == 1)//defend
        {
            vfx.SpawnHitEffect(true, playerIDamagable.GetHitPos());
            playerIDamagable.Repel(repelForce_player, transform.right);
            vfx.CameraShake(0.1f);
            vfx.RumblePulse(rumbleFrequncy.x * 2, rumbleFrequncy.y * 2, rumbleDuration * 2);
            vfx.SlowTimeForSeconds(freezeTimeDuration, 0f);
        }
        else if (dealtDamage == 0)//dealtDamage
        {
            vfx.SpawnHitEffect(true, playerIDamagable.GetHitPos());
            vfx.RumblePulse(rumbleFrequncy.x, rumbleFrequncy.y, rumbleDuration);
            playerIDamagable.Repel(repelForce_player * 2, transform.right);
            vfx.SlowTimeForSeconds(freezeTimeDuration, 0f);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(repelPosition.position, repelRange);
    }
}