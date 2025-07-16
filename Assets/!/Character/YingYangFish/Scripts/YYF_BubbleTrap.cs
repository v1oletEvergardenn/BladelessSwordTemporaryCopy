using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YYF_BubbleTrap : IEnemyAction
{
    private YingYangFish_AI bossAI;
    public float jumpRadius = 5f;
    public float jumpHeight = 3f;

    public int bubbleDamage = 2;
    public float bubbleSpeed = 5f;
    public float bubbleStunDuration = 0.5f;
    public float bubbleStunValue = 0.5f;
    public float bubbleRange = 1.6f;

    public override void Start()
    {
        base.Start();
        bossAI = GetComponent<YingYangFish_AI>();
    }

    public override bool CanAct()
    {
        if (bossAI.isWhiteBusy && bossAI.isBlackBusy) return false;
        else return true;
    }

    public override IEnumerator Act_coroutine(float factor = 0)
    {
        bool isBlack = true;
        if (bossAI.isBlackBusy) { isBlack = false; }
        bossAI.SetBusy(isBlack);

        Animator anim = isBlack ? bossAI.blackAnim : bossAI.whiteAnim;
        Transform fish = isBlack ? bossAI.blackFish : bossAI.whiteFish;
        Transform origin = isBlack ? bossAI.blackOrigin : bossAI.whiteOrigin;
        Transform fishGFX = isBlack ? bossAI.blackFishGFX : bossAI.whiteFishGFX;

        // dive
        yield return bossAI.co_singleFishDive = StartCoroutine(bossAI.IESingleFishDive(isBlack, bossAI.IsPlayerLeft()));
        if (isBlack) { bossAI.SetBlackTargetRotateSpeed(0); bossAI.black_rotateSpeed = 0; }
        else { bossAI.SetWhiteTargetRotateSpeed(0); bossAI.white_rotateSpeed = 0; }
        origin.eulerAngles = new Vector3(0, 0, 180);

        //move to player
        float elpasedTime = 0f;
        float duration = 0.5f;
        Vector3 start = origin.position;
        bool toleft = playerController.FacingRight;
        bool isPlayerMoving = playerController.isRunning;

        while (elpasedTime <= duration)
        {
            elpasedTime += Time.deltaTime;
            Vector3 target = new Vector3(player.transform.position.x + (isPlayerMoving ? (toleft ? 5 : -5) : 0),
                bossAI.waterLevel.position.y + jumpHeight - jumpRadius, 0);
            origin.position = Vector3.Lerp(start, target, elpasedTime / duration);
            yield return null;
        }

        if (isPlayerMoving) origin.DOMove(new Vector3(player.transform.position.x + (toleft ? 5 : -5),
                bossAI.waterLevel.position.y + jumpHeight - jumpRadius, 0), 0.3f);

        // rotate to angle
        float angle = 90; if (toleft) { angle = -90; origin.localScale = new Vector3(-1, 1, 1); }
        origin.Rotate(bossAI.Dir, angle);
        fish.localPosition = new Vector3(0, jumpRadius, 0);

        //jump out
        angle = 180;
        float currentZ = origin.eulerAngles.z;
        float delta = (toleft ? (angle - currentZ + 360f) : (angle - currentZ - 360f)) % 360f;
        origin.DORotate(new Vector3(0, 0, delta), bossAI.sprintRotateSpeed, RotateMode.WorldAxisAdd)
           .SetSpeedBased(true)
           .SetEase(Ease.Linear);

        anim.Play("sprint_swim");
        //emitting bubbles

        yield return new WaitForSeconds(0.22f);
        elpasedTime = 0f;
        duration = 1.3f;
        float emitGap = 0.07f;
        float emitTimer = 0f;
        while (elpasedTime <= duration)
        {
            elpasedTime += Time.deltaTime;
            emitTimer += Time.deltaTime;
            if (emitTimer >= emitGap)
            {
                Vector3 dir = (fish.position - (origin.position + new Vector3(0, jumpHeight))).normalized;
                angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                Vector3 euler = new Vector3(0, 0, angle);

                Bubble bubble = bossAI.selfPooler.SpawnFromPool("bubble", fish.position, Quaternion.identity).GetComponent<Bubble>();
                bubble.SetUp(euler,
                    transform.gameObject,
                    _damage: bubbleDamage,
                    _speed: bubbleSpeed,
                    _stunValue: bubbleStunValue);
                bubble.stunDuration = bubbleStunDuration;
                bubble.explodeRange = bubbleRange;
                emitTimer = 0f;
            }
            yield return null;
        }

        yield return new WaitForSeconds(0.3f);
        origin.localScale = new Vector3(1, 1, 1);
        fish.DOLocalMoveY(3, 1f);

        anim.Play("close_swim");
        //swim back
        yield return bossAI.co_singleReturnToCenter = StartCoroutine(bossAI.IEReturnToCenter(isBlack));

        //end
        bossAI.SetNotBusy(isBlack);
        bossAI.AddActionBreak(actionBreakAmount);
        bossAI.EndAction();
    }
}