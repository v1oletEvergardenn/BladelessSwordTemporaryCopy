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
    public bool setFirstObjectOnEnable = false;

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
        EventSystemExtension.SetSelectObject(FirstSelectObject);
    }

    public void DisSelected()
    {
        OnDisSelected?.Invoke();
    }

    private void OnEnable()
    {
        if (setFirstObjectOnEnable)
        {
            SetFirstObject();
        }
    }
}