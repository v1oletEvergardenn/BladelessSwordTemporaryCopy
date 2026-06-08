using UnityEngine;

public enum PlayerTimelineActionType
{
    None = 0,
    MoveTo = 1,
    Attack = 2,
    Jump = 3
}

public interface IPlayerTimelineActions
{
    void MoveTo(Vector3 worldPosition, bool faceTarget);

    void Attack(bool attackLeft, bool consumeEnergy);

    void Jump();
}

[DisallowMultipleComponent]
public class PlayerTimelineActions : MonoBehaviour, IPlayerTimelineActions
{
    public static PlayerTimelineActions instance;

    [SerializeField] private PlayerAttack playerAttack;
    [SerializeField] private CharacterController2D controller;

    private void Awake()
    {
        if (instance == null)
            instance = this;

        if (playerAttack == null)
            playerAttack = GetComponent<PlayerAttack>();

        if (controller == null)
            controller = GetComponent<CharacterController2D>();
    }

    public void MoveTo(Vector3 worldPosition, bool faceTarget)
    {
        if (controller == null)
            return;

        controller.RunToPosition(worldPosition, null, faceTarget);
    }

    public void Attack(bool attackLeft, bool consumeEnergy)
    {
        if (playerAttack == null)
            return;

        playerAttack.Attack(attackLeft, consumeEnergy);
    }

    public void Jump()
    {
        if (controller == null)
            return;

        controller.Jump();
    }
}