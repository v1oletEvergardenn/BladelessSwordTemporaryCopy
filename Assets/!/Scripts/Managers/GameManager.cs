using Cinemachine;
using EditorAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.InputSystem;
using static SaveSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    private InputMaster inputManager;

    [ButtonField("InitializePlayerSaveData", "initializePlayerData")] public Void holder;

    public GameObject playerPrefab;
    public GameObject player;
    [HideProperty] public Health playerhealth;
    [HideProperty] public PlayerAttack playerAttack;
    [HideProperty] public Energy playerEnergy;
    [HideProperty] public InputPlayer playerInput;
    [HideProperty] public CharacterController2D player_controller;
    [HideProperty] public HeartSwordAbilities hsManager;

    public Material FlashEffectMat;
    [HideInInspector] public CinemachineImpulseSource impulseSource;
    public AnimationCurve outline_flash_anim_curve;
    public bool isInInformationEvent;
    public bool isInDialog;
    public bool GamePaused = false;
    public bool isInPerformingState = false;

    public enum PlayerSkillsType
    {
        None,
        Movement,
        Jump,
        DoubleJump,
        Teleport,
        Attack,
        Boomerang,
        Barrier,
        Defend,
        HeartSword
    }

    private void Awake()
    {
        if (instance == null) { instance = this; }
        else { Destroy(this.gameObject); }
    }

    private void Start()
    {
        impulseSource = GetComponent<CinemachineImpulseSource>();
    }

    public bool LoadAndCreatePlayer()
    {
        PlayerSaveData data = SaveSystem._saveData.playerData;

        if (player == null)
        {
            if (PlayerSave.instance != null) player = PlayerSave.instance.gameObject;
            else player = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
            DontDestroyOnLoad(player);
        }

        playerhealth = player.GetComponentInChildren<Health>();
        playerInput = player.GetComponentInChildren<InputPlayer>();
        playerAttack = player.GetComponentInChildren<PlayerAttack>();
        player_controller = player.GetComponentInChildren<CharacterController2D>();
        playerEnergy = player.GetComponentInChildren<Energy>();
        hsManager = HeartSwordAbilities.instance;

        InitializePlayer();
        PlayerSave.instance.Load(data);
        return true;
    }

    public void InitializePlayer()
    {
        playerhealth.transform.position = Vector3.zero;
        playerhealth.SetMaxHealth(30);
        playerhealth.SetCurrentHealth(30);
        playerEnergy.maxEnergy = 16;
        playerEnergy.currentEnergy = 16;
        hsManager.SetMaxHSPoint(3);
        hsManager.SetCurrentHSPoint(3);
    }

    public void InitializePlayerSaveData()
    {
        PlayerSaveData data = new PlayerSaveData();
        data.currentHealth = 30;
        data.maxHealth = 30;
        data.currentEnergy = 16;
        data.maxEnergy = 16;
        data.currentHSpoint = 0;
        data.maxHSpoint = 3;
        SaveSystem._saveData.playerData = data;
        File.WriteAllText(SaveFileName(SaveSystem.currentSaveSlot), JsonUtility.ToJson(_saveData, true));
    }

    /// <summary>
    /// to be updated
    /// </summary>
    public void CreateNewGame(int slot)
    {
        //created new Game
        SaveSystem.currentSaveSlot = slot;
        if (PlayerSave.instance != null) player = PlayerSave.instance.gameObject;
        else player = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
        DontDestroyOnLoad(player);
        LevelSaveData levelSaveData = new LevelSaveData();
        levelSaveData.lastSavedScene = "YingYangFish_Scene";
        SaveSystem._saveData.levelData = levelSaveData;
        InitializePlayerSaveData();
    }

    private void Update()
    {
    }

    public void LearnSkills(PlayerSkillsType skill)
    {
        if (skill == PlayerSkillsType.Movement)
        {
            player.GetComponent<InputPlayer>().learnedMovement = true;
        }
        else if (skill == PlayerSkillsType.Jump)
        {
            player.GetComponent<InputPlayer>().learnedJump = true;
        }
        else if (skill == PlayerSkillsType.DoubleJump)
        {
            player.GetComponent<InputPlayer>().learnedDoubleJump = true;
        }
        else if (skill == PlayerSkillsType.Teleport)
        {
            player.GetComponent<InputPlayer>().learnedTeleport = true;
        }
        else if (skill == PlayerSkillsType.Attack)
        {
            player.GetComponent<InputPlayer>().learnedAttack = true;
        }
        else if (skill == PlayerSkillsType.Boomerang)
        {
            player.GetComponent<InputPlayer>().learnedBoomerang = true;
        }
        else if (skill == PlayerSkillsType.Barrier)
        {
            player.GetComponent<InputPlayer>().learnedStorm = true;
        }
        else if (skill == PlayerSkillsType.Defend)
        {
            player.GetComponent<InputPlayer>().learnedDefend = true;
        }
        else if (skill == PlayerSkillsType.HeartSword)
        {
            player.GetComponent<InputPlayer>().learnedHeartSword = true;
        }
    }

    public void PauseGame()
    {
        GamePaused = true;
        Time.timeScale = 0f;
        InputMaster.instance.gameplayActions.Disable();
        InputMaster.instance.uiActions.Enable();
        PlayerAttack.instance.anim.SetBool("isRunning", false);
    }

    public void UnpauseGame()
    {
        GamePaused = false;
        Time.timeScale = 1f;
        InputMaster.instance.gameplayActions.Enable();
        InputMaster.instance.uiActions.Disable();
    }
}