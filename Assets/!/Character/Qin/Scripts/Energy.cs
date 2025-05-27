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

    public Transform failedToDoActionSymbol;

    [Header("RestoreEnergy")] public float restoreCD;
    public float restoreCDafterConsume;
    private float restoreTimer;
    private float restoreTime;

    public int attack_energy_consumption;
    public int perfect_attack_energy_restore;

    public int swordTeleport_energy_consumption;

    public float floating_consumption_frequency;

    public float defend_consumption_frequency;

    public float storm_consumption_frequency;

    public int dash_energy_consumption;

    private float floating_timer;
    private float defend_timer;
    private float storm_timer;

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
        if (controller.isFloating) { floating_timer += Time.deltaTime; }
        if (playerAttack.isDefending) { defend_timer += Time.deltaTime; }
        if (!playerAttack.isAttacking && playerAttack.anim.GetBool("storm") && !playerAttack.stormReady) { storm_timer += Time.deltaTime; }
        if (floating_timer >= floating_consumption_frequency)
        {
            FloatingConsume();
            floating_timer = 0f;
        }//floating
        if (defend_timer >= defend_consumption_frequency)
        {
            DefendConsume();
            defend_timer = 0f;
        }//defending

        if (storm_timer >= storm_consumption_frequency)
        {
            StromConsume();
            storm_timer = 0;
        }

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
        VFXManager.instance.FailedToDoAction();
        return false;
    }

    public bool TeleportConsume()
    {
        if (swordTeleport_energy_consumption <= currentEnergy)
        {
            restoreTime = restoreCDafterConsume;
            restoreTimer = 0f;
            ChangeEnergy(swordTeleport_energy_consumption);
            return true;
        }
        VFXManager.instance.FailedToDoAction();
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
        VFXManager.instance.FailedToDoAction();
        return false;
    }

    public void PerfectCounterAttackRestore()
    {
        ChangeEnergy(-perfect_attack_energy_restore);
    }

    public bool FloatingConsume()
    {
        if (currentEnergy < 1)
        {
            playerInput.input_floating = false;
            playerInput.input_floating_timer = 0f;
            VFXManager.instance.FailedToDoAction();
            return false;
        }
        ChangeEnergy(1);
        restoreTime = restoreCDafterConsume;
        restoreTimer = 0f;
        return true;
    }

    public bool StromConsume()
    {
        if (currentEnergy < 1)
        {
            playerAttack.anim.SetBool("storm", false);
            playerAttack.isOnStorm = false;
            playerAttack.isPreparingStorm = false;
            playerAttack.prepareStormTimer = 0f;
            VFXManager.instance.FailedToDoAction();
            return false;
        }
        ChangeEnergy(1);
        restoreTime = restoreCDafterConsume;
        restoreTimer = 0f;
        return true;
    }

    public bool DefendConsume()
    {
        if (currentEnergy < 1)
        {
            playerAttack.EndDefend();
            VFXManager.instance.FailedToDoAction();
            return false;
        }
        ChangeEnergy(1);
        restoreTime = restoreCDafterConsume;
        restoreTimer = 0f;
        return true;
    }
}