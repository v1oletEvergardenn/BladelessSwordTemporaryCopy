using EditorAttributes;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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

    [SerializeField, HideProperty] private List<Ability_UI_segment> segments;
    public Dictionary<IHeartSwordAbility, Ability_UI_segment> abilityToSegment = new Dictionary<IHeartSwordAbility, Ability_UI_segment>();
    [SerializeField, HideProperty] public GameObject SelectHSAbilityUI;
    [SerializeField, HideProperty] public GameObject HSAbiltiyUI;
    [SerializeField, HideProperty] public Btn_HSAbility FirstSelectedHSAbility;

    [FoldoutGroup("ability segment information UI reference", nameof(txt_name), nameof(txt_chineseName),
        nameof(txt_description), nameof(img_showImage))]
    public Void hsAbilityInfoVoid;

    [SerializeField, HideProperty] public TextMeshProUGUI txt_name;
    [SerializeField, HideProperty] public TextMeshProUGUI txt_chineseName;
    [SerializeField, HideProperty] public TextMeshProUGUI txt_description;
    [SerializeField, HideProperty] public Image img_showImage;

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
        abilityToSegment.Clear();

        // Gather all learned abilities
        List<IHeartSwordAbility> learnedAbilities = hs.GetLearnedAbilities();
        // Assign learned abilities to segments in order, do not skip segment indices
        for (int i = 0; i < segments.Count; i++)
        {
            if (i < learnedAbilities.Count)
            {
                segments[i].ability = learnedAbilities[i];
                abilityToSegment[segments[i].ability] = segments[i];
            }
            else
            {
                segments[i].ability = null;
            }
        }

        // open select ability menu
        HSAbiltiyUI.SetActive(false);
        SelectHSAbilityUI.SetActive(true);
        EventSystem.current.SetSelectedGameObject(segments[0].gameObject);
    }

    public void ReturnToHSAbilityMenu()
    {
        SelectHSAbilityUI.SetActive(false);
        HSAbiltiyUI.SetActive(true);
        EventSystem.current.SetSelectedGameObject(FirstSelectedHSAbility.gameObject);
    }

    public void UpdateSegmentUIInfo(Ability_UI_segment segment)
    {
        txt_chineseName.text = segment.ability != null ? segment.ability.GetCurrentAttribute().chineseName : "";
        txt_name.text = segment.ability != null ? segment.ability.GetCurrentAttribute().name : "";
        txt_description.text = segment.ability != null ? segment.ability.GetCurrentAttribute().description : "";
        //img_showImage.sprite = segment.ability != null ? segment.ability.abilityAttributes.icon : null;
    }

    /// <summary>
    /// Changes the ability equipped in the specified slot, handling cases where the new ability is already equipped or
    /// is currently equipped in another slot.
    /// </summary>
    /// <remarks>This method performs the following actions based on the state of the provided ability: <list
    /// type="bullet"> <item>If <paramref name="newAbility"/> is already equipped in the selected slot, it will be
    /// unequipped.</item> <item>If <paramref name="newAbility"/> is equipped in a different slot, it will be swapped
    /// with the ability in the selected slot.</item> <item>If <paramref name="newAbility"/> is not equipped in any
    /// slot, it will replace the current ability in the selected slot.</item> </list> The method ensures that the UI is
    /// updated to reflect the changes in ability assignments.</remarks>
    /// <param name="newAbility">The new ability to equip in the selected slot. Must implement <see cref="IHeartSwordAbility"/>.</param>
    public void ChangeAbility(IHeartSwordAbility newAbility)
    {
        HeartSwordAbilities hsManager = HeartSwordAbilities.instance;
        IHeartSwordAbility oldAbility = null;

        // Find which slot (if any) currently has newAbility equipped
        AbilitySlot? equippedSlot = null;
        if (hsManager.GetWestAbility() == newAbility) equippedSlot = AbilitySlot.West;
        else if (hsManager.GetNorthAbility() == newAbility) equippedSlot = AbilitySlot.North;
        else if (hsManager.GetEastAbility() == newAbility) equippedSlot = AbilitySlot.East;

        // Get the current ability in the selected slot
        switch (slot)
        {
            case AbilitySlot.West: oldAbility = hsManager.GetWestAbility(); break;
            case AbilitySlot.North: oldAbility = hsManager.GetNorthAbility(); break;
            case AbilitySlot.East: oldAbility = hsManager.GetEastAbility(); break;
        }

        // Case 1: newAbility is already equipped in the selected slot, unequip it
        if (equippedSlot == slot)
        {
            hsManager.UnequipAbility(newAbility);
            if (abilityToSegment.ContainsKey(newAbility)) abilityToSegment[newAbility].UpdateUI();
            return;
        }

        // Case 2: newAbility is equipped in another slot, swap
        if (equippedSlot != null && oldAbility != null)
        {
            //swap selected slot 's ability with newAbility
            hsManager.EquipAbility(newAbility, slot, false);
            // Set the swapped slot to oldAbility or null
            switch (equippedSlot)
            {
                case AbilitySlot.West: hsManager.EquipAbility(oldAbility, AbilitySlot.West, false); break;
                case AbilitySlot.North: hsManager.EquipAbility(oldAbility, AbilitySlot.North, false); break;
                case AbilitySlot.East: hsManager.EquipAbility(oldAbility, AbilitySlot.East, false); break;
            }

            abilityToSegment[newAbility].UpdateUI();
            abilityToSegment[oldAbility].UpdateUI();
            return;
        }

        // Case 3: newAbility is not equipped, equip in selected slot, unequip old
        hsManager.EquipAbility(newAbility, slot, true);
        if (oldAbility != null) abilityToSegment[oldAbility].UpdateUI(); ;
        abilityToSegment[newAbility].UpdateUI();
    }

    public void UpdateAllSegmentUI()
    {
        foreach (Ability_UI_segment segment in segments)
        {
            segment.UpdateUI();
        }
    }
}