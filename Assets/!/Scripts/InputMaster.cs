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

    [Header("Input")] public PlayerInput _playerInput;
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

    [HideInInspector] public InputAction inputActionKey;
    [HideInInspector] public bool inputWasEnabled = false;
    [HideInInspector] public Coroutine co_QTE;
    [HideInInspector] public bool isQTE = false;
    public Image qteKey_image;
    public Image qteInteractedKey_image;
    [HideInInspector] public System.Action callBack_success;
    [HideInInspector] public System.Action callBack_fail;

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

        qteInteractedKey_image.fillAmount = 0;
        qteKey.SetActive(false);
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
    }

    /// <summary>
    /// Starts a QTE (Quick Time Event) with a specified key and duration.
    /// </summary>
    /// <param name="key">The input key type the player must press to succeed.</param>
    /// <param name="pos">The screen position to display the QTE prompt.</param>
    /// <param name="duration">The duration over which time will slow down (affects timeScale interpolation).</param>
    /// <param name="_callBack_success">Callback to invoke when the player successfully completes the QTE.</param>
    /// <param name="_callback_fail">Unused in this method; included for interface consistency.</param>
    /// <returns>IEnumerator for coroutine execution.</returns>
    public IEnumerator QTE(InputKeyType key, Vector3 pos, float duration, System.Action _callBack_success, System.Action _callback_fail)
    {
        string deviceLayoutName;
        string controlPath;
        string InputPath;
        callBack_success = _callBack_success;
        callBack_fail = _callback_fail;
        isQTE = true;
        qteInteractedKey_image.fillAmount = 0;
        Time.timeScale = 0.1f;

        #region input key determine

        switch (key)
        {
            case InputKeyType.right_attack_key:
                InputPath = _attackRightAction.bindings[0].effectivePath;
                inputActionKey = _attackRightAction;
                break;

            case InputKeyType.left_attack_key:
                InputPath = _attackLeftAction.bindings[0].effectivePath;
                inputActionKey = _attackLeftAction;
                break;

            case InputKeyType.defend_key:
                InputPath = _defendAction.bindings[0].effectivePath;
                inputActionKey = _defendAction;
                break;

            case InputKeyType.swordTeleport_key:
                InputPath = _teleportAction.bindings[0].effectivePath;
                inputActionKey = _teleportAction;
                break;

            case InputKeyType.jump_key:
                InputPath = _jumpAction.bindings[0].effectivePath;
                inputActionKey = _jumpAction;
                break;

            case InputKeyType.move_key:
                InputPath = _moveAction.bindings[0].effectivePath;
                inputActionKey = _moveAction;
                break;

            case InputKeyType.aim_key:
                InputPath = _attackDirectionAction.bindings[0].effectivePath;
                inputActionKey = _attackDirectionAction;
                break;

            default: InputPath = _EventKeyAction.bindings[0].effectivePath; break;
        }
        inputWasEnabled = inputActionKey.enabled;
        inputActionKey.Enable();
        InputControlPath.ToHumanReadableString(InputPath, out deviceLayoutName, out controlPath);
        qteKey_image.sprite = gamePadicons.GetSprite(controlPath);
        qteKey_image.SetNativeSize();
        qteKey.transform.position = pos;
        qteKey.SetActive(true);

        #endregion input key determine

        float elapsedTime = 0f;

        while (elapsedTime <= duration)
        {
            elapsedTime += Time.deltaTime;
            qteInteractedKey_image.fillAmount = 1 - (elapsedTime / duration);
            yield return null;
        }

        EndQTE(false);
        yield return false;
    }

    /// <summary>
    /// Starts a QTE (Quick Time Event) with no time limit.
    /// </summary>
    /// <param name="key">The input key type the player must press to succeed.</param>
    /// <param name="pos">The screen position to display the QTE prompt.</param>
    /// <param name="duration">The duration over which time will slow down (affects timeScale interpolation).</param>
    /// <param name="_callBack_success">Callback to invoke when the player successfully completes the QTE.</param>
    /// <param name="_callback_fail">Unused in this method; included for interface consistency.</param>
    /// <returns>IEnumerator for coroutine execution.</returns>
    public IEnumerator EndLessQTE(InputKeyType key, Vector3 pos, float duration, System.Action _callBack_success, System.Action _callback_fail)
    {
        string deviceLayoutName;
        string controlPath;
        string InputPath;
        callBack_success = _callBack_success;
        callBack_fail = _callback_fail;
        isQTE = true;
        qteInteractedKey_image.fillAmount = 0;
        Time.timeScale = 0.1f;

        #region input key determine

        switch (key)
        {
            case InputKeyType.right_attack_key:
                InputPath = _attackRightAction.bindings[0].effectivePath;
                inputActionKey = _attackRightAction;
                break;

            case InputKeyType.left_attack_key:
                InputPath = _attackLeftAction.bindings[0].effectivePath;
                inputActionKey = _attackLeftAction;
                break;

            case InputKeyType.defend_key:
                InputPath = _defendAction.bindings[0].effectivePath;
                inputActionKey = _defendAction;
                break;

            case InputKeyType.swordTeleport_key:
                InputPath = _teleportAction.bindings[0].effectivePath;
                inputActionKey = _teleportAction;
                break;

            case InputKeyType.jump_key:
                InputPath = _jumpAction.bindings[0].effectivePath;
                inputActionKey = _jumpAction;
                break;

            case InputKeyType.move_key:
                InputPath = _moveAction.bindings[0].effectivePath;
                inputActionKey = _moveAction;
                break;

            case InputKeyType.aim_key:
                InputPath = _attackDirectionAction.bindings[0].effectivePath;
                inputActionKey = _attackDirectionAction;
                break;

            default: InputPath = _EventKeyAction.bindings[0].effectivePath; break;
        }
        inputWasEnabled = inputActionKey.enabled;
        inputActionKey.Enable();
        InputControlPath.ToHumanReadableString(InputPath, out deviceLayoutName, out controlPath);
        qteKey_image.sprite = gamePadicons.GetSprite(controlPath);
        qteKey_image.SetNativeSize();
        qteKey.transform.position = pos;
        qteKey.SetActive(true);

        #endregion input key determine

        float elapsedTime = 0f;
        float startTimeScale = 0.1f;

        while (true)
        {
            elapsedTime += Time.deltaTime;
            // Decrease timeScale linearly based on elapsedTime and duration
            float t = Mathf.Clamp01(elapsedTime / duration);
            Time.timeScale = Mathf.Lerp(startTimeScale, 0f, t);

            qteInteractedKey_image.fillAmount = 1 - t;

            if (inputActionKey.triggered)
            {
                qteInteractedKey_image.fillAmount = 1;
                qteKey.SetActive(false);
                Time.timeScale = 1f;
                inputActionKey.Disable();
                isQTE = false;
                callBack_success?.Invoke();
                yield break;
            }
            yield return null;
        }
    }

    /// <summary>
    /// Ends the QTE (Quick Time Event) and invokes the appropriate callback based on success or failure.
    /// </summary>
    /// <param name="successful"></param>
    public void EndQTE(bool successful)
    {
        if (isQTE)
        {
            Time.timeScale = 1f;
            StopCoroutine(co_QTE);
            isQTE = false;
            qteKey.SetActive(false);
            if (successful) { callBack_success?.Invoke(); }
            else { callBack_fail.Invoke(); }
            if (!inputWasEnabled) { inputActionKey.Disable(); }
            else { inputActionKey.Enable(); }
            inputActionKey = null;
        }
        else
        {
            Debug.Log("unsuceessful, not in QTE");
        }
    }

    /// <summary>
    /// Starts a QTE (Quick Time Event) with a specified key and duration.
    /// </summary>
    /// <param name="key">The input key type the player must press to succeed.</param>
    /// <param name="pos">The screen position to display the QTE prompt.</param>
    /// <param name="duration">The duration over which time will slow down (affects timeScale interpolation).</param>
    /// <param name="_callBack_success">Callback to invoke when the player successfully completes the QTE.</param>
    /// <param name="_callback_fail">Unused in this method; included for interface consistency.</param>
    /// <returns>IEnumerator for coroutine execution.</returns>
    public void StartQTE(InputKeyType key, Vector3 pos, float duration, System.Action callBack_success, System.Action callback_fail)
    {
        co_QTE = StartCoroutine(QTE(key, pos, duration, callBack_success, callback_fail));
    }

    /// <summary>
    /// Starts a QTE (Quick Time Event) with no time limit.
    /// </summary>
    /// <param name="key">The input key type the player must press to succeed.</param>
    /// <param name="pos">The screen position to display the QTE prompt.</param>
    /// <param name="duration">The duration over which time will slow down (affects timeScale interpolation).</param>
    /// <param name="_callBack_success">Callback to invoke when the player successfully completes the QTE.</param>
    /// <param name="_callback_fail">Unused in this method; included for interface consistency.</param>
    /// <returns>IEnumerator for coroutine execution.</returns>
    public void StartMustSuccessQTE(InputKeyType key, Vector3 pos, float duration, System.Action callBack_success)
    {
        co_QTE = StartCoroutine(EndLessQTE(key, pos, duration, callBack_success, null));
    }

    public void DisableAllActions()
    {
        _moveAction.Disable();
        _attackDirectionAction.Disable();
        _jumpAction.Disable();
        _teleportAction.Disable();
        _attackLeftAction.Disable();
        _attackRightAction.Disable();
        _defendAction.Disable();
        InputPlayer.instance.DisableAllActions();
    }

    public void EnableAllActions()
    {
        _moveAction.Enable();
        _attackDirectionAction.Enable();
        _jumpAction.Enable();
        _teleportAction.Enable();
        _attackLeftAction.Enable();
        _attackRightAction.Enable();
        _defendAction.Enable();
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

public enum EmotionType
{
    None = 0,

    Sigh = 1,

    Question = 2,

    Sweat = 3,

    Idea = 4,

    Whisper = 5,

    Happy = 6,

    Anger = 7,

    Sad = 8,

    Laugh = 9,

    Shock = 10,

    Excited = 11,

    Finger = 12,

    Nervous = 13,

    Greedy = 14,

    Proud = 15,

    Heart = 16,

    Dispirit = 17,

    Shy = 18
}