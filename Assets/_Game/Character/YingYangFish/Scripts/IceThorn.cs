using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class IceThorn : IDamagable
{
    public bool canBeCounterAttacked = true;
    public bool canDealDamage = false;
    public MeleeAttack iceAttack;
    private Health player;
    public Transform GFX;

    public string iceThornBreakCLip = "ice_thorn_break";

    private void Start()
    {
        player = Health.instance;
    }

    // Update is called once per frame
    private void Update()
    {
    }

    public IEnumerator Action(Vector2 pos, float delay)
    {
        canBeCounterAttacked = false;
        canDealDamage = false;
        transform.position = pos;

        //move up animation
        yield return TimeScaleManager.WaitForChannelSeconds(delay, TimeChannel.Enemy);
        canBeCounterAttacked = true;
        canDealDamage = true;
        //after move up, cancel counter attack.
        canBeCounterAttacked = false;
        yield return TimeScaleManager.WaitForChannelSeconds(1.35f, TimeChannel.Enemy);
        canDealDamage = false;
        SoundManager.PlaySound(iceThornBreakCLip);
        yield return TimeScaleManager.WaitForChannelSeconds(0.5f, TimeChannel.Enemy);
        gameObject.SetActive(false);
        yield return null;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!canDealDamage) return;
        if (collision.gameObject.layer == 6 && collision.gameObject == player.gameObject)
        {
            if (canBeCounterAttacked)
            {
                //deal damage when hit player
                if (player != null)
                {
                    HitPlayer(iceAttack, transform, canCounterAttack: true);
                }
            }
            else
            {
                HitPlayer(iceAttack, transform, canCounterAttack: false);
            }
        }
    }

    public void HitPlayer(MeleeAttack melee, Transform attackPos, Vector3 offset = default, bool canCounterAttack = true)
    {
        VFXManager vfx = VFXManager.instance;
        MeleeAttackResult dealtDamage = player.DamageFromMeleeAttack(attackPos, melee.damage, melee.freezeTime, canCounterAttack);
        bool left = player.GetHitPos().x < attackPos.position.x ? true : false;

        if (dealtDamage == MeleeAttackResult.Countered)//counter attack
        {
            vfx.MeleeAttackEffect(melee, player, left);
            GFX.GetComponent<Animator>().Play("hit");
            SoundManager.PlaySound(iceThornBreakCLip);
            canDealDamage = false;
            canBeCounterAttacked = false;
        }
        else if (dealtDamage == MeleeAttackResult.Defended)//defend
        {
            vfx.MeleeAttackEffect(melee, player, left);
        }
        else if (dealtDamage == MeleeAttackResult.DamagedSuccessfully)//dealtDamage
        {
            vfx.MeleeAttackEffect(melee, player, left);
            canDealDamage = false;
            canBeCounterAttacked = false;
        }
    }
}