using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem.XR;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UnityEngine.Rendering.HableCurve;

public class Health : IDamagable
{
    public static Health instance;
    public AnimSetBool anim_bool;
    public Animator anim;
    public Animator bladeAnim;
    private PlayerAttack playerAttack;
    private CharacterController2D controller;
    private InputPlayer inputPlayer;
    public float maxHealth;
    private float currentHealth;
    [HideInInspector] public float healthPercentage;
    private DamageFlash _damageFlash;

    public Image Health_segment;
    public Transform health_parent;
    public List<Image> segments = new List<Image>();
    private Color originalColor;
    public Vector3 revivePosition;

    public Transform hitEffectPosition;
    public bool isDead = false;

    public UnityEvent DEATH;

    private float stun_timer;
    private bool stunned = false;

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
        originalColor = Health_segment.color;
        UpdateHealthSegment();
        revivePosition = transform.position;
    }

    public void IncreaseMaxHealth(int i)
    {
        maxHealth += i;
        UpdateHealthSegment();
    }

    private void Update()
    {
        healthPercentage = currentHealth / maxHealth;
        if (stunned)
        {
            if (stun_timer > 0) { stun_timer -= Time.unscaledDeltaTime; }
            else
            {
                stunned = false;
                anim_bool.Anim_Hit(1);

                if (!isDead) { anim.SetTrigger("stun_after"); }
            }
        }
    }

    private void UpdateHealthSegment()
    {
        for (int i = 0; i < maxHealth; i++)
        {
            if (i >= segments.Count)
            {
                Image _image = Instantiate(Health_segment, health_parent).GetComponent<Image>();
                segments.Add(_image);
            }
        }
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
        SoundManager.PlaySound("player_take_damage");
        currentHealth -= damageAmount;

        _damageFlash.OnDamageFlash();
        inputPlayer.DisableFloat();
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        for (int i = 0; i < maxHealth; i++)
        {
            if (i < currentHealth) { segments[i].color = originalColor; }
            else { segments[i].color = new Color(0, 0, 0, 0); }
        }
        if (stun_duration != 0)
        {
            anim_bool.Anim_Attack(2);
            Stun(stun_duration, sender);
        }
        if (currentHealth <= 0)
        {
            anim_bool.Anim_Attack(2);
            isDead = true;
            controller.isRunningToTarget = false;
            GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            GetComponent<Rigidbody2D>().isKinematic = true;
            anim.SetBool("dead", true);
            if (!stunned) { anim.Play("death"); }
            Invoke("OnDeath", 4f);
        }
        return 0;
    }

    public int DamageDirectlyWithStun(int damageAmount, float t = 0f)
    {
        return Damage(damageAmount, null, t);
    }

    public int Damage(int damageAmount)
    {
        return Damage(damageAmount, null, 0f);
    }

    public int DamageFromMeleeAttack(Transform attackPos, int damageAmount, float t = 0f)
    {
        if (this.transform.gameObject.layer == 14) { return 4; }
        if ((attackPos.position.x < transform.position.x && !controller.FacingRight)
            || (attackPos.position.x > transform.position.x && controller.FacingRight))
        {
            if (playerAttack.isDefending)
            {
                int i = Random.Range(1, 3);
                SoundManager.PlaySound("defend_block" + i);
                anim.Play("defend_hit");
                return 1;
            }
            if (playerAttack.isAttacking) { playerAttack.CounterMeleeAttack(); return 2; }
        }
        DamageDirectlyWithStun(damageAmount, t);
        return 0;
    }

    public void OnDeath()
    {
        DEATH?.Invoke();
    }

    public void Revive()
    {
        transform.position = revivePosition;
        currentHealth = maxHealth;
        anim_bool.Anim_Attack(1);
        isDead = false;
        GetComponent<Rigidbody2D>().isKinematic = false;
        for (int i = 0; i < maxHealth; i++)
        {
            if (i < currentHealth) { segments[i].color = originalColor; }
            else { segments[i].color = new Color(0, 0, 0, 0); }
        }
    }

    public void SetRevivePoint(Transform pos)
    {
        revivePosition = pos.position;
    }

    public void Stun(float duration, Transform sender)
    {
        playerAttack.EndDefend();

        stunned = true;
        stun_timer = duration;
        anim_bool.Anim_Hit(0);

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

    public override void Repel(float force, bool left)
    {
        if (GameManager.instance.isInPerformingState) { return; }
        GetComponent<Rigidbody2D>().AddForce((left ? Vector3.left : Vector3.right) * force, ForceMode2D.Impulse);
    }

    public void ForceRepel(float force, bool left)
    {
        GetComponent<Rigidbody2D>().AddForce((left ? Vector3.left : Vector3.right) * force, ForceMode2D.Impulse);
    }
}