using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public abstract class UIChildNavigate : MonoBehaviour
{
    public List<Selectable> selectables = new List<Selectable>();
    public InputActionReference _navigateReference;
    public Selectable _lastSelected;

    // Start is called before the first frame update
    public virtual void Awake()
    {
        InitializeSlots();
    }

    public virtual void InitializeSlots()
    {
        // Initialize each slot button
        foreach (var selects in selectables)
        {
            AddSelectionListeners(selects);
        }
    }

    public virtual void OnEnable()
    {
        _navigateReference.action.performed += OnNavigate;
    }

    public virtual void OnDisable()
    {
        _navigateReference.action.performed -= OnNavigate;
    }

    public void AddSelectionListeners(Selectable selectable)
    {
        EventTrigger trigger = selectable.gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = selectable.gameObject.AddComponent<EventTrigger>();
        }

        // select event
        EventTrigger.Entry selectEntry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.Select
        };
        selectEntry.callback.AddListener(OnSelect);
        trigger.triggers.Add(selectEntry);

        // deselect event
        EventTrigger.Entry deselectEntry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.Deselect
        };
        deselectEntry.callback.AddListener(OnDeselect);

        trigger.triggers.Add(deselectEntry);

        //pointerenter event
        EventTrigger.Entry pointerEnter = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerEnter
        };
        pointerEnter.callback.AddListener(OnPointerEnter);
        trigger.triggers.Add(pointerEnter);
        //pointerexit event
        EventTrigger.Entry pointerExit = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerExit
        };
        pointerExit.callback.AddListener(OnPointerExit);
        trigger.triggers.Add(pointerExit);
    }

    public abstract void OnSelect(BaseEventData eventData);

    public abstract void OnDeselect(BaseEventData eventData);

    public void OnPointerEnter(BaseEventData eventData)
    {
        PointerEventData pointerEventData = eventData as PointerEventData;
        if (pointerEventData != null)
        {
            Selectable sel = pointerEventData.pointerEnter.GetComponentInParent<Selectable>();
            if (sel == null)
            {
                sel = pointerEventData.pointerEnter.GetComponentInChildren<Selectable>();
            }
            pointerEventData.selectedObject = sel.gameObject;
        }
    }

    public void OnPointerExit(BaseEventData eventData)
    {
        PointerEventData pointerEventData = eventData as PointerEventData;
        if (pointerEventData != null)
        {
            pointerEventData.selectedObject = null;
        }
    }

    public void OnNavigate(InputAction.CallbackContext context)
    {
        if (EventSystem.current.currentSelectedGameObject == null && _lastSelected != null)
        {
            EventSystemExtension.SetSelectObject(_lastSelected.gameObject);
        }
    }
}