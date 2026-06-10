using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static ControllerInput;

public class InputMaster : MonoBehaviour
{
    public static InputMaster instance;
    private GameManager gameManager;
    public ControllerInput input;
    public ControllerInput.GameplayActions gameplayActions;
    public ControllerInput.UIActions uiActions;
    public ControllerInput.WarningWindowActions warningWindowActions;

    public GamePadIcons icons;

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

    [HideInInspector] public InputAction _OpenMapAction;
    [HideInInspector] public InputAction _CloseMapAction;
    [HideInInspector] public InputAction _AbilityX;
    [HideInInspector] public InputAction _AbilityY;
    [HideInInspector] public InputAction _AbilityB;

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

        input = new ControllerInput();

        gameplayActions = input.Gameplay;
        uiActions = input.UI;
        warningWindowActions = input.WarningWindow;

        _playerInput = GetComponent<PlayerInput>();

        _moveAction = gameplayActions.Move;
        _attackDirectionAction = gameplayActions.AttackDirection;
        _jumpAction = gameplayActions.Jump;
        _teleportAction = gameplayActions.Teleport;
        _attackLeftAction = gameplayActions.AttackLeft;
        _attackRightAction = gameplayActions.AttackRight;
        _defendAction = gameplayActions.Defend;
        _EventKeyAction = gameplayActions.EventKey;
        _EventFlipPageAction = gameplayActions.Event_flip_page;
        _MenuOpenAction = gameplayActions.MenuOpen;
        _OpenMapAction = gameplayActions.OpenMap;
        _CloseMapAction = uiActions.MapClose;
        _AbilityX = gameplayActions.AbilityX;
        _AbilityB = gameplayActions.AbilityB;
        _AbilityY = gameplayActions.AbilityY;

        qteInteractedKey_image.fillAmount = 0;
        qteKey.SetActive(false);
    }

    public static void SwitchToUIAction()
    {
        InputMaster.instance.uiActions.Enable();
        InputMaster.instance.gameplayActions.Disable();
        InputMaster.instance.warningWindowActions.Disable();
        if (InputPlayer.instance != null)
        {
            InputPlayer.instance.moveDir = Vector2.zero;
        }
    }

    public static void SwitchToGameplayAction()
    {
        InputMaster.instance.uiActions.Disable();
        InputMaster.instance.gameplayActions.Enable();
        InputMaster.instance.warningWindowActions.Disable();
    }

    public static void SwitchToWarningAction()
    {
        InputMaster.instance.uiActions.Disable();
        InputMaster.instance.gameplayActions.Disable();
        InputMaster.instance.warningWindowActions.Enable();
    }

    private void Start()
    {
        gameManager = GameManager.instance;

        string sceneName = SceneManager.GetActiveScene().name;
        bool isMenuScene = sceneName == "MainMenu" || sceneName == "PreLoad";

        if (isMenuScene)
        {
            SwitchToUIAction();
        }
        else
        {
            SwitchToGameplayAction();
        }
    }

    private void Update()
    {
    }

    private void LogActionCallbacks(InputAction action)
    {
        if (action == null)
        {
            Debug.Log("Action is null.");
            return;
        }

        Debug.Log($"Action '{action.name}' is enabled: {action.enabled}, phase: {action.phase}");
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
        qteKey_image.sprite = icons.gamePadicons.GetSprite(controlPath);
        qteKey_image.SetNativeSize();
        qteKey.transform.position = pos;
        qteKey.SetActive(true);

        #endregion input key determine

        float elapsedTime = 0f;

        while (elapsedTime <= duration)
        {
            elapsedTime += Time.deltaTime;
            qteInteractedKey_image.fillAmount = 1 - (elapsedTime / duration);
            if (inputActionKey.triggered)
            {
                EndQTE(true);
                yield break;
            }
            yield return null;
        }

        EndQTE(false);
        yield return null;
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
        qteKey_image.sprite = icons.gamePadicons.GetSprite(controlPath);
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
                EndQTE(true);
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
            else { callBack_fail?.Invoke(); }
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
    default_key,
    right_attack_key,
    left_attack_key,
    defend_key,
    swordTeleport_key,
    jump_key,
    move_key,
    aim_key,
    AbilityWest_key,
    AbilityNorth_key,
    AbilityEast_key
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