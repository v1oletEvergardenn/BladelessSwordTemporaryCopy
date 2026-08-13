using System.Collections;
using DG.Tweening;
using UnityEngine;

public partial class PlayerControl
{
    #region Teleportation

    public void DesignatedPositionTeleport(Vector3 pos)
    {
        teleported = false;
        StartCoroutine(DesignatedTeleport(pos));
    }

    /// <summary>
    /// Sends the teleport sword to a designated world position, then teleports player to sword.
    /// This variant computes sword direction from player to target.
    /// </summary>
    public IEnumerator DesignatedTeleport(Vector3 pos)
    {
        Vector3 start = transform.position + new Vector3(0f, 1.2f, 0f);
        Vector3 dir = pos - start;

        if ((dir.x <= 0 && FacingRight) || (dir.x >= 0 && !FacingRight))
            Flip();

        gameObject.layer = 14;
        ActionLock.AddExcept(MovementLockKeys.SwordTeleport, Lock.SwordTeleport);
        if (isFalling) anim.Play("tele_pre_fall");
        else if (isJumping) anim.Play("tele_pre_jump");
        else anim.Play("tele_pre_idle");

        TeleportSword.transform.position = start;
        TeleportSword.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
        TeleportSword.SetActive(true);

        bool finished = false;
        TeleportSword.transform.DOMove(pos, TeleportDuration)
            .SetEase(Ease.Linear)
            .OnComplete(() => finished = true)
            .SetTimeDt(this, TimeChannel.Player);

        yield return new WaitUntil(() => finished);
        TeleportToSword();
        gameObject.layer = 6;
        yield return null;
    }

    /// <summary>
    /// Main player-triggered teleport coroutine.
    /// Aligns facing with aiming input, launches sword forward, then resolves teleport.
    /// </summary>
    public IEnumerator TeleportCoroutine(bool right)
    {
        if (playerAttack.isAimingRightStick)
        {
            if (inputPlayer.rightPointLeft == FacingRight) Flip();
        }
        else if (inputPlayer.leftAttackDir != Vector2.zero)
        {
            if (inputPlayer.leftPointLeft == FacingRight) Flip();
        }

        gameObject.layer = 14;
        ActionLock.AddExcept(MovementLockKeys.SwordTeleport, Lock.SwordTeleport);
        if (isFalling) anim.Play("tele_pre_fall");
        else if (isJumping) anim.Play("tele_pre_jump");
        else anim.Play("tele_pre_idle");

        TeleportSword.transform.position = transform.position + new Vector3(0f, 1.2f, 0f);
        TeleportSword.transform.eulerAngles = inputPlayer.pointer.transform.eulerAngles;
        TeleportSword.SetActive(true);

        Vector3 targetPos = TeleportSword.transform.position + TeleportSword.transform.right * TeleportDistance;
        bool finished = false;
        TeleportSword.transform.DOMove(targetPos, TeleportDuration)
            .SetEase(Ease.Linear)
            .OnComplete(() => finished = true)
            .SetTimeDt(this, TimeChannel.Player);

        yield return new WaitUntil(() => finished);
        TeleportToSword();
        gameObject.layer = 6;
        yield return null;
    }

    /// <summary>
    /// Finalizes teleport at sword position.
    /// Applies collision offsets, resets locks/cooldowns, and selects landing teleport animation.
    /// Guarded by <c>teleported</c> to avoid duplicate resolution from trigger/coroutine race.
    /// </summary>
    public void TeleportToSword()
    {
        if (teleported) return;
        teleported = true;
        gameObject.layer = 6;

        RaycastHit2D hit = Physics2D.Raycast(TeleportSword.transform.position, Vector2.down, 1.2f, teleportCheckLayer);
        RaycastHit2D hit_horizontal = Physics2D.Raycast(TeleportSword.transform.position, TeleportSword.transform.right, 0.7f, teleportCheckLayer);
        float offset_y = hit.collider != null ? 1.2f - hit.distance : 0f;
        float offset_x = 0f;

        if (hit_horizontal.collider != null)
            offset_x = FacingRight ? -hit_horizontal.distance - 0.1f : hit_horizontal.distance + 0.1f;

        rb.velocity = Vector3.zero;
        ActionLock.Remove(MovementLockKeys.SwordTeleport);
        teleportTimer = 0f;
        TeleportSword.SetActive(false);

        anim.SetBool("isCombat", true);
        PlayerAttack.instance.combatTimer = 2f;
        if (isFalling) anim.Play("tele_fall");
        else if (isRunning) anim.Play("tele_run");
        else anim.Play("tele_idle");

        transform.position = TeleportSword.transform.position + new Vector3(offset_x, offset_y - 1.2f, 0);
        rb.velocity = Vector2.zero;
    }

    #endregion Teleportation
}