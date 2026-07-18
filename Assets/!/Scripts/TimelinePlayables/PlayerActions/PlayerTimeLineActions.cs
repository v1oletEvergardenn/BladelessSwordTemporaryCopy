using System.Collections;
using UnityEngine;

public enum PlayerTimelineActionType
{
    MoveTo,
    Repel
}

[DisallowMultipleComponent]
public class PlayerTimeLineActions : MonoBehaviour
{
    private const string TimelineCustomMoveLockKey = "TimelineCustomMove";

    public static PlayerTimeLineActions instance;

    [HideInInspector] public PlayerAttack playerAttack;
    [HideInInspector] public CharacterController2D controller;
    [HideInInspector] public Health playerHealth;
    [HideInInspector] public Energy playerEnergy;
    [HideInInspector] public InputPlayer inputPlayer;
    [HideInInspector] public Animator anim;

    private Coroutine _customMoveRoutine;

    private void Awake()
    {
        if (instance == null)
            instance = this;
    }

    private void Start()
    {
        AssignRef();
    }

    public void MoveTo(Vector3 worldPosition, bool isRun)
    {
        AssignRef();
        StopCustomMoveIfRunning();

        if (isRun) controller.RunToPosition(worldPosition);
        else controller.WalkToPosition(worldPosition);
    }

    public void MoveTo(Transform worldPosition, bool isRun)
    {
        AssignRef();
        StopCustomMoveIfRunning();

        if (isRun) controller.RunToPosition(worldPosition.position);
        else controller.WalkToPosition(worldPosition.position);
    }

    public void MoveTo(Vector3 worldPosition, float customSpeed)
    {
        AssignRef();

        if (controller == null)
            return;

        StopCustomMoveIfRunning();
        _customMoveRoutine = StartCoroutine(MoveToCustomSpeedCoroutine(worldPosition, Mathf.Max(0.01f, customSpeed)));
    }

    private IEnumerator MoveToCustomSpeedCoroutine(Vector3 targetPosition, float speed)
    {
        if (inputPlayer != null)
            inputPlayer.movementInputUpdateLock.Add(TimelineCustomMoveLockKey);

        while (controller != null)
        {
            Vector3 current = controller.transform.position;
            float deltaX = targetPosition.x - current.x;
            if (Mathf.Abs(deltaX) <= 0.05f)
                break;

            float dir = Mathf.Sign(deltaX);
            controller.Move(dir, speed);
            yield return null;
        }

        if (controller != null)
        {
            controller.Move(0f, speed);
            Vector3 p = controller.transform.position;
            controller.transform.position = new Vector3(targetPosition.x, p.y, p.z);
        }

        if (inputPlayer != null)
            inputPlayer.movementInputUpdateLock.Remove(TimelineCustomMoveLockKey);

        _customMoveRoutine = null;
    }

    private void StopCustomMoveIfRunning()
    {
        if (_customMoveRoutine == null)
            return;

        StopCoroutine(_customMoveRoutine);
        _customMoveRoutine = null;

        if (inputPlayer != null)
            inputPlayer.movementInputUpdateLock.Remove(TimelineCustomMoveLockKey);
    }

    public void RepelTo(Vector3 worldPosition)
    {
        AssignRef();
        playerHealth.RepelToPosition(worldPosition);
    }

    public void RepelByDistance(float distance)
    {
        AssignRef();
        playerHealth.RepelWithoutDirection(distance);
    }

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