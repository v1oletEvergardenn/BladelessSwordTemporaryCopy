using EditorAttributes;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class HS_SlashWave : IHeartSwordAbility
{
    #region Fields and Properties

    public float actionDuration = 1.5f;
    private float holdThreshold = 0.4f;
    private bool largeSlash = false;
    private float holdTimer = 0f;
    private bool attacked = false;

    [TabGroup(nameof(smallSlashSettings), nameof(largeSlashSettings))]
    [SerializeField] private Void groupHolder;

    [VerticalGroup(nameof(small_slash_damage), nameof(small_slash_speed), nameof(small_slash_stun))]
    [SerializeField, HideInInspector] private Void smallSlashSettings;

    [VerticalGroup(nameof(large_slash_damage), nameof(large_slash_speed), nameof(large_slash_stun))]
    [SerializeField, HideInInspector] private Void largeSlashSettings;

    [SerializeField, HideProperty, PropertyWidth(200f)] public int small_slash_damage = 2;
    [SerializeField, HideProperty, PropertyWidth(200f)] public float small_slash_speed = 200f;
    [SerializeField, HideProperty, PropertyWidth(200f)] public float small_slash_stun = 5f;

    [SerializeField, HideProperty, PropertyWidth(200f)] public int large_slash_damage = 4;
    [SerializeField, HideProperty, PropertyWidth(200f)] public float large_slash_speed = 40f;
    [SerializeField, HideProperty, PropertyWidth(200f)] public float large_slash_stun = 10f;

    private bool effectPlayed = false;

    #endregion Fields and Properties

    #region Unity Lifecycle

    // If you have a Start method, place it here

    public void Update()
    {
        if (!isActive) return;
        if (!isPerforming) return;
        if ((inputMaster._attackLeftAction.IsPressed()
            || inputMaster._attackRightAction.IsPressed())
            && !attacked)
        {
            holdTimer += Time.unscaledDeltaTime;
            if (holdTimer >= holdThreshold)
            {
                if (!effectPlayed)
                {
                    vfx.SpawnEffectWithEnum(Hit_Effect.slash, health.GetHitPos());
                    vfx.RumblePulse(0.3f, 0.4f, 0.2f);
                    effectPlayed = true;
                }
                largeSlash = true;
            }
        }
        if ((inputMaster._attackLeftAction.WasReleasedThisFrame()
            || inputMaster._attackRightAction.WasReleasedThisFrame())
            && !attacked)
        {
            animSet.Anim_Move(0);
            playerAttack.combatTimer = 5f;
            attacked = true;
            co_ability = StartCoroutine(OriginalAct());
        }
    }

    #endregion Unity Lifecycle

    #region Original Ability Performance

    public override bool OriginalAbilityPerformance(bool isLeft)
    {
        if (!CheckEnoughHeartSwordPoints()) return false;
        if (CheckAnyPerformingAbility()) return false;
        if (health.stunned) return false;
        hSAbilityManager.ModifyHSPoint(-GetCurrentAttribute().HS_Cost);
        largeSlash = false;
        hsHitEffectPlayed = false;
        effectPlayed = false;
        hsHitTargets.Clear();
        holdTimer = 0f;
        isPerforming = true;

        return true;
    }

    public override IEnumerator OriginalAct()
    {
        playerAttack.canDefend = false;
        if (largeSlash)
        {
            Attack();
            if (VFXManager.isInBulletTime) yield return new WaitForSecondsRealtime(0.03f);
            else yield return new WaitForSeconds(0.03f);
            LaunchSlash(largeSlash);
        }
        else
        {
            Attack();
            if (VFXManager.isInBulletTime) yield return new WaitForSecondsRealtime(0.03f);
            else yield return new WaitForSeconds(0.03f);
            LaunchSlash(largeSlash);

            if (VFXManager.isInBulletTime) yield return new WaitForSecondsRealtime(0.21f);
            else yield return new WaitForSeconds(0.21f);
            Attack();
            if (VFXManager.isInBulletTime) yield return new WaitForSecondsRealtime(0.03f);
            else yield return new WaitForSeconds(0.03f);
            LaunchSlash(largeSlash);

            if (VFXManager.isInBulletTime) yield return new WaitForSecondsRealtime(0.21f);
            else yield return new WaitForSeconds(0.21f);
            Attack();
            if (VFXManager.isInBulletTime) yield return new WaitForSecondsRealtime(0.03f);
            else yield return new WaitForSeconds(0.03f);
            LaunchSlash(largeSlash);
        }

        if (VFXManager.isInBulletTime) yield return new WaitForSecondsRealtime(0.16f);
        else yield return new WaitForSeconds(0.16f);
        EndAction();
        animSet.Anim_Move(1);
    }

    #endregion Original Ability Performance

    #region Branch Ability Performance

    public override bool FirstBranchAbilityPerformance(bool isLeft)
    {
        if (!playerAttack.canAttack) return false;
        if (controller.isFloating) return false;
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
        if (!playerAttack.canAttack) return false;
        if (controller.isFloating) return false;
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

    public void Attack()
    {
        playerAttack.canAttack = true;
        playerAttack.attackTimer = playerAttack.attackGap + 0.5f;
        bool isLeft = playerAttack.pointerDirection.z > 90f && playerAttack.pointerDirection.z < 270f;
        playerAttack.Attack(isLeft, false);
    }

    public void LaunchSlash(bool large)
    {
        HS_slash_wave_projectile slash = selfPooler.SpawnFromPool("HS_slash_wave_" + (large ? "large" : "small"), playerAttack.counterAttackPoint.position).GetComponent<HS_slash_wave_projectile>();
        slash.SetUp(playerAttack.pointerDirection,
            playerAttack.gameObject,
            _isHostileToPlayer: false,
            _speed: (large ? large_slash_speed : small_slash_speed),
            _damage: (large ? large_slash_damage : small_slash_damage),
            _stunValue: (large ? large_slash_stun : small_slash_stun));
    }

    public override void EndAction()
    {
        if (controller != null) controller.canSwitchNormalAnim = true;
        isPerforming = false;
        hsHitEffectPlayed = false;
        attacked = false;
    }

    #endregion Utility Methods
}