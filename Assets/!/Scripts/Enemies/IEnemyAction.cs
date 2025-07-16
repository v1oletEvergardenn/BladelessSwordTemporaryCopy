using EditorAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    }

    public virtual void Act()
    {
        act_routine = StartCoroutine(Act_coroutine());
    }

    public virtual void CancelAct()
    {
        if (act_routine != null)
        {
            StopCoroutine(act_routine);
        }

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

    public virtual IEnumerator ApplyAttackInCircle(float duration, float range, Transform attackPos, MeleeAttack melee)
    {
        bool hitPlayerAlready = false;
        List<IDamagable> hitIdamagables = new List<IDamagable>();
        List<IProjectile> hitProjectiles = new List<IProjectile>();
        float elapsedTime = 0f;
        while (elapsedTime <= duration)
        {
            elapsedTime += Time.deltaTime;
            Collider2D[] colliders = Physics2D.OverlapCircleAll(attackPos.position, range);
            foreach (Collider2D collider in colliders)
            {
                if (bossController.subDamagables.Contains(collider.GetComponent<IDamagable>())
                    || collider.gameObject == this.gameObject)
                {
                    continue; // Skip if it's the boss itself or a sub-damagable of the boss
                }
                else if (collider.gameObject != player)
                {
                    //Hit Idamagables
                    if (collider.TryGetComponent<IDamagable>(out IDamagable idmg))
                    {
                        if (!hitIdamagables.Contains(idmg))
                        {
                            hitIdamagables.Add(idmg);
                            foreach (IDamagable subIdmg in idmg.subDamagables) { hitIdamagables.Add(subIdmg); }
                            idmg.Damage(melee.damage, this.transform, melee.stun);
                        }
                    }
                    //Hit Iprojectiles
                    if (collider.TryGetComponent<IProjectile>(out IProjectile iProj))
                    {
                        if (!hitProjectiles.Contains(iProj))
                        {
                            hitProjectiles.Add(iProj);
                            iProj.HitByMeleeAttack();
                        }
                    }
                }
            }
            // hit player
            if (!hitPlayerAlready)
            {
                float d = Vector3.Distance(playerIDamagable.GetHitPos(), attackPos.position);
                if (d <= range) { HitPlayer(melee, attackPos); hitPlayerAlready = true; }
            }
            yield return null;
        }

        yield return null;
    }

    public virtual void HitPlayer(MeleeAttack melee, Transform attackPos)
    {
        int dealtDamage = playerIDamagable.DamageFromMeleeAttack(attackPos, melee.damage, melee.stun);
        bool direction = playerIDamagable.GetHitPos().x < attackPos.position.x ? true : false;

        if (dealtDamage == 2)//counter attack
        {
            //counter attack effect
            vfx.SpawnHitEffect(true, playerIDamagable.hitEffectPosition.position);
            playerIDamagable.Repel(melee.repel, direction);
            vfx.CameraShake(melee.cameraShake);
            vfx.RumblePulse(melee.rumble.x * 2, melee.rumble.y * 2, melee.rumbleDuration * 2);
            vfx.SlowTimeForSeconds(melee.freezeTime, 0f);
        }
        else if (dealtDamage == 1)//defend
        {
            vfx.SpawnHitEffect(true, playerIDamagable.GetHitPos());
            playerIDamagable.Repel(melee.repel, direction);
            vfx.CameraShake(melee.cameraShake);
            vfx.RumblePulse(melee.rumble.x * 2, melee.rumble.y * 2, melee.rumbleDuration * 2);
            vfx.SlowTimeForSeconds(melee.freezeTime, 0f);
        }
        else if (dealtDamage == 0)//dealtDamage
        {
            vfx.SpawnHitEffect(true, playerIDamagable.GetHitPos());
            playerIDamagable.Repel(melee.repel * 2, direction);
            vfx.RumblePulse(melee.rumble.x, melee.rumble.y, melee.rumbleDuration);
            vfx.SlowTimeForSeconds(melee.freezeTime, 0f);
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