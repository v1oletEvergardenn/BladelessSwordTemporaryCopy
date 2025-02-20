using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    private InputMaster inputManager;

    public static GameManager instance;
    public GameObject Player;
    public AnimationCurve outline_flash_anim_curve;
    public Health player_Idamagable;
    public PlayerAttack playerAttack;
    public InputPlayer playerInput;
    public CharacterController2D player_controller;
    public Material FlashEffectMat;
    [HideInInspector] public CinemachineImpulseSource impulseSource;
    public bool isInInformationEvent;
    public bool isInDialog;
    public bool GamePaused = false;

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
            Player.GetComponent<InputPlayer>().learnedMovement = true;
        }
        else if (skill == PlayerSkillsType.Jump)
        {
            Player.GetComponent<InputPlayer>().learnedJump = true;
        }
        else if (skill == PlayerSkillsType.DoubleJump)
        {
            Player.GetComponent<InputPlayer>().learnedDoubleJump = true;
        }
        else if (skill == PlayerSkillsType.Teleport)
        {
            Player.GetComponent<InputPlayer>().learnedTeleport = true;
        }
        else if (skill == PlayerSkillsType.Attack)
        {
            Player.GetComponent<InputPlayer>().learnedAttack = true;
        }
        else if (skill == PlayerSkillsType.Boomerang)
        {
            Player.GetComponent<InputPlayer>().learnedBoomerang = true;
        }
        else if (skill == PlayerSkillsType.Barrier)
        {
            Player.GetComponent<InputPlayer>().learnedBarrier = true;
        }
        else if (skill == PlayerSkillsType.Defend)
        {
            Player.GetComponent<InputPlayer>().learnedDefend = true;
        }
        else if (skill == PlayerSkillsType.HeartSword)
        {
            Player.GetComponent<InputPlayer>().learnedHeartSword = true;
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