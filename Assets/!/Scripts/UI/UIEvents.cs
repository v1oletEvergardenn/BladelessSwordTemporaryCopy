using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UIEvents : MonoBehaviour
{
    public UnityEvent onenableEvent;

    public void OnEnable()
    {
        onenableEvent?.Invoke();
    }
}