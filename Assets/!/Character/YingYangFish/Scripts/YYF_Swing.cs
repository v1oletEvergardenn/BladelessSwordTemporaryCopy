using DG.Tweening;
using EditorAttributes;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class YYF_Swing : IEnemyAction
{
    private YingYangFish_AI bossAI;

    public GameObject swingEffect;
    public GameObject swing_outline;

    public MeleeAttack swingAttack = new MeleeAttack(2, 0.5f, 0.2f, new Vector2(0.25f, 0.4f), 0.2f, 20f, 0.1f);
    public float swingRange;
    public float swingAttackDuration;
    public float stunValue = 35f;

    private float localFactor;

    public override void Start()
    {
        base.Start();
        bossAI = GetComponent<YingYangFish_AI>();
    }

    public override void CancelAct()
    {
        base.CancelAct();
        swingEffect.SetActive(false);
        swing_outline.SetActive(false);
    }

    public override IEnumerator Act_coroutine(float factor = 0)
    {
        localFactor = factor;
        if (factor != 3)
        {
            yield return bossAI.co_sprintBackEqual = StartCoroutine(bossAI.IESprintBackEqual());

            if (bossAI.white_distanceToCenter > bossAI.minMaxDistanceTocenter.x)
            {
                yield return bossAI.co_IEcloseSwim = StartCoroutine(bossAI.IECloseSwim(true));
            }
        }

        bossAI.SetBlackTargetRotateSpeed(bossAI.idleRotateSpeed / 4);
        bossAI.SetWhiteTargetRotateSpeed(bossAI.idleRotateSpeed / 4);

        bossAI.whiteAnim.Play("swing");
        bossAI.blackAnim.Play("swing");

        if (factor == 1)
        {
            yield return new WaitForSeconds(0.4f);
            swing_outline.SetActive(true);
            InputKeyType inputKey = InputKeyType.left_attack_key;
            if (bossAI.IsPlayerLeft()) { inputKey = InputKeyType.right_attack_key; }
            InputMaster.instance.StartQTE(inputKey, player.transform.position + new Vector3(0, 4, 0), 0.3f, () =>
            {
                playerAttack.Attack(bossAI.IsPlayerLeft() ? false : true);
            }, null);
        }//qte
        else if (factor == 2)
        {
            yield return new WaitForSeconds(0.4f);
            swing_outline.SetActive(true);
            InputKeyType inputKey = InputKeyType.swordTeleport_key;
            Vector3 pos = new Vector3(12, 5, 0);
            if (bossAI.IsPlayerLeft()) { pos = new Vector3(-12, 5, 0); }
            InputMaster.instance.StartQTE(inputKey, player.transform.position + new Vector3(0, 4, 0),
                0.3f, () => { playerController.DesignatedPositionTeleport(player.transform.position + pos); }, null);
            yield return new WaitUntil(() => !InputMaster.instance.isQTE);
        }
        else
        {
            yield return new WaitForSeconds(0.5f);
            swing_outline.SetActive(true);
            yield return new WaitForSeconds(0.3f);
        }

        float x = transform.position.x + 5;
        if (bossAI.IsPlayerLeft()) { x = transform.position.x - 5; }
        transform.DOMoveX(x, 0.3f).SetEase(Ease.InQuint);
        if (factor == 1) { yield return new WaitUntil(() => !InputMaster.instance.isQTE); }
        else { yield return new WaitForSeconds(0.2f); }

        bossAI.whiteAnim.Play("swing_attack");
        bossAI.blackAnim.Play("swing_attack");

        swingEffect.transform.eulerAngles = bossAI.whiteFish.eulerAngles;
        swingEffect.SetActive(true);

        StartCoroutine(ApplyAttackInCircle(swingAttackDuration, swingRange, transform, swingAttack));
        yield return new WaitForSeconds(.7f);

        swingEffect.SetActive(false);
        swing_outline.SetActive(false);

        if (factor == 0 || factor == 1 || factor == 3)
        {
            bossAI.SetNormalRotateSpeed();
            if (factor == 1) { CharacterController2D.instance.FaceTarget(this.transform); }
            yield return bossAI.co_IEcloseSwim = StartCoroutine(bossAI.IECloseSwim(false));

            bossAI.AddActionBreak(actionBreakAmount);
            bossAI.EndAction();
        }
        else if (factor == 2)
        {
            CharacterController2D.instance.FaceTarget(this.transform);
            yield return StartCoroutine(Act_coroutine(3));
            yield return null;
        }
    }

    public override void Hit(MeleeAttack melee, Transform attackPos)
    {
        int dealtDamage = playerIDamagable.DamageFromMeleeAttack(attackPos, melee.damage, melee.stun);
        bool left = playerIDamagable.GetHitPos().x < attackPos.position.x ? true : false;

        if (dealtDamage == 2)//counter attack
        {
            //counter attack effect
            vfx.SpawnHitEffect(true, playerIDamagable.hitEffectPosition.position);
            playerIDamagable.Repel(melee.repel, left);
            vfx.CameraShake(melee.cameraShake);
            vfx.RumblePulse(melee.rumble.x * 2, melee.rumble.y * 2, melee.rumbleDuration * 2);
            vfx.SlowTimeForSeconds(melee.freezeTime, 0f);

            bossAI.DecreaseStun(stunValue);
            if (bossAI.actionList.Count != 0 && bossAI.actionList[0] == this)
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
                    bossAI.InsertAction(bossAI.splash_white);
                    bossAI.InsertAction(bossAI.waterSpear);
                    bossAI.InsertAction(bossAI.dive);
                }
            }
        }
        else if (dealtDamage == 1)//defend
        {
            vfx.SpawnHitEffect(true, playerIDamagable.GetHitPos());
            playerIDamagable.Repel(melee.repel, left);
            vfx.CameraShake(melee.cameraShake);
            vfx.RumblePulse(melee.rumble.x * 2, melee.rumble.y * 2, melee.rumbleDuration * 2);
            vfx.SlowTimeForSeconds(melee.freezeTime, 0f);
        }
        else if (dealtDamage == 0)//dealtDamage
        {
            vfx.SpawnHitEffect(true, playerIDamagable.GetHitPos());
            if (localFactor == 2)
            {
                playerIDamagable.ForceRepel(melee.repel * 2, left);
            }
            else
            {
                playerIDamagable.Repel(melee.repel * 2, left);
            }

            vfx.RumblePulse(melee.rumble.x, melee.rumble.y, melee.rumbleDuration);
            vfx.SlowTimeForSeconds(melee.freezeTime, 0f);
        }
    }

    public void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, swingRange);
    }
}