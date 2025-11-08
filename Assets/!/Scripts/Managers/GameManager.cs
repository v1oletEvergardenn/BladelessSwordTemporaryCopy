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
    public bool NOTSTARTATMAINMENU = false;

    public static GameManager instance;

    private InputMaster inputManager;

    [ButtonField("InitializePlayerSaveData", "initializePlayerData")] public Void holder;
    [ButtonField("AutoSaveGame", "AutoSaveGame")] public Void holder2;
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
        CheckIfStartFromMainMenu();
    }

    /// <summary>
    /// Ensures the game starts from the main menu by resetting the current save slot and initializing the player
    /// reference if necessary.
    /// </summary>
    /// <remarks>If the active scene is not the "MainMenu", the current save slot is reset to -1. If a player
    /// instance exists, it creates a reference to the existing player; otherwise, it creates a new player instance. No
    /// action is taken if the active scene is "MainMenu".</remarks>
    public void CheckIfStartFromMainMenu()
    {
        if (SceneManager.GetActiveScene().name != "MainMenu")
        {
            SaveSystem.currentSaveSlot = -1;
            if (CharacterController2D.instance != null) CreatePlayerReference(CharacterController2D.instance.gameObject);
            else CreateNewPlayer();
        }
    }

    private void Start()
    {
        impulseSource = GetComponent<CinemachineImpulseSource>();
    }

    public bool LoadPlayer()
    {
        PlayerSaveData data = SaveSystem._saveData.playerData;
        CreateNewPlayer();
        PlayerSave.instance.Load(data);
        PlayerSave.instance.Load(SaveSystem._saveData.heartSwordData);
        return true;
    }

    /// <summary>
    /// Creates a new player instance if one does not already exist.
    /// </summary>
    /// <remarks>If a saved player instance is available, it will be used; otherwise, a new player instance
    /// is instantiated using the specified prefab. The player object is marked as persistent  across scene loads. This
    /// method also initializes the player and establishes necessary references.</remarks>
    public void CreateNewPlayer()
    {
        if (player == null)
        {
            if (PlayerSave.instance != null) player = PlayerSave.instance.gameObject;
            else player = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
            DontDestroyOnLoad(player);
        }
        InputMaster.instance.SwitchToGameplayAction();
        CreatePlayerReference(player);
        InitializePlayer();
    }

    public void CreatePlayerReference(GameObject _player)
    {
        if (_player == null) return;
        player = _player;
        playerhealth = _player.GetComponentInChildren<Health>();
        playerInput = _player.GetComponentInChildren<InputPlayer>();
        playerAttack = _player.GetComponentInChildren<PlayerAttack>();
        player_controller = _player.GetComponentInChildren<CharacterController2D>();
        playerEnergy = _player.GetComponentInChildren<Energy>();
        hsManager = _player.GetComponentInChildren<HeartSwordAbilities>();
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

    /// <summary>
    /// to be updated
    /// </summary>
    public void CreateNewGame(int slot)
    {
        //created new Game
        SaveSystem.currentSaveSlot = slot;
        CreateNewPlayer();
        SceneManager.LoadScene("YingYangFish_Scene");
        InputMaster.instance.SwitchToGameplayAction();
    }

    public void AutoSaveGame()
    {
        print("autosaved game");
        SaveSystem.AutoSave();
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
        PlayerAttack.instance.anim.SetBool("isRunning", false);
    }

    public void UnpauseGame()
    {
        GamePaused = false;
        Time.timeScale = 1f;
        InputMaster.instance.gameplayActions.Enable();
    }
}