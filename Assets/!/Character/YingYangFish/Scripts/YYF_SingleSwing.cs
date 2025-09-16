using DG.Tweening;
using Mobsoft.PixelStyleWaterShader;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YYF_SingleSwing : IEnemyAction
{
    private YingYangFish_AI bossAI;
    public bool showRange = true;
    public GameObject swingEffect;
    public GameObject swing_outline;
    public float attackTime = 0.5f;
    public float jumpRadius = 5f;
    public float jumpHeight = 3f;
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

    public override bool CanAct()
    {
        if (bossAI.isWhiteBusy && bossAI.isBlackBusy) return false;
        else return true;
    }

    public override IEnumerator Act_coroutine(float factor = 0)
    {
        // initialize
        proceedCall = false;
        bool isBlack = false;
        if (bossAI.isWhiteBusy && !bossAI.isBlackBusy) { isBlack = true; }
        bossAI.SetBusy(isBlack);
        //references
        Animator anim = isBlack ? bossAI.blackAnim : bossAI.whiteAnim;
        Transform fish = isBlack ? bossAI.blackFish : bossAI.whiteFish;
        Transform origin = isBlack ? bossAI.blackOrigin : bossAI.whiteOrigin;
        Transform fishGFX = isBlack ? bossAI.blackFishGFX : bossAI.whiteFishGFX;

        // dive
        yield return bossAI.co_singleFishDive = StartCoroutine(bossAI.IESingleFishDive(isBlack, bossAI.IsPlayerLeft()));
        if (bossAI.initialAction == bossAI.waterSpear)
        { bossAI.waterSpear.OnProceedCall(); }
        if (isBlack) { bossAI.SetBlackTargetRotateSpeed(0); bossAI.black_rotateSpeed = 0; }
        else { bossAI.SetWhiteTargetRotateSpeed(0); bossAI.white_rotateSpeed = 0; }
        Vector3 target = player.transform.position + new Vector3(0, 3, 0);
        bool toLeft = target.x < origin.position.x;

        // move to appropriate x position
        if (toLeft) { origin.DOMove(new Vector3(target.x + 2, bossAI.waterLevel.position.y - 6, 0), 1f); }
        else { origin.DOMove(new Vector3(target.x - 2, bossAI.waterLevel.position.y - 6, 0), 1f); }
        yield return new WaitForSeconds(1);

        //reset to initial
        origin.eulerAngles = Vector3.zero;
        fish.localPosition = new Vector3(0, 1, 0);
        fishGFX.DOLocalRotate(new Vector3(0, 0, 0), 0.1f);
        fishGFX.DOLocalMove(new Vector3(0, 0, 0), 0.1f);

        // rotate to target position angle
        float angle = 210;
        if (!toLeft) { angle += 90; }
        origin.Rotate(bossAI.Dir, angle);

        //jump out
        float temp_x = toLeft ? player.transform.position.x - 2 : player.transform.position.x + 2;
        origin.DOMove(new Vector3(temp_x, bossAI.waterLevel.position.y + 4.5f, 0), 0.5f).SetEase(Ease.OutSine);

        //pre swing attack

        yield return new WaitForSeconds(0.3f);
        anim.Play("swing", 0, 0.5f);
        swing_outline.transform.SetParent(origin);
        swing_outline.transform.position = origin.position;
        swing_outline.SetActive(true);

        //swing attack movement
        yield return new WaitForSeconds(0.3f);
        float x = origin.position.x + 5;
        if (player.transform.position.x <= fish.position.x) { x = origin.position.x - 5; }
        origin.DOMoveX(x, 0.3f).SetEase(Ease.InQuint);

        //actual attack
        yield return new WaitForSeconds(0.2f);
        anim.Play("swing_attack");
        Vector3 dirToPlayer = player.transform.position - swingEffect.transform.position;
        float angleToPlayer = Mathf.Atan2(dirToPlayer.y, dirToPlayer.x) * Mathf.Rad2Deg;
        swingEffect.transform.eulerAngles = new Vector3(0, 0, angleToPlayer);
        swingEffect.transform.SetParent(origin);
        swingEffect.transform.position = origin.position;
        swingEffect.SetActive(true);
        StartCoroutine(ApplyAttackInCircle(swingAttackDuration, swingRange, origin, swingAttack));

        if (factor == 1)
        {
            yield return new WaitForSeconds(0.8f);
            swingEffect.SetActive(false);
            swing_outline.SetActive(false);
            anim.Play("swing", 0, 0.5f);
            swing_outline.transform.SetParent(origin);
            swing_outline.transform.position = origin.position;
            swing_outline.SetActive(true);

            //swing attack movement
            yield return new WaitForSeconds(0.3f);
            x = origin.position.x + 5;
            if (player.transform.position.x <= fish.position.x) { x = origin.position.x - 5; }
            origin.DOMoveX(x, 0.3f).SetEase(Ease.InQuint);

            //actual attack
            yield return new WaitForSeconds(0.2f);
            anim.Play("swing_attack");
            dirToPlayer = player.transform.position - swingEffect.transform.position;
            angleToPlayer = Mathf.Atan2(dirToPlayer.y, dirToPlayer.x) * Mathf.Rad2Deg;
            swingEffect.transform.eulerAngles = new Vector3(0, 0, angleToPlayer);
            swingEffect.transform.SetParent(origin);
            swingEffect.transform.position = origin.position;
            swingEffect.SetActive(true);
            StartCoroutine(ApplyAttackInCircle(swingAttackDuration, swingRange, origin, swingAttack));
        }

        //dive again
        if (isBlack) bossAI.isReturnDive_black = true;
        else bossAI.isReturnDive_white = true;
        yield return bossAI.co_return_singleFishDive = StartCoroutine(bossAI.IESingleFishDive(isBlack, origin.position.x >= transform.position.x));
        swingEffect.SetActive(false);
        swing_outline.SetActive(false);
        origin.localScale = new Vector3(1, 1, 1);
        fish.DOLocalMoveY(3, 1f);

        //swim back
        yield return bossAI.co_singleReturnToCenter = StartCoroutine(bossAI.IEReturnToCenter(isBlack));

        //end
        bossAI.SetNotBusy(isBlack);
        bossAI.AddActionBreak(actionBreakAmount);
        bossAI.EndAction();
    }

    public override void HitPlayer(MeleeAttack melee, Transform attackPos)
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
            //combo
            bossAI.DecreaseStun(stunValue);
        }
        else if (dealtDamage == 1)//defend
        {
            //effect
            vfx.SpawnHitEffect(true, playerIDamagable.GetHitPos());
            playerIDamagable.Repel(melee.repel, left);
            vfx.CameraShake(melee.cameraShake);
            vfx.RumblePulse(melee.rumble.x * 2, melee.rumble.y * 2, melee.rumbleDuration * 2);
            vfx.SlowTimeForSeconds(melee.freezeTime, 0f);
        }
        else if (dealtDamage == 0)//dealtDamage
        {
            //effect
            vfx.SpawnHitEffect(true, playerIDamagable.GetHitPos());
            playerIDamagable.Repel(melee.repel * 2, left);
            vfx.RumblePulse(melee.rumble.x, melee.rumble.y, melee.rumbleDuration);
            vfx.SlowTimeForSeconds(melee.freezeTime, 0f);
        }
    }

    public void OnDrawGizmos()
    {
        if (!showRange) return;
        Gizmos.DrawWireSphere(transform.position, swingRange);
    }
}