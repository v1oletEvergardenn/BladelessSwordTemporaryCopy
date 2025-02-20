using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class EventObject : MonoBehaviour
{
    public UnityEvent _Event;
    public GameObject interactSign;

    // Start is called before the first frame update
    public virtual void InteractEvent()
    {
        _Event?.Invoke();
    }

    public virtual void ShowInteractSign(bool b)
    {
        interactSign.gameObject.SetActive(b);
    }
}