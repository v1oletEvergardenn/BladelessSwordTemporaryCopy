using Microlight.MicroBar;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BossHealthBar : MonoBehaviour
{
    public TextMeshProUGUI bossNameText;
    public MicroBar healthBar;
    public MicroBar bossBreakBar;

    public GameObject panel;

    public void Initialize(string bossName, float maxHealthAmount, float maxBreakAmount)
    {
        bossNameText.text = bossName;
        healthBar.Initialize(maxHealthAmount);
        bossBreakBar.Initialize(maxBreakAmount);
    }

    public void Show()
    {
        panel.SetActive(true);
    }

    public void Hide()
    {
        panel.SetActive(false);
    }
}