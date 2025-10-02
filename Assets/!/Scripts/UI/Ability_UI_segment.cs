using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Ability_UI_segment : MonoBehaviour, IPointerEnterHandler, ISelectHandler
{
    public TextMeshProUGUI chineseText;
    public Image icon;
    public Image panel;
    public Image input;
    [SerializeField] private Color equipedColor;
    [SerializeField] private Color unequipedColor;
    public IHeartSwordAbility ability;
    private List<IHeartSwordAbilityBranch> availableBranch = new List<IHeartSwordAbilityBranch>();

    public void OnEnable()
    {
        UpdateUI();
    }

    private void Update()
    {
        if (InputMaster.instance.uiActions.Navigate.WasPressedThisFrame()) NavigateBranches();
    }

    public void NavigateBranches()
    {
        if (EventSystem.current.currentSelectedGameObject != gameObject) return;
        if (ability == null) return;
        int branchCount = availableBranch.Count;
        if (branchCount == 0) return;

        // Find current index, -1 means "none branch" (no branch equipped)
        int currentIndex = -1;
        if (ability.equippedBranch != null)
            currentIndex = availableBranch.IndexOf(ability.equippedBranch);

        float nav = InputMaster.instance.uiActions.Navigate.ReadValue<Vector2>().y;
        int nextIndex = currentIndex;

        if (nav >= 0.5f)
        {
            // Navigate up
            if (currentIndex == -1)
                nextIndex = branchCount - 1; // from none to last
            else if (currentIndex == 0)
                nextIndex = -1; // from first to none
            else
                nextIndex = currentIndex - 1;
        }
        else if (nav <= -0.5f)
        {
            // Navigate down
            if (currentIndex == -1)
                nextIndex = 0; // from none to first
            else if (currentIndex == branchCount - 1)
                nextIndex = -1; // from last to none
            else
                nextIndex = currentIndex + 1;
        }
        else
        {
            return; // No navigation input
        }

        // Equip the branch or set to none
        if (nextIndex == -1)
            ability.ChangeBranch(null);
        else
            ability.ChangeBranch(availableBranch[nextIndex]);
        UpdateUI();
    }

    public void UpdateUI()
    {
        panel.color = unequipedColor;
        availableBranch.Clear();
        if (ability != null)
        {
            chineseText.text = ability.abilityAttributes.chineseName;
            icon.sprite = ability.abilityAttributes.icon;
            availableBranch = ability.GetAvailableBranches();
            HSAbilitySwapMenu.instance.UpdateSegmentUIInfo(this);
            if (ability.isEquipped)
            {
                panel.color = equipedColor;
                input.color = new Color(1, 1, 1, 1);
                input.sprite = HeartSwordAbilities.instance.GetInputSpriteOnAbility(ability);
            }
            else
            {
                input.color = new Color(1, 1, 1, 0);
            }
        }
        else
        {
            input.color = new Color(1, 1, 1, 0);
            chineseText.text = "";
            icon.sprite = null;
        }
    }

    public void ChangeAbility()
    {
        if (ability == null) return;
        //StartCoroutine(IEChangeAbility());
        panel.color = equipedColor;
        HSAbilitySwapMenu.instance.ChangeAbility(ability);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        HSAbilitySwapMenu.instance.UpdateSegmentUIInfo(this);
    }

    public void OnSelect(BaseEventData eventData)
    {
        // Trigger the same effect as pointer enter when selected by controller
        HSAbilitySwapMenu.instance.UpdateSegmentUIInfo(this);
    }
}