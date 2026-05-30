using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gatling_bubbles : IProjectile
{
    public string anim_after_hit = "after_hit";
    public float death_delay_time_after_hit = 0f;
    private Animator anim;
    public Hit_Effect hitEffect;
    public Gradient counterAttackedColor;
    public Gradient normalColor;
    public TrailRenderer trail;

    // Heart-shape spread behavior
    [Header("Heart Spread & Track Settings")]
    public float turnSpeed = 90f;              // How fast bullet turns toward player (degrees/sec)

    public float turnAcceleration = 120f;      // How fast turn speed ramps up for smoothness
    public float moveSpeed = 60f;              // Constant move speed
    public float aimThreshold = 3f;            // Angle threshold to stop turning

    private enum BulletPhase
    { Turning, Shooting }

    private BulletPhase currentPhase = BulletPhase.Turning;

    private IDamagable trackTarget;
    private float currentTurnSpeed = 0f;       // Ramps up for smooth turning
    private float maxTurnSpeed;
    private int turnDirection = 1;             // 1 = counter-clockwise, -1 = clockwise

    public override void Start()
    {
        base.Start();
        anim = GetComponent<Animator>();
    }

    public override ProjectileBuilder SetUp(Vector3 direction, GameObject _owner)
    {
        currentPhase = BulletPhase.Shooting;
        currentTurnSpeed = 0f;
        trackTarget = null;
        return base.SetUp(direction, _owner);
    }

    /// <summary>
    /// Sets up the bullet with heart-shape behavior.
    /// Bullet launches backwards, turns slowly toward player, then flies straight.
    /// </summary>
    /// <param name="spawnPos">Where bullet spawns</param>
    /// <param name="spreadAngle">Angle offset from backward direction (positive = left, negative = right)</param>
    /// <param name="_trackTarget">Player to aim at</param>
    /// <param name="_owner">Owner game object</param>
    /// <param name="_moveSpeed">Movement speed</param>
    /// <param name="_turnSpeed">Max turn speed</param>
    /// <param name="_damage">Bullet damage</param>
    public void SetUpHeartSpread(Vector3 spawnPos, float spreadAngle, IDamagable _trackTarget,
        GameObject _owner, float _moveSpeed, float _turnSpeed, float _damage)
    {
        ResetAttributes();
        owner = _owner;
        isHostileToPlayer = true;
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        trail.colorGradient = normalColor;
        trackTarget = _trackTarget;

        // Calculate backward direction (away from player)
        Vector3 toPlayer = _trackTarget.GetHitPos() - spawnPos;
        float angleToPlayer = Mathf.Atan2(toPlayer.y, toPlayer.x) * Mathf.Rad2Deg;
        float backwardAngle = angleToPlayer + 180f; // Opposite of player direction

        // Add spread angle offset
        float launchAngle = backwardAngle + spreadAngle;
        transform.position = spawnPos;
        transform.eulerAngles = new Vector3(0, 0, launchAngle);

        // Determine turn direction based on which side of center the bullet is
        // Left side (positive spread) turns clockwise (-1), right side turns counter-clockwise (+1)
        // This creates the heart shape - both sides curve toward center
        turnDirection = spreadAngle >= 0 ? -1 : 1;

        // Phase setup
        currentPhase = BulletPhase.Turning;
        currentTurnSpeed = 0f;
        maxTurnSpeed = _turnSpeed;

        attribute.damage = _damage;
        attribute.speed = _moveSpeed;
        moveSpeed = _moveSpeed;
        originalSpeed = _moveSpeed;

        rb.velocity = transform.right * attribute.speed;

        isPerfect = false;
        lifeTimer = 0f;
        collided = false;
    }

    public override void Update()
    {
        if (collided) return;

        lifeTimer += Time.deltaTime;
        if (lifeTimer >= lifeTime)
        {
            Die();
            return;
        }

        switch (currentPhase)
        {
            case BulletPhase.Turning:
                UpdateTurningPhase();
                break;

            case BulletPhase.Shooting:
                UpdateShootingPhase();
                break;
        }
    }

    private void UpdateTurningPhase()
    {
        if (trackTarget == null)
        {
            currentPhase = BulletPhase.Shooting;
            return;
        }

        // Smoothly ramp up turn speed for smooth curve
        currentTurnSpeed = Mathf.MoveTowards(currentTurnSpeed, maxTurnSpeed, turnAcceleration * Time.deltaTime);

        // Calculate angle to player
        Quaternion targetRotation = CalculateWantedRotation(trackTarget.GetHitPos());
        float angleToTarget = Quaternion.Angle(transform.rotation, targetRotation);

        // Check if we're aimed at the player
        if (angleToTarget <= aimThreshold)
        {
            // Snap to exact aim and stop turning
            transform.rotation = targetRotation;
            currentPhase = BulletPhase.Shooting;
        }
        else
        {
            // Turn in the predetermined direction (creates heart curve)
            float rotationAmount = currentTurnSpeed * Time.deltaTime * turnDirection;
            transform.Rotate(0, 0, rotationAmount);

            // Alternative: if we've turned past the target, snap to it
            // This prevents bullets from spinning if they overshoot
            float newAngleToTarget = Quaternion.Angle(transform.rotation, targetRotation);
            if (newAngleToTarget > angleToTarget + 5f) // We're getting further, we overshot
            {
                transform.rotation = targetRotation;
                currentPhase = BulletPhase.Shooting;
            }
        }

        // Move forward
        rb.velocity = transform.right * attribute.speed;
    }

    private void UpdateShootingPhase()
    {
        // No turning - shoot straight in current direction
        rb.velocity = transform.right * attribute.speed;
    }

    public override void OnTriggerEnter2D(Collider2D collision)
    {
        IDamagable target = collision.gameObject.GetComponent<IDamagable>();
        if (target != null && collision.gameObject != owner && !collided)
        {
            if (isHostileToPlayer && collision.gameObject.layer == 13) { return; }

            if (collision.gameObject == gameManager.player)
            {
                if (collision.gameObject.layer == 14) { return; }
                if (!isHostileToPlayer) { return; }

                vfx.RumblePulse(hitEffectSettings.frequency_norm, hitEffectSettings.rumbleDuration);
                vfx.SlowTimeForSeconds(hitEffectSettings.freezeTime, hitEffectSettings.Time_scale);
                gameManager.playerhealth.Repel(hitEffectSettings.repelForce, transform.right.x < 0 ? true : false);
            }

            Hit();

            if (collision.gameObject == gameManager.player) YingYangFish_AI.instance.gatling.Hit(this);
            else if (collision.gameObject == YingYangFish_AI.instance.gameObject) YingYangFish_AI.instance.gatling.HitSelf(this);
            else target.Damage(attribute, transform);
        }
        else if (collision.gameObject != owner && (stopLayer.value & (1 << collision.gameObject.layer)) > 0 && !collided)
        {
            Hit();
        }
    }

    public override void HitByHSAttack()
    {
        Hit();
    }

    public override void HitByMeleeAttack()
    {
        Hit();
    }

    public override void Hit()
    {
        if (anim != null) anim.Play(anim_after_hit);
        vfx.SpawnEffectWithEnum(hitEffect, transform.position);
        Stop();
        Invoke("Die", death_delay_time_after_hit);
    }

    public void Stop()
    {
        rb.velocity = Vector3.zero;
        rb.gravityScale = 0;
        attribute.speed = 0f;
        collided = true;
        currentPhase = BulletPhase.Shooting;
        trackTarget = null;
    }

    public override void PerfectCounterAttack()
    {
        base.PerfectCounterAttack();
        trail.colorGradient = counterAttackedColor;
    }

    public override void NormalCounterAttack()
    {
        base.NormalCounterAttack();
        trail.colorGradient = counterAttackedColor;
    }
}