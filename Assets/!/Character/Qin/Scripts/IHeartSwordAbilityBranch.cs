using EditorAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IHeartSwordAbilityBranch : MonoBehaviour
{
    [GUIColor(GUIColor.Lime)]
    [FoldoutGroup("Attributes", nameof(abilityAttributes),
       nameof(HS_Cost), nameof(isTriggeredByAttackKey),
       nameof(canBeStopped), nameof(learned), nameof(isActive), nameof(toggleToActivate))]
    public Void abilityVoid1;

    [SerializeField, HideProperty] public SO_HeartSwordAttribute abilityAttributes;
    [SerializeField, HideProperty][Range(0, 10)] public int HS_Cost;
    [SerializeField, HideProperty] public bool isTriggeredByAttackKey = true;
    [SerializeField, HideProperty] public bool canBeStopped = true;
    [SerializeField, HideProperty] public bool learned = false;
    [SerializeField, HideProperty] public bool isActive = false;
    [SerializeField, HideProperty] public bool toggleToActivate = false;
    [GUIColor(144f, 151f, 222f)] public MeleeAttack HS_attack_effect = new MeleeAttack(2, 0.5f, 0.05f, new Vector2(1, 1.4f), 0.1f, 20f, 0.1f);
    [HideProperty] public IHeartSwordAbility parentAbility;

    // Start is called before the first frame update
    private void Start()
    {
        parentAbility = GetComponent<IHeartSwordAbility>();
    }
}