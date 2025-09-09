using Microlight.MicroBar;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Energy : MonoBehaviour
{
    public static Energy instance;
    private PlayerAttack playerAttack;
    private CharacterController2D controller;
    private InputPlayer playerInput;

    public float maxEnergy;
    public float currentEnergy;
    public float energyPercentage;
    public MicroBar energyBar;

    public Transform failedToDoActionSymbol;

    [Header("RestoreEnergy")][Range(0, 5)] public float restore_pre_second;
    [Range(0, 2)] public float restoreCDafterConsume;
    private float restoreTimer;
    private float restoreTime;

    [Range(0, 10)] public float attack_energy_consumption;
    [Range(0, 15)] public float perfect_attack_energy_restore;
    [Range(0, 10)] public float swordTeleport_energy_consumption;
    [Range(0, 10)] public float dash_energy_consumption;
    [Range(0, 10)] public float doubleJump_energy_consumption;

    [Range(0, 5)] public float float_consume_pre_second;
    [Range(0, 5)] public float defend_consume_pre_second;
    [Range(0, 5)] public float storm_consume_pre_second;

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
        energyBar.Initialize(maxEnergy);
    }

    private void Update()
    {
        restoreTimer += Time.unscaledDeltaTime;
        energyPercentage = (float)currentEnergy / maxEnergy;
        if (controller.isFloating) { FloatingConsume(); }
        if (playerAttack.isDefending) { DefendConsume(); }
        if (!playerAttack.isCounterAttacking &&
            playerAttack.anim.GetBool("storm") &&
            !playerAttack.stormReady) { StormConsume(); }

        if (restoreTimer >= restoreTime)
        {
            ChangeEnergy(-restore_pre_second * Time.deltaTime);
        }
    }

    public void IncreaseMaxEnergy(float i)
    {
        maxEnergy += i;
    }

    public void ChangeEnergy(float amount)
    {
        currentEnergy -= amount;
        currentEnergy = Mathf.Clamp(currentEnergy, 0, maxEnergy);
        energyBar.UpdateBar(currentEnergy);
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

    public bool DoubleJumpConsume()
    {
        if (doubleJump_energy_consumption <= currentEnergy)
        {
            restoreTime = restoreCDafterConsume;
            restoreTimer = 0f;
            ChangeEnergy(doubleJump_energy_consumption);
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
        if (currentEnergy <= 0)
        {
            playerInput.input_floating = false;
            playerInput.input_floating_timer = 0f;
            VFXManager.instance.FailedToDoAction();
            return false;
        }
        ChangeEnergy(float_consume_pre_second * Time.deltaTime);
        restoreTime = restoreCDafterConsume;
        restoreTimer = 0f;
        return true;
    }

    public bool StormConsume()
    {
        if (currentEnergy <= 0)
        {
            playerAttack.anim.SetBool("storm", false);
            playerAttack.isOnStorm = false;
            playerAttack.isPreparingStorm = false;
            playerAttack.prepareStormTimer = 0f;
            VFXManager.instance.FailedToDoAction();
            return false;
        }
        ChangeEnergy(storm_consume_pre_second * Time.deltaTime);
        restoreTime = restoreCDafterConsume;
        restoreTimer = 0f;
        return true;
    }

    public bool DefendConsume()
    {
        if (currentEnergy <= 0)
        {
            playerAttack.EndDefend();
            VFXManager.instance.FailedToDoAction();
            return false;
        }
        ChangeEnergy(defend_consume_pre_second * Time.deltaTime);
        restoreTime = restoreCDafterConsume;
        restoreTimer = 0f;
        return true;
    }
}