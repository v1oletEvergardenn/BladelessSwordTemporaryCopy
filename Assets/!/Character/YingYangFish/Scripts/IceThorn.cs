using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class IceThorn : IDamagable
{
    private bool canBeCounterAttacked = true;
    private bool canDealDamage = true;
    public Vector2 startLocalPos = new Vector2(-1.2f, -2.2f);
    public MeleeAttack iceAttack;
    private Health player;

    private void Start()
    {
        player = Health.instance;
    }

    // Update is called once per frame
    private void Update()
    {
    }

    public IEnumerator Action(Vector2 pos)
    {
        canBeCounterAttacked = true;
        canDealDamage = true;
        transform.position = pos + startLocalPos;

        //move up animation
        yield return transform
                   .DOMove(pos, 0.3f)
                   .SetEase(Ease.InSine)
                   .WaitForCompletion();
        //after move up, cancel counter attack.
        canBeCounterAttacked = false;
        yield return new WaitForSeconds(1f);

        canDealDamage = false;
        yield return new WaitForSeconds(0.4f);
        gameObject.SetActive(false);
        yield return null;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!canDealDamage) return;
        if (collision.gameObject.layer == 6)
        {
            if (canBeCounterAttacked && collision.gameObject == player.gameObject)
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
        print("damaged!");
        VFXManager vfx = VFXManager.instance;
        int dealtDamage = player.DamageFromMeleeAttack(attackPos, melee.damage, melee.stun, canCounterAttack);
        bool left = player.GetHitPos().x < attackPos.position.x ? true : false;

        if (dealtDamage == 2)//counter attack
        {
            vfx.MeleeAttackEffect(melee, player, left);
        }
        else if (dealtDamage == 1)//defend
        {
            vfx.MeleeAttackEffect(melee, player, left);
        }
        else if (dealtDamage == 0)//dealtDamage
        {
            vfx.MeleeAttackEffect(melee, player, left);
        }
    }
}