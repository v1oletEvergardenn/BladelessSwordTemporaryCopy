using EditorAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public enum AbilitySlot
{
    West,
    North,
    East
}

public class HSAbilitySwapMenu : MonoBehaviour
{
    public static HSAbilitySwapMenu instance;
    private AbilitySlot slot;

    [GUIColor(144f, 151f, 222f)]
    [FoldoutGroup("HS_ability swap menu", nameof(FirstSelectedHSAbility), nameof(segments), nameof(SelectHSAbilityUI),
        nameof(HSAbiltiyUI))]
    public Void hsAbilitySwapMenuVoid;

    [SerializeField, HideProperty] public List<Ability_UI_segment> segments;
    [SerializeField, HideProperty] public GameObject SelectHSAbilityUI;
    [SerializeField, HideProperty] public GameObject HSAbiltiyUI;
    [SerializeField, HideProperty] public Btn_HSAbility FirstSelectedHSAbility;

    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    private void Start()
    {
        SelectHSAbilityUI.SetActive(false);
    }

    public void SelectSlotBeforeSwitchAbility(Btn_HSAbility btnSlot)
    {
        slot = btnSlot.abilitySlot;
    }

    public void OpenSelectAbilityMenu()
    {
        HeartSwordAbilities hs = HeartSwordAbilities.instance;
        // iterate through available abilities
        for (int i = 0; i < segments.Count; i++)
        {
            segments[i].equipped = false;
            segments[i].ability = null;
            if (i < hs.allAbilities.Count)
            {
                segments[i].ability = hs.allAbilities[i];
                if (segments[i].ability == hs.abilityWest ||
                     segments[i].ability == hs.abilityNorth ||
                      segments[i].ability == hs.abilityEast)
                {
                    segments[i].equipped = true;
                }
            }
        }

        HSAbiltiyUI.SetActive(false);
        SelectHSAbilityUI.SetActive(true);
        EventSystem.current.SetSelectedGameObject(segments[0].gameObject);
        // open select ability menu
    }

    public void ChangeAbility(IHeartSwordAbility newAbility)
    {
        HeartSwordAbilities hsManager = HeartSwordAbilities.instance;

        switch (slot)
        {
            case AbilitySlot.West:
                hsManager.abilityWest = newAbility;
                hsManager.abilityWest.EquipAbility();
                break;

            case AbilitySlot.North:
                hsManager.abilityNorth = newAbility;
                hsManager.abilityNorth.EquipAbility();
                break;

            case AbilitySlot.East:
                hsManager.abilityEast = newAbility;
                hsManager.abilityEast.EquipAbility();
                break;
        }
        //SelectHSAbilityUI.SetActive(false);
        //HSAbiltiyUI.SetActive(true);
        //EventSystem.current.SetSelectedGameObject(FirstSelectedHSAbility.gameObject);
    }
}