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
    public GameObject swingEffect;
    public GameObject sinlgeSwingEffect;
    public GameObject swing_outline;

    public MeleeAttack swingAttack = new MeleeAttack(2, 0.5f, 0.2f, new Vector2(0.25f, 0.4f), 0.2f, 20f, 0.1f);
    public float swingRange;
    public float largeSwingRange;
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
        sinlgeSwingEffect.SetActive(false);
        swing_outline.SetActive(false);
    }

    public override IEnumerator Act_coroutine(float factor = 0)
    {
        proceedCall = false;
        if (factor == 0 || factor == 1)
        {
            bool isBlack = factor == 0 ? true : false;
            Animator anim = isBlack ? bossAI.blackAnim : bossAI.whiteAnim;
            Transform fish = isBlack ? bossAI.blackFish : bossAI.whiteFish;
            Transform origin = isBlack ? bossAI.blackOrigin : bossAI.whiteOrigin;
            Transform fishGFX = isBlack ? bossAI.blackFishGFX : bossAI.whiteFishGFX;

            Vector3 target = player.transform.position + new Vector3(0, 3, 0);
            bool toLeft = target.x < origin.position.x;

            // move to appropriate x position
            if (toLeft) { origin.DOMove(new Vector3(target.x + 2, bossAI.waterLevel.position.y - 6, 0), 0.5f); }
            else { origin.DOMove(new Vector3(target.x - 2, bossAI.waterLevel.position.y - 6, 0), 0.5f); }

            //reset to initial
            origin.eulerAngles = Vector3.zero;
            fish.localPosition = new Vector3(0, 1, 0);
            fishGFX.DOLocalRotate(new Vector3(0, 0, 0), 0.1f);
            fishGFX.DOLocalMove(new Vector3(0, 0, 0), 0.1f);

            // rotate to target position angle
            float angle = -90;
            origin.Rotate(bossAI.Dir, angle);
            yield return new WaitForSeconds(0.5f);

            //jump out
            float temp_x = toLeft ? player.transform.position.x - 2 : player.transform.position.x + 2;
            origin.DOMove(new Vector3(temp_x, bossAI.waterLevel.position.y + 4.5f, 0), 0.5f).SetEase(Ease.OutSine);

            //pre swing attack
            anim.Play("swing");
            fishGFX.DOLocalRotate(new Vector3(0, 0, -720), 0.6f, RotateMode.FastBeyond360).SetEase(Ease.OutSine);
            yield return new WaitForSeconds(0.35f);

            swing_outline.transform.SetParent(fishGFX);
            swing_outline.transform.position = fishGFX.position;
            swing_outline.SetActive(true);

            //swing attack movement
            yield return new WaitForSeconds(0.3f);
            if (isBlack)
            {
                //ice attack
            }

            float _x = origin.position.x + 5;
            if (player.transform.position.x <= fish.position.x) { _x = fish.position.x - 5; }
            origin.DOMoveX(_x, 0.3f).SetEase(Ease.InQuint);

            //actual attack
            yield return new WaitForSeconds(0.2f);
            Vector3 dirToPlayer = player.transform.position - sinlgeSwingEffect.transform.position;
            float angleToPlayer = Mathf.Atan2(dirToPlayer.y, dirToPlayer.x) * Mathf.Rad2Deg;
            sinlgeSwingEffect.transform.SetParent(fishGFX);
            sinlgeSwingEffect.transform.position = fishGFX.position;
            sinlgeSwingEffect.transform.eulerAngles = new Vector3(0, 0, angleToPlayer);
            sinlgeSwingEffect.SetActive(true);
            StartCoroutine(ApplyAttackInCircle(swingAttackDuration, isBlack ? swingRange : largeSwingRange, fishGFX, swingAttack));

            yield return new WaitForSeconds(0.2f);
            //return to water
            yield return bossAI.co_return_singleFishDive = StartCoroutine(bossAI.IESingleFishDive(isBlack, !toLeft));
        }
        else if (factor == 2)
        {
            swing_outline.transform.SetParent(transform);
            swing_outline.transform.localPosition = Vector3.zero;

            Transform origin = bossAI.fish_origin;

            //reset to initial
            origin.position = bossAI.blackOrigin.position;
            bossAI.blackOrigin.localPosition = Vector3.zero;
            bossAI.whiteOrigin.localPosition = Vector3.zero;
            bossAI.blackFish.localPosition = new Vector3(0, 0.3f, 0);
            bossAI.whiteFish.localPosition = new Vector3(0, 0.3f, 0);
            bossAI.blackOrigin.eulerAngles = Vector3.zero;
            bossAI.whiteOrigin.eulerAngles = Vector3.zero;
            bossAI.whiteFishGFX.DOLocalRotate(new Vector3(0, 0, 0), 0.1f);
            bossAI.whiteFishGFX.DOLocalMove(new Vector3(0, 0, 0), 0.1f);
            bossAI.blackFishGFX.DOLocalRotate(new Vector3(0, 0, 0), 0.1f);
            bossAI.blackFishGFX.DOLocalMove(new Vector3(0, 0, 0), 0.1f);

            // move to appropriate x position
            Vector3 target = player.transform.position + new Vector3(0, 3, 0);
            bool toLeft = target.x < origin.position.x;
            if (toLeft) { origin.DOMove(new Vector3(target.x + 2, bossAI.waterLevel.position.y - 6, 0), 0.5f); }
            else { origin.DOMove(new Vector3(target.x - 2, bossAI.waterLevel.position.y - 6, 0), 0.5f); }

            // rotate to target position angle
            float angle = -90;
            bossAI.whiteOrigin.Rotate(bossAI.Dir, angle);
            bossAI.blackOrigin.Rotate(bossAI.Dir, 90);
            yield return new WaitForSeconds(0.5f);

            //out of the water
            float temp_x = toLeft ? player.transform.position.x - 2 : player.transform.position.x + 2;
            origin.DOMove(new Vector3(temp_x, bossAI.waterLevel.position.y + 4.5f, 0), 0.5f).SetEase(Ease.OutSine);

            //pre swing attack
            bossAI.blackAnim.Play("swing");
            bossAI.whiteAnim.Play("swing");
            bossAI.blackFishGFX.DOLocalRotate(new Vector3(0, 0, -720), 0.6f, RotateMode.FastBeyond360).SetEase(Ease.OutSine);
            bossAI.whiteFishGFX.DOLocalRotate(new Vector3(0, 0, -720), 0.6f, RotateMode.FastBeyond360).SetEase(Ease.OutSine);
            yield return new WaitForSeconds(0.35f);

            swing_outline.transform.SetParent(origin);
            swing_outline.transform.position = origin.position;
            swing_outline.SetActive(true);

            //swing attack movement
            yield return new WaitForSeconds(0.3f);

            //ice attack

            float _x = origin.position.x + 5;
            if (player.transform.position.x <= origin.position.x) { _x = origin.position.x - 5; }
            origin.DOMoveX(_x, 0.3f).SetEase(Ease.InQuint);

            //actual attack
            yield return new WaitForSeconds(0.2f);
            Vector3 dirToPlayer = player.transform.position - sinlgeSwingEffect.transform.position;
            float angleToPlayer = Mathf.Atan2(dirToPlayer.y, dirToPlayer.x) * Mathf.Rad2Deg;
            swingEffect.transform.SetParent(origin);
            swingEffect.transform.position = origin.position;
            swingEffect.transform.eulerAngles = new Vector3(0, 0, angleToPlayer);
            swingEffect.SetActive(true);
            StartCoroutine(ApplyAttackInCircle(swingAttackDuration, largeSwingRange, origin, swingAttack));

            //return to water
            yield return new WaitForSeconds(0.2f);
            yield return bossAI.co_multiCoroutine = StartCoroutine(bossAI.StartMultipleCoroutines(new List<IEnumerator>() {
                 bossAI.IESingleFishDive(true, !toLeft),
                 bossAI.IESingleFishDive(false, !toLeft)
            }));
        }
        swingEffect.SetActive(false);
        swing_outline.SetActive(false);
        sinlgeSwingEffect.SetActive(false);
        bossAI.AddActionBreak(actionBreakAmount);
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
            if (localFactor == 2) { playerIDamagable.ForceRepel(melee.repel * 2, left); }
            else { playerIDamagable.Repel(melee.repel * 2, left); }
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