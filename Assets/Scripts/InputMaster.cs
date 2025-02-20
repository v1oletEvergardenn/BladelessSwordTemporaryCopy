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
}