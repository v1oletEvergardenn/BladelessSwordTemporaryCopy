using EditorAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class QX_add_thrust : IEnemyAction
{
    private QianXiaoAI bossAI;
    public IEnemyAction actionSender;
    public Transform hitBox;
    public float repelForce = 100f;
    public int damageAmount = 3;
    public float stunDuration = 0.5f;
    public float freezeTimeDuration;
    [SerializeField, MinMaxSlider(0, 3f)] public Vector2 rumbleFrequncy = new Vector2(0.25f, 0.4f);
    public float rumbleDuration = 0.2f;

    public override void Start()
    {
        base.Start();
        bossAI = GetComponent<QianXiaoAI>();
    }

    //public override void Act()
    //{
    //    isThisActing = true;
    //    StartCoroutine(Act_coroutine());
    //}

    //public override IEnumerator Act_coroutine()
    //{
    //    if (bossAI.isStage2) { anim.Play("S2_thrust"); }
    //    else { anim.Play("S1_thrust"); }
    //    yield return new WaitForSeconds(0.5f);
    //    //teleport
    //    bossAI.Teleport(player.transform.position - player.transform.right * 5);
    //    yield return new WaitForSeconds(0.2f);
    //    bossAI.canFlip = false;
    //    yield return new WaitForSeconds(0.7f);
    //    hitBox.gameObject.SetActive(true);
    //    yield return new WaitForSeconds(0.1f);
    //    hitBox.gameObject.SetActive(false);
    //    yield return new WaitForSeconds(action_time);
    //    bossAI.canFlip = true;
    //    isThisActing = false;
    //    bossAI.inAct = false;
    //    if (actionSender != null)
    //    {
    //        bossAI.NextAction();
    //        actionSender = null;
    //    }
    //    else
    //    {
    //        bossAI.NextAction();
    //    }
    //    yield return null;
    //}

    public void hit()
    {
        int dealtDamage = playerIDamagable.DamageFromMeleeAttack(transform, damageAmount, stunDuration);

        if (dealtDamage == 2)//counter attack
        {
            //counter attack effect
            vfx.SpawnLargeSlashEffect(playerIDamagable.GetHitPos(), false);

            playerIDamagable.Repel(repelForce, transform.right);
            vfx.CameraShake(0.1f);
            vfx.RumblePulse(rumbleFrequncy.x * 2, rumbleFrequncy.y * 2, rumbleDuration * 2);
            vfx.SlowTimeForSeconds(freezeTimeDuration, 0f);
        }
        else if (dealtDamage == 1)//defend
        {
            vfx.SpawnLargeSlashEffect(playerIDamagable.GetHitPos(), false);
            playerIDamagable.Repel(repelForce, transform.right);
            vfx.CameraShake(0.1f);
            vfx.RumblePulse(rumbleFrequncy.x * 2, rumbleFrequncy.y * 2, rumbleDuration * 2);
            vfx.SlowTimeForSeconds(freezeTimeDuration, 0f);
        }
        else if (dealtDamage == 0)//dealtDamage
        {
            vfx.SpawnLargeSlashEffect(playerIDamagable.GetHitPos(), false);
            vfx.RumblePulse(rumbleFrequncy.x, rumbleFrequncy.y, rumbleDuration);
            playerIDamagable.Repel(repelForce * 2, transform.right);
            vfx.SlowTimeForSeconds(freezeTimeDuration, 0f);
        }
    }
}