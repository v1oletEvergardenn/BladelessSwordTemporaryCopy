using EditorAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UIEvents : MonoBehaviour
{
    [GUIColor(GUIColor.Lime)][Title("OnEnable Event", 15)] public Void onenableVoid;

    public UnityEvent onenableEvent;

    [GUIColor(144f, 151f, 222f)][Title("OnDisable Event", 15)] public Void disableVoid;

    public UnityEvent disableEvent;

    [GUIColor(GUIColor.Cyan)][Title("BackToLastPage Event", 15)] public Void backToLastPageVoid;

    public UnityEvent backToLastPageEvent;
    public GameObject menuToGoBack;

    private void Update()
    {
        if (InputMaster.instance.uiActions.Cancel.WasPressedThisFrame()) { BackToLastPage(); }
    }

    public void OnEnable()
    {
        onenableEvent?.Invoke();
    }

    public void OnDisable()
    {
        disableEvent?.Invoke();
    }

    public void BackToLastPage()
    {
        backToLastPageEvent?.Invoke();
        if (menuToGoBack != null)
        {
            this.gameObject.SetActive(false);
            menuToGoBack.SetActive(true);
        }
    }
}