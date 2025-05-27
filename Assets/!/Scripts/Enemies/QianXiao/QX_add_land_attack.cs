using EditorAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UIElements.Experimental;
using UnityEngineInternal;

public class QX_add_land_attack : IEnemyAction
{
    private QianXiaoAI bossAI;
    public IEnemyAction actionSender;
    public float rotationSpeed = 100f;
    public float dashingSpeed = 30f;

    //private bool dashing;
    //private QX_AnimTrigger animTrigger;
    //private Vector3 desiredRotation;
    //private bool isGrounded = false;
    //private bool groundChecked = false;
    //private Vector3 dashingDir;
    public Transform slashEffectPos;

    public float attackDetectRange = 20f;
    public int slash_Damage = 10;
    public int land_Damage = 4;

    [Header("slash_effects")][SerializeField] public float slash_rumbleDuration = 0.1f;
    [SerializeField, MinMaxSlider(0, 3f)] public Vector2 slash_rumbleFrequncy = new Vector2(0.25f, 0.4f);
    public float slash_stunDuration = 0.2f;
    public float slash_freezeTimeDuration = 0.2f;
    public float slash_RepelForce = 20f;
    public float slash_cameraShakeForce = 0.2f;

    [Header("land_effects")][SerializeField] public float land_rumbleDuration = 0.1f;
    [SerializeField, MinMaxSlider(0, 3f)] public Vector2 land_rumbleFrequncy = new Vector2(0.25f, 0.4f);
    public float land_stunDuration = 0.2f;
    public float land_freezeTimeDuration = 0.2f;
    public float land_RepelForce = 20f;
    public float land_cameraShakeForce = 0.2f;

    public override void Start()
    {
        base.Start();
        bossAI = GetComponent<QianXiaoAI>();
        //animTrigger = bossAI.GFX.GetComponent<QX_AnimTrigger>();
    }

    //public void Update()
    //{
    //    if (isThisActing && bossAI.isStage2)
    //    {
    //        if (dashing)
    //        {
    //            transform.position += dashingDir * dashingSpeed * Time.deltaTime;
    //            float d = Vector3.Distance(playerIDamagable.GetHitPos(), transform.position);
    //            if (d <= attackDetectRange)
    //            {
    //                anim.Play("dash_attack_attack");
    //            }
    //            Vector3 v = bossAI.GFX.transform.localPosition;
    //            if (v.y < 0)
    //            {
    //                bossAI.GFX.transform.localPosition += new Vector3(0, 8f * Time.deltaTime, 0);
    //            }
    //            else
    //            {
    //                bossAI.GFX.transform.localPosition = new Vector3(0, 0, 0);
    //            }
    //        }
    //        if (isGrounded && !groundChecked)
    //        {
    //            anim.Play("dash_attack_land");
    //            animTrigger.Outline_MeleeActivate(0);
    //            //anim.Play("S2_land_on_ground");
    //            groundChecked = true;
    //            dashing = false;
    //            bossAI.rb.gravityScale = 60f;
    //            transform.rotation = Quaternion.identity;
    //            if (bossAI.GFX.transform.localScale.x == -1)
    //            {
    //                bossAI.GFX.transform.localScale = new Vector3(1, 1, 1);
    //                bossAI.isFacingRight = !bossAI.isFacingRight;
    //            }

    //            bossAI.canFlip = true;
    //            //land attack
    //        }
    //    }
    //}

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //if (collision.gameObject.layer == 7)
        //{
        //    isGrounded = true;
        //}
        //if (collision.gameObject.layer == 18)
        //{
        //    dashing = false;
        //    isGrounded = true;
        //}
    }

    public override void Act()
    {
        if (bossAI.isStage2) { StartCoroutine(Act_coroutine()); }
        else
        {
            bossAI.inAct = false;
            if (actionSender != null)
            {
                bossAI.NextAction();
                actionSender = null;
            }
            else
            {
                bossAI.NextAction();
            }
        }
    }

    //public override IEnumerator Act_coroutine()
    //{
    //    if (bossAI.isGrounded)
    //    {
    //        bossAI.inAct = false;
    //        if (actionSender != null)
    //        {
    //            bossAI.NextAction();
    //            actionSender = null;
    //        }
    //        else
    //        {
    //            bossAI.NextAction();
    //        }
    //        yield return null;
    //    }
    //    else
    //    {
    //        isThisActing = true;
    //        //turning to player
    //        groundChecked = false;
    //        isGrounded = false;
    //        anim.Play("dash_attack_prepare");
    //        animTrigger.Outline_MeleeActivate(1);
    //        yield return new WaitForSeconds(2f);
    //        vfx.SpawnSlashEffect(transform.position);
    //        animTrigger.Outline_Flash();
    //        yield return new WaitForSeconds(0.3f);
    //        bossAI.canFlip = false;
    //        Vector3 dir = player.transform.position - transform.position;
    //        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90;
    //        Quaternion q = Quaternion.AngleAxis(angle, Vector3.forward);
    //        if (angle >= -180f)
    //        {
    //            bossAI.GFX.transform.localScale = new Vector3(1, 1, 1);
    //            bossAI.isFacingRight = true;
    //        }
    //        else
    //        {
    //            bossAI.GFX.transform.localScale = new Vector3(-1, 1, 1);
    //            bossAI.isFacingRight = false;
    //        }

    //        transform.rotation = q;
    //        vfx.SpawnDashEffect(transform.position, transform.rotation);
    //        anim.Play("dash_attack_dashing");
    //        dashing = true;
    //        dashingDir = (player.transform.position - transform.position).normalized;
    //        yield return new WaitForSeconds(action_time);
    //        isThisActing = false;
    //        bossAI.SetFlyEngine(true);
    //        bossAI.inAct = false;
    //        if (actionSender != null)
    //        {
    //            bossAI.NextAction();
    //            actionSender = null;
    //        }
    //        else
    //        {
    //            bossAI.NextAction();
    //        }
    //    }
    //}

    public Quaternion CalculateWantedRotation(Vector3 _targetPos)
    {
        float angle = Mathf.Atan2(_targetPos.y - transform.position.y, _targetPos.x - transform.position.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(new Vector3(0, 0, angle));
        return targetRotation;
    }

    public float GetRotZFromDirection(Vector3 dir)
    {
        float rotZ = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        return rotZ;
    }

    public void DamageSlash()
    {
        int dealtDamage = playerIDamagable.DamageFromMeleeAttack(transform, slash_Damage, slash_stunDuration);

        if (dealtDamage == 2)//counter attack
        {
            //counter attack effect
            vfx.SpawnLargeSlashEffect(playerIDamagable.hitEffectPosition.position);

            playerIDamagable.Repel(slash_RepelForce, transform.right.x < 0 ? true : false);
            vfx.CameraShake(slash_cameraShakeForce);
            vfx.RumblePulse(slash_rumbleFrequncy.x * 2, slash_rumbleFrequncy.y * 2, slash_rumbleDuration * 2);
            vfx.SlowTimeForSeconds(slash_freezeTimeDuration, 0f);
        }
        else if (dealtDamage == 1)//defended
        {
            vfx.SpawnLargeSlashEffect(playerIDamagable.GetHitPos());
            playerIDamagable.Repel(slash_RepelForce, transform.right.x < 0 ? true : false);
            vfx.CameraShake(slash_cameraShakeForce);
            vfx.RumblePulse(slash_rumbleFrequncy.x * 2, slash_rumbleFrequncy.y * 2, slash_rumbleDuration * 2);
            vfx.SlowTimeForSeconds(slash_freezeTimeDuration, 0f);
        }
        else if (dealtDamage == 0)//dealt damage
        {
            vfx.SpawnLargeSlashEffect(playerIDamagable.GetHitPos());
            vfx.RumblePulse(slash_rumbleFrequncy.x, slash_rumbleFrequncy.y, slash_rumbleDuration);
            playerIDamagable.Repel(slash_RepelForce * 2, transform.right.x < 0 ? true : false);
            vfx.SlowTimeForSeconds(slash_freezeTimeDuration, 0f);
            vfx.CameraShake(slash_cameraShakeForce);
        }
    }

    public void DamageLand()
    {
        int dealtDamage = playerIDamagable.Damage(land_Damage, transform);

        if (dealtDamage == 1)//defended
        {
            vfx.SpawnSlashEffect(playerIDamagable.GetHitPos());
            playerIDamagable.Repel(land_RepelForce, transform.right.x < 0 ? true : false);
            vfx.CameraShake(land_cameraShakeForce);
            vfx.RumblePulse(land_rumbleFrequncy.x * 2, land_rumbleFrequncy.y * 2, land_rumbleDuration * 2);
            vfx.SlowTimeForSeconds(land_freezeTimeDuration, 0f);
        }
        else if (dealtDamage == 0)//dealt damage
        {
            vfx.SpawnSlashEffect(playerIDamagable.GetHitPos());
            vfx.RumblePulse(land_rumbleFrequncy.x, land_rumbleFrequncy.y, land_rumbleDuration);
            playerIDamagable.Repel(land_RepelForce * 2, transform.right.x < 0 ? true : false);
            vfx.SlowTimeForSeconds(land_freezeTimeDuration, 0f);
            vfx.CameraShake(land_cameraShakeForce);
        }
    }

    public void Land_slash_effect()
    {
        vfx.SpawnSlashEffect(transform.position + new Vector3(0, 0.2f, 0), true);
    }
}