using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems; // Remove if not using TextMeshPro

public class WarningSystem : MonoBehaviour
{
    public static WarningSystem instance;
    private static bool _isWarningActive = false;
    public static bool IsWarningActive => _isWarningActive;

    [Header("UI References")]
    public GameObject warningPanel;

    public TextMeshProUGUI messageText; // Use TMP_Text if using TextMeshPro
    public Button confirmButton;
    public Button cancelButton;

    private Action onConfirm;
    private Action onCancel;

    private GameObject lastSelectedObj;

    private void Awake()
    {
        if (instance == null) { instance = this; }
        else { Destroy(gameObject); }
        HideWarning();
    }

    private void Start()
    {
        //InputMaster.instance.warningWindowActions.Cancel.performed += ctx => Cancel();
        //InputMaster.instance.warningWindowActions.Confirm.performed += ctx => Confirm();
    }

    /// <summary>
    /// Show a warning popup with message and actions.
    /// </summary>
    /// <param name="message">Message to display</param>
    /// <param name="onConfirm">Action to run on confirm</param>
    /// <param name="onCancel">Optional action to run on cancel</param>
    public static void ShowWarning(string message, Action onConfirm, Action onCancel = null)
    {
        _isWarningActive = true;
        InputMaster.instance.SwitchToWarningAction();

        instance.lastSelectedObj = EventSystem.current.currentSelectedGameObject;
        EventSystem.current.SetSelectedGameObject(instance.confirmButton.gameObject);
        instance.warningPanel.SetActive(true);
        instance.messageText.text = message;
        instance.onConfirm = onConfirm;
        instance.onCancel = onCancel;

        instance.confirmButton.onClick.RemoveAllListeners();
        instance.cancelButton.onClick.RemoveAllListeners();

        instance.confirmButton.onClick.AddListener(instance.Confirm);
        instance.cancelButton.onClick.AddListener(instance.Cancel);
    }

    private void Confirm()
    {
        var action = onConfirm;
        InputMaster.instance.SwitchToUIAction();
        HideWarning();
        action?.Invoke();
    }

    private void Cancel()
    {
        var action = onCancel;
        InputMaster.instance.SwitchToUIAction();
        HideWarning();
        action?.Invoke();
    }

    private void HideWarning()
    {
        _isWarningActive = false;
        if (lastSelectedObj != null) EventSystem.current.SetSelectedGameObject(lastSelectedObj);
        warningPanel.SetActive(false);
        messageText.text = "";
        onConfirm = null;
        onCancel = null;
    }
}