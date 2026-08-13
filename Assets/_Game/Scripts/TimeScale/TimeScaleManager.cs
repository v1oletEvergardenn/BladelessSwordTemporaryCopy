using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Layered time system with channel-based, stackable modifiers.
/// </summary>
public class TimeScaleManager : MonoBehaviour
{
    public static TimeScaleManager instance;

    public static float GlobalDt => instance.GetDeltaTime(TimeChannel.Global);
    public static float GameplayDt => instance.GetDeltaTime(TimeChannel.Gameplay);
    public static float EnemyDt => instance.GetDeltaTime(TimeChannel.Enemy);
    public static float PlayerDt => instance.GetDeltaTime(TimeChannel.Player);
    public static float ProjDt => instance.GetDeltaTime(TimeChannel.Projectile);
    public static float CameraDt => instance.GetDeltaTime(TimeChannel.Camera);
    public static float UIDt => instance.GetDeltaTime(TimeChannel.UI);

    public static float GlobalScale => instance.GetCompositeScale(TimeChannel.Global);
    public static float GameplayScale => instance.GetCompositeScale(TimeChannel.Gameplay);
    public static float EnemyScale => instance.GetCompositeScale(TimeChannel.Enemy);
    public static float PlayerScale => instance.GetCompositeScale(TimeChannel.Player);
    public static float ProjScale => instance.GetCompositeScale(TimeChannel.Projectile);
    public static float CameraScale => instance.GetCompositeScale(TimeChannel.Camera);
    public static float UIScale => instance.GetCompositeScale(TimeChannel.UI);

    [Serializable]
    private sealed class ChannelState
    {
        public float baseScale = 1f;
        public float localScale = 1f;
        public readonly List<TimeModifier> modifiers = new List<TimeModifier>();
    }

    [Serializable]
    public sealed class ChannelDebugSnapshot
    {
        public TimeChannel channel;
        public float baseScale;
        public float localScale;
        public float compositeScale;
        public List<TimeModifier> modifiers;
    }

    private readonly Dictionary<TimeChannel, ChannelState> channels = new Dictionary<TimeChannel, ChannelState>();
    private int nextModifierId = 1;
    private float baseFixedDeltaTime = 0.02f;

    private void Awake()
    {
        if (instance == null) { instance = this; }
        else { Destroy(gameObject); return; }

        baseFixedDeltaTime = Time.fixedDeltaTime;
        InitializeChannels();
        RecalculateAll();
    }

    private void Update()
    {
        TickModifiers();
        RecalculateAll();
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            Time.timeScale = 1f;
            Time.fixedDeltaTime = baseFixedDeltaTime;
            instance = null;
        }
    }

    private void InitializeChannels()
    {
        channels.Clear();

        Array values = Enum.GetValues(typeof(TimeChannel));
        for (int i = 0; i < values.Length; i++)
        {
            TimeChannel channel = (TimeChannel)values.GetValue(i);
            channels[channel] = new ChannelState();
        }
    }

    public void SetBaseScale(TimeChannel channel, float scale)
    {
        ChannelState state = channels[channel];
        state.baseScale = Mathf.Max(0f, scale);
        RecalculateChannel(channel);
    }

    public float GetBaseScale(TimeChannel channel)
    {
        return channels[channel].baseScale;
    }

    /// <summary>
    /// Adds a stackable modifier to one channel.
    /// duration < 0 means persistent.
    /// </summary>
    public static int AddModifier(
        TimeChannel channel,
        string source,
        float multiplier,
        float duration = -1f,
        bool tickWithUnscaledTime = true,
        int priority = 0)
    {
        ChannelState state = instance.channels[channel];
        TimeModifier modifier = new TimeModifier
        {
            id = instance.nextModifierId++,
            source = string.IsNullOrEmpty(source) ? "Unknown" : source,
            multiplier = Mathf.Max(0f, multiplier),
            duration = duration,
            remaining = duration,
            tickWithUnscaledTime = tickWithUnscaledTime,
            priority = priority,
            expired = false,
        };

        state.modifiers.Add(modifier);
        instance.RecalculateChannel(channel);
        return modifier.id;
    }

    public void RemoveModifierById(int id)
    {
        Array values = Enum.GetValues(typeof(TimeChannel));
        for (int i = 0; i < values.Length; i++)
        {
            TimeChannel channel = (TimeChannel)values.GetValue(i);
            ChannelState state = channels[channel];
            for (int m = state.modifiers.Count - 1; m >= 0; m--)
            {
                if (state.modifiers[m].id == id)
                {
                    state.modifiers.RemoveAt(m);
                    RecalculateChannel(channel);
                    return;
                }
            }
        }
    }

    public static int RemoveModifiersBySource(string source)
    {
        if (string.IsNullOrEmpty(source))
            return 0;

        int removed = 0;
        Array values = Enum.GetValues(typeof(TimeChannel));
        for (int i = 0; i < values.Length; i++)
        {
            TimeChannel channel = (TimeChannel)values.GetValue(i);
            ChannelState state = instance.channels[channel];
            for (int m = state.modifiers.Count - 1; m >= 0; m--)
            {
                if (state.modifiers[m].source == source)
                {
                    state.modifiers.RemoveAt(m);
                    removed++;
                }
            }
            instance.RecalculateChannel(channel);
        }

        return removed;
    }

    public void ClearChannel(TimeChannel channel)
    {
        channels[channel].modifiers.Clear();
        RecalculateChannel(channel);
    }

    public void ClearAll()
    {
        Array values = Enum.GetValues(typeof(TimeChannel));
        for (int i = 0; i < values.Length; i++)
        {
            TimeChannel channel = (TimeChannel)values.GetValue(i);
            channels[channel].modifiers.Clear();
            channels[channel].baseScale = 1f;
            channels[channel].localScale = 1f;
        }
    }

    public float GetLocalScale(TimeChannel channel)
    {
        return channels[channel].localScale;
    }

    /// <summary>
    /// Final scale after hierarchy composition.
    /// </summary>
    public float GetCompositeScale(TimeChannel channel)
    {
        float global = channels[TimeChannel.Global].localScale;

        if (channel == TimeChannel.Global)
            return Mathf.Max(0f, global);

        if (channel == TimeChannel.UI)
            return Mathf.Max(0f, global * channels[TimeChannel.UI].localScale);

        float gameplay = channels[TimeChannel.Gameplay].localScale;

        if (channel == TimeChannel.Gameplay)
            return Mathf.Max(0f, global * gameplay);

        if (channel == TimeChannel.Player)
            return Mathf.Max(0f, global * gameplay * channels[TimeChannel.Player].localScale);

        if (channel == TimeChannel.Enemy)
            return Mathf.Max(0f, global * gameplay * channels[TimeChannel.Enemy].localScale);

        if (channel == TimeChannel.Projectile)
            return Mathf.Max(0f, global * gameplay * channels[TimeChannel.Projectile].localScale);

        if (channel == TimeChannel.Camera)
            return Mathf.Max(0f, global * gameplay * channels[TimeChannel.Camera].localScale);

        return 1f;
    }

    public float GetDeltaTime(TimeChannel channel)
    {
        return Time.unscaledDeltaTime * instance.GetCompositeScale(channel);
    }

    public float GetFixedDeltaTime(TimeChannel channel)
    {
        return baseFixedDeltaTime * GetCompositeScale(channel);
    }

    public List<ChannelDebugSnapshot> CreateDebugSnapshot()
    {
        List<ChannelDebugSnapshot> result = new List<ChannelDebugSnapshot>();
        Array values = Enum.GetValues(typeof(TimeChannel));

        for (int i = 0; i < values.Length; i++)
        {
            TimeChannel channel = (TimeChannel)values.GetValue(i);
            ChannelState state = channels[channel];

            ChannelDebugSnapshot snapshot = new ChannelDebugSnapshot
            {
                channel = channel,
                baseScale = state.baseScale,
                localScale = state.localScale,
                compositeScale = GetCompositeScale(channel),
                modifiers = new List<TimeModifier>(),
            };

            for (int m = 0; m < state.modifiers.Count; m++)
            {
                snapshot.modifiers.Add(state.modifiers[m].Clone());
            }

            result.Add(snapshot);
        }

        return result;
    }

    public static void HitFreeze(float duration, float gameplayScale = 0.05f)
    {
        AddModifier(TimeChannel.Player, "HitStop", gameplayScale, duration, true);
        AddModifier(TimeChannel.Enemy, "HitStop", gameplayScale, duration, true);
        AddModifier(TimeChannel.Projectile, "HitStop", gameplayScale, duration, true);
    }

    public static void EnterBulletTime(
        float playerScale = 1f,
        float enemyScale = 0.1f,
        float cameraScale = 1f,
        float projectileScale = 0.1f)
    {
        VFXManager.StartBossBreakEffect();
        RemoveModifiersBySource("BulletTime");
        AddModifier(TimeChannel.Player, "BulletTime", playerScale, -1f, true);
        AddModifier(TimeChannel.Enemy, "BulletTime", enemyScale, -1f, true);
        AddModifier(TimeChannel.Projectile, "BulletTime", projectileScale, -1f, true);
        AddModifier(TimeChannel.Camera, "BulletTime", cameraScale, -1f, true);
    }

    public static void ExitBulletTime()
    {
        VFXManager.EndBossBreakEffect();
        RemoveModifiersBySource("BulletTime");
    }

    public static void SetPause(bool pause)
    {
        if (pause)
        {
            RemoveModifiersBySource("Pause");
            AddModifier(TimeChannel.Gameplay, "Pause", 0f, -1f, true);
        }
        else
        {
            RemoveModifiersBySource("Pause");
        }
    }

    private void TickModifiers()
    {
        float dt = Time.deltaTime;
        float dtUnscaled = Time.unscaledDeltaTime;

        Array values = Enum.GetValues(typeof(TimeChannel));
        for (int i = 0; i < values.Length; i++)
        {
            TimeChannel channel = (TimeChannel)values.GetValue(i);
            ChannelState state = channels[channel];

            bool changed = false;

            for (int m = state.modifiers.Count - 1; m >= 0; m--)
            {
                TimeModifier modifier = state.modifiers[m];
                if (!modifier.IsTimed)
                    continue;

                modifier.remaining -= modifier.tickWithUnscaledTime ? dtUnscaled : dt;

                if (modifier.remaining <= 0f)
                {
                    state.modifiers.RemoveAt(m);
                    changed = true;
                }
            }

            if (changed)
            {
                RecalculateChannel(channel);
            }
        }
    }

    private void RecalculateAll()
    {
        Array values = Enum.GetValues(typeof(TimeChannel));
        for (int i = 0; i < values.Length; i++)
        {
            RecalculateChannel((TimeChannel)values.GetValue(i));
        }
    }

    private void RecalculateChannel(TimeChannel channel)
    {
        ChannelState state = channels[channel];
        float local = Mathf.Max(0f, state.baseScale);

        for (int i = 0; i < state.modifiers.Count; i++)
        {
            float m = Mathf.Max(0f, state.modifiers[i].multiplier);
            if (m < local) local = m;
        }

        state.localScale = Mathf.Max(0f, local);
    }

    public static float Delta(TimeChannel channel)
    {
        if (instance == null)
            return Time.deltaTime;

        return instance.GetDeltaTime(channel);
    }

    public static float FixedDelta(TimeChannel channel)
    {
        if (instance == null)
            return Time.fixedDeltaTime;

        return instance.GetFixedDeltaTime(channel);
    }

    public static IEnumerator WaitForChannelSeconds(float seconds, TimeChannel channel)
    {
        if (seconds <= 0f) yield break;

        float timer = 0f;
        while (timer < seconds)
        {
            timer += Delta(channel);
            yield return null;
        }
    }

    public static IEnumerator WaitForUnscaledSeconds(float seconds)
    {
        if (seconds <= 0f) yield break;

        float timer = 0f;
        while (timer < seconds)
        {
            timer += Time.unscaledDeltaTime;
            yield return null;
        }
    }

    public static void SetOrAddModifier(TimeChannel channel, string source, float multiplier, float duration = -1f, bool tickWithUnscaledTime = true)
    {
        if (instance == null) return;

        ChannelState state = instance.channels[channel];
        float clamped = Mathf.Max(0f, multiplier);

        for (int i = 0; i < state.modifiers.Count; i++)
        {
            TimeModifier mod = state.modifiers[i];
            if (mod.source == source)
            {
                mod.multiplier = clamped;
                mod.duration = duration;
                mod.remaining = duration;
                mod.tickWithUnscaledTime = tickWithUnscaledTime;
                instance.RecalculateChannel(channel);
                return;
            }
        }

        AddModifier(channel, source, clamped, duration, tickWithUnscaledTime);
    }
}