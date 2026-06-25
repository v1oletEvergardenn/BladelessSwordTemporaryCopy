using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public enum PlayerTimelineActionType
{
    MoveTo,
    Repel
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

    // repel (target-based)
    public void RepelTo(Vector3 worldPosition)
    {
        AssignRef();
        playerHealth.RepelToPosition(worldPosition);
    }

    // Utility
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