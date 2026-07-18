using DG.Tweening;
using EditorAttributes;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.UI.Image;

public class YYF_Swing : IEnemyAction
{
    private YingYangFish_AI bossAI;

    public bool showRange = true;
    public GameObject singleSwingEffect;
    public GameObject swingEffect_ice;
    public GameObject swing_outline;
    public GameObject swing_outline_ice;
    public Collider2D ice_hitBox;
    public Collider2D small_ice_hitBox;

    public MeleeAttack swingAttack = new MeleeAttack(2, 0.5f, 0.2f, new Vector2(0.25f, 0.4f), 0.2f, 20f, 0.1f);
    public float swingRange;
    public float swingAttackDuration;

    public float bossbreakValue = 8f;
    private float localFactor;

    public override void Start()
    {
        base.Start();
        bossAI = GetComponent<YingYangFish_AI>();
    }

    public override void CancelAct()
    {
        base.CancelAct();
        singleSwingEffect.SetActive(false);
        swing_outline.SetActive(false);
    }

    public override IEnumerator Act_coroutine(float factor = 0, Transform _target = null)
    {
        Transform trueTarget = _target != null ? _target : player.transform;

        if (factor == 0 || factor == 1)
        {
            bossAI.StopRotate(false);

            bool isBlack = factor == 0 ? true : false;
            YYF_fish YYFFish = bossAI.SpawnFish(isBlack);
            Animator anim = YYFFish.anim;
            Transform fish = YYFFish.fish;
            Transform origin = YYFFish.transform;
            Transform fishGFX = YYFFish.fishGFX;
            fish.localPosition = new Vector3(0, 0f, 0);

            Vector3 target = trueTarget.position + new Vector3(0, 3, 0);

            bool toLeft = GameManager.instance.isInPerformingState ? !isBlack : GetAttackDirection();
            // move to appropriate x position
            origin.DOMove(new Vector3(target.x, bossAI.waterLevel.position.y - 6, 0), 0.1f).SetTimeDt(this, TimeChannel.Enemy);

            // rotate to target position angle
            float angle = -90;
            origin.Rotate(bossAI.Dir, angle);
            yield return WaitForEnemy(0.1f);

            //jump out
            float temp_x = toLeft ? trueTarget.position.x + 4 : trueTarget.position.x - 4;
            origin.DOMove(new Vector3(temp_x, bossAI.waterLevel.position.y + (isBlack ? 2.5f : 4.5f), 0), 0.5f)
                .SetEase(Ease.OutSine)
                .SetTimeDt(this, TimeChannel.Enemy);

            //pre swing attack
            anim.Play("swing");
            fishGFX.DOLocalRotate(new Vector3(0, 0, -720), 0.6f, RotateMode.FastBeyond360).SetEase(Ease.OutSine)
                .SetTimeDt(this, TimeChannel.Enemy);

            if (isBlack)
            {
                yield return WaitForEnemy(0.55f);
                toLeft = trueTarget.position.x < fish.transform.position.x;

                //outline for ice
                swing_outline_ice.SetActive(false);
                swing_outline_ice.transform.position = fishGFX.position;
                if (toLeft) { swing_outline_ice.transform.localScale = new Vector3(-1, 1, 1); }
                else { swing_outline_ice.transform.localScale = new Vector3(1, 1, 1); }
                swing_outline_ice.SetActive(true);

                //ice animation
                swingEffect_ice.SetActive(false);
                swingEffect_ice.transform.position = bossAI.CreateWaterLevelYAxis(fish.position.x);
                swingEffect_ice.transform.eulerAngles = Vector3.zero;
                if (toLeft) { swingEffect_ice.transform.localScale = new Vector3(-1, 1, 1); }
                else { swingEffect_ice.transform.localScale = new Vector3(1, 1, 1); }
                swingEffect_ice.SetActive(true);

                yield return WaitForEnemy(0.05f);
                StartCoroutine(ApplyAttackInCollider(swingAttackDuration, fishGFX, small_ice_hitBox, swingAttack));

                // actual attack
                yield return WaitForEnemy(0.45f);
                StartCoroutine(ApplyAttackInCollider(swingAttackDuration, fishGFX, ice_hitBox, swingAttack));
            }
            else
            {
                yield return WaitForEnemy(0.35f);
                swing_outline.SetActive(false);
                swing_outline.transform.SetParent(fishGFX);
                swing_outline.transform.position = fishGFX.position;
                swing_outline.SetActive(true);

                //swing attack movement
                yield return WaitForEnemy(0.3f);

                float _x = origin.position.x + 5;
                if (trueTarget.position.x <= fish.position.x) { _x = fish.position.x - 5; }
                origin.DOMoveX(_x, 0.3f).SetEase(Ease.InQuint)
                      .SetTimeDt(this, TimeChannel.Enemy);

                yield return WaitForEnemy(0.2f);
                Vector3 dirToPlayer = trueTarget.position - singleSwingEffect.transform.position;
                float angleToPlayer = Mathf.Atan2(dirToPlayer.y, dirToPlayer.x) * Mathf.Rad2Deg;
                singleSwingEffect.SetActive(false);
                singleSwingEffect.transform.SetParent(fishGFX);
                singleSwingEffect.transform.position = fishGFX.position;
                singleSwingEffect.transform.eulerAngles = new Vector3(0, 0, angleToPlayer);
                singleSwingEffect.SetActive(true);

                StartCoroutine(ApplyAttackInCircle(swingAttackDuration, swingRange, fishGFX, swingAttack));
            }

            //return to water
            yield return bossAI.co_return_singleFishDive = StartCoroutine(bossAI.IESingleFishDive(YYFFish));
            if (isBlack) swingEffect_ice.SetActive(false);
            else singleSwingEffect.SetActive(false);
            swing_outline.SetActive(false);
        }
        else if (factor == 2)
        {
            swing_outline.transform.SetParent(transform);
            swing_outline.transform.localPosition = Vector3.zero;

            YYF_fish black_YYFFish = bossAI.SpawnFish(true);
            YYF_fish white_YYFFish = bossAI.SpawnFish(false);

            //reset to initial
            black_YYFFish.fish.localPosition = new Vector3(0, 0.3f, 0);
            white_YYFFish.fish.localPosition = new Vector3(0, 0.3f, 0);
            Transform whiteGFX = white_YYFFish.fishGFX;
            Transform blackGFX = black_YYFFish.fishGFX;

            // move to appropriate x position
            Vector3 target = trueTarget.position + new Vector3(0, 3, 0);
            bool toLeft = GetAttackDirection();

            // rotate to target position angle
            float angle = -90;
            white_YYFFish.transform.Rotate(bossAI.Dir, angle);
            black_YYFFish.transform.Rotate(bossAI.Dir, 90);
            yield return WaitForEnemy(0.1f);

            //out of the water
            float temp_x = toLeft ? trueTarget.position.x - 4 : trueTarget.position.x + 4;
            black_YYFFish.transform.DOMove(new Vector3(temp_x, bossAI.waterLevel.position.y + 2.5f, 0), 0.5f)
                .SetEase(Ease.OutSine)
                .SetTimeDt(this, TimeChannel.Enemy);
            white_YYFFish.transform.DOMove(new Vector3(temp_x, bossAI.waterLevel.position.y + 4.5f, 0), 0.5f)
                .SetEase(Ease.OutSine)
                .SetTimeDt(this, TimeChannel.Enemy);

            //pre swing attack
            black_YYFFish.anim.Play("swing");
            white_YYFFish.anim.Play("swing");
            black_YYFFish.fishGFX.DOLocalRotate(new Vector3(0, 0, -720), 0.6f, RotateMode.FastBeyond360).
                SetEase(Ease.OutSine)
                .SetTimeDt(this, TimeChannel.Enemy);
            white_YYFFish.fishGFX.DOLocalRotate(new Vector3(0, 0, -720), 0.6f, RotateMode.FastBeyond360).
                SetEase(Ease.OutSine)
                .SetTimeDt(this, TimeChannel.Enemy);

            yield return WaitForEnemy(0.35f);
            swing_outline.SetActive(false);
            swing_outline.transform.SetParent(whiteGFX);
            swing_outline.transform.position = whiteGFX.position;
            swing_outline.SetActive(true);

            yield return WaitForEnemy(0.2f);
            toLeft = trueTarget.position.x < black_YYFFish.fish.transform.position.x;
            //outline for ice
            swing_outline_ice.SetActive(false);
            swing_outline_ice.transform.position = blackGFX.position;
            if (toLeft) { swing_outline_ice.transform.localScale = new Vector3(-1, 1, 1); }
            else { swing_outline_ice.transform.localScale = new Vector3(1, 1, 1); }
            swing_outline_ice.SetActive(true);

            //ice animation
            swingEffect_ice.SetActive(false);
            swingEffect_ice.transform.position = new Vector3(black_YYFFish.fish.position.x, bossAI.waterLevel.position.y);
            swingEffect_ice.transform.eulerAngles = Vector3.zero;
            if (toLeft) { swingEffect_ice.transform.localScale = new Vector3(-1, 1, 1); }
            else { swingEffect_ice.transform.localScale = new Vector3(1, 1, 1); }
            swingEffect_ice.SetActive(true);

            yield return WaitForEnemy(0.05f);
            StartCoroutine(ApplyAttackInCollider(swingAttackDuration, blackGFX, small_ice_hitBox, swingAttack));

            yield return WaitForEnemy(0.05f);
            float _x = white_YYFFish.transform.position.x + 5;
            if (trueTarget.position.x <= white_YYFFish.fish.position.x) { _x = white_YYFFish.fish.position.x - 5; }
            white_YYFFish.transform.DOMoveX(_x, 0.3f).SetEase(Ease.InQuint).SetTimeDt(this, TimeChannel.Enemy);

            //actual attack
            yield return WaitForEnemy(0.2f);
            Vector3 dirToPlayer = trueTarget.position - singleSwingEffect.transform.position;
            float angleToPlayer = Mathf.Atan2(dirToPlayer.y, dirToPlayer.x) * Mathf.Rad2Deg;
            singleSwingEffect.SetActive(false);
            singleSwingEffect.transform.SetParent(whiteGFX);
            singleSwingEffect.transform.position = whiteGFX.position;
            singleSwingEffect.transform.eulerAngles = new Vector3(0, 0, angleToPlayer);
            singleSwingEffect.SetActive(true);

            StartCoroutine(ApplyAttackInCircle(swingAttackDuration, swingRange, whiteGFX, swingAttack));
            yield return WaitForEnemy(0.2f);
            StartCoroutine(ApplyAttackInCollider(swingAttackDuration, blackGFX, ice_hitBox, swingAttack));

            yield return StartCoroutine(bossAI.StartMultipleCoroutines(new List<IEnumerator>{
               bossAI.IESingleFishDive(black_YYFFish),
               bossAI.IESingleFishDive(white_YYFFish)
            }));
            black_YYFFish.gameObject.SetActive(false);
            white_YYFFish.gameObject.SetActive(false);
            swing_outline.SetActive(false);
            singleSwingEffect.SetActive(false);
            swingEffect_ice.SetActive(false);
        }
        attackDirectionSet = false;
        yield return null;
    }

    /// <summary>
    /// Processes a melee attack on the player, applying damage, stun effects, and visual feedback.
    /// </summary>
    /// <remarks>The method determines the outcome of the attack based on the damage dealt: <list
    /// type="bullet"> <item><description>If the player counters the attack, additional effects such as camera shake,
    /// rumble, and time slow are applied, and the boss's stun value is reduced.</description></item>
    /// <item><description>If the player defends, the attack is repelled with visual and feedback
    /// effects.</description></item> <item><description>If the attack deals damage, the player is repelled with varying
    /// intensity based on local factors.</description></item> </list> This method also handles special conditions, such
    /// as triggering boss combo actions during specific health thresholds.</remarks>
    /// <param name="melee">The melee attack details, including damage, stun, and other effects.</param>
    /// <param name="attackPos">The position of the attack relative to the player.</param>
    /// <param name="offset">The offset to apply to the attack position.</param>
    public override void HitPlayer(MeleeAttack melee, Transform attackPos, Vector3 offset = default)
    {
        MeleeAttackResult dealtDamage = playerIDamagable.DamageFromMeleeAttack(attackPos, melee.damage, melee.breakAmount);
        bool left = playerIDamagable.GetHitPos().x < attackPos.position.x ? true : false;

        if (dealtDamage == MeleeAttackResult.Countered)//counter attack
        {
            vfx.MeleeAttackEffect(melee, playerIDamagable, left);
            bossAI.DoBreak(bossbreakValue);
            //counter attack feedback
        }
        else if (dealtDamage == MeleeAttackResult.Defended)//defend
        {
            vfx.MeleeAttackEffect(melee, playerIDamagable, left);
        }
        else if (dealtDamage == MeleeAttackResult.DamagedSuccessfully)//dealtDamage
        {
            vfx.MeleeAttackEffect(melee, playerIDamagable, left);
        }
    }

    public void OnDrawGizmos()
    {
        if (!showRange) return;
        Gizmos.DrawWireSphere(transform.position, swingRange);
    }
}