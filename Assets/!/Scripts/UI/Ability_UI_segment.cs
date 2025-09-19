using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;

public class Ability_UI_segment : MonoBehaviour
{
    public TextMeshProUGUI chineseText;
    public Image icon;
    public Image panel;
    public Color equipedColor;
    public Color unequipedColor;
    public bool equipped = false;
    public IHeartSwordAbility ability;
    private bool selected = false;

    public void OnEnable()
    {
        selected = false;
        panel.color = unequipedColor;
        if (ability != null)
        {
            chineseText.text = ability.abilityAttributes.chineseName;
            icon.sprite = ability.abilityAttributes.icon;
            if (equipped)
            {
                panel.color = equipedColor;
            }
        }
        else
        {
            chineseText.text = "";
            icon.sprite = null;
        }
    }

    public void ChangeAbility()
    {
        if (ability == null || selected) return;
        StartCoroutine(IEChangeAbility());
        selected = true;
    }

    public IEnumerator IEChangeAbility()
    {
        float duration = 0.3f; // Duration of the tween in seconds
        float elapsed = 0f;
        Color startColor = unequipedColor;
        Color endColor = equipedColor;

        // Tween the color
        while (elapsed < duration)
        {
            panel.color = Color.Lerp(startColor, endColor, elapsed / duration);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        panel.color = endColor; // Ensure final color is set
        HeartSwordAbilities.instance.ChangeAbility(ability);
    }
}