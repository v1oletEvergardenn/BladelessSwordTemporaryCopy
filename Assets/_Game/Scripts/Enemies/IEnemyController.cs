using EditorAttributes;
using Microlight.MicroBar;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.U2D;
using UnityEngine.UI;
using Void = EditorAttributes.Void;

[RequireComponent(typeof(DamageFlash), typeof(Rigidbody2D))]
public abstract class IEnemyController : IDamagable
{
    #region HEALTH

    public int maxHealth;
    public GameObject HealthUI;
    public MicroBar healthBar;
    [HideInInspector] public float healthPercentage;
    [HideInInspector] public int lastAttackId = -1;
    [HideInInspector] public float lastAttackTime = -1f;
    [HideInInspector] public const float attackCooldown = 0.05f; // 50ms window to prevent double hit
    [HideInInspector] public bool canTakeDamage = true;

    #endregion HEALTH

    #region BREAK

    public float maxBreak = 10;
    public MicroBar bossBreakBar;
    public float currentBreak;
    public float breakDuration = 5f;
    [HideInInspector] public bool isBossBreaking = false;

    #endregion BREAK

    #region BASIC_LOGIC

    public bool isGrounded = true;
    public LayerMask m_WhatIsGround;
    public Collider2D col;
    public bool IN_COMBAT = false;
    public Transform leftBoundary;
    public Transform rightBoundary;
    public GameObject GFX;
    public InternalObjectPooler selfPooler;
    public List<List<ActionCaller>> actionList = new List<List<ActionCaller>>();
    public IEnemyAction lastAction;

    public bool canFlip = true;
    public bool isFacingRight;
    public bool inAct = false;

    #endregion BASIC_LOGIC

    #region PRIVATE VARIABLES

    public bool isActing = false;
    public IEnemyAction initialAction;
    public bool canMove = false;
    public float distanceToPlayer;
    public float currentHealth;
    public bool DEAD = false;
    [HideInInspector] public DamageFlash flash;
    [HideInInspector] public Rigidbody2D rb;
    [HideInInspector] public AnimationCurve outline_flash_anim_curve;
    [HideInInspector] public SpriteRenderer sprite;
    [HideInInspector] public Animator anim;
    [HideInInspector] public Transform player;
    [HideInInspector] public PlayerAttack playerAttack;
    [HideInInspector] public Energy playerEnergy;
    [HideInInspector] public PlayerControl playerController;

    [HideInInspector] public float enemyDelta => TimeScaleManager.EnemyDt;

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
        playerController = PlayerControl.instance;
        player = playerAttack.gameObject.transform;
        anim = GFX.GetComponent<Animator>();
        currentHealth = maxHealth;
        currentBreak = maxBreak;
        healthBar.Initialize(maxHealth);
        bossBreakBar.Initialize(maxBreak);
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
        return actionList[1][0].action;
    }

    /// <summary>
    /// Removes the current action from the action list.
    /// </summary>
    public virtual void EndAction()
    {
        print("ended Action");
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
    public virtual void AddAction(IEnemyAction action, float factor = 0, float delay = 0)
    {
        ActionCaller i = new ActionCaller(action, factor, delay);
        actionList[0].Add(i);
    }

    public Coroutine co_act;

    /// <summary>
    /// Coroutine for executing actions in the action list.
    /// </summary>
    public virtual IEnumerator Act()
    {
        while (actionList.Count > 0 && actionList[0] != null)
        {
            foreach (List<ActionCaller> list in actionList)
            {
                yield return co_multiActions = StartCoroutine(StartMultipleActions(list));
                EndAction();
            }
        }

        isActing = false;

        StartAction();
        yield return null;
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
            elapsedTime += enemyDelta;
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
        canTakeDamage = true;
        Damage(maxHealth);
    }

    public virtual void ForceBossBreak()
    {
        DoBreak(maxBreak);
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
    public virtual void DoBreak(float amount)
    {
        if (isBossBreaking || DEAD) { return; }

        currentBreak -= amount;
        currentBreak = Mathf.Clamp(currentBreak, 0, maxBreak);

        if (bossBreakBar != null && maxBreak > 0)
            bossBreakBar.UpdateBar(currentBreak);

        if (currentBreak <= 0)
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
        TimeScaleManager.EnterBulletTime();
        float duration = breakDuration;
        float elapsed = 0f;
        float startBreak = currentBreak;

        while (elapsed < duration)
        {
            elapsed += TimeScaleManager.GlobalDt; // Use unscaled time to be immune to bullet time
            float t = Mathf.Clamp01(elapsed / duration);
            currentBreak = Mathf.Lerp(startBreak, maxBreak, t);

            if (bossBreakBar != null && maxBreak > 0)
                bossBreakBar.UpdateBar(currentBreak);

            yield return null;
        }

        currentBreak = maxBreak;
        if (bossBreakBar != null && maxBreak > 0)
            bossBreakBar.UpdateBar(maxBreak);
        TimeScaleManager.ExitBulletTime();
        isBossBreaking = false;
        yield return WaitForEnemy(1f);
        StartAction();
        yield return null;
    }

    public virtual void CancelAllAction()
    {
    }

    private IEnumerator Run(ActionCaller caller, Action onDone)
    {
        yield return WaitForEnemy(caller.delay);
        yield return caller.action.act_routine = StartCoroutine(caller.action.Act_coroutine(caller.factor));
        onDone?.Invoke();
    }

    public Coroutine co_multiRun;

    private IEnumerator Run(IEnumerator caller, Action onDone)
    {
        yield return StartCoroutine(caller);
        onDone?.Invoke();
    }

    public Coroutine co_multiActions;
    public Coroutine co_multiCoroutine;

    // Waits until all provided coroutines complete.
    public IEnumerator StartMultipleActions(List<ActionCaller> caller)
    {
        if (caller == null || caller.Count == 0) yield break;

        int remaining = caller.Count;
        for (int i = 0; i < caller.Count; i++)
        {
            co_multiRun = StartCoroutine(Run(caller[i], () => remaining--));
        }

        yield return new WaitUntil(() => remaining <= 0);
    }

    public IEnumerator StartMultipleCoroutines(List<IEnumerator> caller)
    {
        if (caller == null || caller.Count == 0) yield break;

        int remaining = caller.Count;
        for (int i = 0; i < caller.Count; i++)
        {
            if (caller[i] != null) co_multiRun = StartCoroutine(Run(caller[i], () => remaining--));
            if (caller[i] == null) remaining--;
        }

        yield return new WaitUntil(() => remaining <= 0);
    }

    // Convert a coroutine to a Task so you can use async/await + Task.WhenAll.
    public Task AsTask(IEnumerator routine)
    {
        var tcs = new TaskCompletionSource<bool>();
        StartCoroutine(RunTask(routine, tcs));
        return tcs.Task;
    }

    private IEnumerator RunTask(IEnumerator routine, TaskCompletionSource<bool> tcs)
    {
        yield return StartCoroutine(routine);
        tcs.SetResult(true);
    }

    public T RandomChoice<T>(params T[] options)
    {
        if (options == null || options.Length == 0) throw new ArgumentException("options must contain at least one element", nameof(options));
        return options[UnityEngine.Random.Range(0, options.Length)];
    }

    public T RandomChoice<T>(T a, T b)
    {
        return UnityEngine.Random.value < 0.5f ? a : b;
    }

    public IEnumerator WaitForEnemy(float seconds)
    {
        yield return TimeScaleManager.WaitForChannelSeconds(seconds, TimeChannel.Enemy);
    }

    #endregion UTILITY
}

[Serializable]
public class ActionCaller
{
    public IEnemyAction action;
    public float factor;
    public float delay;

    public ActionCaller(IEnemyAction _action, float _factor = 0, float _delay = 0)
    {
        action = _action;
        factor = _factor;
        delay = _delay;
    }
}