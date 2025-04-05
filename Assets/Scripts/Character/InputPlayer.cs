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

public enum PlayerState
{
    idling,
    running,
    jumping,
    idle_attacking,
    run_attacking
}

public class InputPlayer : MonoBehaviour
{
    [HideInInspector] public ControllerInput INPUTcontrol;
    [HideInInspector] public ControllerInput.GameplayActions control;
    [HideInInspector] public CharacterController2D controller;
    private PlayerAttack playerAttack;
    [HideInInspector] public Animator anim;
    private GameManager gameManager;
    private InputMaster inputManager;
    private Health health;
    public static InputPlayer instance;

    private float horizontalMove = 0f;
    [HideInInspector] public bool input_floating = false;
    [HideInInspector] public float input_floating_timer = 0f;
    public float HS_cancel_time = 1.2f;
    [HideInInspector] public float HS_cancel_timer = 0f;
    [HideInInspector] public float HS_hold_timer = 0f;
    private Vector2 moveDir;

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

    [Header("EventTrigger")] private EventObject currentEventObject;

    [Header("MapCamera")] public Transform MapCamera;

    [Header("LearnSkills")] public bool learnedMovement = true;
    public bool learnedJump = true;
    public bool learnedTeleport = true;
    public bool learnedDoubleJump = false;
    public bool learnedAttack = false;
    public bool learnedBoomerang = false;
    public bool learnedBarrier = false;
    public bool learnedDefend = false;
    public bool learnedHeartSword = false;
    // Start is called before the first frame update

    private void Awake()
    {
        if (instance == null) { instance = this; }
    }

    private void Start()
    {
        pointerSpriteRenderer = pointer.GetComponent<SpriteRenderer>();
        controller = GetComponent<CharacterController2D>();
        playerAttack = GetComponent<PlayerAttack>();
        inputManager = InputMaster.instance;
        health = GetComponent<Health>();
        gameManager = GameManager.instance;
        gameManager.playerInput = this;
        gameManager.player_Idamagable = Health.instance;
        gameManager.playerAttack = PlayerAttack.instance;
        gameManager.player_controller = CharacterController2D.instance;
        gameManager.Player = gameManager.player_Idamagable.gameObject;

        anim = controller.anim;

        INPUTcontrol = inputManager.input;
        control = inputManager.gameplayActions;
        control.Jump.canceled += ctx => OnEndJump();
        control.Move.performed += ctx => moveDir = ctx.ReadValue<Vector2>();
        control.Move.performed += ctx => leftAttackDir = ctx.ReadValue<Vector2>();
        control.Move.canceled += ctx => leftAttackDir = Vector2.zero;
        control.AttackDirection.performed += ctx => rightAttackDir = ctx.ReadValue<Vector2>();
        control.AttackDirection.canceled += ctx => rightAttackDir = Vector2.zero;
        control.Defend.canceled += ctx => playerAttack.EndDefend();
    }

    // Update is called once per frame
    private void Update()
    {
        pointer.transform.position = transform.position + new Vector3(0, pointerOffset, 0);
        if (control.EventKey.WasPressedThisFrame())
        {
            if (DialogManager.instance != null && DialogManager.instance.Printer.activeInHierarchy)
            {
                DialogManager.instance.Click_Window();
                return;
            }//dialog
            if (currentEventObject != null)
            {
                currentEventObject.Interact(true);
                return;
            }//eventObject
        }

        if (control.EventKey.WasReleasedThisFrame())
        {
            if (currentEventObject != null)
            {
                currentEventObject.Interact(false);
                return;
            }//eventObject
        }

        if (health.isDead) { return; }

        if (gameManager.isInInformationEvent | gameManager.isInDialog) { anim.SetBool("isRunning", false); GetComponent<Rigidbody2D>().velocity = new Vector2(0, GetComponent<Rigidbody2D>().velocity.y); return; }

        if (learnedDoubleJump && input_floating) { input_floating_timer += Time.deltaTime; controller.floatingTime = input_floating_timer; }
        if (learnedDoubleJump && input_floating_timer > 0f) { controller.input_floating = true; } else { controller.input_floating = false; }

        float x = moveDir.x;
        if (x < 0 && x > -0.4) { x = -0.4f; }
        if (x > 0 && x < 0.4) { x = 0.4f; }
        if (Mathf.Abs(moveDir.magnitude) < 0.7f) { x = 0f; }
        horizontalMove = x;
        if (moveDir.x == 0) { horizontalMove = 0; }
        if (learnedMovement) { controller.Move(horizontalMove * Time.fixedDeltaTime); }// horizontal movement
        if (learnedJump && control.Jump.WasPressedThisFrame()) { OnJump(); }//jump
        if (learnedAttack)
        {
            if (!control.AttackLeft.IsPressed() && control.AttackRight.WasPressedThisFrame())
            {
                playerAttack.Attack(false);
                if (learnedBarrier)
                { anim.SetBool("storm", true); playerAttack.isPreparingStorm = true; playerAttack.prepareStormTimer = 0f; }
            }
            else if (!control.AttackRight.IsPressed() && control.AttackLeft.WasPressedThisFrame())
            {
                playerAttack.Attack(true);
                if (learnedBarrier)
                { anim.SetBool("storm", true); playerAttack.isPreparingStorm = true; playerAttack.prepareStormTimer = 0f; }
            }
        }
        if (learnedBarrier)
        {
            if ((!control.AttackLeft.IsPressed() && control.AttackRight.WasReleasedThisFrame()) || (!control.AttackRight.IsPressed() && control.AttackLeft.WasReleasedThisFrame()))
            {
                anim.SetBool("storm", false); playerAttack.OnStorm(); playerAttack.isPreparingStorm = false;
            }
        }
        if (learnedDefend && control.Defend.WasPressedThisFrame() &&
            !playerAttack.isPreparingStorm) { playerAttack.OnDefend(); }//defend
        if (learnedTeleport && control.Teleport.WasPressedThisFrame()) { controller.SwordTeleport(); }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            WaveController wave = ObjectPooler.instance.SpawnFromPool("wave", transform.position).GetComponent<WaveController>();
            print(wave);
            wave.CallWave();
        }
    }

    private void FixedUpdate()
    {
        if (gameManager.isInInformationEvent | gameManager.isInDialog | health.isDead) { pointerSpriteRenderer.sprite = null; return; }
        OnAttackDirection();// set counter attack direction
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

    public float GetRotZFromDirection(Vector3 dir)
    {
        float rotZ = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        return rotZ;
    }

    private void OnAttackDirection()
    {
        if (rightAttackDir.x > 0) { rightPointLeft = false; } else { rightPointLeft = true; }
        if (leftAttackDir.x > 0) { leftPointLeft = false; } else { leftPointLeft = true; }

        if (rightAttackDir != Vector2.zero)
        {
            pointerSpriteRenderer.sprite = rightPointer;
            playerAttack.isAimingRightStick = true;
            float angle = Vector2.SignedAngle(transform.up, rightAttackDir) + 90;
            pointer.rotation = Quaternion.Euler(0, 0, angle);

            //float z = GetRotZFromDirection(rightAttackDir);//get z of rotation
            //if (!controller.m_FacingRight) { z = GetRotZFromDirection(-rightAttackDir); }//reverse when face left
            //z = Mathf.Clamp(z, -90, 90);
            //if (!controller.m_FacingRight) { z += 180; }
            //pointer.eulerAngles = new Vector3(0, 0, z);//the target rotation

            float tempMinimumAngle = lockOnTargetAngle;
            float closestTargetZ = Mathf.Infinity;
            Collider2D[] colliders = Physics2D.OverlapCircleAll(pointer.position, pointerLength, rightPointerLayerMask);
            foreach (Collider2D col in colliders)
            {
                IDamagable damagable;
                if (col.TryGetComponent<IDamagable>(out damagable))
                {
                    float _tempAngle = Vector3.Angle(pointer.right, (damagable.GetHitPos() - pointer.position).normalized);
                    float TargetZ = GetRotZFromDirection(damagable.GetHitPos() - pointer.position);
                    if (_tempAngle <= tempMinimumAngle) { closestTargetZ = TargetZ; tempMinimumAngle = _tempAngle; }
                }
            }//check angle, if small enough, snap on it.
            if (closestTargetZ != Mathf.Infinity) { pointer.eulerAngles = new Vector3(0, 0, closestTargetZ); }

            float dist = pointerLength;
            RaycastHit2D hit = Physics2D.Raycast(pointer.position, pointer.right, 100f, rightPointerLayerMask);
            if (hit.collider != null && hit.collider.gameObject != this.gameObject) { dist = hit.distance; }// cut the pointer length based on collision
            pointer.GetComponent<SpriteRenderer>().size = new Vector2(dist / 2, 0.155f);

            playerAttack.direction = new Vector3(0, 0, pointer.rotation.eulerAngles.z);
        }// right stick
        else if (leftAttackDir != Vector2.zero)
        {
            playerAttack.isAimingRightStick = false;
            pointerSpriteRenderer.sprite = leftPointer;
            float angle = Vector2.SignedAngle(transform.up, leftAttackDir) + 90;
            pointer.rotation = Quaternion.Euler(0, 0, angle);
            pointer.GetComponent<SpriteRenderer>().size = new Vector2(1.55f, 0.155f);
            playerAttack.direction = new Vector3(0, 0, pointer.rotation.eulerAngles.z);
        }//left stick
        else
        {
            pointerSpriteRenderer.sprite = leftPointer;
            pointer.rotation = transform.rotation;
            pointer.GetComponent<SpriteRenderer>().size = new Vector2(1.55f, 0.15f);
            if (controller.m_FacingRight)
            {
                playerAttack.direction = Vector3.zero;
            }
            else
            {
                playerAttack.direction = new Vector3(0, 0, -180);
            }
            playerAttack.isAimingRightStick = false;
        }// none stick
    }//change pointer Direction and counter attack direction.

    public Quaternion CalculateWantedRotation(Vector3 _targetPos, Vector3 fromPos)
    {
        float angle = Mathf.Atan2(_targetPos.y - transform.position.y, _targetPos.x - transform.position.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(new Vector3(0, 0, angle));
        return targetRotation;
    }

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
}