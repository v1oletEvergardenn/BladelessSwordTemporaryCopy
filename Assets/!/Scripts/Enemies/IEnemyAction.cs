using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

[Serializable]
public abstract class IEnemyAction : MonoBehaviour
{
    [HideInInspector] public Animator anim;
    [HideInInspector] public IEnemyController controller;
    [HideInInspector] public SpriteRenderer sprite;
    [HideInInspector] public GameObject player;
    [HideInInspector] public Health playerIDamagable;
    [HideInInspector] public ObjectPooler pooler;
    [HideInInspector] public VFXManager vfx;
    [HideInInspector] public Rigidbody2D rb;
    [HideInInspector] public PlayerAttack playerAttack;
    [HideInInspector] public Energy playerEnergy;
    [HideInInspector] public CharacterController2D playerController;

    [HideInInspector] public IEnemyController bossController;
    public int actionBreakAmount = 1;
    public float stunValue = 1;

    [HideInInspector] public bool proceedCall = false;
    public Coroutine act_routine;

    public virtual void Start()
    {
        bossController = GetComponent<IEnemyController>();
        playerIDamagable = Health.instance;
        player = playerIDamagable.gameObject;
        vfx = VFXManager.instance;
        rb = GetComponent<Rigidbody2D>();
        controller = GetComponent<IEnemyController>();
        anim = controller.anim;
        sprite = controller.sprite;
        pooler = ObjectPooler.instance;
        playerAttack = PlayerAttack.instance;
        playerEnergy = Energy.instance;
        playerController = CharacterController2D.instance;
        proceedCall = false;
    }

    public virtual void Act()
    {
        act_routine = StartCoroutine(Act_coroutine());
    }

    public virtual void CancelAct()
    {
        TryStopCoroutine(act_routine);
        StopAllCachedCoroutines();

        //OutLine_Activate(0);
    }

    /// <summary>
    /// Checks if the enemy can perform an action based on its current state.
    /// </summary>
    /// <returns></returns>
    public virtual bool CanAct()
    {
        return true;
    }

    public virtual IEnumerator Act_coroutine(float factor = 0)
    {
        yield return null;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="duration"></param>
    /// <param name="range"></param>
    /// <param name="attackPos"></param>
    /// <param name="melee"></param>
    /// <returns></returns>
    public virtual IEnumerator ApplyAttackInCircle(float duration, float range, Transform attackPos, MeleeAttack melee, Vector3 offset = default)
    {
        List<IDamagable> hitIdamagables = new List<IDamagable>();
        List<IProjectile> hitProjectiles = new List<IProjectile>();
        float elapsedTime = 0f;
        while (elapsedTime <= duration)
        {
            elapsedTime += Time.deltaTime;
            Collider2D[] colliders = Physics2D.OverlapCircleAll(attackPos.position + offset, range);
            foreach (Collider2D collider in colliders)
            {
                if (bossController.subDamagables.Contains(collider.GetComponent<IDamagable>())
                    || collider.gameObject == this.gameObject)
                {
                    continue; // Skip if it's the boss itself or a sub-damagable of the boss
                }

                //Hit Idamagables
                if (collider.TryGetComponent<IDamagable>(out IDamagable idmg))
                {
                    if (!hitIdamagables.Contains(idmg))
                    {
                        hitIdamagables.Add(idmg);
                        foreach (IDamagable subIdmg in idmg.subDamagables) { hitIdamagables.Add(subIdmg); }
                        if (idmg == playerIDamagable) { HitPlayer(melee, attackPos, offset); }
                        else { idmg.Damage(melee.damage, this.transform, melee.stun); }
                    }
                }
                //Hit Iprojectiles
                if (collider.TryGetComponent<IProjectile>(out IProjectile iProj))
                {
                    if (!hitProjectiles.Contains(iProj) || !iProj.IsOwner(this.gameObject))
                    {
                        hitProjectiles.Add(iProj);
                        iProj.HitByMeleeAttack();
                    }
                }
            }
            yield return null;
        }

        yield return null;
    }

    public virtual IEnumerator ApplyAttackInCollider(float duration, Transform attackPos, Collider2D attack_collider, MeleeAttack melee, Vector3 offset = default)
    {
        List<IDamagable> hitIdamagables = new List<IDamagable>();
        List<IProjectile> hitProjectiles = new List<IProjectile>();
        float elapsedTime = 0f;
        while (elapsedTime <= duration)
        {
            elapsedTime += Time.deltaTime;
            List<Collider2D> results = new List<Collider2D>();
            Physics2D.OverlapCollider(attack_collider, new ContactFilter2D().NoFilter(), results);
            foreach (Collider2D collider in results)
            {
                if (bossController.subDamagables.Contains(collider.GetComponent<IDamagable>())
                    || collider.gameObject == this.gameObject)
                {
                    continue; // Skip if it's the boss itself or a sub-damagable of the boss
                }

                //Hit Idamagables
                if (collider.TryGetComponent<IDamagable>(out IDamagable idmg))
                {
                    if (!hitIdamagables.Contains(idmg))
                    {
                        hitIdamagables.Add(idmg);
                        foreach (IDamagable subIdmg in idmg.subDamagables) { hitIdamagables.Add(subIdmg); }
                        if (idmg == playerIDamagable) { HitPlayer(melee, attackPos, offset); }
                        else { idmg.Damage(melee.damage, this.transform, melee.stun); }
                    }
                }
                //Hit Iprojectiles
                if (collider.TryGetComponent<IProjectile>(out IProjectile iProj))
                {
                    if (!hitProjectiles.Contains(iProj) || !iProj.IsOwner(this.gameObject))
                    {
                        hitProjectiles.Add(iProj);
                        iProj.HitByMeleeAttack();
                    }
                }
            }

            yield return null;
        }
        yield return null;
    }

    public virtual void HitPlayer(MeleeAttack melee, Transform attackPos, Vector3 offset = default)
    {
        int dealtDamage = playerIDamagable.DamageFromMeleeAttack(attackPos, melee.damage, melee.stun);
        bool left = playerIDamagable.GetHitPos().x < attackPos.position.x ? true : false;

        if (dealtDamage == 2)//counter attack
        {
            vfx.MeleeAttackEffect(melee, playerIDamagable, left);
            bossController.DecreaseStun(stunValue);
        }
        else if (dealtDamage == 1)//defend
        {
            vfx.MeleeAttackEffect(melee, playerIDamagable, left);
        }
        else if (dealtDamage == 0)//dealtDamage
        {
            vfx.MeleeAttackEffect(melee, playerIDamagable, left);
        }
    }

    public bool Possibility(float i)
    {
        float chance = UnityEngine.Random.Range(0, 100);
        if (chance <= i) { return true; }
        else { return false; }
    }

    public void TryStopCoroutine(Coroutine i)
    {
        if (i != null) { StopCoroutine(i); }
    }

    /// <summary>
    /// Calculates the desired rotation to face a specified target position.
    /// </summary>
    /// <remarks>The calculated rotation is based on the angle between the object's current position and the
    /// target position in the X-Y plane. The Z-axis rotation is adjusted accordingly.</remarks>
    /// <param name="_targetPos">The target position in world space that the rotation should face.</param>
    /// <returns>A <see cref="Quaternion"/> representing the rotation required to face the target position.</returns>
    public Quaternion CalculateWantedRotation(Vector3 _targetPos, Vector3 _originPos)
    {
        return Quaternion.Euler(CalculateWantedEuler(_targetPos, _originPos));
    }

    /// <summary>
    /// Calculates the desired Euler angle for rotation based on the position of a target relative to an origin.
    /// </summary>
    /// <remarks>This method is typically used to determine the rotation angle required to face a target
    /// position from a given origin position in a 2D plane.</remarks>
    /// <param name="_targetPos">The position of the target as a <see cref="Vector3"/>.</param>
    /// <param name="_originPos">The position of the origin as a <see cref="Vector3"/>.</param>
    /// <returns>A <see cref="Vector3"/> representing the Euler angle, where the Z component contains the angle in degrees
    /// calculated using the arctangent of the difference in Y and X coordinates between the target and origin. The X
    /// and Y components are set to 0.</returns>
    public Vector3 CalculateWantedEuler(Vector3 _targetPos, Vector3 _originPos)
    {
        return new Vector3(0, 0, Mathf.Atan2(_targetPos.y - _originPos.y, _targetPos.x - _originPos.x) * Mathf.Rad2Deg);
    }

    public T RandomPick<T>(params T[] items)
    {
        if (items == null || items.Length == 0)
            throw new ArgumentException("At least one item must be provided.");

        int index = UnityEngine.Random.Range(0, items.Length);
        return items[index];
    }

    /// <summary>
    /// used to notify that the action can proceed after waiting for an external event or condition.
    /// </summary>
    public virtual void OnProceedCall()
    {
        proceedCall = true;
        // Optional override in derived classes
    }

    public List<Coroutine> allCachedCoroutines = new List<Coroutine>();

    public virtual IEnumerator StartMultipleCoroutines(List<IEnumerator> caller)
    {
        if (caller == null || caller.Count == 0) yield break;

        int remaining = caller.Count;
        for (int i = 0; i < caller.Count; i++)
        {
            if (caller[i] != null)
            {
                int index = i;
                StartCoroutine(Run(caller[i], () => remaining--, c => allCachedCoroutines.Add(c)));
            }
            else
            {
                remaining--;
            }
        }

        yield return new WaitUntil(() => remaining <= 0);
    }

    public void StopAllCachedCoroutines()
    {
        foreach (Coroutine c in allCachedCoroutines) TryStopCoroutine(c);
        allCachedCoroutines.Clear();
    }

    private IEnumerator Run(IEnumerator caller, Action onDone, Action<Coroutine> routineCache)
    {
        Coroutine c = StartCoroutine(caller);
        routineCache?.Invoke(c);
        yield return c;
        onDone?.Invoke();
    }
}

[System.Serializable]
public struct MeleeAttack
{
    public int damage;
    public float stun;
    public float freezeTime;
    public float repel;
    [SerializeField, MinMaxSlider(0, 3f)] public Vector2 rumble;
    public float rumbleDuration;
    public float cameraShake;

    public MeleeAttack(int damage, float stun, float freezeTime, Vector2 rumble, float rumbleDuration, float repel, float cameraShake)
    {
        this.damage = damage;
        this.stun = stun;
        this.freezeTime = freezeTime;
        this.rumble = rumble;
        this.rumbleDuration = rumbleDuration;
        this.repel = repel;
        this.cameraShake = cameraShake;
    }
}