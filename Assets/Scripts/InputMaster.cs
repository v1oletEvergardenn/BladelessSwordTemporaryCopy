using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;
using UnityEngine.InputSystem.Samples.RebindUI;
using static ControllerInput;
using System;
using UnityEngine.UI;

public class InputMaster : MonoBehaviour
{
    public static InputMaster instance;
    private GameManager gameManager;
    public ControllerInput input;
    public ControllerInput.GameplayActions gameplayActions;
    public ControllerInput.UIActions uiActions;

    public GamepadIcons gamePadicons;

    [Header("Input")] private PlayerInput _playerInput;
    [HideInInspector] public InputAction _moveAction;
    [HideInInspector] public InputAction _attackDirectionAction;
    [HideInInspector] public InputAction _jumpAction;
    [HideInInspector] public InputAction _teleportAction;
    [HideInInspector] public InputAction _attackLeftAction;
    [HideInInspector] public InputAction _attackRightAction;
    [HideInInspector] public InputAction _defendAction;
    [HideInInspector] public InputAction _EventKeyAction;
    [HideInInspector] public InputAction _EventFlipPageAction;
    [HideInInspector] public InputAction _MenuOpenAction;

    [Header("QTE")]
    public GameObject qteKey;

    public Image qteKey_image;

    private void Awake()
    {
        if (instance == null) { instance = this; }
        else { Destroy(this.gameObject); }
        NewInput();

        _playerInput = GetComponent<PlayerInput>();

        _moveAction = _playerInput.actions["Move"];
        _attackDirectionAction = _playerInput.actions["AttackDirection"];
        _jumpAction = _playerInput.actions["Jump"];
        _teleportAction = _playerInput.actions["Teleport"];
        _attackLeftAction = _playerInput.actions["AttackLeft"];
        _attackRightAction = _playerInput.actions["AttackRight"];
        _defendAction = _playerInput.actions["Defend"];
        _EventKeyAction = _playerInput.actions["EventKey"];
        _EventFlipPageAction = _playerInput.actions["Event_flip_page"];
        _MenuOpenAction = _playerInput.actions["MenuOpen"];
    }

    public void NewInput()
    {
        if (input != null)
        {
            input.Disable();
        }
        input = new ControllerInput();
        input.Enable();
        gameplayActions = input.Gameplay;
        uiActions = input.UI;
        gameplayActions.Enable();
        uiActions.Disable();
    }

    private void Start()
    {
        gameManager = GameManager.instance;

        StartCoroutine(QTE(InputKeyType.left_attack_key, PlayerAttack.instance.transform.position + new Vector3(0, 3, 0), 2f));
    }

    public IEnumerator QTE(InputKeyType key, Vector3 pos, float duration)
    {
        string deviceLayoutName;
        string controlPath;
        string InputPath;
        switch (key)
        {
            case InputKeyType.right_attack_key:
                InputPath = _attackRightAction.bindings[0].effectivePath;
                break;

            case InputKeyType.left_attack_key:
                InputPath = _attackLeftAction.bindings[0].effectivePath;
                break;

            case InputKeyType.defend_key:
                InputPath = _defendAction.bindings[0].effectivePath;
                break;

            case InputKeyType.swordTeleport_key:
                InputPath = _teleportAction.bindings[0].effectivePath;
                break;

            case InputKeyType.jump_key:
                InputPath = _jumpAction.bindings[0].effectivePath;
                break;

            case InputKeyType.move_key:
                InputPath = _moveAction.bindings[0].effectivePath;
                break;

            case InputKeyType.aim_key:
                InputPath = _attackDirectionAction.bindings[0].effectivePath;
                break;

            default: InputPath = _EventKeyAction.bindings[0].effectivePath; break;
        }
        InputControlPath.ToHumanReadableString(InputPath, out deviceLayoutName, out controlPath);
        qteKey_image.sprite = gamePadicons.GetSprite(controlPath);
        qteKey_image.SetNativeSize();
        qteKey.transform.position = pos;
        qteKey.SetActive(true);
        yield return new WaitForSecondsRealtime(duration);
        qteKey.SetActive(false);

        yield return null;
    }
}

public enum InputKeyType
{
    right_attack_key,
    left_attack_key,
    defend_key,
    swordTeleport_key,
    jump_key,
    move_key,
    aim_key
}

[Serializable]
public struct GamepadIcons
{
    public Sprite buttonSouth;
    public Sprite buttonNorth;
    public Sprite buttonEast;
    public Sprite buttonWest;
    public Sprite startButton;
    public Sprite selectButton;
    public Sprite leftTrigger;
    public Sprite rightTrigger;
    public Sprite leftShoulder;
    public Sprite rightShoulder;
    public Sprite dpad;
    public Sprite dpadUp;
    public Sprite dpadDown;
    public Sprite dpadLeft;
    public Sprite dpadRight;
    public Sprite leftStick;
    public Sprite rightStick;
    public Sprite leftStickPress;
    public Sprite rightStickPress;

    public Sprite GetSprite(string controlPath)
    {
        // From the input system, we get the path of the control on device. So we can just
        // map from that to the sprites we have for gamepads.
        switch (controlPath)
        {
            case "buttonSouth": return buttonSouth;
            case "buttonNorth": return buttonNorth;
            case "buttonEast": return buttonEast;
            case "buttonWest": return buttonWest;
            case "start": return startButton;
            case "select": return selectButton;
            case "leftTrigger": return leftTrigger;
            case "rightTrigger": return rightTrigger;
            case "leftShoulder": return leftShoulder;
            case "rightShoulder": return rightShoulder;
            case "dpad": return dpad;
            case "dpad/up": return dpadUp;
            case "dpad/down": return dpadDown;
            case "dpad/left": return dpadLeft;
            case "dpad/right": return dpadRight;
            case "leftStick": return leftStick;
            case "rightStick": return rightStick;
            case "leftStickPress": return leftStickPress;
            case "rightStickPress": return rightStickPress;
        }
        return null;
    }
}