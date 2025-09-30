using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using static ControllerInput;
using UnityEngine.InputSystem;

public enum TutType
{
    basicMovement,
    dash,
    status,
    qi,
    hs_bar,
    counterAttack,
    defend,
    heartSwordAttack,
    swordJump,
    swordTeleport,
    swordStorm,
    bossBattle,
    levelMechanism,
    guard
}

public class MenuManager : MonoBehaviour
{
    public static MenuManager instance;
    [Header("Pause InGame Canvas")] public GameObject PauseGameCanvas;
    public List<GameObject> Tabs;
    private int currentIndexTab = 0;
    public bool canChangeTab = true;
    public bool canCloseMenu = true;

    [Header("Save Point Canvas")] public GameObject SavePointCanvas;
    public GameObject SavePointMenu;
    public GameObject HSAbilitySwapMenu;
    [Header("End Canvas")] public GameObject EndGameCanvas;

    private void Awake()
    {
        if (instance == null) { instance = this; }
    }

    private void Start()
    {
        PauseGameCanvas.SetActive(false);
        SavePointCanvas.SetActive(false);
        InputMaster.instance.uiActions.FlipPage_LB.performed += ctx => PreviousTab();
        InputMaster.instance.uiActions.FlipPage_RB.performed += ctx => NextTab();

        InputMaster.instance._MenuOpenAction.performed += ctx => OpenPauseGameCanvas();
        InputMaster.instance.uiActions.MenuClose.performed += ctx => CloseMenu();
    }

    public void CloseMenu()
    {
        print(1);
        if (PauseGameCanvas.activeInHierarchy)
        {
            ClosePauseGameCanvas();
        }
        else if (SavePointCanvas.activeInHierarchy)
        {
            CloseSavePointCanvas();
        }
    }

    public void OpenPauseGameCanvas()
    {
        GameManager.instance.PauseGame();
        currentIndexTab = -1;
        NextTab();
        PauseGameCanvas.SetActive(true);
        canChangeTab = true;
        InputMaster.instance.SwitchToUIAction();
    }

    public void ClosePauseGameCanvas()
    {
        if (!canCloseMenu) return;
        GameManager.instance.UnpauseGame();
        PauseGameCanvas.SetActive(false);
        InputMaster.instance.SwitchToGameplayAction();
    }

    public void OpenSavePointCanvas()
    {
        SavePointCanvas.SetActive(true);
        SavePointMenu.SetActive(true);
        HSAbilitySwapMenu.SetActive(false);
        EventSystem.current.SetSelectedGameObject(SavePointMenu.GetComponent<FirstSelectObjectSerializer>().Selected());
        InputMaster.instance.SwitchToUIAction();
    }

    public void CloseSavePointCanvas()
    {
        SavePointCanvas.SetActive(false);
        InputMaster.instance.SwitchToGameplayAction();
    }

    public void NextTab()
    {
        if (!canChangeTab) return;
        currentIndexTab++;
        ShowTab();
    }

    public void PreviousTab()
    {
        if (!canChangeTab) return;
        currentIndexTab--;
        ShowTab();
    }

    public void ShowTab()
    {
        currentIndexTab = currentIndexTab % Tabs.Count;
        if (currentIndexTab < 0) { currentIndexTab = Tabs.Count - 1; }
        foreach (GameObject obj in Tabs)
        {
            if (obj != null)
            {
                obj.SetActive(false);
                obj.GetComponent<FirstSelectObjectSerializer>().DisSelected();
            }
        }
        if (Tabs[currentIndexTab] != null)
        {
            Tabs[currentIndexTab].SetActive(true);
            EventSystem.current.SetSelectedGameObject(Tabs[currentIndexTab].GetComponent<FirstSelectObjectSerializer>().Selected());
        }
    }

    public void EndCanvas()
    {
        GameManager.instance.PauseGame();
        EndGameCanvas.SetActive(true);
        EventSystem.current.SetSelectedGameObject(EndGameCanvas.GetComponent<FirstSelectObjectSerializer>().Selected());
    }

    public void CanSwitchTab(bool b)
    {
        canChangeTab = b;
    }

    public void CanCloseMenu(bool b)
    {
        canCloseMenu = b;
    }
}