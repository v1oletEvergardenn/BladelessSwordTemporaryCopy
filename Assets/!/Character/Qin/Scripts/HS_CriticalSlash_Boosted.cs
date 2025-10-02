using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using static UnityEngine.Rendering.DebugUI;

public class HS_CriticalSlash_Boosted : IHeartSwordAbilityBranch
{
    private HS_CriticalSlash criticalSlash;

    public override bool BranchAbilityPerformance(bool isLeft)
    {
        if (!CheckEnoughHeartSwordPoints()) return false;
        if (CheckAnyPerformingAbility()) return false;
        if (health.stunned) return false;
        if (controller.FacingRight == isLeft) { controller.Flip(); }
        hSAbilityManager.ModifyHSPoint(-HS_Cost);
        print("performing branch ability");
        inputPlayer.DisableAllActions();
        animSet.Anim_Hit(0);
        controller.canSwitchNormalAnim = false;
        hsHitEffectPlayed = false;
        playerAttack.combatTimer = 5f;
        parentAbility.isPerforming = true;
        hsHitTargets.Clear();
        parentAbility.co_ability = StartCoroutine(Act());

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
        criticalSlash.hitBox.enabled = true;
        playerAttack.isCounterAttacking = true;
        while (timer < playerAttack.counterAttackCheckDuration)
        {
            timer += VFXManager.isInBulletTime ? Time.unscaledDeltaTime : Time.deltaTime;
            CheckHSCounterAttack(criticalSlash.hitBox);
            yield return null;
        }
        if (VFXManager.isInBulletTime) yield return new WaitForSecondsRealtime(criticalSlash.actionDuration - 2.76f - playerAttack.counterAttackCheckDuration);
        else yield return new WaitForSeconds(criticalSlash.actionDuration - 2.76f - playerAttack.counterAttackCheckDuration);

        criticalSlash.EndAction();
        animSet.Anim_Hit(1);
    }

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
        criticalSlash = GetComponent<HS_CriticalSlash>();
    }

    // Update is called once per frame
    private void Update()
    {
    }
}