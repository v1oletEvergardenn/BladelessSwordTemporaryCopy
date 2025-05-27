using EditorAttributes;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.U2D;
using UnityEngine.UI;

[RequireComponent(typeof(DamageFlash), typeof(Rigidbody2D), typeof(IEnemyActionIdle))]
public abstract class IEnemyController : IDamagable
{
    #region HEALTH

    [FoldoutGroup("Health", nameof(maxHealth), nameof(HealthUI), nameof(healthBar))] public Void healthVoid;
    [SerializeField, HideInInspector] public int maxHealth;
    [SerializeField, HideInInspector] public GameObject HealthUI;
    [SerializeField, HideInInspector] public Image healthBar;
    [HideInInspector] public int lastAttackId = -1;
    [HideInInspector] public float lastAttackTime = -1f;
    [HideInInspector] public const float attackCooldown = 0.05f; // 50ms window to prevent double hit

    #endregion HEALTH

    #region STUN

    [FoldoutGroup("stunning", nameof(maxStun), nameof(stunBar), nameof(currentStun), nameof(stunDuration))] public Void stunVoid1;
    [SerializeField, HideInInspector] public float maxStun = 10;
    [SerializeField, HideInInspector] public Image stunBar;
    [SerializeField, HideInInspector] public float currentStun;
    [SerializeField, HideInInspector] public float stunDuration = 5f;
    [HideInInspector] public bool isBossBreaking = false;

    #endregion STUN

    #region GROUNDCHECK

    [FoldoutGroup("GroundCheck", nameof(isGrounded), nameof(m_WhatIsGround), nameof(col))] public Void groundvoid;
    [SerializeField, HideInInspector] public bool isGrounded = true;
    [SerializeField, HideInInspector] public LayerMask m_WhatIsGround;
    [SerializeField, HideInInspector] public Collider2D col;

    #endregion GROUNDCHECK

    #region BASIC_LOGIC

    [FoldoutGroup("basic_logic", nameof(IN_COMBAT), nameof(canFlip), nameof(isFacingRight),
        nameof(inAct), nameof(speed), nameof(leftBoundary), nameof(rightBoundary), nameof(GFX),
        nameof(maxActionBreakCapacity), nameof(currentActionBreakAmount), nameof(breakDuration), nameof(selfPooler))]
    public Void logicvoid;

    [SerializeField, HideInInspector] public bool IN_COMBAT = false;
    [SerializeField, HideInInspector] public bool canFlip = true;
    [SerializeField, HideInInspector] public bool isFacingRight;
    [SerializeField, HideInInspector] public bool inAct = false;
    [SerializeField, HideInInspector] public float speed = 20f;
    [SerializeField, HideInInspector] public Transform leftBoundary;
    [SerializeField, HideInInspector] public Transform rightBoundary;
    [SerializeField, HideInInspector] public GameObject GFX;
    [SerializeField, HideInInspector] public int maxActionBreakCapacity = 10;
    [SerializeField, HideInInspector] public int currentActionBreakAmount = 0;
    [SerializeField, HideInInspector] public float breakDuration = 3f;
    [SerializeField, HideInInspector] public InternalObjectPooler selfPooler;

    [HideInInspector] public List<IEnemyAction> actionList = new List<IEnemyAction>();

    #endregion BASIC_LOGIC

    #region PRIVATE VARIABLES

    [HideInInspector] public bool isActing = false;
    [HideInInspector] public IEnemyAction initialAction;
    [HideInInspector] public bool canMove = false;
    [HideInInspector] public float distanceToPlayer;
    [HideInInspector] public float currentHealth;
    [HideInInspector] public bool DEAD = false;
    [HideInInspector] public DamageFlash flash;
    [HideInInspector] public Rigidbody2D rb;
    [HideInInspector] public AnimationCurve outline_flash_anim_curve;
    [HideInInspector] public SpriteRenderer sprite;
    [HideInInspector] public Animator anim;
    [HideInInspector] public Transform player;
    [HideInInspector] public PlayerAttack playerAttack;
    [HideInInspector] public Energy playerEnergy;
    [HideInInspector] public CharacterController2D playerController;

    #endregion PRIVATE VARIABLES

    #region UNITY_LifeCycle

    /// <summary>
    /// Unity Start method. Initializes references and sets up initial state.
    /// </summary>
    public virtual void Start()
    {
        sprite = GFX.GetComponent<SpriteRenderer>();
        playerAttack = PlayerAttack.instance;
        playerEnergy = Energy.instance;
        playerController = CharacterController2D.instance;
        player = playerAttack.gameObject.transform;
        anim = GFX.GetComponent<Animator>();
        currentHealth = maxHealth;
        currentStun = maxStun;
        healthBar.fillAmount = currentHealth / maxHealth;
        stunBar.fillAmount = currentStun / maxStun;
        flash = GetComponent<DamageFlash>();
        rb = GetComponent<Rigidbody2D>();
        outline_flash_anim_curve = GameManager.instance.outline_flash_anim_curve;
        actionList.Clear();
    }

    #endregion UNITY_LifeCycle

    #region ACTION_MANAGEMENT

    /// <summary>
    /// Returns the next action in the action list, or null if not available.
    /// </summary>
    public virtual IEnemyAction NextAction()
    {
        if (actionList.Count < 2) { return null; }
        return actionList[1];
    }

    /// <summary>
    /// Removes the current action from the action list.
    /// </summary>
    public virtual void EndAction()
    {
        if (actionList.Count > 0)
        {
            actionList.RemoveAt(0);
        }
    }

    /// <summary>
    /// Inserts an action into the action list at the specified index.
    /// </summary>
    /// <param name="action">The action to insert.</param>
    /// <param name="index">The index to insert at (default is 1).</param>
    public virtual void InsertAction(IEnemyAction action, int index = 1)
    {
        if (actionList.Count == 0) { actionList.Add(action); }
        else
        {
            actionList.Insert(index, action);
        }
    }

    public Coroutine co_act;

    /// <summary>
    /// Coroutine for executing actions in the action list.
    /// </summary>
    public virtual IEnumerator Act()
    {
        while (actionList.Count > 0 && actionList[0] != null)
        {
            IEnemyAction action = actionList[0];
            yield return action.act_routine = StartCoroutine(action.Act_coroutine());
            yield return null;
        }
        isActing = false;
        if (currentActionBreakAmount >= maxActionBreakCapacity) { yield return StartCoroutine(Break()); }//break
        else { StartAction(); }//startover
        yield return null;
    }

    /// <summary>
    /// Coroutine for handling action breaks.
    /// </summary>
    public virtual IEnumerator Break()
    {
        yield return new WaitForSeconds(breakDuration);
        currentActionBreakAmount = 0;
        StartAction();
        yield return null;
    }

    /// <summary>
    /// Adds to the current action break amount.
    /// </summary>
    /// <param name="amount">Amount to add.</param>
    public virtual void AddActionBreak(int amount)
    {
        currentActionBreakAmount += amount;
    }

    #endregion ACTION_MANAGEMENT

    #region VISUALS

    /// <summary>
    /// Sets the outline effect on the sprite.
    /// </summary>
    /// <param name="i">Outline value.</param>
    public virtual void OutLine_Activate(int i)
    {
        sprite.material.SetFloat("_OutLine", i);
    }

    /// <summary>
    /// Starts the outline flash effect.
    /// </summary>
    public virtual void OutLineFlash()
    {
        StartCoroutine(IEOutLineFlash());
    }

    /// <summary>
    /// Coroutine for the outline flash effect.
    /// </summary>
    public virtual IEnumerator IEOutLineFlash()
    {
        float currentFlashAmount = 0f;
        float elapsedTime = 0f;
        while (elapsedTime < 0.2f)
        {
            elapsedTime += Time.deltaTime;
            currentFlashAmount = outline_flash_anim_curve.Evaluate(elapsedTime);
            sprite.material.SetFloat("_OutLineThickness", currentFlashAmount);
            yield return null;
        }
    }

    #endregion VISUALS

    #region COMBAT

    /// <summary>
    /// Activates combat mode for the enemy.
    /// </summary>
    public virtual void ActivateCombat()
    {
        StartCoroutine(IE_Activate());
    }

    /// <summary>
    /// Coroutine for activating combat mode.
    /// </summary>
    public virtual IEnumerator IE_Activate()
    {
        IN_COMBAT = true;
        yield return null;
    }

    /// <summary>
    /// Starts the enemy's action routine. Intended to be overridden.
    /// </summary>
    public virtual void StartAction()
    {
    }

    #endregion COMBAT

    #region GROUND_CHECK

    /// <summary>
    /// Checks if the enemy is grounded using a raycast.
    /// </summary>
    public virtual void GroundCheck()
    {
        bool wasGrounded = isGrounded;
        isGrounded = false;
        RaycastHit2D ray = Physics2D.Raycast(col.bounds.center, Vector2.down, col.bounds.extents.y + 0.1f, m_WhatIsGround);
        if (ray.collider != null)
        {
            isGrounded = true;
        }
    }

    #endregion GROUND_CHECK

    #region UTILITY

    /// <summary>
    /// Returns true with the given probability (0-100).
    /// </summary>
    /// <param name="i">Chance percentage.</param>
    public bool Possibility(float i)
    {
        float chance = UnityEngine.Random.Range(0, 100);
        if (chance <= i) { return true; }
        else { return false; }
    }

    /// <summary>
    /// Gets the boundary closest to the enemy.
    /// </summary>
    public virtual Transform GetCloseBoundary()
    {
        float mid = (leftBoundary.position.x + rightBoundary.position.x) / 2;
        if (transform.position.x < mid) { return leftBoundary; }
        else { return rightBoundary; }
    }

    /// <summary>
    /// Gets the boundary farthest from the enemy.
    /// </summary>
    public virtual Transform GetFarBoundary()
    {
        float mid = (leftBoundary.position.x + rightBoundary.position.x) / 2;
        if (transform.position.x < mid) { return rightBoundary; }
        else { return leftBoundary; }
    }

    /// <summary>
    /// Gets the boundary farthest from the player.
    /// </summary>
    public virtual Transform GetBoundaryFarOfPlayer()
    {
        float mid = (leftBoundary.position.x + rightBoundary.position.x) / 2;
        if (player.position.x < mid) { return rightBoundary; }
        else { return leftBoundary; }
    }

    /// <summary>
    /// Instantly kills the enemy.
    /// </summary>
    public virtual void ForceDie()
    {
        Damage(maxHealth);
    }

    public virtual void ForceStun()
    {
        DecreaseStun(maxStun);
    }

    /// <summary>
    /// Returns the closer of two targets based on Y distance.
    /// </summary>
    public virtual Transform GetCloserTargetOutOfTwo(Transform a, Transform b)
    {
        float dist_a = Mathf.Abs(transform.position.y - a.position.y);
        float dist_b = Mathf.Abs(transform.position.y - b.position.y);
        if (dist_a < dist_b) { return a; } else { return b; }
    }

    /// <summary>
    /// Returns the farther of two targets based on Y distance.
    /// </summary>
    public virtual Transform GetFarTargetOutOfTwo(Transform a, Transform b)
    {
        float dist_a = Mathf.Abs(transform.position.y - a.position.y);
        float dist_b = Mathf.Abs(transform.position.y - b.position.y);
        if (dist_a < dist_b) { return b; } else { return a; }
    }

    /// <summary>
    /// Gets the center X position of the map.
    /// </summary>
    public virtual float GetCenterXOfMap()
    {
        return (leftBoundary.position.x + rightBoundary.position.x) * 0.5f;
    }

    /// <summary>
    /// Checks if the player is to the left of the enemy.
    /// </summary>
    public virtual bool IsPlayerLeft()
    {
        if (player.position.x <= transform.position.x)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Attempts to stop a coroutine if it is running.
    /// </summary>
    /// <param name="i">Coroutine to stop.</param>
    public void TryStopCoroutine(Coroutine i)
    {
        if (i != null) { StopCoroutine(i); }
    }

    /// <summary>
    /// Increases the stun value by the given amount. If the stun exceeds maxStun,
    /// resets stun, cancels all actions, and triggers the boss break effect.
    /// </summary>
    /// <param name="amount">Amount to increase stun by.</param>
    public virtual void DecreaseStun(float amount)
    {
        if (isBossBreaking || DEAD) { return; }

        currentStun -= amount;
        currentStun = Mathf.Clamp(currentStun, 0, maxStun);

        if (stunBar != null && maxStun > 0)
            stunBar.fillAmount = Mathf.Clamp01((float)currentStun / maxStun);

        if (currentStun <= 0)
        {
            isActing = false;
            // Cancel all actions
            StartCoroutine(BossBreak());
        }
    }

    public virtual IEnumerator BossBreak()
    {
        isBossBreaking = true;
        CancelAllAction();
        VFXManager.instance.BulletTime();
        float duration = stunDuration;
        float elapsed = 0f;
        float startStun = currentStun;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime; // Use unscaled time to be immune to bullet time
            float t = Mathf.Clamp01(elapsed / duration);
            currentStun = Mathf.Lerp(startStun, maxStun, t);

            if (stunBar != null && maxStun > 0)
                stunBar.fillAmount = Mathf.Clamp01(currentStun / maxStun);

            yield return null;
        }

        currentStun = maxStun;
        if (stunBar != null && maxStun > 0)
            stunBar.fillAmount = 1f;

        VFXManager.instance.UnBulletTime();
        isBossBreaking = false;
        yield return new WaitForSeconds(1f);
        StartAction();
        yield return null;
    }

    public virtual void CancelAllAction()
    {
    }

    #endregion UTILITY
}