using System;

public enum TimeChannel
{
    Global = 0,
    Gameplay = 1,
    Player = 2,
    Enemy = 3,
    Projectile = 4,
    Camera = 5,
    UI = 6,
}

[Serializable]
public sealed class TimeModifier
{
    public int id;
    public string source;
    public float multiplier;
    public float duration;
    public float remaining;
    public bool tickWithUnscaledTime;
    public int priority;
    public bool expired;

    public bool IsTimed => duration >= 0f;

    public TimeModifier Clone()
    {
        return new TimeModifier
        {
            id = id,
            source = source,
            multiplier = multiplier,
            duration = duration,
            remaining = remaining,
            tickWithUnscaledTime = tickWithUnscaledTime,
            priority = priority,
            expired = expired,
        };
    }
}