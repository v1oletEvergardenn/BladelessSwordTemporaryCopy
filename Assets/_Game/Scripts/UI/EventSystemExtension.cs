using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class EventSystemExtension
{
    public static void SetSelectObject(GameObject obj)
    {
        if (obj.TryGetComponent<EventTrigger>(out EventTrigger sel))
        {
            sel.GetComponent<Selectable>().Select();
            sel.OnSelect(new BaseEventData(EventSystem.current));
        }
        else
        {
            EventSystem.current.SetSelectedGameObject(obj);
        }
    }
}