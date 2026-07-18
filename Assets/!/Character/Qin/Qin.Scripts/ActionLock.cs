using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Flags]
public enum Lock
{
    Move = 1 << 0,
    Flip = 1 << 1,
    Attack = 1 << 2,
    Defend = 1 << 3,
    Jump = 1 << 4,
    SwordJump = 1 << 5,
    SwordTeleport = 1 << 6,
    Storm = 1 << 7,
    Mobility = Move | Flip | Jump,
    Offense = Attack | Storm,
    Defense = Defend,
    Combat = Offense | Defense,
    All = Move | Flip | Attack | Defend | Jump | SwordJump | SwordTeleport | Storm
}

public class ActionLock : MonoBehaviour
{
    #region Singleton

    public static ActionLock instance { get; private set; }

    #endregion Singleton

    #region Data Stores

    // Source of truth:
    // action -> (lockName -> count)
    private readonly Dictionary<Lock, Dictionary<string, int>> _namedLocks = new Dictionary<Lock, Dictionary<string, int>>();

    // lockName -> active timers (used for inspector remaining-time display).
    private readonly Dictionary<string, List<LockTimer>> _timedLocks = new Dictionary<string, List<LockTimer>>();

    // lockName -> callbacks invoked once lockName fully unlocks.
    private readonly Dictionary<string, List<Action>> _unlockCallbacks = new Dictionary<string, List<Action>>();

    #endregion Data Stores

    #region Inspector Debug

    [SerializeField] private List<string> moveLocks = new List<string>();
    [SerializeField] private List<string> flipLocks = new List<string>();
    [SerializeField] private List<string> attackLocks = new List<string>();
    [SerializeField] private List<string> defendLocks = new List<string>();
    [SerializeField] private List<string> jumpLocks = new List<string>();
    [SerializeField] private List<string> swordJumpLocks = new List<string>();
    [SerializeField] private List<string> swordTeleportLocks = new List<string>();
    [SerializeField] private List<string> stormLocks = new List<string>();

    #endregion Inspector Debug

    #region Unity Lifecycle

    private void Awake()
    {
        instance = this;
        RefreshInspectorLists();
    }

    private void Update()
    {
        // Refresh while timed locks are active so remaining duration is visible in inspector.
        if (_timedLocks.Count > 0)
        {
            RefreshInspectorLists();
        }
    }

    #endregion Unity Lifecycle

    #region Public API

    public static bool Can(Lock actions)
    {
        foreach (Lock action in EnumerateActions(actions))
        {
            if (instance.GetNamedTotalCount(action) > 0) return false;
        }

        return true;
    }

    /// <summary>
    /// Adds or replaces a named lock.
    /// If the same lock name already exists, it is fully replaced:
    /// - old action entries are removed
    /// - old timers are cancelled
    /// - old callbacks are discarded
    /// </summary>
    public static IDisposable Add(string lockName, Lock actions, Action onUnlocked = null)
    {
        instance.StopMovementIfNeeded(actions);
        instance.ReplaceLock(lockName, actions);

        instance.RegisterUnlockCallback(lockName, onUnlocked);
        instance.RefreshInspectorLists();
        return new BlockToken(lockName);
    }

    /// <summary>
    /// Adds/replaces a named lock with duration.
    /// </summary>
    public static IDisposable Add(string lockName, Lock actions, float duration, bool useUnscaledTime = false, Action onUnlocked = null)
    {
        Add(lockName, actions, onUnlocked);

        if (duration <= 0f)
        {
            Remove(lockName);
            return null;
        }

        LockTimer timer = new LockTimer(duration, useUnscaledTime);
        instance.RegisterTimer(lockName, timer);
        instance.StartCoroutine(instance.RemoveAfterDelay(lockName, timer));
        return new BlockToken(lockName);
    }

    /// <summary>
    /// Adds/replaces a named lock where passed actions are NOT locked.
    /// Example: AddExcept("someLock", Lock.Move | Lock.Jump) locks everything except Move and Jump.
    /// </summary>
    public static IDisposable AddExcept(string lockName, Lock excludedActions, Action onUnlocked = null)
    {
        Lock actionsToLock = GetActionsExcept(excludedActions);
        return Add(lockName, actionsToLock, onUnlocked);
    }

    /// <summary>
    /// Adds/replaces a timed named lock where passed actions are NOT locked.
    /// </summary>
    public static IDisposable AddExcept(string lockName, Lock excludedActions, float duration, bool useUnscaledTime = false, Action onUnlocked = null)
    {
        Lock actionsToLock = GetActionsExcept(excludedActions);
        return Add(lockName, actionsToLock, duration, useUnscaledTime, onUnlocked);
    }

    /// <summary>
    /// Removes one count for this lock name from each action containing it.
    /// </summary>
    public static void Remove(string lockName)
    {
        instance.RemoveInternal(lockName, null);
    }

    public static void ClearAll()
    {
        foreach (KeyValuePair<string, List<LockTimer>> pair in instance._timedLocks)
        {
            List<LockTimer> timers = pair.Value;
            for (int i = 0; i < timers.Count; i++)
            {
                timers[i].Cancelled = true;
            }
        }

        instance._timedLocks.Clear();
        instance._namedLocks.Clear();
        instance._unlockCallbacks.Clear();
        instance.StopAllCoroutines();
        instance.RefreshInspectorLists();
    }

    #endregion Public API

    #region Lock Mutation

    private void ReplaceLock(string lockName, Lock actions)
    {
        RemoveAllByName(lockName);

        foreach (Lock action in EnumerateActions(actions))
        {
            AddNamedLock(action, lockName);
        }
    }

    private void RemoveAllByName(string lockName)
    {
        List<Lock> actions = new List<Lock>(_namedLocks.Keys);

        for (int i = 0; i < actions.Count; i++)
        {
            Lock action = actions[i];

            if (!_namedLocks.TryGetValue(action, out Dictionary<string, int> named)) continue;

            named.Remove(lockName);

            if (named.Count == 0)
            {
                _namedLocks.Remove(action);
            }
        }

        if (_timedLocks.TryGetValue(lockName, out List<LockTimer> timers))
        {
            for (int i = 0; i < timers.Count; i++)
            {
                timers[i].Cancelled = true;
            }

            _timedLocks.Remove(lockName);
        }

        // Replacement semantics: old callbacks are intentionally dropped.
        _unlockCallbacks.Remove(lockName);
    }

    private void RemoveInternal(string lockName, LockTimer timerToConsume)
    {
        int beforeCount = GetLockNameCount(lockName);

        List<Lock> actions = new List<Lock>(_namedLocks.Keys);

        for (int i = 0; i < actions.Count; i++)
        {
            Lock action = actions[i];

            if (!_namedLocks.TryGetValue(action, out Dictionary<string, int> named)) continue;
            if (!named.TryGetValue(lockName, out int count)) continue;

            if (count <= 1) named.Remove(lockName);
            else named[lockName] = count - 1;

            if (named.Count == 0) _namedLocks.Remove(action);
        }

        if (timerToConsume != null)
        {
            RemoveSpecificTimer(lockName, timerToConsume);
        }
        else
        {
            ConsumeAnyTimer(lockName);
        }

        int afterCount = GetLockNameCount(lockName);
        if (beforeCount > 0 && afterCount == 0)
        {
            InvokeAllUnlockCallbacks(lockName);
        }

        RefreshInspectorLists();
    }

    private void AddNamedLock(Lock action, string lockName)
    {
        if (!_namedLocks.TryGetValue(action, out Dictionary<string, int> named))
        {
            named = new Dictionary<string, int>();
            _namedLocks[action] = named;
        }

        named[lockName] = named.TryGetValue(lockName, out int count) ? count + 1 : 1;
    }

    #endregion Lock Mutation

    #region Timer Management

    private IEnumerator RemoveAfterDelay(string lockName, LockTimer timer)
    {
        while (!timer.Cancelled && timer.Remaining > 0f)
        {
            float delta = timer.UseUnscaledTime ? Time.unscaledDeltaTime : TimeScaleManager.GameplayDt;
            timer.Remaining -= delta;
            yield return null;
        }

        if (timer.Cancelled) yield break;
        RemoveInternal(lockName, timer);
    }

    private void RegisterTimer(string lockName, LockTimer timer)
    {
        if (!_timedLocks.TryGetValue(lockName, out List<LockTimer> list))
        {
            list = new List<LockTimer>();
            _timedLocks[lockName] = list;
        }

        list.Add(timer);
    }

    private void RemoveSpecificTimer(string lockName, LockTimer timer)
    {
        if (!_timedLocks.TryGetValue(lockName, out List<LockTimer> list)) return;

        timer.Cancelled = true;
        list.Remove(timer);

        if (list.Count == 0) _timedLocks.Remove(lockName);
    }

    private void ConsumeAnyTimer(string lockName)
    {
        if (!_timedLocks.TryGetValue(lockName, out List<LockTimer> list)) return;
        if (list.Count == 0) return;

        LockTimer timer = list[0];
        timer.Cancelled = true;
        list.RemoveAt(0);

        if (list.Count == 0) _timedLocks.Remove(lockName);
    }

    #endregion Timer Management

    #region Callback Management

    private void RegisterUnlockCallback(string lockName, Action callback)
    {
        if (callback == null) return;

        if (!_unlockCallbacks.TryGetValue(lockName, out List<Action> callbacks))
        {
            callbacks = new List<Action>();
            _unlockCallbacks[lockName] = callbacks;
        }

        callbacks.Add(callback);
    }

    private void InvokeAllUnlockCallbacks(string lockName)
    {
        if (!_unlockCallbacks.TryGetValue(lockName, out List<Action> callbacks)) return;

        _unlockCallbacks.Remove(lockName);

        for (int i = 0; i < callbacks.Count; i++)
        {
            callbacks[i]?.Invoke();
        }
    }

    #endregion Callback Management

    #region Queries / Utility

    private int GetNamedTotalCount(Lock action)
    {
        if (!_namedLocks.TryGetValue(action, out Dictionary<string, int> named)) return 0;

        int total = 0;
        foreach (KeyValuePair<string, int> entry in named)
        {
            total += entry.Value;
        }

        return total;
    }

    private static Lock GetActionsExcept(Lock excludedActions)
    {
        return Lock.All & ~excludedActions;
    }

    private int GetLockNameCount(string lockName)
    {
        int total = 0;

        foreach (KeyValuePair<Lock, Dictionary<string, int>> pair in _namedLocks)
        {
            if (pair.Value.TryGetValue(lockName, out int count))
            {
                total += count;
            }
        }

        return total;
    }

    private void StopMovementIfNeeded(Lock actions)
    {
        if ((actions & Lock.Move) == 0) return;

        CharacterController2D controller = CharacterController2D.instance;
        if (controller == null) return;

        controller.StopMovement();
    }

    private static IEnumerable<Lock> EnumerateActions(Lock actions)
    {
        if ((actions & Lock.Move) != 0) yield return Lock.Move;
        if ((actions & Lock.Flip) != 0) yield return Lock.Flip;
        if ((actions & Lock.Attack) != 0) yield return Lock.Attack;
        if ((actions & Lock.Defend) != 0) yield return Lock.Defend;
        if ((actions & Lock.Jump) != 0) yield return Lock.Jump;
        if ((actions & Lock.SwordJump) != 0) yield return Lock.SwordJump;
        if ((actions & Lock.SwordTeleport) != 0) yield return Lock.SwordTeleport;
        if ((actions & Lock.Storm) != 0) yield return Lock.Storm;
    }

    #endregion Queries / Utility

    #region Inspector Helpers

    private void RefreshInspectorLists()
    {
        moveLocks = BuildActionList(Lock.Move);
        flipLocks = BuildActionList(Lock.Flip);
        attackLocks = BuildActionList(Lock.Attack);
        defendLocks = BuildActionList(Lock.Defend);
        jumpLocks = BuildActionList(Lock.Jump);
        swordJumpLocks = BuildActionList(Lock.SwordJump);
        swordTeleportLocks = BuildActionList(Lock.SwordTeleport);
        stormLocks = BuildActionList(Lock.Storm);
    }

    private List<string> BuildActionList(Lock action)
    {
        List<string> result = new List<string>();

        if (!_namedLocks.TryGetValue(action, out Dictionary<string, int> named))
        {
            return result;
        }

        List<KeyValuePair<string, int>> entries = new List<KeyValuePair<string, int>>(named);
        entries.Sort((a, b) => string.CompareOrdinal(a.Key, b.Key));

        for (int i = 0; i < entries.Count; i++)
        {
            KeyValuePair<string, int> entry = entries[i];
            string countText = entry.Value > 1 ? " x" + entry.Value : string.Empty;
            string durationText = GetRemainingDurationText(entry.Key);
            result.Add(entry.Key + countText + durationText);
        }

        return result;
    }

    private string GetRemainingDurationText(string lockName)
    {
        if (!_timedLocks.TryGetValue(lockName, out List<LockTimer> list)) return string.Empty;
        if (list.Count == 0) return string.Empty;

        float maxRemaining = 0f;
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].Remaining > maxRemaining) maxRemaining = list[i].Remaining;
        }

        if (maxRemaining <= 0f) return string.Empty;
        return " (" + maxRemaining.ToString("0.00") + "s)";
    }

    #endregion Inspector Helpers

    #region Nested Types

    private sealed class LockTimer
    {
        public float Remaining;
        public readonly bool UseUnscaledTime;
        public bool Cancelled;

        public LockTimer(float duration, bool useUnscaledTime)
        {
            Remaining = duration;
            UseUnscaledTime = useUnscaledTime;
            Cancelled = false;
        }
    }

    #endregion Nested Types
}

public sealed class BlockToken : IDisposable
{
    private readonly string _lockName;
    private bool _disposed;

    public BlockToken(string lockName)
    {
        _lockName = lockName;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        ActionLock.Remove(_lockName);
    }
}