/// <summary>
/// Default movement state implementation.
/// Delegates all behavior to the normal-state methods on <see cref="PlayerControl"/>.
/// </summary>
public sealed class NormalMovementState : IMovementState
{
    #region References

    public NormalMovementState(PlayerControl controller) : base(controller)
    {
        base.controller = controller;
        base.playerAttack = PlayerAttack.instance;
        base.energy = Energy.instance;
        base.health = Health.instance;
        base.anim = controller.anim;
        base.legAnim = controller.legAnim;
        base.rb = controller.rb;
    }

    public override PlayerMovementStateType Type => PlayerMovementStateType.Normal;

    #endregion References
}