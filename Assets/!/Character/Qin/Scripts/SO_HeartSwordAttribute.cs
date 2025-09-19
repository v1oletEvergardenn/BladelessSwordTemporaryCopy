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
}