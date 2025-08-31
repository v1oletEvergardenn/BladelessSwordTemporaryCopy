using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    private InputMaster inputManager;

    public static GameManager instance;
    [HideInInspector] public GameObject player;
    public AnimationCurve outline_flash_anim_curve;
    [HideInInspector] public Health playerhealth;
    [HideInInspector] public PlayerAttack playerAttack;
    [HideInInspector] public InputPlayer playerInput;
    public CharacterController2D player_controller;
    public Material FlashEffectMat;
    [HideInInspector] public CinemachineImpulseSource impulseSource;
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

    public List<GameObject> NotTobeDestoryedObjects;

    private void Awake()
    {
        if (instance == null) { instance = this; }
        else { Destroy(this.gameObject); }
        //player_Idamagable = Player.GetComponent<Health>();
        //playerInput = Player.GetComponent<InputPlayer>();
        //playerAttack = Player.GetComponent<PlayerAttack>();
        //player_controller = Player.GetComponent<CharacterController2D>();
    }

    private void Start()
    {
        impulseSource = GetComponent<CinemachineImpulseSource>();
        foreach (GameObject obj in NotTobeDestoryedObjects)
        {
            if (obj != null) DontDestroyOnLoad(obj);
        }
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