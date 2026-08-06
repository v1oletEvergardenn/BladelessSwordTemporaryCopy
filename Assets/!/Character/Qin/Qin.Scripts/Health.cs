using Microlight.MicroBar;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem.XR;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum MeleeAttackResult
{
    DamagedSuccessfully = 0,
    Defended = 1,
    Countered = 2,
    Dashed = 4
}

public class Health : IDamagable
{
    public static Health instance;
    public Animator anim;
    private PlayerAttack playerAttack;
    private CharacterController2D controller;
    private InputPlayer inputPlayer;
    private HeartSwordAbilities hsManager;
    [SerializeField] private float maxHealth;
    [SerializeField] private float currentHealth;
    public MicroBar healthBar;
    public GameObject characterUI;

    [HideInInspector] public float healthPercentage;
    private DamageFlash _damageFlash;

    public Vector3 revivePosition;

    public Transform hitEffectPosition;
    public bool isDead = false;

    public UnityEvent DEATH;

    private float stun_timer;
    public bool stunned = false;

    private void Awake()
    {
        if (instance == null) { instance = this; }
    }

    // Start is called before the first frame update
    private void Start()
    {
        currentHealth = maxHealth;
        playerAttack = GetComponent<PlayerAttack>();
        _damageFlash = GetComponent<DamageFlash>();
        inputPlayer = GetComponent<InputPlayer>();
        controller = GetComponent<CharacterController2D>();
        hsManager = HeartSwordAbilities.instance;
        revivePosition = transform.position;
        healthBar.Initialize(maxHealth);
    }

    public void IncreaseMaxHealth(int i)
    {
        maxHealth += i;
    }

    private void Update()
    {
        healthPercentage = currentHealth / maxHealth;
    }

    public void DamageDirectly(int damageAmount)
    {
        Damage(damageAmount, null, 0f);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="damageAmount"></param>
    /// <param name="sender"> damage source, null meaning undefendable</param>
    /// <param name="stun_duration"> duration for stunning when received damage</param>
    /// <returns>-1: target is dead, damage unsuccessfully
    /// 0: damaged successfully
    /// 1: target is defending</returns>
    public override int Damage(float damageAmount, Transform sender, float stun_duration = 0f, bool damageFlash = true, float stunValue = 0)
    {
        print(stun_duration);
        if (isDead) return -1;
        if (playerAttack.isDefending)
        {
            if (sender != null)
            {
                if (sender.TryGetComponent<IProjectile>(out IProjectile proj))
                {
                    if ((proj.transform.right.x < 0 && controller.FacingRight) ||
                        (proj.transform.right.x > 0 && !controller.FacingRight))
                    {
                        int i = Random.Range(1, 3);
                        SoundManager.PlaySound("defend_block" + i);
                        anim.Play("defend_hit");
                        return 1;
                    }
                }
            }

            //float x = sender.transform.position.x;
            //if (controller.m_FacingRight && x > transform.position.x) { return; }
            //if (!controller.m_FacingRight && x < transform.position.x) { return; }
        }
        if (sender != null && sender.TryGetComponent<IProjectile>(out IProjectile projectile))
        {
            if (!projectile.muteHitSound)
            {
                SoundManager.PlaySound("player_take_damage");
            }
        }
        else
        {
            SoundManager.PlaySound("player_take_damage");
        }
        currentHealth -= damageAmount;
        if (damageAmount > 0)
        {
            if (damageAmount >= 10) healthBar.UpdateBar(currentHealth, UpdateAnim.CriticalDamage);
            else healthBar.UpdateBar(currentHealth);

            _damageFlash.OnDamageFlash();
            inputPlayer.DisableFloat();
            CharacterController2D.CancelJump();
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
            if (stun_duration != 0)
            {
                playerAttack.EndAttack();
                Stun(stun_duration, sender);
            }
        }

        if (currentHealth <= 0)
        {
            playerAttack.EndAttack();
            isDead = true;
            controller.SetIsRunningToTarget(false);
            GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            anim.SetBool("dead", true);
            anim.Play("death");
            Invoke("OnDeath", 4f);
        }
        return 0;
    }

    public int DamageDirectlyWithStun(float damageAmount, float t = 0f)
    {
        return Damage(damageAmount, null, t);
    }

    public int Damage(float damageAmount)
    {
        return Damage(damageAmount, null, 0f);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="attackPos"></param>
    /// <param name="damageAmount"></param>
    /// <param name="stunTime"></param>
    /// <returns>
    ///  0: damaged successfully
    /// 1: target is defending,
    /// 2: target is countering,
    /// 4: target is immune,</returns>
    public MeleeAttackResult DamageFromMeleeAttack(Transform attackPos, float damageAmount, float stunTime = 0f, bool canCounterAttack = true)
    {
        if (this.transform.gameObject.layer == 14) { return MeleeAttackResult.Dashed; }

        bool damageFromLeft = attackPos.position.x < transform.position.x;

        if (playerAttack.isDefending &&
            !controller.FacingRight == damageFromLeft)
        {
            playerAttack.DefendHit(); return MeleeAttackResult.Defended;
        }
        else if (canCounterAttack &&
            playerAttack.isCounterAttacking &&
            playerAttack.isAttackingLeft == damageFromLeft)
        {
            playerAttack.CounterMeleeAttack(); return MeleeAttackResult.Countered;
        }

        DamageDirectlyWithStun(damageAmount, stunTime);
        return MeleeAttackResult.DamagedSuccessfully;
    }

    public MeleeAttackResult DamageFromMeleeAttack(Transform attackPos, MeleeAttack melee, bool canCounterAttack = true)
    {
        return DamageFromMeleeAttack(attackPos, melee.damage, melee.freezeTime, canCounterAttack);
    }

    public void OnDeath()
    {
        DEATH?.Invoke();
        MenuManager.instance.EndCanvas();
    }

    public void SetCharacterUI(bool active)
    {
        characterUI.SetActive(active);
    }

    public void Revive()
    {
        transform.position = revivePosition;
        currentHealth = maxHealth;
        ActionLock.ClearAll();
        stunned = false;
        isDead = false;
        GetComponent<Rigidbody2D>().isKinematic = false;
    }

    public void SetRevivePoint(Transform pos)
    {
        revivePosition = pos.position;
    }

    public void Stun(float duration, Transform sender)
    {
        if (GameManager.instance.isInPerformingState) { return; }

        playerAttack.EndDefend();
        hsManager.CancelAllAbilities();
        stunned = true;
        //anim_bool.Anim_Hit(0);
        ActionLock.Add("stunned", Lock.All, duration, onUnlocked: () => { anim.SetTrigger("stun_after"); stunned = false; });

        bool damageFromBehind = false;//determines the animation
        if (sender != null)
        {
            float x_sender = sender.transform.position.x;
            float x_player = transform.position.x;
            // not facing towards the sender
            if ((x_sender < x_player && controller.FacingRight) || (x_sender > x_player && !controller.FacingRight))
            {
                damageFromBehind = true;
            }
        }
        if (damageFromBehind) { anim.Play("hit_behind"); }
        else { anim.Play("hit"); }
    }

    public override void Repel(float distance, bool left)
    {
        if (GameManager.instance.isInPerformingState) { return; }
        base.Repel(distance, left);
    }

    public float GetCurrentHealth()
    { return currentHealth; }

    public float GetMaxHealth()
    { return maxHealth; }

    public float SetCurrentHealth(float h)
    {
        currentHealth = h;
        healthBar.UpdateBar(currentHealth);
        return currentHealth;
    }

    public float SetMaxHealth(float h)
    {
        maxHealth = h;
        healthBar.Initialize(maxHealth);
        return maxHealth;
    }
}