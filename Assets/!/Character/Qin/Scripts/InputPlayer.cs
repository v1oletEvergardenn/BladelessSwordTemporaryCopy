using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using EditorAttributes;
using Unity.VisualScripting;
using System.Data;
using Doublsb.Dialog;
using UnityEngine.SceneManagement;
using System;
using Mobsoft.PixelStyleWaterShader;
using static UnityEngine.EventSystems.EventTrigger;

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

    #region Movement & Input State

    private float horizontalMove = 0f;
    [HideInInspector] public bool input_floating = false;
    [HideInInspector] public float input_floating_timer = 0f;
    public float HS_cancel_time = 1.2f;
    [HideInInspector] public float HS_cancel_timer = 0f;
    [HideInInspector] public float HS_hold_timer = 0f;
    private Vector2 moveDir;

    #endregion Movement & Input State

    #region Pointer & Attack Direction

    [Title("pointerSetting", 15)] public Transform pointer;
    public float pointerOffset = 1f;
    private SpriteRenderer pointerSpriteRenderer;
    public float pointerLength = 15f;
    public float lockOnTargetAngle = 10f;
    [SerializeField] private Sprite leftPointer;
    [SerializeField] private Sprite rightPointer;
    [SerializeField] private LayerMask rightPointerLayerMask;
    [HideInInspector] public Vector2 rightAttackDir;
    [HideInInspector] public Vector2 leftAttackDir;
    public bool rightPointLeft = false;
    public bool leftPointLeft = false;

    #endregion Pointer & Attack Direction

    #region Event & Map Camera

    [Header("EventTrigger")] private EventObject currentEventObject;
    [Header("MapCamera")] public Transform MapCamera;

    #endregion Event & Map Camera

    #region Skills & Progression

    [Header("LearnSkills")] public bool learnedMovement = true;
    public bool learnedJump = true;
    public bool learnedTeleport = true;
    public bool learnedDoubleJump = false;
    public bool learnedAttack = false;
    public bool learnedBoomerang = false;
    public bool learnedStorm = false;
    public bool learnedDefend = false;
    public bool learnedHeartSword = false;

    #endregion Skills & Progression

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
        gameManager.player_Idamagable = Health.instance;
        gameManager.playerAttack = PlayerAttack.instance;
        gameManager.player_controller = CharacterController2D.instance;
        gameManager.Player = gameManager.player_Idamagable.gameObject;

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
    }

    // --- Helper Methods ---

    private bool CheckAndHandlePauseOrQTE()
    {
        // Space key test event
        if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            TestEvent();
        }
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
        }
    }

    private void UpdateMovementInput()
    {
        float x = moveDir.x;
        if (x < 0 && x > -0.4f) x = -0.4f;
        if (x > 0 && x < 0.4f) x = 0.4f;
        if (Mathf.Abs(moveDir.magnitude) < 0.7f) x = 0f;
        horizontalMove = x;
        if (moveDir.x == 0) horizontalMove = 0;
        if (learnedMovement && !controller.isRunningToTarget)
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

        if (controller.isFloating) return;
        //checks if any active heart sword ability is triggered by attack key
        //if does, cancel the attack input and perform the ability instead
        if (hSAbilitiesManager.currentActivatedAbility != null && hSAbilitiesManager.currentActivatedAbility.isTriggeredByAttackKey)
        {
            bool success = false;
            if (leftJustPressed) { success = hSAbilitiesManager.currentActivatedAbility.PerformAbility(true); }
            else if (rightJustPressed) { success = hSAbilitiesManager.currentActivatedAbility.PerformAbility(false); }
            if (success) return;
        }

        if (!leftPressed && rightJustPressed)
        {
            playerAttack.Attack(false);

            if (learnedStorm)
            {
                playerAttack.isPreparingStorm = true;
                playerAttack.prepareStormTimer = 0f;
            }
        }
        else if (!rightPressed && leftJustPressed)
        {
            playerAttack.Attack(true);

            if (learnedStorm)
            {
                playerAttack.isPreparingStorm = true;
                playerAttack.prepareStormTimer = 0f;
            }
        }
    }

    private void HandleAbilityInput()
    {
        if (!inputMaster._AbilityB.IsPressed() && !inputMaster._AbilityX.IsPressed() && !inputMaster._AbilityY.IsPressed())
        {
            hSAbilitiesManager.currentActivatedAbility = null;
            hSAbilitiesManager.abilityX.isActive = false;
            hSAbilitiesManager.abilityY.isActive = false;
            hSAbilitiesManager.abilityB.isActive = false;
            return;
        }

        if (inputMaster._AbilityX.WasPressedThisFrame()) { hSAbilitiesManager.ActivateAbility(hSAbilitiesManager.abilityX); return; }
        if (inputMaster._AbilityY.WasPressedThisFrame()) { hSAbilitiesManager.ActivateAbility(hSAbilitiesManager.abilityY); return; }
        if (inputMaster._AbilityB.WasPressedThisFrame()) { hSAbilitiesManager.ActivateAbility(hSAbilitiesManager.abilityB); return; }
    }

    private void HandleDefendInput()
    {
        if (learnedDefend && inputMaster._defendAction.WasPressedThisFrame() && !playerAttack.isPreparingStorm)
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
    private void TestEvent()
    {
        Time.timeScale = 0.2f;
    }

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
        // Update pointer direction flags based on input
        rightPointLeft = rightAttackDir.x <= 0;
        if (!controller.isRunningToTarget) { leftPointLeft = leftAttackDir.x <= 0; }

        if (rightAttackDir != Vector2.zero)
        {
            // Set pointer sprite and aiming state for right stick
            pointerSpriteRenderer.sprite = rightPointer;
            playerAttack.isAimingRightStick = true;

            // Calculate the initial pointer rotation based on right stick input
            float angle = Vector2.SignedAngle(transform.up, rightAttackDir) + 90;
            pointer.rotation = Quaternion.Euler(0, 0, angle);

            // Prepare for target snapping
            float tempMinimumAngle = lockOnTargetAngle;
            float closestTargetZ = float.PositiveInfinity;
            Vector3 pointerPos = pointer.position;
            Vector3 pointerRight = pointer.right;
            float pointerLen = pointerLength;

            // Find all potential targets within pointer range and layer mask
            Collider2D[] colliders = Physics2D.OverlapCircleAll(pointerPos, pointerLen, rightPointerLayerMask);
            foreach (Collider2D col in colliders)
            {
                // Only consider objects that implement IDamagable
                if (!col.TryGetComponent<IDamagable>(out var damagable))
                    continue;

                // Calculate direction and distance to the target
                Vector3 targetPos = damagable.GetHitPos();
                Vector2 toTarget = (targetPos - pointerPos);
                float distance = toTarget.magnitude;
                Vector2 direction = toTarget / distance;

                // Check if the target is within the lock-on angle
                float _tempAngle = Vector3.Angle(pointerRight, direction);
                if (_tempAngle > tempMinimumAngle)
                    continue;

                // Raycast to ensure there are no obstacles between pointer and target
                RaycastHit2D hit = Physics2D.Raycast(pointerPos, direction, distance, rightPointerLayerMask);
                if (hit.collider != null && hit.collider.gameObject != col.gameObject)
                    continue;

                // If this target is the closest within angle, remember its rotation
                float targetZ = GetRotZFromDirection(toTarget);
                if (_tempAngle < tempMinimumAngle)
                {
                    closestTargetZ = targetZ;
                    tempMinimumAngle = _tempAngle;
                }
            }

            // If a valid target was found, snap the pointer to it
            if (!float.IsPositiveInfinity(closestTargetZ))
                pointer.eulerAngles = new Vector3(0, 0, closestTargetZ);

            // --- Pointer length stops at first collider in pointer's direction ---
            float visualLength = pointerLen;
            RaycastHit2D pointerHit = Physics2D.Raycast(pointerPos, pointer.right, pointerLen, rightPointerLayerMask);
            if (pointerHit.collider != null && pointerHit.collider.gameObject != this.gameObject)
            {
                visualLength = pointerHit.distance;
            }
            pointerSpriteRenderer.size = new Vector2(visualLength / 2, 0.155f);

            // Update attack direction for use in attack logic
            playerAttack.pointerDirection = new Vector3(0, 0, pointer.rotation.eulerAngles.z);
        }
        else if (leftAttackDir != Vector2.zero)
        {
            playerAttack.isAimingRightStick = false;
            pointerSpriteRenderer.sprite = leftPointer;
            float angle = Vector2.SignedAngle(transform.up, leftAttackDir) + 90;
            pointer.rotation = Quaternion.Euler(0, 0, angle);

            // Pointer length stops at first collider in left stick direction
            float visualLength = 1.55f;
            RaycastHit2D pointerHit = Physics2D.Raycast(pointer.position, pointer.right, visualLength, rightPointerLayerMask);
            if (pointerHit.collider != null && pointerHit.collider.gameObject != this.gameObject)
            {
                visualLength = pointerHit.distance;
            }
            pointerSpriteRenderer.size = new Vector2(visualLength, 0.155f);

            playerAttack.pointerDirection = new Vector3(0, 0, pointer.rotation.eulerAngles.z);
        }
        else
        {
            if (!controller.isRunningToTarget) { leftPointLeft = !controller.FacingRight; }
            pointerSpriteRenderer.sprite = leftPointer;
            pointer.rotation = transform.rotation;
            pointerSpriteRenderer.size = new Vector2(1.55f, 0.15f);
            playerAttack.pointerDirection = controller.FacingRight ? Vector3.zero : new Vector3(0, 0, -180);
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

    #region Trigger & EventObject Handling

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 15)//event objects
        {
            currentEventObject = collision.GetComponent<EventObject>();
            currentEventObject.ShowInteractSign(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 15)//event objects
        {
            currentEventObject.ShowInteractSign(false);
            currentEventObject.EndInteraction();
            currentEventObject = null;
        }
    }

    #endregion Trigger & EventObject Handling

    #region Utility

    public void DisableAllActions()
    {
        moveDir = Vector2.zero;
        leftAttackDir = Vector2.zero;
        rightAttackDir = Vector2.zero;
        DisableFloat();
        anim.SetBool("storm", false);
        playerAttack.isPreparingStorm = false;
    }

    #endregion Utility
}