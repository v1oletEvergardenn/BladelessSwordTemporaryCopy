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
                ability = hsManager.abilityWest;
                inputKeyType = InputKeyType.AbilityWest_key;
                break;

            case AbilitySlot.North:
                ability = hsManager.abilityNorth;
                inputKeyType = InputKeyType.AbilityNorth_key;
                break;

            case AbilitySlot.East:
                ability = hsManager.abilityEast;
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
        HeartSwordAbilities.instance.SelectSlotBeforeSwitchAbility(this);
        HeartSwordAbilities.instance.OpenSelectAbilityMenu();
    }
}