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
        StartCoroutine(DeferredSelect());
    }

    private IEnumerator DeferredSelect()
    {
        // Clear current selection immediately so the submit that opened this UI doesn't hit a control
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);

        // Wait at least one frame to let the input that opened this UI be released/ignored
        yield return null;
        if (EventSystem.current != null && FirstSelectObject != null)
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