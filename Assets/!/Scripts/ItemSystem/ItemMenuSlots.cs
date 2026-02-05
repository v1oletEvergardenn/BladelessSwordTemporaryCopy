using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemMenuSlots : MonoBehaviour
{
    public TextMeshProUGUI title;
    public TextMeshProUGUI description;
    public Image selectedImage;
    public Image line;

    [Range(0, 5)] public int slotIndex;
    public ItemLevel slotLevel;

    // Start is called before the first frame update
    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(TryToChangeItem);
    }

    private void OnEnable()
    {
        FadeTween();
        UpdateUI();
        InputMaster.instance.uiActions.Uninstall.performed += ctx => TryUnequipItem();
    }

    private void OnDisable()
    {
        InputMaster.instance.uiActions.Uninstall.performed -= ctx => TryUnequipItem();
    }

    public void FadeTween()
    {
        line.fillAmount = 0f;
        title.alpha = 0f;
        description.alpha = 0f;
        line.DOFillAmount(1f, 1f).SetEase(Ease.OutQuad).OnComplete(() =>
        {
            title.DOFade(1f, 0.5f).SetEase(Ease.OutQuad);
            description.DOFade(1f, 0.5f).SetEase(Ease.OutQuad);
        });
        UpdateUI();
    }

    public void FinishFadeTween()
    {
        line.DOComplete();
        title.DOComplete();
        description.DOComplete();
        line.fillAmount = 1f;
        title.alpha = 1f;
        description.alpha = 1f;
    }

    public void OnSelect()
    {
        selectedImage.rectTransform.DOComplete();
        selectedImage.rectTransform.DOAnchorPosY(-20f, 0.2f).SetEase(Ease.OutQuad);
    }

    public void OnDeselect()
    {
        selectedImage.rectTransform.DOComplete();
        selectedImage.rectTransform.DOAnchorPosY(0f, 0.2f).SetEase(Ease.OutQuad);
    }

    public void TryUnequipItem()
    {
        if (EventSystem.current.currentSelectedGameObject == gameObject)
        {
            UnequipItem();
        }
    }

    public void UnequipItem()
    {
        ItemManager.UnequipItem(slotIndex);
        UpdateUI();
    }

    public void UpdateUI()
    {
        ItemManager.GetItemSlotFromCurrentModule(slotIndex, out ItemSlot slot);
        if (slot.item == null || slot.slotLevel == ItemLevel.None)
        {
            title.text = "";
            description.text = "";
        }
        else
        {
            title.text = slot.item.itemName;
            description.text = slot.item.itemDescription;
        }
    }

    public void TryToChangeItem()
    {
        UpdateUI();
        // Open the item selection menu for this slot

        MenuManager.instance.OpenItemSelectionMenu(this, slotLevel);
    }
}