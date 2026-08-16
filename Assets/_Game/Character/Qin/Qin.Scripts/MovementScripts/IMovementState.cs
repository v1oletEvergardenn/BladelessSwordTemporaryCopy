/// <summary>
/// Contract for a player movement state.
/// </summary>
public interface IMovementState
{
    /// <summary>
    /// Enum type of this movement state.
    /// </summary>
    PlayerMovementStateType Type { get; }

    /// <summary>
    /// Called when state becomes active.
    /// </summary>
    void Enter();

    /// <summary>
    /// Called when state is exited.
    /// </summary>
    void Exit();

    /// <summary>
    /// Per-frame update.
    /// </summary>
    void Tick();

    /// <summary>
    /// Per-physics-step update.
    /// </summary>
    void FixedTick();

    /// <summary>
    /// Handle movement with speed override.
    /// </summary>
    void Move(float input, float speed);

    /// <summary>
    /// Trigger jump.
    /// </summary>
    void Jump();

    /// <summary>
    /// Trigger double jump.
    /// </summary>
    void DoubleJump(float holdTime);

    /// <summary>
    /// Trigger sword teleport.
    /// </summary>
    void SwordTeleport();
}