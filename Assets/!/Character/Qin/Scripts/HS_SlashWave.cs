using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class HS_SlashWave : IHeartSwordAbility
{
    public float actionDuration = 1.5f;

    private bool largeSlash = false;

    public int small_slash_damage = 2;
    public float small_slash_speed = 200f;
    public float small_slash_stun = 5f;

    public int large_slash_damage = 4;
    public float large_slash_speed = 40f;
    public float large_slash_stun = 10f;

    public override bool PerformAbility(bool isLeft)
    {
        if (!CheckEnoughHeartSwordPoints()) return false;
        if (isPerforming) return false;
        hSAbilityManager.ModifyHSPoint(-HS_Cost);
        animSet.Anim_Move(0);
        largeSlash = isLeft;
        hsHitEffectPlayed = false;
        playerAttack.combatTimer = 5f;
        isPerforming = true;
        hsHitTargets.Clear();
        co_ability = StartCoroutine(Act());

        return true;
    }

    public void Update()
    {
        if (!isPerforming) return;

        playerAttack.canDefend = false;
    }

    public override IEnumerator Act()
    {
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
        controller.canSwitchNormalAnim = true;
        isPerforming = false;
        hsHitEffectPlayed = false;
    }
}