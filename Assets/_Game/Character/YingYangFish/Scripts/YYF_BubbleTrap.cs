using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YYF_BubbleTrap : IEnemyAction
{
    private YingYangFish_AI bossAI;

    [HeaderAttribute("Bubble Emitting Settings")]
    public int bubbleCount = 7;

    public float arcSpreadAngle = 160f;
    public float delayAfterEmit = 0.22f;
    public float jumpRadius = 5f;
    public float jumpHeight = 3f;

    [HeaderAttribute("Bubble Settings")]
    public IProjectileBasicAttributes bubbleAttribute;

    public float bubbleRange = 1.6f;

    public override void Start()
    {
        base.Start();
        bossAI = GetComponent<YingYangFish_AI>();
    }

    public override IEnumerator Act_coroutine(float factor = 0, Transform _target = null)
    {
        Transform trueTarget = _target != null ? _target : player.transform;

        bool isBlack = factor == 0 ? true : false;
        YYF_fish YYFFish = bossAI.SpawnFish(isBlack);
        Animator anim = YYFFish.anim;
        Transform fish = YYFFish.fish;
        Transform origin = YYFFish.transform;
        Transform fishGFX = YYFFish.fishGFX;

        bossAI.ResetFishGFX(YYFFish);
        bossAI.ResetFish(YYFFish);
        // dive
        //yield return bossAI.co_singleFishDive = StartCoroutine(bossAI.IESingleFishDive(isBlack, bossAI.IsPlayerLeft()));
        if (isBlack) { bossAI.SetBlackRotateSpeed(0); bossAI.black_rotateSpeed = 0; }
        else { bossAI.SetWhiteRotateSpeed(0); bossAI.white_rotateSpeed = 0; }
        origin.eulerAngles = new Vector3(0, 0, 180);

        //move to player
        bool toleft = GameManager.instance.isInPerformingState ? isBlack : GetAttackDirection();

        origin.position = new Vector3(trueTarget.position.x,
                bossAI.waterLevel.position.y + jumpHeight - jumpRadius,
                0); ;

        // rotate to angle
        float angle = 90; if (toleft) { angle = -90; origin.localScale = new Vector3(-1, 1, 1); }
        origin.Rotate(bossAI.Dir, angle);
        fish.localPosition = new Vector3(0, jumpRadius, 0);

        //jump out
        angle = 180;
        float currentZ = origin.eulerAngles.z;
        float delta = (toleft ? (angle - currentZ + 360f) : (angle - currentZ - 360f)) % 360f;

        bool complete = false;
        origin.DORotate(new Vector3(0, 0, delta), bossAI.fastRotateSpeed, RotateMode.WorldAxisAdd)
           .SetSpeedBased(true)
           .SetEase(Ease.Linear)
           .OnComplete(() => complete = true)
           .SetTimeDt(this, TimeChannel.Enemy);
        anim.Play("fast_down");

        // pre-calculate spawn positions symmetrically across the top arc
        int count = Mathf.Max(1, bubbleCount);
        float halfSpread = arcSpreadAngle / 2f;
        Vector3 circleCenter = origin.position;
        Vector3 convergencePoint = new Vector3(circleCenter.x, bossAI.waterLevel.position.y, 0f);
        Vector3[] spawnPositions = new Vector3[count];
        Vector3[] spawnEulers = new Vector3[count];

        for (int i = 0; i < count; i++)
        {
            float t = count > 1 ? (i / (float)(count - 1)) * 2f - 1f : 0f;
            float spawnAngleDeg = 90f + t * halfSpread;
            float spawnAngleRad = spawnAngleDeg * Mathf.Deg2Rad;

            spawnPositions[i] = new Vector3(
                circleCenter.x + jumpRadius * Mathf.Cos(spawnAngleRad),
                circleCenter.y + jumpRadius * Mathf.Sin(spawnAngleRad),
                0f);

            Vector3 fireDir = (convergencePoint - spawnPositions[i]).normalized;
            float fireAngle = Mathf.Atan2(fireDir.y, fireDir.x) * Mathf.Rad2Deg;
            spawnEulers[i] = new Vector3(0, 0, fireAngle);
        }

        // reverse spawn order: right to left when toleft, left to right otherwise
        if (!toleft)
        {
            System.Array.Reverse(spawnPositions);
            System.Array.Reverse(spawnEulers);
        }

        // spawn bubbles one by one at pre-calculated positions as the fish travels
        float elpasedTime = 0f;
        float duration = 1.2f;
        List<Bubble> bubbles = new List<Bubble>();
        int nextSpawnIndex = 0;
        float spawnInterval = duration / count;
        float nextSpawnTime = 0f;

        while (elpasedTime <= duration)
        {
            elpasedTime += TimeScaleManager.EnemyDt;

            if (nextSpawnIndex < count && elpasedTime >= nextSpawnTime)
            {
                Bubble bubble = bossAI.selfPooler.SpawnFromPool("bubble", spawnPositions[nextSpawnIndex], Quaternion.identity).GetComponent<Bubble>();
                bubbles.Add(bubble);

                bubble.SetUp(spawnEulers[nextSpawnIndex], transform.gameObject).
                    SetAttributes(bubbleAttribute).
                    SetDelay(delayAfterEmit, true);
                bubble.explodeRange = bubbleRange;

                nextSpawnTime += spawnInterval;
                nextSpawnIndex++;
            }

            yield return null;
        }
        yield return new WaitUntil(() => complete);
        origin.localScale = new Vector3(1, 1, 1);
        attackDirectionSet = false;
        YYFFish.gameObject.SetActive(false);
        yield return null;
    }
}