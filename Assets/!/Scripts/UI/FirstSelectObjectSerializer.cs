using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FirstSelectObjectSerializer : MonoBehaviour
{
    public GameObject FirstSelectObject;
    public UnityEvent OnSelected;
    public UnityEvent OnDisSelected;

    private void Start()
    {
        if (FirstSelectObject == null)
        {
            FirstSelectObject = GetComponentInChildren<Selectable>().gameObject;
        }
    }

    public GameObject Selected()
    {
        OnSelected?.Invoke();
        return FirstSelectObject;
    }

    public void SetFirstObject()
    {
        EventSystem.current.SetSelectedGameObject(FirstSelectObject);
    }

    public void DisSelected()
    {
        OnDisSelected?.Invoke();
    }
}