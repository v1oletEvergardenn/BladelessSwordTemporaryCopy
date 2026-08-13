/// <summary>
/// Default movement state implementation.
/// Delegates all behavior to the normal-state methods on <see cref="PlayerControl"/>.
/// </summary>
public sealed class NormalMovementState : IMovementState
{
    private readonly PlayerControl _controller;
    public PlayerMovementStateType Type => PlayerMovementStateType.Normal;

    public NormalMovementState(PlayerControl controller)
    {
        _controller = controller;
    }

    public void Enter()
    {
    }

    public void Exit()
    {
    }

    public void Tick()
    {
        _controller.TickNormalState();
    }

    public void FixedTick()
    {
        _controller.FixedTickNormalState();
    }

    /// <summary>
    /// Handles horizontal movement using controller-selected speed.
    /// </summary>
    /// <param name="input">Horizontal movement input.</param>
    public void Move(float input)
    {
        _controller.MoveNormalState(input, _controller.GetSpeed());
    }

    /// <summary>
    /// Handles horizontal movement with explicit speed override.
    /// </summary>
    /// <param name="input">Horizontal movement input.</param>
    /// <param name="speed">Movement speed override.</param>
    public void Move(float input, float speed)
    {
        _controller.MoveNormalState(input, speed);
    }

    /// <summary>
    /// Triggers normal jump behavior.
    /// </summary>
    public void Jump()
    {
        _controller.JumpNormalState();
    }

    /// <summary>
    /// Triggers normal double-jump behavior.
    /// </summary>
    /// <param name="holdTime">Input hold duration used for force interpolation.</param>
    public void DoubleJump(float holdTime)
    {
        _controller.DoubleJumpNormalState(holdTime);
    }

    /// <summary>
    /// Triggers normal sword-teleport behavior.
    /// </summary>
    public void SwordTeleport()
    {
        _controller.SwordTeleportNormalState();
    }
}