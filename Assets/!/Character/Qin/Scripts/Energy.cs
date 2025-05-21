using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Energy : MonoBehaviour
{
    public static Energy instance;
    private PlayerAttack playerAttack;
    private CharacterController2D controller;
    private InputPlayer playerInput;

    public Image Energy_segment;
    public Transform energy_parent;
    public List<Image> segments = new List<Image>();
    private Color originalColor;

    public int maxEnergy;
    public int currentEnergy;
    public Image energyBar;

    [Header("RestoreEnergy")] public float restoreCD;
    public float restoreCDafterConsume;
    private float restoreTimer;
    private float restoreTime;

    [Header("Attack")] public int attack_energy_consumption;
    public int perfect_attack_energy_restore;

    [Header("boomerang")] public float boomerang_consumption_frequency;

    private float boomerang_timer;

    [Header("floating")] public float floating_consumption_frequency;
    private float floating_timer;

    [Header("defend")] public float defend_consumption_frequency;
    private float defend_timer;

    [Header("barrier")] public int barrier_energy_consumption;
    [Header("dash")] public int dash_energy_consumption;

    private void Awake()
    {
        if (instance == null) { instance = this; }
    }

    // Start is called before the first frame update
    private void Start()
    {
        currentEnergy = maxEnergy;

        playerAttack = GetComponent<PlayerAttack>();
        controller = GetComponent<CharacterController2D>();
        playerInput = GetComponent<InputPlayer>();
        originalColor = Energy_segment.color;
        UpdateEnergySegment();
    }

    private void Update()
    {
        restoreTimer += Time.deltaTime;
        if (playerAttack.isBoomeranging && !playerAttack._boomerang.stickedInToWall) { boomerang_timer += Time.deltaTime; }
        if (controller.isFloating) { floating_timer += Time.deltaTime; }
        if (playerAttack.isDefending) { defend_timer += Time.deltaTime; }
        if (boomerang_timer >= boomerang_consumption_frequency)
        {
            BoomerangConsume();
            boomerang_timer = 0f;
        }//boomeranging
        if (floating_timer >= floating_consumption_frequency)
        {
            FloatingConsume();
            ChangeEnergy(1);
            floating_timer = 0f;
        }//floating
        if (defend_timer >= defend_consumption_frequency)
        {
            DefendConsume();
            ChangeEnergy(1);
            defend_timer = 0f;
        }//defending

        if (restoreTimer >= restoreTime)
        {
            ChangeEnergy(-1);
            restoreTime = restoreCD;
            restoreTimer = 0f;
        }
    }

    public void IncreaseMaxEnergy(int i)
    {
        maxEnergy += i;
        UpdateEnergySegment();
    }

    private void UpdateEnergySegment()
    {
        for (int i = 0; i < maxEnergy; i++)
        {
            if (i >= segments.Count)
            {
                Image _image = Instantiate(Energy_segment, energy_parent).GetComponent<Image>();
                segments.Add(_image);
            }
        }
    }

    public void ChangeEnergy(int amount)
    {
        currentEnergy -= amount;
        currentEnergy = Mathf.Clamp(currentEnergy, 0, maxEnergy);
        for (int i = 0; i < maxEnergy; i++)
        {
            if (i < currentEnergy)
            {
                segments[i].color = originalColor;
            }
            else
            {
                segments[i].color = new Color(0, 0, 0, 0);
            }
        }
    }

    public bool AttackConsume()
    {
        if (attack_energy_consumption <= currentEnergy)
        {
            restoreTime = restoreCDafterConsume;
            restoreTimer = 0f;
            ChangeEnergy(attack_energy_consumption);
            return true;
        }
        return false;
    }

    public bool DashConsume()
    {
        if (dash_energy_consumption <= currentEnergy)
        {
            restoreTime = restoreCDafterConsume;
            restoreTimer = 0f;
            ChangeEnergy(dash_energy_consumption);
            return true;
        }
        return false;
    }

    public bool BarrierConsume()
    {
        if (barrier_energy_consumption <= currentEnergy)
        {
            restoreTime = restoreCDafterConsume;
            restoreTimer = 0f;
            ChangeEnergy(barrier_energy_consumption);
            return true;
        }
        return false;
    }

    public void PerfectCounterAttackRestore()
    {
        ChangeEnergy(-perfect_attack_energy_restore);
    }

    public bool BoomerangConsume()
    {
        if (currentEnergy <= 1)
        {
            playerAttack.RetreiveBoomerang();
            controller.canMove = true;
            //playerAttack._boomerang.SetBool(true);
            return false;
        }
        restoreTime = restoreCDafterConsume;
        restoreTimer = 0f;
        ChangeEnergy(1);
        boomerang_timer = 0f;
        return true;
    }

    public bool FloatingConsume()
    {
        if (currentEnergy < 1)
        {
            playerInput.input_floating = false;
            playerInput.input_floating_timer = 0f;
            return false;
        }
        restoreTime = restoreCDafterConsume;
        restoreTimer = 0f;
        return true;
    }

    public bool DefendConsume()
    {
        if (currentEnergy < 1)
        {
            playerAttack.EndDefend();
            return false;
        }
        restoreTime = restoreCDafterConsume;
        restoreTimer = 0f;
        return true;
    }
}