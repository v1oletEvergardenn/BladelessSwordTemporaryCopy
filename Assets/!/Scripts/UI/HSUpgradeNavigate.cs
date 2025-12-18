using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class HSUpgradeNavigate : UIChildNavigate
{
    public Vector3 original_scale = new Vector3(0.7f, 0.7f, 0.7f);

    public override void OnDeselect(BaseEventData eventData)
    {
        eventData.selectedObject.transform.DOComplete();
        eventData.selectedObject.transform.DOScale(original_scale, 0.2f);
    }

    public override void OnSelect(BaseEventData eventData)
    {
        eventData.selectedObject.transform.DOComplete();
        eventData.selectedObject.transform.DOScale(original_scale * 1.1f, 0.2f);
    }
}