using EditorAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UIEvents : MonoBehaviour
{
    public bool setSelectedObjectOnEnable = false;
    public GameObject selectedObjectOnEnable;
    public UnityEvent onenableEvent;

    public UnityEvent disableEvent;

    public UnityEvent backToLastPageEvent;
    public GameObject menuToGoBack;

    public void OnEnable()
    {
        onenableEvent?.Invoke();
        if (setSelectedObjectOnEnable) SetSelectObject(selectedObjectOnEnable);
        //if (menuToGoBack != null) InputMaster.instance.uiActions.Cancel.performed += BackToLastPage;
    }

    public void OnDisable()
    {
        //if (menuToGoBack != null) InputMaster.instance.uiActions.Cancel.performed -= BackToLastPage;
        disableEvent?.Invoke();
    }

    public void Update()
    {
        if (InputMaster.instance.uiActions.Cancel.WasPressedThisFrame()) { BackToLastPage(); }
    }

    public void BackToLastPage()
    {
        if (WarningSystem.IsWarningActive) return;
        backToLastPageEvent?.Invoke();
        if (menuToGoBack != null)
        {
            this.gameObject.SetActive(false);
            menuToGoBack.SetActive(true);
        }
    }

    public void SetSelectObject(GameObject obj)
    {
        EventSystemExtension.SetSelectObject(obj);
    }
}