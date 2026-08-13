using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HS_CriticalSlash : IHeartSwordAbility
{
    public float actionDuration = 1.5f;
    public Collider2D hitBox;

    #region Unity Lifecycle

    public override void Start()
    {
        base.Start();
        hitBox.enabled = false;
    }

    #endregion Unity Lifecycle

    #region Original Ability Performance

    public override bool OriginalAbilityPerformance(bool isLeft)
    {
        if (!CheckEnoughHeartSwordPoints()) return false;
        if (CheckAnyPerformingAbility()) return false;
        if (health.stunned) return false;
        if (controller.FacingRight == isLeft) { controller.Flip(); }
        hSAbilityManager.ModifyHSPoint(-GetCurrentAttribute().HS_Cost);

        inputPlayer.DisableAllActions();

        QuestManager.OnAction(GameManager.instance.playerQuestActionKey.HS_CriticalSlash_released);
        ActionLock.Add("HS_CriticalSlash", Lock.All);
        controller.canSwitchNormalAnim = false;
        hsHitEffectPlayed = false;
        playerAttack.combatTimer = 5f;
        isPerforming = true;
        hsHitTargets.Clear();
        co_ability = StartCoroutine(OriginalAct());

        return true;
    }

    public override IEnumerator OriginalAct()
    {
        float timer = 0f;

        anim.Play("idle");
        yield return null;
        anim.Play("HS_critical_slash");
        yield return TimeScaleManager.WaitForChannelSeconds(2.76f, TimeChannel.Player);
        hitBox.enabled = true;
        playerAttack.isCounterAttacking = true;
        while (timer < playerAttack.counterAttackCheckDuration)
        {
            timer += TimeScaleManager.Delta(TimeChannel.Player);
            CheckHSCounterAttack(hitBox);
            yield return null;
        }
        yield return TimeScaleManager.WaitForChannelSeconds(
            actionDuration - 2.76f - playerAttack.counterAttackCheckDuration,
            TimeChannel.Player);

        EndAction();
    }

    #endregion Original Ability Performance

    #region Branch Ability Performance

    public override bool FirstBranchAbilityPerformance(bool isLeft)
    {
        if (!playerAttack.CanAttack()) return false;
        if (health.stunned) return false;
        if (CheckAnyPerformingAbility()) return false;
        if (playerAttack.attackTimer < playerAttack.attackGap) return false;
        if (hSAbilityManager.currentHS_point < GetCurrentAttribute().HS_Cost) { return false; }
        if (controller.FacingRight == isLeft) { controller.Flip(); }

        CancelAction();
        co_ability = StartCoroutine(FirstBranchAct());
        return true;
    }

    public override bool SecondBranchAbilityPerformance(bool isLeft)
    {
        if (!playerAttack.CanAttack()) return false;
        if (health.stunned) return false;
        if (CheckAnyPerformingAbility()) return false;
        if (playerAttack.attackTimer < playerAttack.attackGap) return false;
        if (hSAbilityManager.currentHS_point < GetCurrentAttribute().HS_Cost) { return false; }
        if (controller.FacingRight == isLeft) { controller.Flip(); }

        CancelAction();
        co_ability = StartCoroutine(SecondBranchAct());
        return true;
    }

    public override IEnumerator FirstBranchAct()
    {
        yield return StartCoroutine(OriginalAct());
    }

    public override IEnumerator SecondBranchAct()
    {
        yield return StartCoroutine(OriginalAct());
    }

    #endregion Branch Ability Performance

    #region Utility Methods

    public override void EndAction()
    {
        if (controller != null) controller.canSwitchNormalAnim = true;
        isPerforming = false;
        hitBox.enabled = false;
        hsHitEffectPlayed = false;
        ActionLock.Remove("HS_CriticalSlash");
    }

    public override void HitTarget()
    {
        QuestManager.OnAction(GameManager.instance.playerQuestActionKey.HS_CriticalSlash_hit);
    }

    #endregion Utility Methods
}