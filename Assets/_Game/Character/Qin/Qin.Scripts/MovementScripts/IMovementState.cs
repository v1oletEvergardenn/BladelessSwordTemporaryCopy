using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem.XR;

/// <summary>
/// Contract for a player movement state.
/// </summary>
public abstract class IMovementState
{
    public abstract PlayerMovementStateType Type { get; }

    public PlayerControl controller;
    public PlayerAttack playerAttack;
    public Energy energy;
    public Health health;
    public Animator anim;
    public Animator legAnim;
    public InputPlayer inputPlayer;
    public Rigidbody2D rb;

    public IMovementState(PlayerControl controller)
    {
        this.controller = controller;
        this.playerAttack = PlayerAttack.instance;
        this.energy = Energy.instance;
        this.health = Health.instance;
        this.anim = controller.anim;
        this.legAnim = controller.legAnim;
        this.rb = controller.rb;
    }

    /// <summary>
    /// Called when state becomes active.
    /// </summary>
    public virtual void Enter()
    { return; }

    /// <summary>
    /// Called when state is exited.
    /// </summary>
    public virtual void Exit()
    { return; }

    /// <summary>
    /// Per-frame update.
    /// </summary>
    public virtual void Tick()
    { controller.TickNormalState(); }

    /// <summary>
    /// Per-physics-step update.
    /// </summary>
    public virtual void FixedTick()
    { controller.FixedTickNormalState(); }

    /// <summary>
    /// Handle movement with speed override.
    /// </summary>
    public virtual void Move(float input, float speed)
    { controller.MoveNormalState(input, speed); }

    /// <summary>
    /// Trigger jump.
    /// </summary>
    public virtual void Jump()
    { controller.JumpNormalState(); }

    /// <summary>
    /// Trigger double jump.
    /// </summary>
    public virtual void DoubleJump(float holdTime)
    { controller.DoubleJumpNormalState(holdTime); }

    /// <summary>
    /// Trigger sword teleport.
    /// </summary>
    public virtual void SwordTeleport()
    {
        controller.SwordTeleportNormalState();
    }

    public virtual void HandleFallingAnimation()
    {
        controller.HandleFallingAnimationNormalState();
    }

    public virtual void HandleLandingAnimation()
    {
        controller.HandleLandingAnimationNormalState();
    }

    public virtual void HandleJumpAnimation()
    {
        controller.HandleJumpAnimationNormalState();
    }

    public virtual void HandleGroundedAnimation()
    {
        controller.HandleGroundedAnimationNormalState();
    }

    #region Animation Helpers

    public virtual void PlayBackAttack(bool isHSAttack) => controller.PlayBackAttack(isHSAttack);

    public virtual string AttackClip(string phase) => controller.AttackClip(phase);

    public virtual string HSAttackClip(string phase) => controller.HSAttackClip(phase);

    public virtual string AttackBackClip(bool isHSAttack) => controller.AttackBackClip(isHSAttack);

    public virtual bool IsAttackIdleState() => controller.IsAttackIdleState();

    public virtual bool IsAttackRunState() => controller.IsAttackRunState();

    public virtual bool IsAttackJumpState() => controller.IsAttackJumpState();

    public virtual bool IsAttackFallState() => controller.IsAttackFallState();

    public virtual bool IsHSAttackJumpState() => controller.IsHSAttackJumpState();

    public virtual bool IsHSAttackFallState() => controller.IsHSAttackFallState();

    public virtual bool IsHSAttackRunState() => controller.IsHSAttackRunState();

    public virtual bool IsHSAttackIdleState() => controller.IsHSAttackIdleState();

    public virtual bool IsStormReadyState() => controller.IsStormReadyState();

    public virtual bool IsStormPreState() => controller.IsStormPreState();

    public virtual bool IsAttackRunOrIdleState() => controller.IsAttackRunOrIdleState();

    public virtual bool IsHSAttackRunOrIdleState() => controller.IsHSAttackRunOrIdleState();

    public virtual bool IsAttackJumpOrFallState() => controller.IsAttackJumpOrFallState();

    public virtual bool IsHSAttackJumpOrFallState() => controller.IsHSAttackJumpOrFallState();

    public virtual bool CheckName(string name) => controller.CheckName(name);

    public virtual void PlayAnim(string clip, float duration = -1) => controller.PlayAnim(clip, duration);

    public virtual void PlayAnimClipInCombat(string normalClip, string combatClip) => controller.PlayAnimClipInCombat(normalClip, combatClip);

    #endregion Animation Helpers
}