using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class BackToLastMenu : MonoBehaviour, IBackToLastMenu
{
    public GameObject goBackObject;
    public UnityEvent OnBack;

    public void GoBack()
    {
        if (goBackObject != null)
        {
            EventSystemExtension.SetSelectObject(goBackObject);
        }
        OnBack?.Invoke();
    }

    public void SetEventOnBack(Action callback)
    {
        OnBack.AddListener(() => callback());
    }
}