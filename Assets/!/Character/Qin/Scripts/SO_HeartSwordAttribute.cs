using EditorAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HearSword ability", menuName = "HeartSword/ability attribute")]
public class SO_HeartSwordAttribute : ScriptableObject
{
    public string attributeName;
    public string chineseName;
    [TextArea(3, 10)] public string description;
    public Sprite icon;

    [Range(0, 10)] public int HS_Cost;
    public bool isTriggeredByAttackKey = true; // If false, triggered by ability key
    public bool canBeStopped = true;
    [GUIColor(144f, 151f, 222f)] public MeleeAttack HS_attack_effect = new MeleeAttack(2, 0.5f, 0.05f, new Vector2(1, 1.4f), 0.1f, 20f, 0.1f);
}