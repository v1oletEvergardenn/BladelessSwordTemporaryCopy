using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StoreItemBtnNavigates : UIChildNavigate
{
    public override void Awake()
    {
    }

    public override void OnDeselect(BaseEventData eventData)
    {
    }

    public override void OnSelect(BaseEventData eventData)
    {
        _lastSelected = eventData.selectedObject.GetComponent<Selectable>();
        TradeItemBtn itemBtn = _lastSelected.GetComponent<TradeItemBtn>();

        MenuManager.instance.UpdateStoreItemDetail(itemBtn.itemIndex);
    }
}