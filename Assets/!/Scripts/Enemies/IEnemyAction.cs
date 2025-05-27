using System.Collections;
using UnityEngine;
using EditorAttributes;
using System;

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
    public int actionBreakAmount = 1;
    public Coroutine act_routine;

    public virtual void Start()
    {
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

    public virtual IEnumerator Act_coroutine(float factor = 0)
    {
        yield return null;
    }

    public virtual IEnumerator ApplyAttackInCircle(float duration, float range, Transform attackPos, MeleeAttack melee)
    {
        bool hitAlready = false;
        float elapsedTime = 0f;
        while (!hitAlready && elapsedTime <= duration)
        {
            float d = Vector3.Distance(playerIDamagable.GetHitPos(), attackPos.position);
            if (d <= range) { Hit(melee, attackPos); hitAlready = true; }
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        yield return null;
    }

    public virtual void Hit(MeleeAttack melee, Transform attackPos)
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