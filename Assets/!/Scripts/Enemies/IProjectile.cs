using Codice.CM.Common;
using EditorAttributes;
using log4net.Util;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(Rigidbody2D))]
public abstract class IProjectile : MonoBehaviour
{
    public bool showPivot;
    [ShowIf(nameof(showPivot))] public Vector3 pivotOffset;
    [ShowIf(nameof(showPivot))] public Color color = Color.red;

    [Header("Attributes")]
    public IProjectileBasicAttributes attribute;

    public LayerMask stopLayer = 1 << 7 | 1 << 10 | 1 << 11 | 1 << 18;
    public float rotationSpeed = 100f;
    public float lifeTime = 10f;
    public float delay = 0f;

    public ProjectileHitEffectSettings hitEffectSettings;

    [HideInInspector] public float originalSpeed;
    [HideInInspector] public GameObject owner;
    [HideInInspector] public bool followTarget;
    [HideInInspector] public IDamagable target;
    [HideInInspector] public bool collided = false;
    [HideInInspector] public bool delayTriggered = false;
    [HideInInspector] public float lifeTimer = 0f;
    [HideInInspector] public bool isHostileToPlayer;
    [HideInInspector] public bool isPerfect;
    [HideInInspector] public bool canInterruptDelay = false;
    [HideInInspector] public bool collisionEnabled = true;
    [HideInInspector] public float delayTimer = 0;
    // References to managers

    [HideInInspector] public VFXManager vfx;
    [HideInInspector] public Rigidbody2D rb;
    [HideInInspector] public GameManager gameManager;

    public virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        vfx = VFXManager.instance;
        gameManager = GameManager.instance;
        originalSpeed = attribute.speed;
    }

    public virtual ProjectileBuilder SetUp(Vector3 direction, GameObject _owner)
    {
        ResetAttributes();
        transform.eulerAngles = direction;
        owner = _owner;
        rb = GetComponent<Rigidbody2D>();
        //rb.velocity = transform.right * attribute.speed / 10;
        return new ProjectileBuilder(this);
    }

    public virtual void PerfectCounterAttack()
    {
        vfx.RumblePulse(hitEffectSettings.frequncy_perfect.x, hitEffectSettings.frequncy_perfect.y, hitEffectSettings.rumbleDuration);
        vfx.CameraShake(hitEffectSettings.cameraShakeForce.y);
        isPerfect = true;
        vfx.SpawnHitEffect(true, GetPivot());
    }

    public virtual void NormalCounterAttack()
    {
        vfx.RumblePulse(hitEffectSettings.frequency_norm.x, hitEffectSettings.frequency_norm.y, hitEffectSettings.rumbleDuration);
        vfx.CameraShake(hitEffectSettings.cameraShakeForce.x);
        isPerfect = false;
        vfx.SpawnHitEffect(false, GetPivot());
    }

    public void ResetAttributes()
    {
        owner = null;
        followTarget = false;
        target = null;
        attribute.speed = originalSpeed;
        collided = false;
        isHostileToPlayer = true;
        delayTimer = 0f;
        delay = 0f;
        lifeTimer = 0f;
        delayTriggered = false;
        collisionEnabled = true;
    }

    public virtual void FixedUpdate()
    {
        if (IsInDelay())
        {
            rb.velocity = Vector2.zero;
            return;
        }

        if (rb.gravityScale != 0 && delayTriggered)
        {
            transform.right = rb.velocity;
        } //rotate the projectile direction following gravity
    }

    public virtual void Update()
    {
        if (collided) { return; }

        if (target != null && followTarget)
        {
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                CalculateWantedRotation(target.GetHitPos()),
                rotationSpeed * Time.deltaTime);
        }

        if (IsInDelay())
        {
            delayTimer += Time.deltaTime;
        }
        else
        {
            if (!delayTriggered)
            {
                rb.velocity = transform.right * attribute.speed / 10;
                delayTriggered = true;
                collisionEnabled = true;
            }
        }

        lifeTimer += Time.deltaTime;
        if (lifeTimer >= lifeTime)
        {
            Die();
        }
    }

    public virtual void OnTriggerEnter2D(Collider2D collision)
    {
        // If canInterruptDelay is set and we're still in delay, cancel the delay on any collision
        if (canInterruptDelay && IsInDelay())
        {
            delayTimer = delay;
        }
        if (!collisionEnabled) return;
        IDamagable target = collision.gameObject.GetComponent<IDamagable>();
        if (target != null && !IsOwner(collision.gameObject) && !collided)
        {
            if (isHostileToPlayer && collision.gameObject.layer == 13) { return; }
            if (collision.gameObject == gameManager.player && collision.gameObject.layer == 14) { return; }
            vfx.SpawnHitEffect(false, GetPivot());
            target.Damage(attribute.damage, this.transform, attribute.stunDuration, bossBreakValue: attribute.bossBreakValue);
            this.gameObject.SetActive(false);
        }
        else if (collision.gameObject != owner && (stopLayer.value & (1 << collision.gameObject.layer)) > 0)
        {
            Die();
            collided = true;
        }
    }

    public bool IsInDelay() => delay > 0f && delayTimer < delay;

    public Vector3 GetPivot()
    {
        return transform.position + transform.right * pivotOffset.x + transform.up * pivotOffset.y;
    }

    public virtual void Die()
    {
        this.gameObject.SetActive(false);
        transform.position = Vector3.zero;
        ResetAttributes();// Reset position to avoid issues when reusing the object from the pool
    }

    public abstract void HitByHSAttack();

    public abstract void HitByMeleeAttack();

    public abstract void Hit();

    public Quaternion CalculateWantedRotation(Vector3 _targetPos)
    {
        float angle = Mathf.Atan2(_targetPos.y - transform.position.y, _targetPos.x - transform.position.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(new Vector3(0, 0, angle));
        return targetRotation;
    }

    public virtual void OnDrawGizmosSelected()
    {
        if (!showPivot) return;
        Gizmos.color = color;
        Gizmos.DrawLine(this.transform.position, GetPivot());
        Gizmos.DrawWireSphere(GetPivot(), 0.05f);
    }

    public virtual bool IsOwner(GameObject obj)
    {
        if (obj == owner) return true;

        // Resolve the root IDamagable from the owner.
        // Check SubDamageable FIRST — SubDamageable also implements IDamagable,
        // so checking IDamagable first would incorrectly treat a child as the root.
        IDamagable rootOwner = null;
        if (owner.TryGetComponent<SubDamageable>(out SubDamageable ownerSub))
            rootOwner = ownerSub.ParentDamageable;
        else if (owner.TryGetComponent<IDamagable>(out IDamagable ownerIdmg))
            rootOwner = ownerIdmg;

        if (rootOwner == null) return false;

        // Check if obj is the root owner's GameObject (e.g. owner=B, obj=A)
        if (rootOwner.gameObject == obj) return true;

        // Check if obj is a sub-damageable that belongs to the root owner
        if (obj.TryGetComponent<SubDamageable>(out SubDamageable objSub))
            return rootOwner.subDamagables.Contains(objSub);

        return false;
    }
}

public class ProjectileBuilder
{
    private readonly IProjectile _projectile;

    public ProjectileBuilder(IProjectile projectile)
    {
        _projectile = projectile;
    }

    public ProjectileBuilder SetAttributes(IProjectileBasicAttributes attributes)
    {
        _projectile.attribute = attributes;
        _projectile.originalSpeed = attributes.speed;
        return this;
    }

    public ProjectileBuilder SetDamage(float damage)
    {
        _projectile.attribute.damage = damage;
        return this;
    }

    public ProjectileBuilder SetSpeed(float speed)
    {
        _projectile.attribute.speed = speed;
        _projectile.originalSpeed = speed;
        return this;
    }

    public ProjectileBuilder SetStunDuration(float stunDuration)
    {
        _projectile.attribute.stunDuration = stunDuration;
        return this;
    }

    public ProjectileBuilder SetBossBreakValue(float bossBreakValue)
    {
        _projectile.attribute.bossBreakValue = bossBreakValue;
        return this;
    }

    public ProjectileBuilder SetAdditionalSpeed(float additionSpeed)
    {
        _projectile.attribute.speed = _projectile.originalSpeed + additionSpeed;
        return this;
    }

    public ProjectileBuilder SetFollowTarget(IDamagable target)
    {
        if (target == null) return this;
        _projectile.target = target;
        _projectile.followTarget = true;
        if (target != null) _projectile.transform.rotation = _projectile.CalculateWantedRotation(target.GetHitPos());
        return this;
    }

    public ProjectileBuilder SetTarget(IDamagable target)
    {
        if (target == null) return this;
        _projectile.target = target;
        if (target != null) _projectile.transform.rotation = _projectile.CalculateWantedRotation(target.GetHitPos());
        return this;
    }

    public ProjectileBuilder SetHostileToPlayer(bool hostile = true)
    {
        _projectile.isHostileToPlayer = hostile;
        return this;
    }

    public ProjectileBuilder SetGravity(float gravityScale)
    {
        _projectile.rb.gravityScale = gravityScale;
        return this;
    }

    public ProjectileBuilder SetDelay(float delay, bool canInterruptDelay)
    {
        _projectile.delay = delay;
        _projectile.delayTriggered = false;
        _projectile.canInterruptDelay = canInterruptDelay;
        if (!canInterruptDelay)
        {
            _projectile.collisionEnabled = false;
        }

        return this;
    }
}

[System.Serializable]
public struct IProjectileBasicAttributes
{
    public float damage;
    public float speed;
    public float stunDuration;
    public float bossBreakValue;
}

[System.Serializable]
public struct ProjectileHitEffectSettings
{
    [EditorAttributes.TabGroup(nameof(RumbleSettings), nameof(TimeSettings), nameof(OtherSettings))]
    [SerializeField] private Void groupHolder;

    [EditorAttributes.VerticalGroup(nameof(rumbleDuration), nameof(frequency_norm), nameof(frequncy_perfect))]
    [SerializeField, HideInInspector] private Void RumbleSettings;

    [HideInInspector][Range(0, 1f)] public float rumbleDuration;
    [HideInInspector][EditorAttributes.MinMaxSlider(0, 1.5f)] public Vector2 frequency_norm;
    [HideInInspector][EditorAttributes.MinMaxSlider(0, 1.5f)] public Vector2 frequncy_perfect;

    [EditorAttributes.VerticalGroup(nameof(freezeTime), nameof(Time_scale))]
    [SerializeField, HideInInspector] private Void TimeSettings;

    [HideInInspector][Range(0, 0.5f)] public float freezeTime;
    [HideInInspector][Range(0, 1f)] public float Time_scale;

    [EditorAttributes.VerticalGroup(nameof(cameraShakeForce), nameof(repelForce))]
    [SerializeField, HideInInspector] private Void OtherSettings;

    [HideInInspector][Range(0, 300f)] public float repelForce;
    [HideInInspector][EditorAttributes.MinMaxSlider(0, 0.2f)] public Vector2 cameraShakeForce;
}