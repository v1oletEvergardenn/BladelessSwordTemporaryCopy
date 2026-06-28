using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using EditorAttributes;
using Unity.VisualScripting;
using System.Data;
using UnityEngine.SceneManagement;
using System;
using static UnityEngine.EventSystems.EventTrigger;
using Void = EditorAttributes.Void;
using Doublsb.Dialog;

public class InputPlayer : MonoBehaviour
{
    #region Singleton & References

    //[HideInInspector] public ControllerInput.GameplayActions control;
    [HideInInspector] public CharacterController2D controller;

    private PlayerAttack playerAttack;
    [HideInInspector] public Animator anim;
    private GameManager gameManager;
    private InputMaster inputMaster;
    private Health health;
    private Energy energy;
    public static InputPlayer instance;
    private HeartSwordAbilities hSAbilitiesManager;

    #endregion Singleton & References

    #region Pointer & Attack Direction

    [FoldoutGroup("Pointer Variables", nameof(pointer), nameof(pointerOffset),
        nameof(pointerLength), nameof(lockOnTargetAngle), nameof(leftPointerAutoAiming), nameof(leftPointer),
        nameof(rightPointer), nameof(rightPointerLayerMask))]
    [SerializeField] private Void pointerGroupHold;

    [SerializeField, HideProperty] public Transform pointer;
    [SerializeField, HideProperty] public float pointerOffset = 1f;
    [SerializeField, HideProperty] public float pointerLength = 15f;
    [SerializeField, HideProperty] public float lockOnTargetAngle = 10f;
    [SerializeField, HideProperty] public bool leftPointerAutoAiming = true;
    [SerializeField, HideProperty] private Sprite leftPointer;
    [SerializeField, HideProperty] private Sprite rightPointer;
    [SerializeField, HideProperty] private LayerMask rightPointerLayerMask;
    [HideProperty] public bool rightPointLeft = false;
    [HideProperty] public bool leftPointLeft = false;

    [HideProperty] public Vector2 rightAttackDir;
    [HideProperty] public Vector2 leftAttackDir;
    private SpriteRenderer pointerSpriteRenderer;

    #endregion Pointer & Attack Direction

    #region Event & Map Camera

    [Header("EventTrigger")] private EventObject currentEventObject;
    [Header("MapCamera")] public Transform MapCamera;

    #endregion Event & Map Camera

    #region Skills & Progression

    [FoldoutGroup("LearnSkills", nameof(learnedMovement), nameof(learnedJump),
        nameof(learnedTeleport), nameof(learnedDoubleJump), nameof(learnedAttack),
        nameof(learnedBoomerang), nameof(learnedStorm), nameof(learnedDefend),
        nameof(learnedHeartSword))]
    [SerializeField] private Void learnSkillsGroupHold;

    [SerializeField, HideProperty] public bool learnedMovement = true;
    [SerializeField, HideProperty] public bool learnedJump = true;
    [SerializeField, HideProperty] public bool learnedTeleport = true;
    [SerializeField, HideProperty] public bool learnedDoubleJump = false;
    [SerializeField, HideProperty] public bool learnedAttack = false;
    [SerializeField, HideProperty] public bool learnedBoomerang = false;
    [SerializeField, HideProperty] public bool learnedStorm = false;
    [SerializeField, HideProperty] public bool learnedDefend = false;
    [SerializeField, HideProperty] public bool learnedHeartSword = false;

    #endregion Skills & Progression

    #region Movement & Input State

    [HideInInspector] public bool input_floating = false;
    [HideInInspector] public float input_floating_timer = 0f;
    private float horizontalMove = 0f;
    [HideInInspector] public Vector2 moveDir;

    #endregion Movement & Input State

    #region Unity Lifecycle

    private void Awake()
    {
        if (instance == null) { instance = this; }
    }

    private void Start()
    {
        pointerSpriteRenderer = pointer.GetComponent<SpriteRenderer>();
        controller = GetComponent<CharacterController2D>();
        playerAttack = GetComponent<PlayerAttack>();
        energy = GetComponent<Energy>();
        inputMaster = InputMaster.instance;
        health = GetComponent<Health>();
        hSAbilitiesManager = HeartSwordAbilities.instance;

        gameManager = GameManager.instance;
        gameManager.playerInput = this;
        gameManager.playerhealth = Health.instance;
        gameManager.playerAttack = PlayerAttack.instance;
        gameManager.playerEnergy = Energy.instance;
        gameManager.hsManager = HeartSwordAbilities.instance;
        gameManager.player_controller = CharacterController2D.instance;
        gameManager.player = gameManager.playerhealth.gameObject;

        anim = controller.anim;

        inputMaster._jumpAction.canceled += ctx => OnEndJump();
        inputMaster._moveAction.performed += ctx => moveDir = ctx.ReadValue<Vector2>();
        inputMaster._moveAction.performed += ctx => leftAttackDir = ctx.ReadValue<Vector2>();
        inputMaster._moveAction.canceled += ctx => leftAttackDir = Vector2.zero;
        inputMaster._attackDirectionAction.performed += ctx => rightAttackDir = ctx.ReadValue<Vector2>();
        inputMaster._attackDirectionAction.canceled += ctx => rightAttackDir = Vector2.zero;
        inputMaster._defendAction.canceled += ctx => playerAttack.EndDefend();
    }

    private void Update()
    {
        if (CheckAndHandlePauseOrQTE()) return;

        UpdatePointerPosition();

        if (gameManager.isInInformationEvent | gameManager.isInDialog | health.isDead)
        { pointerSpriteRenderer.sprite = null; }
        else { OnAttackDirection(); }
        if (HandleEventKeyInput()) return;
        HandleStormRelease();
        if (health.isDead) return;
        if (HandleDialogOrInfoEvent()) return;

        UpdateFloatingState();
        UpdateMovementInput();
        HandleJumpInput();
        HandleAttackInput();
        HandleAbilityInput();
        HandleDefendInput();
        HandleTeleportInput();

        if (Input.GetKeyDown(KeyCode.Minus))
        {
            Time.timeScale -= 0.2f;
        }
        else if (Input.GetKeyDown(KeyCode.Equals))
        {
            Time.timeScale += 0.2f;
        }
    }

    // --- Helper Methods ---

    private bool CheckAndHandlePauseOrQTE()
    {
        if (gameManager.GamePaused) return true;
        if (inputMaster.isQTE) return true;
        return false;
    }

    private void UpdatePointerPosition()
    {
        pointer.transform.position = transform.position + new Vector3(0, pointerOffset, 0);
    }

    private bool HandleEventKeyInput()
    {
        if (inputMaster._EventKeyAction.WasPressedThisFrame())
        {
            if (DialogManager.instance != null && DialogManager.instance.Printer.activeInHierarchy)
            {
                DialogManager.instance.Click_Window();
                return true;
            }
            if (currentEventObject != null)
            {
                currentEventObject.Interact(true);
                return true;
            }
        }

        if (inputMaster._EventKeyAction.WasReleasedThisFrame())
        {
            if (currentEventObject != null)
            {
                currentEventObject.Interact(false);
                return true;
            }
        }
        return false;
    }

    private void HandleStormRelease()
    {
        if (learnedStorm)
        {
            bool noAttackPressed = !inputMaster._attackLeftAction.IsPressed() && !inputMaster._attackRightAction.IsPressed();
            if (noAttackPressed && playerAttack.isPreparingStorm)
            {
                anim.SetBool("storm", false);
                playerAttack.OnStorm();
                playerAttack.isPreparingStorm = false;
                ActionLock.Remove("StormPreparing");
            }
        }
    }

    private bool HandleDialogOrInfoEvent()
    {
        if (gameManager.isInInformationEvent | gameManager.isInDialog)
        {
            anim.SetBool("isRunning", false);
            GetComponent<Rigidbody2D>().velocity = new Vector2(0, GetComponent<Rigidbody2D>().velocity.y);
            return true;
        }
        return false;
    }

    private void UpdateFloatingState()
    {
        if (learnedDoubleJump && input_floating)
        {
            input_floating_timer += Time.deltaTime;
            controller.floatingTime = input_floating_timer;
        }
        if (learnedDoubleJump && input_floating_timer > 0f)
        {
            controller.input_floating = true;
        }
        else
        {
            controller.input_floating = false;
            ActionLock.Remove("floating");
        }
    }

    public BoolLock movementInputUpdateLock = new BoolLock();

    private void UpdateMovementInput()
    {
        float x = moveDir.x;
        //if (x < 0 && x > -0.4f) x = -0.4f;
        //if (x > 0 && x < 0.4f) x = 0.4f;
        //if (Mathf.Abs(moveDir.magnitude) < 0.7f) x = 0f;
        horizontalMove = (x <= 0 ? -1 : 1);
        if (moveDir.x == 0) horizontalMove = 0;
        if (learnedMovement && !movementInputUpdateLock.Check())
        {
            controller.Move(horizontalMove);
        }
    }

    private void HandleJumpInput()
    {
        if (learnedJump && inputMaster._jumpAction.WasPressedThisFrame())
        {
            OnJump();
        }
    }

    private void HandleAttackInput()
    {
        bool leftPressed = inputMaster._attackLeftAction.IsPressed();
        bool rightPressed = inputMaster._attackRightAction.IsPressed();
        bool rightJustPressed = inputMaster._attackRightAction.WasPressedThisFrame();
        bool leftJustPressed = inputMaster._attackLeftAction.WasPressedThisFrame();
        IHeartSwordAbility currentAbility = hSAbilitiesManager.GetCurrentActivatedAbility();

        if (controller.isFloating) return;
        if (health.stunned) return;
        //checks if any active heart sword ability is triggered by attack key
        //if does, cancel the attack input and perform the ability instead
        if (currentAbility != null &&
            currentAbility.GetCurrentAttribute().isTriggeredByAttackKey)
        {
            bool success = false;
            if (leftJustPressed) { success = currentAbility.CheckPerformAbility(true); }
            else if (rightJustPressed) { success = currentAbility.CheckPerformAbility(false); }
            if (success)
            {
                QuestManager.OnAction(ObjectiveType.HeartSword, currentAbility.questActionID);
                return;
            }
        }

        if (!leftPressed && rightJustPressed)
        {
            playerAttack.Attack(false);

            if (learnedStorm)
            {
                playerAttack.isPreparingStorm = true;
                playerAttack.prepareStormTimer = 0f;
                ActionLock.Add("StormPreparing", Lock.Defend | Lock.SwordJump | Lock.SwordTeleport);
            }
        }
        else if (!rightPressed && leftJustPressed)
        {
            playerAttack.Attack(true);

            if (learnedStorm)
            {
                playerAttack.isPreparingStorm = true;
                playerAttack.prepareStormTimer = 0f;
                ActionLock.Add("StormPreparing", Lock.Defend | Lock.SwordJump | Lock.SwordTeleport);
            }
        }
    }

    private void HandleAbilityInput()
    {
        input(inputMaster._AbilityB, hSAbilitiesManager.GetEastAbility());
        input(inputMaster._AbilityX, hSAbilitiesManager.GetWestAbility());
        input(inputMaster._AbilityY, hSAbilitiesManager.GetNorthAbility());

        void input(InputAction input, IHeartSwordAbility ability)
        {
            if (ability == null) return;
            if (input.WasPressedThisFrame())
            {
                if (ability.toggleToActivate)
                {
                    if (ability.isActive) hSAbilitiesManager.Deactivateability(ability);
                    else hSAbilitiesManager.ActivateAbility(ability);
                }
                else hSAbilitiesManager.ActivateAbility(ability);
            }
            else if (input.WasReleasedThisFrame())
            {
                if (!ability.toggleToActivate) hSAbilitiesManager.Deactivateability(ability);
            }
        }
    }

    private void HandleDefendInput()
    {
        if (learnedDefend && inputMaster._defendAction.WasPressedThisFrame())
        {
            playerAttack.OnDefend();
        }
    }

    private void HandleTeleportInput()
    {
        if (learnedTeleport && inputMaster._teleportAction.WasPressedThisFrame())
        {
            controller.SwordTeleport();
        }
    }

    #endregion Unity Lifecycle

    #region Input & Action Handlers

    /// <summary>
    /// Test function to trigger an event.
    /// </summary>

    private void OnJump()
    {
        if (controller.coyoteTimer > 0f) { input_floating = false; input_floating_timer = 0f; controller.Jump(); controller.coyoteTimer = 0f; }//on land jump/ first jump
        else { input_floating = true; }
    }//jump and start float

    public void DisableFloat()
    {
        input_floating = false;
        input_floating_timer = 0f;
    }

    private void OnEndJump()
    {
        if (input_floating_timer > 0f) { controller.DoubleJump(input_floating_timer); }//double jump, jump higher if hold longer
        input_floating = false;
        input_floating_timer = 0f;
        //floating
    }//double jump and floating

    #endregion Input & Action Handlers

    #region Pointer & Attack Direction Logic

    public float GetRotZFromDirection(Vector3 dir)
    {
        float rotZ = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        return rotZ;
    }

    /// <summary>
    /// Handles the attack direction based on input from the right and left sticks.
    /// </summary>
    private void OnAttackDirection()
    {
        rightPointLeft = rightAttackDir.x <= 0;
        if (!controller.isRunningToTarget) leftPointLeft = leftAttackDir.x <= 0;

        // Helper for auto-aiming
        float GetAutoAimAngle(Vector3 pointerPos, Vector3 pointerRight, float pointerLen)
        {
            float tempMinimumAngle = lockOnTargetAngle;
            float closestTargetZ = float.PositiveInfinity;
            Collider2D[] colliders = Physics2D.OverlapCircleAll(pointerPos, pointerLen, rightPointerLayerMask);
            foreach (Collider2D col in colliders)
            {
                if (!col.TryGetComponent<IDamagable>(out var damagable)) continue;
                Vector3 targetPos = damagable.GetHitPos();
                Vector2 toTarget = (targetPos - pointerPos);
                float distance = toTarget.magnitude;
                Vector2 direction = toTarget / distance;
                float _tempAngle = Vector3.Angle(pointerRight, direction);
                if (_tempAngle > tempMinimumAngle) continue;
                RaycastHit2D hit = Physics2D.Raycast(pointerPos, direction, distance, rightPointerLayerMask);
                if (hit.collider != null && hit.collider.gameObject != col.gameObject) continue;
                float targetZ = GetRotZFromDirection(toTarget);
                if (_tempAngle < tempMinimumAngle)
                {
                    closestTargetZ = targetZ;
                    tempMinimumAngle = _tempAngle;
                }
            }
            return float.IsPositiveInfinity(closestTargetZ) ? float.NaN : closestTargetZ;
        }

        // Helper for pointer length
        float GetPointerLength(Vector3 origin, Vector3 direction, float maxLength)
        {
            RaycastHit2D hit = Physics2D.Raycast(origin, direction, maxLength, rightPointerLayerMask);
            if (hit.collider != null && hit.collider.gameObject != this.gameObject)
                return hit.distance;
            return maxLength;
        }

        if (rightAttackDir != Vector2.zero)
        {
            if (!playerAttack.isAimingRightStick) QuestManager.OnAction(ObjectiveType.PlayerInput, PlayerInputObjectiveIDs.aim);
            pointerSpriteRenderer.sprite = rightPointer;
            playerAttack.isAimingRightStick = true;
            float angle = Vector2.SignedAngle(transform.up, rightAttackDir) + 90;
            pointer.rotation = Quaternion.Euler(0, 0, angle);

            // Auto-aim for right stick
            float autoAimAngle = GetAutoAimAngle(pointer.position, pointer.right, pointerLength);
            if (!float.IsNaN(autoAimAngle))
                pointer.eulerAngles = new Vector3(0, 0, autoAimAngle);

            float visualLength = GetPointerLength(pointer.position, pointer.right, pointerLength);
            pointerSpriteRenderer.size = new Vector2(visualLength / 2, 0.155f);
            playerAttack.pointerDirection = new Vector3(0, 0, pointer.rotation.eulerAngles.z);
        }
        else if (leftAttackDir != Vector2.zero)
        {
            playerAttack.isAimingRightStick = false;
            pointerSpriteRenderer.sprite = leftPointer;
            float angle = Vector2.SignedAngle(transform.up, leftAttackDir) + 90;
            pointer.rotation = Quaternion.Euler(0, 0, angle);

            float visualLength = 1.55f;
            if (leftPointerAutoAiming)
            {
                float autoAimAngle = GetAutoAimAngle(pointer.position, pointer.right, pointerLength);
                if (!float.IsNaN(autoAimAngle))
                    pointer.eulerAngles = new Vector3(0, 0, autoAimAngle);
                playerAttack.pointerDirection = new Vector3(0, 0, pointer.rotation.eulerAngles.z);
            }
            else
            {
                visualLength = GetPointerLength(pointer.position, pointer.right, visualLength);
                playerAttack.pointerDirection = new Vector3(0, 0, pointer.rotation.eulerAngles.z);
            }
            pointerSpriteRenderer.size = new Vector2(visualLength, 0.155f);
        }
        else
        {
            if (!controller.isRunningToTarget) leftPointLeft = !controller.FacingRight;
            pointerSpriteRenderer.sprite = leftPointer;
            pointer.rotation = transform.rotation;
            pointerSpriteRenderer.size = new Vector2(1.55f, 0.15f);
            playerAttack.pointerDirection = controller.FacingRight ? Vector3.zero : new Vector3(0, 0, 180);
            playerAttack.isAimingRightStick = false;
        }
    }

    public Quaternion CalculateWantedRotation(Vector3 _targetPos, Vector3 fromPos)
    {
        float angle = Mathf.Atan2(_targetPos.y - transform.position.y, _targetPos.x - transform.position.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(new Vector3(0, 0, angle));
        return targetRotation;
    }

    #endregion Pointer & Attack Direction Logic

    #region Trigger

    private void OnTriggerEnter2D(Collider2D collision)
    {
        CollisionWithEventObject(collision);
    }

    public void CollisionWithEventObject(Collider2D collision)
    {
        if (collision.gameObject.layer == 15)//event objects
        {
            if (collision.TryGetComponent<EventObject>(out EventObject obj))
            {
                currentEventObject = obj;
                obj.ShowInteractSign(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        ExitWithEventObject(collision);
    }

    private void ExitWithEventObject(Collider2D collision)
    {
        if (collision.gameObject.layer == 15)//event objects
        {
            if (currentEventObject != null)
            {
                currentEventObject.ShowInteractSign(false);
                currentEventObject.EndInteraction();
                currentEventObject = null;
            }
        }
    }

    #endregion Trigger

    #region Utility

    public void DisableAllActions()
    {
        moveDir = Vector2.zero;
        leftAttackDir = Vector2.zero;
        rightAttackDir = Vector2.zero;
        DisableFloat();
        anim.SetBool("storm", false);
        playerAttack.isPreparingStorm = false;
        ActionLock.Remove("StormPreparing");
    }

    #endregion Utility
}