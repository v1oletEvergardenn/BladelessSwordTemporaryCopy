using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public enum PlayerTimelineActionType
{
    MoveTo = 0,
    Jump = 1,
    Attack = 2,
    Defend = 3,
    SwordTeleport = 4,
    Repel = 5,
}

[DisallowMultipleComponent]
public class PlayerTimeLineActions : MonoBehaviour
{
    public static PlayerTimeLineActions instance;

    [HideInInspector] public PlayerAttack playerAttack;
    [HideInInspector] public CharacterController2D controller;
    [HideInInspector] public Health playerHealth;
    [HideInInspector] public Energy playerEnergy;
    [HideInInspector] public InputPlayer inputPlayer;
    [HideInInspector] public Animator anim;

    private void Awake()
    {
        if (instance == null)
            instance = this;
    }

    private void Start()
    {
        AssignRef();
    }

    // move
    public void MoveTo(Vector3 worldPosition)
    {
        AssignRef();
        controller.RunToPosition(worldPosition);
    }

    public void MoveTo(Transform worldPosition)
    {
        AssignRef();
        controller.RunToPosition(worldPosition.position);
    }

    //jump
    public void Jump()
    {
        AssignRef();
        if (controller == null)
            return;

        controller.Jump();
    }

    //attack
    public void Attack(bool attackLeft)
    {
        AssignRef();
        playerAttack.Attack(attackLeft, false);
    }

    //defend

    public void Defend(float duration)
    {
        playerAttack.isDefending = true;
        //playerAttack.animSet.Anim_Defend(0);
        anim.Play("defend");

        playerAttack.OnDefend();
    }

    public void EndDefend()
    {
        //playerAttack.animSet.Anim_Defend(1);
        anim.SetBool("isCombat", true);
        playerAttack.combatTimer = 2;
        if (controller.isFalling) { anim.Play("fall_combat"); }
        else if (controller.isJumping) { anim.Play("jump_combat"); }
        else { anim.Play("idle_combat"); }
        playerAttack.isDefending = false;
    }

    //sword teleport

    //repel

    //Utility
    public void AssignRef()
    {
        if (playerAttack == null) playerAttack = PlayerAttack.instance;
        if (controller == null) controller = CharacterController2D.instance;
        if (inputPlayer == null) inputPlayer = InputPlayer.instance;
        if (playerHealth == null) playerHealth = Health.instance;
        if (playerEnergy == null) playerEnergy = Energy.instance;
        if (anim == null) anim = PlayerAttack.instance.anim;
    }
}