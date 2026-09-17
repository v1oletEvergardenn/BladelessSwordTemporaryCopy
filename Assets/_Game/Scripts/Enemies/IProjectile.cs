using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;

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

    public ProjectileHit hitEffect;

    public float originalSpeed;
    public GameObject owner;
    public bool followTarget;
    public Transform target;
    public bool collided = false;
    public bool delayTriggered = false;
    public float lifeTimer = 0f;
    public bool isHostileToPlayer;
    public bool isPerfect;
    public bool canInterruptDelay = false;
    public bool collisionEnabled = true;
    public float delayTimer = 0;
    public bool aiming = false;
    // References to managers

    public VFXManager vfx;
    public Rigidbody2D rb;
    public GameManager gameManager;

    public string formationSound = "projectile_formation";
    public string counterAttackSound_normal = "projectile_counter";
    public string counterAttackSound_perfect = "projectile_counter_perfect";
    public string hitSound = "projectile_hit";

    public bool muteHitSound = false;

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
        return new ProjectileBuilder(this);
    }

    public virtual void PerfectCounterAttack()
    {
        hitEffect.AllEffects(ProjectileHitResult.Perfect);
        isPerfect = true;
    }

    public virtual void NormalCounterAttack()
    {
        hitEffect.AllEffects(ProjectileHitResult.Normal);
        isPerfect = false;
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
        aiming = false;
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

        float dt = TimeScaleManager.Delta(TimeChannel.Projectile);

        if (target != null && (followTarget || aiming))
        {
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                CalculateWantedRotation(GetTargetHitPosition(target)),
                rotationSpeed * dt);
        }

        if (IsInDelay())
        {
            delayTimer += dt;
        }
        else
        {
            if (!delayTriggered)
            {
                rb.velocity = transform.right * attribute.speed / 10f * TimeScaleManager.ProjScale;
                aiming = false;
                delayTriggered = true;
                collisionEnabled = true;
            }
            else if (rb.gravityScale == 0f)
            {
                rb.velocity = transform.right * attribute.speed / 10f * TimeScaleManager.ProjScale;
            }
        }

        lifeTimer += dt;
        if (lifeTimer >= lifeTime)
        {
            Die();
        }
    }

    public virtual void OnTriggerEnter2D(Collider2D collision)
    {
        CheckCollision(collision);
    }

    public void CheckHitPlayer(IDamagable target)
    {
        //get counterAttacked;
        if ((gameManager.player.GetComponent<PlayerAttack>().isCounterAttacking &&
                    gameManager.player.GetComponent<PlayerAttack>().isAttackingLeft == IsFacingRight())
                    || gameManager.player.GetComponent<PlayerAttack>().isOnStorm)
        {
            hitEffect.Repel(ProjectileHitResult.Perfect, this, target);
            gameManager.player.GetComponent<PlayerAttack>().CounterAttack(this, true);
        }
        else//hit Player, damage and repel
        {
            hitEffect.AllEffectsWithRepel(ProjectileHitResult.Hit, this, target);
            vfx.SpawnHitEffect(false, GetHitPos());
            target.Damage(attribute, transform);
            Hit();
        }
    }

    public void CheckCollision(Collider2D collision)
    {
        // If canInterruptDelay is set and we're still in delay, cancel the delay on any collision
        if (canInterruptDelay && IsInDelay()) delayTimer = delay;
        if (!collisionEnabled) return;
        IDamagable target = collision.gameObject.GetComponent<IDamagable>();

        if (target != null && !IsOwner(collision.gameObject) && !collided)
        {
            //check if the target is hostile to the projectile
            if (CheckHostile(collision.gameObject)) { return; }
            // hit player
            else if (collision.gameObject == gameManager.player)
            {
                if (target.gameObject.layer == 14) { return; }
                if (!isHostileToPlayer) { return; }
                CheckHitPlayer(target);
                SoundManager.PlaySound(hitSound);
            }
            // hit other things
            else
            {
                SoundManager.PlaySound(hitSound);
                vfx.SpawnHitEffect(false, GetHitPos());
                target.Damage(attribute, transform);
                Hit();
            }
        }
        //hit ground or wall
        else if (collision.gameObject != owner &&
            (stopLayer.value & (1 << collision.gameObject.layer)) > 0)
        {
            Die();
            SoundManager.PlaySound(hitSound);
            collided = true;
        }
    }

    public void CheckCollisionHS(Collider2D collision)
    {
        IProjectile proj = collision.gameObject.GetComponent<IProjectile>();
        if (proj != null && proj.isHostileToPlayer && !collided)
        {
            proj.HitByHSAttack();
        }
    }

    public bool IsInDelay() => delay > 0f && delayTimer < delay;

    public Vector3 GetHitPos()
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
        Gizmos.DrawLine(this.transform.position, GetHitPos());
        Gizmos.DrawWireSphere(GetHitPos(), 0.05f);
    }

    public virtual bool IsOwner(GameObject obj)
    {
        if (obj == owner) return true;
        if (owner == null) return false;
        // Resolve the root IDamagable from the owner.
        // Check SubDamageable FIRST ?SubDamageable also implements IDamagable,
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

    public Vector3 GetTargetHitPosition(Transform target)
    {
        Vector3 Pos = target.position;
        if (target.TryGetComponent<IDamagable>(out var damagable))
        {
            Pos = damagable.GetHitPos();
        }
        return Pos;
    }

    public bool IsFacingRight()
    {
        return transform.right.x >= 0;
    }

    public IEnumerator WaitForProj(float i)
    {
        yield return TimeScaleManager.WaitForChannelSeconds(i, TimeChannel.Projectile);
    }

    public bool CheckHostile(GameObject target) => isHostileToPlayer && (target.layer == 13 || target.layer == 25);
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
        _projectile.attribute.speed += additionSpeed;
        return this;
    }

    public ProjectileBuilder SetHitEffect(ProjectileHit hitEffect)
    {
        _projectile.hitEffect = hitEffect;
        return this;
    }

    public ProjectileBuilder SetFollowTarget(IDamagable target)
    {
        if (target == null) return this;
        _projectile.target = target.transform;
        _projectile.followTarget = true;
        if (target != null) _projectile.transform.rotation = _projectile.CalculateWantedRotation(target.GetHitPos());
        return this;
    }

    public ProjectileBuilder SetFollowTarget(Transform target)
    {
        if (target == null) return this;
        _projectile.target = target;
        _projectile.followTarget = true;
        if (target != null) _projectile.transform.rotation = _projectile.CalculateWantedRotation(_projectile.GetTargetHitPosition(target));
        return this;
    }

    public ProjectileBuilder SetTarget(IDamagable target)
    {
        if (target == null) return this;
        _projectile.target = target.transform;
        if (target != null) _projectile.transform.rotation = _projectile.CalculateWantedRotation(target.GetHitPos());
        return this;
    }

    public ProjectileBuilder SetTarget(Transform target)
    {
        if (target == null) return this;
        _projectile.target = target;
        _projectile.transform.rotation = _projectile.CalculateWantedRotation(target.position);
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

    public ProjectileBuilder SetDelay(float delay, bool canInterruptDelay, bool aiming = false)
    {
        _projectile.delay = delay;
        _projectile.delayTriggered = false;
        _projectile.canInterruptDelay = canInterruptDelay;
        _projectile.aiming = aiming;
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

public enum ProjectileHitResult
{
    Hit,
    Perfect,
    Normal
}

[System.Serializable]
public class ProjectileHit
{
    [LabelText("Copy Perfect"), OnValueChanged(nameof(OnCopyPerfectForHitChanged))]
    public bool copyPerfectForHit;

    [LabelText("Copy Normal"), OnValueChanged(nameof(OnCopyNormalForHitChanged))]
    public bool copyNormalForHit;

    [DisableIf(nameof(IsHitCopied)), OnValueChanged(nameof(OnHitValuesChanged))]
    public Vector2 frequency_hit = new Vector2(0.1f, 0.3f);

    [DisableIf(nameof(IsHitCopied)), Range(0, 1f), OnValueChanged(nameof(OnHitValuesChanged))] public float rumbleDuration_hit = 0.1f;
    [DisableIf(nameof(IsHitCopied)), Range(0, 0.5f), OnValueChanged(nameof(OnHitValuesChanged))] public float hitFreezeDuration_hit = 0.1f;
    [DisableIf(nameof(IsHitCopied)), Range(0, 20f), OnValueChanged(nameof(OnHitValuesChanged))] public float repel_hit = 3f;
    [DisableIf(nameof(IsHitCopied)), OnValueChanged(nameof(OnHitValuesChanged))] public Vector2 cameraShakeForce_hit = new Vector2(0.05f, 0.1f);

    [LabelText("Copy Hit"), OnValueChanged(nameof(OnCopyHitForPerfectChanged))]
    public bool copyHitForPerfect;

    [LabelText("Copy Normal"), OnValueChanged(nameof(OnCopyNormalForPerfectChanged))]
    public bool copyNormalForPerfect;

    [DisableIf(nameof(IsPerfectCopied)), OnValueChanged(nameof(OnPerfectValuesChanged))]
    public Vector2 frequency_perfect = new Vector2(0.1f, 0.3f);

    [DisableIf(nameof(IsPerfectCopied)), Range(0, 1f), OnValueChanged(nameof(OnPerfectValuesChanged))] public float rumbleDuration_perfect = 0.1f;
    [DisableIf(nameof(IsPerfectCopied)), Range(0, 0.5f), OnValueChanged(nameof(OnPerfectValuesChanged))] public float hitFreezeDuration_perfect = 0.1f;
    [DisableIf(nameof(IsPerfectCopied)), Range(0, 20f), OnValueChanged(nameof(OnPerfectValuesChanged))] public float repel_perfect = 3f;
    [DisableIf(nameof(IsPerfectCopied)), OnValueChanged(nameof(OnPerfectValuesChanged))] public Vector2 cameraShakeForce_perfect = new Vector2(0.05f, 0.1f);

    [LabelText("Copy Hit"), OnValueChanged(nameof(OnCopyHitForNormalChanged))]
    public bool copyHitForNormal;

    [LabelText("Copy Perfect"), OnValueChanged(nameof(OnCopyPerfectForNormalChanged))]
    public bool copyPerfectForNormal;

    [DisableIf(nameof(IsNormalCopied)), OnValueChanged(nameof(OnNormalValuesChanged))]
    public Vector2 frequency_normal = new Vector2(0.1f, 0.3f);

    [DisableIf(nameof(IsNormalCopied)), Range(0, 1f), OnValueChanged(nameof(OnNormalValuesChanged))] public float rumbleDuration_normal = 0.1f;
    [DisableIf(nameof(IsNormalCopied)), Range(0, 0.5f), OnValueChanged(nameof(OnNormalValuesChanged))] public float hitFreezeDuration_normal = 0.1f;
    [DisableIf(nameof(IsNormalCopied)), Range(0, 20f), OnValueChanged(nameof(OnNormalValuesChanged))] public float repel_normal = 3f;
    [DisableIf(nameof(IsNormalCopied)), OnValueChanged(nameof(OnNormalValuesChanged))] public Vector2 cameraShakeForce_normal = new Vector2(0.05f, 0.1f);

    private bool _isSyncing;
    private bool IsHitCopied => copyPerfectForHit || copyNormalForHit;
    private bool IsPerfectCopied => copyHitForPerfect || copyNormalForPerfect;
    private bool IsNormalCopied => copyHitForNormal || copyPerfectForNormal;

    private void OnCopyPerfectForHitChanged()
    {
        if (copyPerfectForHit) copyNormalForHit = false;
        SyncAllCopies();
    }

    private void OnCopyNormalForHitChanged()
    {
        if (copyNormalForHit) copyPerfectForHit = false;
        SyncAllCopies();
    }

    private void OnCopyHitForPerfectChanged()
    {
        if (copyHitForPerfect) copyNormalForPerfect = false;
        SyncAllCopies();
    }

    private void OnCopyNormalForPerfectChanged()
    {
        if (copyNormalForPerfect) copyHitForPerfect = false;
        SyncAllCopies();
    }

    private void OnCopyHitForNormalChanged()
    {
        if (copyHitForNormal) copyPerfectForNormal = false;
        SyncAllCopies();
    }

    private void OnCopyPerfectForNormalChanged()
    {
        if (copyPerfectForNormal) copyHitForNormal = false;
        SyncAllCopies();
    }

    private void OnHitValuesChanged()
    {
        if (_isSyncing) return;
        _isSyncing = true;
        if (copyHitForPerfect) CopyHitToPerfect();
        if (copyHitForNormal) CopyHitToNormal();
        _isSyncing = false;
    }

    private void OnPerfectValuesChanged()
    {
        if (_isSyncing) return;
        _isSyncing = true;
        if (copyPerfectForHit) CopyPerfectToHit();
        if (copyPerfectForNormal) CopyPerfectToNormal();
        _isSyncing = false;
    }

    private void OnNormalValuesChanged()
    {
        if (_isSyncing) return;
        _isSyncing = true;
        if (copyNormalForHit) CopyNormalToHit();
        if (copyNormalForPerfect) CopyNormalToPerfect();
        _isSyncing = false;
    }

    private void SyncAllCopies()
    {
        if (_isSyncing) return;
        _isSyncing = true;

        if (copyPerfectForHit) CopyPerfectToHit();
        else if (copyNormalForHit) CopyNormalToHit();

        if (copyHitForPerfect) CopyHitToPerfect();
        else if (copyNormalForPerfect) CopyNormalToPerfect();

        if (copyHitForNormal) CopyHitToNormal();
        else if (copyPerfectForNormal) CopyPerfectToNormal();

        _isSyncing = false;
    }

    private void CopyPerfectToHit()
    {
        frequency_hit = frequency_perfect;
        rumbleDuration_hit = rumbleDuration_perfect;
        hitFreezeDuration_hit = hitFreezeDuration_perfect;
        repel_hit = repel_perfect;
        cameraShakeForce_hit = cameraShakeForce_perfect;
    }

    private void CopyNormalToHit()
    {
        frequency_hit = frequency_normal;
        rumbleDuration_hit = rumbleDuration_normal;
        hitFreezeDuration_hit = hitFreezeDuration_normal;
        repel_hit = repel_normal;
        cameraShakeForce_hit = cameraShakeForce_normal;
    }

    private void CopyHitToPerfect()
    {
        frequency_perfect = frequency_hit;
        rumbleDuration_perfect = rumbleDuration_hit;
        hitFreezeDuration_perfect = hitFreezeDuration_hit;
        repel_perfect = repel_hit;
        cameraShakeForce_perfect = cameraShakeForce_hit;
    }

    private void CopyNormalToPerfect()
    {
        frequency_perfect = frequency_normal;
        rumbleDuration_perfect = rumbleDuration_normal;
        hitFreezeDuration_perfect = hitFreezeDuration_normal;
        repel_perfect = repel_normal;
        cameraShakeForce_perfect = cameraShakeForce_normal;
    }

    private void CopyHitToNormal()
    {
        frequency_normal = frequency_hit;
        rumbleDuration_normal = rumbleDuration_hit;
        hitFreezeDuration_normal = hitFreezeDuration_hit;
        repel_normal = repel_hit;
        cameraShakeForce_normal = cameraShakeForce_hit;
    }

    private void CopyPerfectToNormal()
    {
        frequency_normal = frequency_perfect;
        rumbleDuration_normal = rumbleDuration_perfect;
        hitFreezeDuration_normal = hitFreezeDuration_perfect;
        repel_normal = repel_perfect;
        cameraShakeForce_normal = cameraShakeForce_perfect;
    }

    public void CameraShake(ProjectileHitResult result)
    {
        switch (result)
        {
            case ProjectileHitResult.Hit:
                VFXManager.instance.CameraShake(cameraShakeForce_hit.x);
                break;

            case ProjectileHitResult.Perfect:
                VFXManager.instance.CameraShake(cameraShakeForce_perfect.x);
                break;

            case ProjectileHitResult.Normal:
                VFXManager.instance.CameraShake(cameraShakeForce_normal.x);
                break;
        }
    }

    public void RumblePulse(ProjectileHitResult result)
    {
        switch (result)
        {
            case ProjectileHitResult.Hit:
                VFXManager.instance.RumblePulse(frequency_hit.x, frequency_hit.y, rumbleDuration_hit);
                break;

            case ProjectileHitResult.Perfect:
                VFXManager.instance.RumblePulse(frequency_perfect.x, frequency_perfect.y, rumbleDuration_perfect);
                break;

            case ProjectileHitResult.Normal:
                VFXManager.instance.RumblePulse(frequency_normal.x, frequency_normal.y, rumbleDuration_normal);
                break;
        }
    }

    public void HitFreeze(ProjectileHitResult result)
    {
        switch (result)
        {
            case ProjectileHitResult.Hit:
                TimeScaleManager.HitFreeze(hitFreezeDuration_hit);
                break;

            case ProjectileHitResult.Perfect:
                TimeScaleManager.HitFreeze(hitFreezeDuration_perfect);
                break;

            case ProjectileHitResult.Normal:
                TimeScaleManager.HitFreeze(hitFreezeDuration_normal);
                break;
        }
    }

    public void Repel(ProjectileHitResult result, IProjectile sender, IDamagable target)
    {
        switch (result)
        {
            case ProjectileHitResult.Hit:
                target.Repel(repel_hit, sender.transform.right.x < 0);
                break;

            case ProjectileHitResult.Perfect:
                target.Repel(repel_perfect, sender.transform.right.x < 0);
                break;

            case ProjectileHitResult.Normal:
                target.Repel(repel_normal, sender.transform.right.x < 0);
                break;
        }
    }

    public void AllEffects(ProjectileHitResult result)
    {
        CameraShake(result);
        RumblePulse(result);
        HitFreeze(result);
    }

    public void AllEffectsWithRepel(ProjectileHitResult result, IProjectile sender, IDamagable target)
    {
        CameraShake(result);
        RumblePulse(result);
        HitFreeze(result);
        Repel(result, sender, target);
    }
}