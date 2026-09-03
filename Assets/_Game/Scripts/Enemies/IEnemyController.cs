using DG.Tweening;
using EditorAttributes;
using Microlight.MicroBar;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.VirtualTexturing;
using UnityEngine.U2D;
using UnityEngine.UI;
using Void = EditorAttributes.Void;

[RequireComponent(typeof(DamageFlash), typeof(Rigidbody2D))]
public abstract class IEnemyController : IDamagable
{
    #region HEALTH

    public string BossName;
    public int maxHealth;
    public BossHealthBar bossUI;
    [HideInInspector] public float healthPercentage;
    [HideInInspector] public int lastAttackId = -1;
    [HideInInspector] public float lastAttackTime = -1f;
    [HideInInspector] public const float attackCooldown = 0.05f; // 50ms window to prevent double hit
    [HideInInspector] public bool canTakeDamage = true;

    #endregion HEALTH

    #region BREAK

    public float maxBreak = 10;
    public float currentBreak;
    public float breakDuration = 5f;
    [HideInInspector] public bool isBossBreaking = false;

    #endregion BREAK

    #region BASIC_LOGICin

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

    #endregion BASIC_LOGICin

    public float moveSpeed = 12f;

    #region PRIVATE VARIABLES

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
    private bool isActLoopRunning;
    [HideInInspector] public const int MaxActionBuildAttempts = 3;

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
        bossUI.Initialize(BossName, maxHealth, maxBreak);
        flash = GetComponent<DamageFlash>();
        rb = GetComponent<Rigidbody2D>();
        outline_flash_anim_curve = GameManager.instance.outline_flash_anim_curve;
        actionList.Clear();
    }

    public virtual void Update()
    {
        GroundCheck();
    }

    #endregion UNITY_LifeCycle

    #region ACTION_MANAGEMENT

    public virtual IEnemyAction NextAction()
    {
        if (actionList == null || actionList.Count == 0)
        {
            return null;
        }

        int nonEmptyGroupCount = 0;

        for (int i = 0; i < actionList.Count; i++)
        {
            List<ActionCaller> group = actionList[i];
            if (group == null || group.Count == 0)
            {
                continue;
            }

            IEnemyAction firstActionInGroup = null;
            for (int j = 0; j < group.Count; j++)
            {
                if (group[j] != null && group[j].action != null)
                {
                    firstActionInGroup = group[j].action;
                    break;
                }
            }

            if (firstActionInGroup == null)
            {
                continue;
            }

            // "Next action" means the first action of the second executable group.
            if (nonEmptyGroupCount == 1)
            {
                return firstActionInGroup;
            }

            nonEmptyGroupCount++;
        }

        return null;
    }

    public virtual IEnemyAction NextAction(IEnemyAction currentAction)
    {
        if (currentAction == null || actionList == null || actionList.Count == 0)
        {
            return NextAction();
        }

        for (int i = 0; i < actionList.Count; i++)
        {
            List<ActionCaller> group = actionList[i];
            if (group == null || group.Count == 0)
            {
                continue;
            }

            bool currentFoundInThisGroup = false;
            for (int j = 0; j < group.Count; j++)
            {
                if (group[j] != null && group[j].action == currentAction)
                {
                    currentFoundInThisGroup = true;
                    break;
                }
            }

            if (!currentFoundInThisGroup)
            {
                continue;
            }

            // Return first executable action from the next groups.
            for (int k = i + 1; k < actionList.Count; k++)
            {
                List<ActionCaller> nextGroup = actionList[k];
                if (nextGroup == null || nextGroup.Count == 0)
                {
                    continue;
                }

                for (int m = 0; m < nextGroup.Count; m++)
                {
                    if (nextGroup[m] != null && nextGroup[m].action != null)
                    {
                        return nextGroup[m].action;
                    }
                }
            }

            return null;
        }

        return NextAction();
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

    public virtual void StartAction()
    {
        if (DEAD) return;
        if (isActLoopRunning) return;
        if (!TryBuildNextActionList()) return;
        if (co_act != null)
        {
            StopCoroutine(co_act);
            co_act = null;
        }

        co_act = StartCoroutine(Act());
    }

    /// <summary>
    /// Coroutine for executing actions in the action list.
    /// </summary>
    public virtual IEnumerator Act()
    {
        isActLoopRunning = true;

        while (!DEAD)
        {
            if (actionList == null || actionList.Count == 0)
            {
                break;
            }

            foreach (List<ActionCaller> list in actionList)
            {
                if (list == null || list.Count == 0)
                {
                    continue;
                }

                yield return co_multiActions = StartCoroutine(StartMultipleActions(list));
            }

            if (!TryBuildNextActionList())
            {
                break;
            }
        }

        isActLoopRunning = false;
        co_act = null;
    }

    public virtual bool TryBuildNextActionList()
    {
        return false;
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
            elapsedTime += TimeScaleManager.EnemyDt;
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

    public override int Damage(float damageAmount, Transform sender = null, float stunDuration = 0, bool damageFlash = true, float stunValue = 0)
    {
        if (DEAD) { damageAmount = 0; }
        int attackId = sender != null ? sender.GetInstanceID() : 0;
        if (lastAttackId == attackId && Time.time - lastAttackTime < attackCooldown) return 0;

        lastAttackId = attackId;
        lastAttackTime = Time.time;

        if (damageFlash) flash.OnDamageFlash();

        if (!isBossBreaking) { damageAmount *= 0.5f; }

        currentHealth -= damageAmount;

        if (!IN_COMBAT)
        {
            IN_COMBAT = true;
            bossUI.Show();
        }

        UpdateBossUI();
        DoBreak(stunValue * 1.5f);

        if (currentHealth <= 0)
        {
            DEAD = true;
        }

        return 0;
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

    public bool IsGrounded()
    {
        RaycastHit2D ray = Physics2D.Raycast(col.bounds.center, Vector2.down, col.bounds.extents.y + 0.1f, m_WhatIsGround);
        if (ray.collider != null)
        {
            return true;
        }
        return false;
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

        UpdateBossUI();

        if (currentBreak <= 0)
        {
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

            UpdateBossUI();

            yield return null;
        }

        currentBreak = maxBreak;
        UpdateBossUI();
        TimeScaleManager.ExitBulletTime();
        isBossBreaking = false;
        yield return WaitForEnemy(1f);
        StartAction();
        yield return null;
    }

    public virtual void CancelAllAction()
    {
        StopCurrentMove();
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

    public void UpdateHealthUI()
    {
        if (bossUI != null)
        {
            bossUI.healthBar.UpdateBar(currentHealth);
        }
    }

    public void UpdateBossBreakUI()
    {
        if (bossUI != null)
        {
            bossUI.bossBreakBar.UpdateBar(currentBreak);
        }
    }

    public void UpdateBossUI()
    {
        UpdateHealthUI();
        UpdateBossBreakUI();
    }

    #endregion UTILITY

    #region MOVEMENT_X_DOTWEEN

    [HideInInspector] public Tween currentMoveTween;
    [HideInInspector] public bool isFollowing;

    private Transform ResolveMoveTarget(Transform target)
    {
        if (target != null) return target;
        if (player != null) return player;
        if (playerAttack != null) return playerAttack.transform;
        return null;
    }

    public virtual void StopCurrentMove(bool complete = false)
    {
        isFollowing = false;

        if (currentMoveTween != null && currentMoveTween.IsActive())
        {
            currentMoveTween.Kill(complete);
        }
    }

    public void Move(Transform target = null, float speed = -1f)
    {
        StartCoroutine(IEMove(target, speed));
    }

    public void Move(float x, float speed = -1f, bool relative = true)
    {
        StartCoroutine(IEMove(x, speed, relative));
    }

    public void MoveCurrentDirection(float x, float speed = -1f)
    {
        float direction = isFacingRight ? 1f : -1f;
        Move(x * direction, speed);
    }

    public void MoveByDuration(float duration, Transform target = null)
    {
        StartCoroutine(IEMoveByDuration(duration, target));
    }

    public void MoveByDuration(float x, float duration, bool relative = true)
    {
        StartCoroutine(IEMoveByDuration(x, duration, relative));
    }

    public void Follow(Transform target = null, float speed = -1f, float stopDistance = 1f)
    {
        StartCoroutine(IEFollow(target, speed, stopDistance));
    }

    public IEnumerator IEMoveCurrentDirection(float x, float speed = -1f)
    {
        float direction = isFacingRight ? 1f : -1f;
        yield return StartCoroutine(IEMove(x * direction, speed));
    }

    //Move to target position in Speed.
    public virtual IEnumerator IEMove(Transform target = null, float speed = -1f)
    {
        Transform moveTarget = ResolveMoveTarget(target);
        yield return StartCoroutine(IEMove(moveTarget.position.x, speed, false));
    }

    //Move to x position in Speed. If relative is true, x is added to current position.
    public virtual IEnumerator IEMove(float x, float speed = -1f, bool relative = true)
    {
        StopCurrentMove();
        float targetX = relative ? transform.position.x + x : x;
        float distance = Mathf.Abs(targetX - transform.position.x);
        float moveSpeedToUse = (speed > 0) ? speed : moveSpeed;
        float duration = distance / moveSpeedToUse;
        FaceTarget(targetX);
        currentMoveTween = transform.DOMoveX(targetX, duration)
           .SetEase(Ease.InOutSine)
           .SetTimeDt(this, TimeChannel.Enemy);

        yield return currentMoveTween.WaitForCompletion();
        currentMoveTween = null;
    }

    //Move to target position in duration.
    public virtual IEnumerator IEMoveByDuration(float duration, Transform target = null)
    {
        Transform moveTarget = ResolveMoveTarget(target);
        yield return StartCoroutine(IEMoveByDuration(moveTarget.position.x, duration, false));
    }

    //Move to x position in duration. If relative is true, x is added to current position.
    public virtual IEnumerator IEMoveByDuration(float x, float duration, bool relative = true)
    {
        StopCurrentMove();
        float targetX = relative ? transform.position.x + x : x;

        FaceTarget(targetX);
        currentMoveTween = transform.DOMoveX(targetX, duration)
            .SetEase(Ease.InOutSine)
            .SetTimeDt(this, TimeChannel.Enemy);

        yield return currentMoveTween.WaitForCompletion();
        currentMoveTween = null;
    }

    // Follow target until stop distance is reached
    public virtual IEnumerator IEFollow(Transform target = null, float speed = -1f, float stopDistance = 3f)
    {
        Transform moveTarget = ResolveMoveTarget(target);
        if (moveTarget == null) yield break;

        StopCurrentMove();
        isFollowing = true;

        float moveSpeedToUse = speed > 0f ? speed : moveSpeed;
        float stopDistanceToUse = Mathf.Max(0f, stopDistance);

        while (isFollowing && !DEAD && moveTarget != null)
        {
            float deltaX = moveTarget.position.x - transform.position.x;
            float absDeltaX = Mathf.Abs(deltaX);
            float remaining = absDeltaX - stopDistanceToUse;

            if (remaining <= 0f)
            {
                break;
            }

            FaceTarget(moveTarget.position.x);
            transform.position += transform.right * moveSpeedToUse * TimeScaleManager.EnemyDt;
            yield return null;
        }

        isFollowing = false;
    }

    #endregion MOVEMENT_X_DOTWEEN

    #region Flip

    public void FaceTarget(Transform target = null)
    {
        Transform moveTarget = ResolveMoveTarget(target);
        if ((moveTarget.position.x <= transform.position.x && isFacingRight) ||
            (moveTarget.position.x > transform.position.x && !isFacingRight))
        {
            Flip();
        }
    }

    public void FaceTarget(Vector3 pos)
    {
        if ((pos.x <= transform.position.x && isFacingRight) ||
            (pos.x > transform.position.x && !isFacingRight))
        {
            Flip();
        }
    }

    public void FaceTarget(float x)
    {
        if ((x <= transform.position.x && isFacingRight) ||
            (x > transform.position.x && !isFacingRight))
        {
            Flip();
        }
    }

    public void Face(bool right)
    {
        if (isFacingRight != right)
        {
            Flip();
        }
    }

    public void Flip(bool ignoreCamFollowFlip = false)
    {
        isFacingRight = !isFacingRight;
        transform.Rotate(new Vector3(0, 1, 0), 180);
    }

    #endregion Flip
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