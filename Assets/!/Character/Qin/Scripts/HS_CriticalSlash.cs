using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class HS_CriticalSlash : IHeartSwordAbility
{
    public float actionDuration = 1.5f;
    public Collider2D hitBox;

    public override void Start()
    {
        base.Start();
        hitBox.enabled = false;
    }

    public override bool OriginalAbilityPerformance(bool isLeft)
    {
        if (!CheckEnoughHeartSwordPoints()) return false;
        if (CheckAnyPerformingAbility()) return false;
        if (health.stunned) return false;
        if (controller.FacingRight == isLeft) { controller.Flip(); }
        hSAbilityManager.ModifyHSPoint(-GetCurrentAttribute().HS_Cost);

        print("performing original ability");

        inputPlayer.DisableAllActions();
        animSet.Anim_Hit(0);
        controller.canSwitchNormalAnim = false;
        hsHitEffectPlayed = false;
        playerAttack.combatTimer = 5f;
        isPerforming = true;
        hsHitTargets.Clear();
        co_ability = StartCoroutine(Act());

        return true;
    }

    public override IEnumerator Act()
    {
        float timer = 0f;

        anim.Play("idle");
        yield return null;
        anim.Play("HS_critical_slash");
        if (VFXManager.isInBulletTime) yield return new WaitForSecondsRealtime(2.76f);
        else yield return new WaitForSeconds(2.76f);
        hitBox.enabled = true;
        playerAttack.isCounterAttacking = true;
        while (timer < playerAttack.counterAttackCheckDuration)
        {
            timer += VFXManager.isInBulletTime ? Time.unscaledDeltaTime : Time.deltaTime;
            CheckHSCounterAttack(hitBox);
            yield return null;
        }
        if (VFXManager.isInBulletTime) yield return new WaitForSecondsRealtime(actionDuration - 2.76f - playerAttack.counterAttackCheckDuration);
        else yield return new WaitForSeconds(actionDuration - 2.76f - playerAttack.counterAttackCheckDuration);

        EndAction();
        animSet.Anim_Hit(1);
    }

    public override void EndAction()
    {
        if (controller != null) controller.canSwitchNormalAnim = true;
        isPerforming = false;
        hitBox.enabled = true;
        hsHitEffectPlayed = false;
    }
}