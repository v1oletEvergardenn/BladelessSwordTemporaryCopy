using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;
using static ControllerInput;

public class InputMaster : MonoBehaviour
{
    public static InputMaster instance;
    private GameManager gameManager;
    public ControllerInput input;
    public ControllerInput.GameplayActions gameplayActions;
    public ControllerInput.UIActions uiActions;

    public Sprite right_attack_key;
    public Sprite left_attack_key;
    public Sprite defend_key;
    public Sprite swordTeleport_key;
    public Sprite jump_key;
    public Sprite left_move_key;
    public Sprite right_move_key;
    public Sprite move_key;
    public Sprite left_aim_key;
    public Sprite right_aim_key;
    public Sprite aim_key;

    private void Awake()
    {
        if (instance == null) { instance = this; }
        else { Destroy(this.gameObject); }
        NewInput();
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

    public IEnumerator QTE(InputKeyType key, Vector3 pos, float duration)
    {
        switch (key)
        {
            case InputKeyType.right_attack_key:
                break;

            case InputKeyType.left_attack_key:
                break;

            case InputKeyType.defend_key:
                break;

            case InputKeyType.swordTeleport_key:
                break;

            case InputKeyType.jump_key:
                break;

            case InputKeyType.left_move_key:
                break;

            case InputKeyType.right_move_key:
                break;

            case InputKeyType.move_key:
                break;

            case InputKeyType.left_aim_key:
                break;

            case InputKeyType.right_aim_key:
                break;

            case InputKeyType.aim_key:
                break;

            default: break;
        }
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
    left_move_key,
    right_move_key,
    move_key,
    left_aim_key,
    right_aim_key,
    aim_key
}