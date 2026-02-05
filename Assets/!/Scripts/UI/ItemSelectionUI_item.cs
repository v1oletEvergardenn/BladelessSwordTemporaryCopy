using DG.Tweening;
using EditorAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemSelectionUI_item : MonoBehaviour, IBackToLastMenu
{
    public Item item;
    public Image selectedImage;
    [HideProperty] public ItemSelectionUI itemUI;

    public void OnSelect()
    {
        selectedImage.rectTransform.DOComplete();
        selectedImage.rectTransform.DOAnchorPosY(-20f, 0.4f).SetEase(Ease.OutQuad);
    }

    public void OnDeselect()
    {
        selectedImage.rectTransform.DOComplete();
        selectedImage.rectTransform.DOAnchorPosY(0f, 0.4f).SetEase(Ease.OutQuad);
    }

    public void GoBack()
    {
        MenuManager.instance.CloseItemSelectionMenu();
    }

    public void EquipItem()
    {
        print(gameObject.name);
        itemUI.EquipItem(item);
    }
}