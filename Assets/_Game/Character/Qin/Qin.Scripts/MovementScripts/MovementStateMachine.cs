using System.Collections.Generic;

/// <summary>
/// Lightweight state machine for player movement.
/// </summary>
public sealed class MovementStateMachine
{
    private readonly Dictionary<PlayerMovementStateType, IMovementState> _states =
        new Dictionary<PlayerMovementStateType, IMovementState>();

    /// <summary>
    /// Currently active movement state.
    /// </summary>
    public IMovementState Current { get; private set; }

    /// <summary>
    /// Type of the currently active movement state.
    /// </summary>
    public PlayerMovementStateType? CurrentType => Current?.Type;

    /// <summary>
    /// Register or replace a movement state implementation.
    /// </summary>
    public void Register(IMovementState state)
    {
        if (state == null)
            return;

        _states[state.Type] = state;
    }

    /// <summary>
    /// Switch to a registered movement state.
    /// </summary>
    public bool ChangeState(PlayerMovementStateType type)
    {
        if (!_states.TryGetValue(type, out var next))
            return false;

        if (Current == next)
            return false;

        Current?.Exit();
        Current = next;
        Current.Enter();
        return true;
    }

    /// <summary>
    /// Tick current state from Update.
    /// </summary>
    public void Tick() => Current?.Tick();

    /// <summary>
    /// Tick current state from FixedUpdate.
    /// </summary>
    public void FixedTick() => Current?.FixedTick();

    /// <summary>
    /// Forward movement input.
    /// </summary>
    public void Move(float input) => Current?.Move(input);

    /// <summary>
    /// Forward movement input with speed override.
    /// </summary>
    public void Move(float input, float speed) => Current?.Move(input, speed);

    /// <summary>
    /// Forward jump.
    /// </summary>
    public void Jump() => Current?.Jump();

    /// <summary>
    /// Forward double jump.
    /// </summary>
    public void DoubleJump(float holdTime) => Current?.DoubleJump(holdTime);

    /// <summary>
    /// Forward sword teleport.
    /// </summary>
    public void SwordTeleport() => Current?.SwordTeleport();
}