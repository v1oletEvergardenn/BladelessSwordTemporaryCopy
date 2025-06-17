using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.RuleTile.TilingRuleOutput;
using EditorAttributes;

[RequireComponent(typeof(Rigidbody2D))]
public abstract class IProjectile : MonoBehaviour
{
    [Header("Attributes")] public int damage = 10;
    public float speed;
    [HideInInspector] public float originalSpeed;
    public float rotationSpeed = 100f;
    public float stunDuration = 0.3f;
    public float lifeTime = 10f;
    public float stunValue = 1;
    public LayerMask stopLayer = 1 << 7 | 1 << 10 | 1 << 11 | 1 << 18;
    [HideInInspector] public GameObject owner;
    [HideInInspector] public bool followTarget;
    [HideInInspector] public IDamagable target;
    [HideInInspector] public Rigidbody2D rb;
    [HideInInspector] public bool collided = false;
    [HideInInspector] public float lifeTimer = 0f;
    [HideInInspector] public bool isHostileToPlayer;
    [HideInInspector] public bool isPerfect;

    public bool showPivot;
    [SerializeField, ShowField(nameof(showPivot))] private Vector3 pivotOffset;
    [ShowField(nameof(showPivot))] public Color color = Color.red;

    [Title("rumbling Setting", 15)][SerializeField] public float rumbleDuration_normal = 0.1f;
    [SerializeField, MinMaxSlider(0, 1.5f)] public Vector2 rumbleFrequncy_normal = new Vector2(0.25f, 0.4f);

    [Space(20f)][SerializeField] public float rumbleDuration_perfect = 0.15f;
    [SerializeField, MinMaxSlider(0, 1.5f)] public Vector2 rumbleFrequncy_perfect = new Vector2(0.5f, 1f);

    [Title("cameraShake Setting", 15)]
    [SerializeField, Suffix("perfect"), MinMaxSlider(0, 0.2f)] public Vector2 cameraShakeForce = new Vector2(0, 0.1f);

    public float freezeTimeDuration = 0.2f;
    public float slowTimeScale = 0.1f;
    public float repelForce = 20f;

    [HideInInspector] public VFXManager vfx;
    [HideInInspector] public GameManager gameManager;

    public virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        vfx = VFXManager.instance;
        gameManager = GameManager.instance;
        originalSpeed = speed;
    }

    /// <summary>
    /// Initializes or resets the projectile's attributes for reuse, including direction, owner, speed, target, and various optional behaviors.
    /// This method allows flexible configuration of the projectile's movement, damage, targeting, and interaction logic,
    /// making it suitable for pooling and dynamic setup at runtime.
    /// </summary>
    /// <param name="dir">The initial direction (in Euler angles) to orient the projectile.</param>
    /// <param name="_owner">The GameObject that owns or fired the projectile (used to prevent self-collision).</param>
    /// <param name="additionSpeed">Additional speed to add to the projectile's base speed (default: 0).</param>
    /// <param name="_followTarget">If true, the projectile will continuously follow its target (default: false).</param>
    /// <param name="_target">The target to follow or face, implementing IDamagable (default: null).</param>
    /// <param name="_isHostileToPlayer">If true, the projectile is hostile to the player (default: true).</param>
    /// <param name="_damage">Overrides the projectile's damage value if not zero (default: 0).</param>
    /// <param name="_speed">Overrides the projectile's speed if not -1 (default: -1).</param>
    /// <param name="gravityScale">Sets the Rigidbody2D's gravity scale (default: 0).</param>
    /// <param name="_stunValue">Overrides the projectile's stun value if not zero (default: 0).</param>
    public virtual void SetUp(Vector3 dir,
        GameObject _owner,
        float additionSpeed = 0f,
        bool _followTarget = false,
        IDamagable _target = null,
        bool _isHostileToPlayer = true,
        int _damage = 0,
        float _speed = -1,
        float gravityScale = 0,
        float _stunValue = 0)
    {
        ResetAttributes();
        owner = _owner;
        followTarget = _followTarget;
        target = _target;
        isHostileToPlayer = _isHostileToPlayer;
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = gravityScale;
        if (_stunValue != 0) { stunValue = _stunValue; }
        if (_damage != 0) damage = _damage;
        transform.eulerAngles = dir;// rotate to given direction
        if (target != null) { transform.rotation = CalculateWantedRotation(target.GetHitPos()); } //rotate to face target
        if (_speed != -1) { speed = _speed; originalSpeed = _speed; }
        speed = originalSpeed + additionSpeed;
        isPerfect = false;
        lifeTimer = 0f;
        rb.velocity = transform.right * speed / 10;
    }

    public virtual void PerfectCounterAttack()
    {
        vfx.RumblePulse(rumbleFrequncy_perfect.x, rumbleFrequncy_perfect.y, rumbleDuration_perfect);
        vfx.CameraShake(cameraShakeForce.y);
        isPerfect = true;
        vfx.SpawnHitEffect(true, GetPivot());
    }

    public virtual void NormalCounterAttack()
    {
        vfx.RumblePulse(rumbleFrequncy_normal.x, rumbleFrequncy_normal.y, rumbleDuration_normal);
        vfx.CameraShake(cameraShakeForce.x);
        isPerfect = false;
        vfx.SpawnHitEffect(false, GetPivot());
    }

    public void ResetAttributes()
    {
        owner = null;
        followTarget = false;
        target = null;
        speed = originalSpeed;
        collided = false;
        isHostileToPlayer = true;
    }

    public virtual void FixedUpdate()
    {
        if (rb.gravityScale != 0)
        {
            transform.right = rb.velocity;
        }//rotate the projectile direction following gravity
    }

    public virtual void Update()
    {
        if (collided) { return; }
        lifeTimer += Time.deltaTime;
        if (lifeTimer >= lifeTime)
        {
            Die();
        }
        if (target != null)
        {
            if (followTarget)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, CalculateWantedRotation(target.GetHitPos()), rotationSpeed * Time.deltaTime);
            }//follow target
        }
    }

    public virtual void OnTriggerEnter2D(Collider2D collision)
    {
        IDamagable target = collision.gameObject.GetComponent<IDamagable>();
        if (target != null && collision.gameObject != owner && !collided)
        {
            if (isHostileToPlayer && collision.gameObject.layer == 13) { return; }
            if (collision.gameObject == gameManager.Player && collision.gameObject.layer == 14) { return; }
            vfx.SpawnHitEffect(false, GetPivot());
            target.Damage(damage, transform, stunDuration, stunValue: stunValue);
            this.gameObject.SetActive(false);
        }
        else if (collision.gameObject != owner && (stopLayer.value & (1 << collision.gameObject.layer)) > 0)
        {
            Die();
            collided = true;
        }
    }

    public Vector3 GetPivot()
    {
        return transform.position + transform.right * pivotOffset.x + transform.up * pivotOffset.y;
    }

    public virtual void OnDrawGizmosSelected()
    {
        if (!showPivot) return;
        Gizmos.color = color;
        Gizmos.DrawLine(this.transform.position, GetPivot());
        Gizmos.DrawWireSphere(GetPivot(), 0.05f);
    }

    public virtual void Die()
    {
        this.gameObject.SetActive(false);
    }

    public Quaternion CalculateWantedRotation(Vector3 _targetPos)
    {
        float angle = Mathf.Atan2(_targetPos.y - transform.position.y, _targetPos.x - transform.position.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(new Vector3(0, 0, angle));
        return targetRotation;
    }
}