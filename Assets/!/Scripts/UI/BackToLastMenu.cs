using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BackToLastMenu : MonoBehaviour, IBackToLastMenu
{
    public GameObject goBackObject;

    public void GoBack()
    {
        if (goBackObject != null)
        {
            EventSystem.current.SetSelectedGameObject(goBackObject);
        }
    }
}