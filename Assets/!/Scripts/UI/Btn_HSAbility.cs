using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Btn_HSAbility : MonoBehaviour
{
    public Image inputImage;
    private InputKeyType inputKeyType;

    //public Image abilityIcon;
    public IHeartSwordAbility ability;

    public AbilitySlot abilitySlot;

    // Start is called before the first frame update
    private void Start()
    {
    }

    public void OnEnable()
    {
        UpdateUI();
    }

    public void UpdateUI()
    {
        HeartSwordAbilities hsManager = HeartSwordAbilities.instance;
        switch (abilitySlot)
        {
            case AbilitySlot.West:
                if (hsManager.abilityWest != null) ability = hsManager.abilityWest;
                else { ability = null; }
                inputKeyType = InputKeyType.AbilityWest_key;
                break;

            case AbilitySlot.North:
                if (hsManager.abilityNorth != null) ability = hsManager.abilityNorth;
                else { ability = null; }
                inputKeyType = InputKeyType.AbilityNorth_key;
                break;

            case AbilitySlot.East:
                if (hsManager.abilityEast != null) ability = hsManager.abilityEast;
                else { ability = null; }
                inputKeyType = InputKeyType.AbilityEast_key;
                break;
        }
        Sprite sprite = InputMaster.instance.icons.gamePadicons.GetSprite(inputKeyType);
        if (sprite != null) inputImage.sprite = sprite;

        if (ability != null)
        {
            //abilityIcon.sprite = ability.abilityAttributes.icon;
        }
    }

    public void SelectAbility()
    {
        HSAbilitySwapMenu.instance.SelectSlotBeforeSwitchAbility(this);
        HSAbilitySwapMenu.instance.OpenSelectAbilityMenu();
    }
}