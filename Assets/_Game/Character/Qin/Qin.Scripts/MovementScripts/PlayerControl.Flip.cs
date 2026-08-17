using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PlayerControl
{
    #region Flipping & Facing

    public void FaceTarget(Transform target)
    {
        if ((target.position.x <= transform.position.x && FacingRight) ||
            (target.position.x > transform.position.x && !FacingRight))
        {
            Flip();
        }
    }

    public void FaceTarget(Vector3 pos)
    {
        if ((pos.x <= transform.position.x && FacingRight) ||
            (pos.x > transform.position.x && !FacingRight))
        {
            Flip();
        }
    }

    public void Face(bool right)
    {
        if (FacingRight != right)
        {
            Flip();
        }
    }

    /// <summary>
    /// Handles direction flip rules during regular movement and counter-attack states.
    /// Counter-attack path preserves animation continuity before applying final facing change.
    /// </summary>
    public void HandleFlipping(float move)
    {
        if (playerAttack.isCounterAttacking)
        {
            if (isRunning)
            {
                bool shouldFlip = (move > 0f && !FacingRight) || (move < 0f && FacingRight);
                if (!shouldFlip)
                    return;

                var state = anim.GetCurrentAnimatorStateInfo(0);
                float duration = state.normalizedTime;
                if (IsAttackRunState(state) && playerAttack.isAttackingLeft == (move > 0f))
                {
                    if (playerAttack.attackIndex == 1 && duration < (35f / 71f))
                        anim.Play("attack_back_" + playerAttack.attackIndex, 0, duration * (71f / 35f));
                    else if (playerAttack.attackIndex == 2)
                        anim.Play("attack_back_" + playerAttack.attackIndex, 0, duration);
                    else
                        anim.Play(anim.GetBool("storm") ? "storm_pre_run" : "run_combat");
                }

                Flip();
                return;
            }

            if (playerAttack.isAttackingLeft == FacingRight)
                Flip();

            return;
        }

        if (move > 0f && !FacingRight) Flip();
        else if (move < 0f && FacingRight) Flip();
    }

    public void Flip(bool ignoreCamFollowFlip = false)
    {
        if (!CanFlip()) return;
        var state = anim.GetCurrentAnimatorStateInfo(0);
        if ((state.IsName("attack_back_" + playerAttack.attackIndex) ||
            state.IsName("HS_attack_back_" + playerAttack.attackIndex)) &&
            playerAttack.isAttackingLeft == FacingRight)
            return;

        FacingRight = !FacingRight;
        transform.Rotate(new Vector3(0, 1, 0), 180);
        playerAttack.counterAttackPoint.Rotate(new Vector3(1, 0, 0), 180);

        if (!ignoreCamFollowFlip && camFollowDirection != FacingRight)
        {
            camFollowDirection = !camFollowDirection;
            camFollow.CallTurn();
        }
    }

    public void ForceFlip()
    {
        FacingRight = !FacingRight;
        transform.Rotate(new Vector3(0, 1, 0), 180);
        playerAttack.counterAttackPoint.Rotate(new Vector3(1, 0, 0), 180);

        if (camFollowDirection != FacingRight)
        {
            camFollowDirection = !camFollowDirection;
            camFollow.CallTurn();
        }
    }

    #endregion Flipping & Facing
}