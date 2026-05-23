using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static UnityEngine.UI.Image;
using Random = UnityEngine.Random;

public class YingYangFish_AI : IEnemyController
{
    #region COROUTINES

    [HideInInspector] public Coroutine co_sprintToAngle;
    [HideInInspector] public Coroutine co_fishAppear;
    [HideInInspector] public Coroutine co_sprintToAngleAndDive;
    [HideInInspector] public Coroutine co_return_singleFishDive;

    #endregion COROUTINES

    [HideInInspector] public Vector3 Dir;
    [HideInInspector] public SpriteRenderer blackSprite;
    [HideInInspector] public SpriteRenderer whiteSprite;
    [HideInInspector] public Animator blackAnim;
    [HideInInspector] public Animator whiteAnim;
    [HideInInspector] public Animator centerAnim;
    [HideInInspector] public bool closerFish_Black;

    private float white_rotateVelocity;
    private float black_rotateVelocity;

    #region FISH REFERENCES

    [SerializeField] public Transform center;
    [SerializeField] public Transform fish_origin;
    [SerializeField] public Transform blackFish;
    [SerializeField] public Transform whiteFish;
    [SerializeField] public Transform blackFishGFX;
    [SerializeField] public Transform whiteFishGFX;
    [SerializeField] public Transform blackOrigin;
    [SerializeField] public Transform whiteOrigin;

    #endregion FISH REFERENCES

    #region ROTATION & MOVEMENT

    [SerializeField] public float idleRotateSpeed;
    [SerializeField] public float fastRotateSpeed;
    [SerializeField] public float swimSpeed = 30f;
    [SerializeField] public float fastSwimSpeed = 60f;

    [HideInInspector] public Transform movingTarget;
    [HideInInspector] public float black_rotateSpeed;
    [HideInInspector] public float white_rotateSpeed;
    [HideInInspector] public float black_targetRotateSpeed;
    [HideInInspector] public float white_targetRotateSpeed;

    #endregion ROTATION & MOVEMENT

    #region GENERAL REFERENCES

    [SerializeField] public Transform endCanvas;
    [SerializeField] public Transform waterLevel;
    [SerializeField] public GameObject EventInteract;
    [SerializeField] public GameObject black_particle;
    [SerializeField] public GameObject white_particle;
    [SerializeField] public Sprite black_tex;
    [SerializeField] public Sprite white_tex;
    [SerializeField] public GameObject swimEffect;
    [SerializeField] public CameraLimit camLimit;
    [SerializeField] public GeneralEventInteraction interaction;

    #endregion GENERAL REFERENCES

    [HideInInspector] public bool secondPhase;

    #region Action Fields

    [SerializeField] public YYF_WaterSpear waterSpear;
    [SerializeField] public YYF_Swing swing;
    [SerializeField] public YYF_BubbleTrap bubbleTrap;
    [SerializeField] public YYF_Gatling gatling;
    [SerializeField] public YYF_Splash splash;

    #endregion Action Fields

    public static YingYangFish_AI instance;

    public bool enableTest = false;

    [SerializeField] private List<YYFActionPhase> TEST = new List<YYFActionPhase>();

    [SerializeField] private List<YYFActionPhase> centerStart1 = new List<YYFActionPhase>();
    [SerializeField] private List<YYFActionPhase> centerStart2 = new List<YYFActionPhase>();
    [SerializeField] private List<YYFActionPhase> centerStart3 = new List<YYFActionPhase>();
    [SerializeField] private List<YYFActionPhase> centerStart1_lowHealth = new List<YYFActionPhase>();
    [SerializeField] private List<YYFActionPhase> centerStart2_lowHealth = new List<YYFActionPhase>();
    [SerializeField] private List<YYFActionPhase> centerStart3_lowHealth = new List<YYFActionPhase>();

    [SerializeField] private List<YYFActionPhase> splashStart = new List<YYFActionPhase>();
    [SerializeField] private List<YYFActionPhase> swingStart = new List<YYFActionPhase>();
    [SerializeField] private List<YYFActionPhase> bubbleTrapStart = new List<YYFActionPhase>();
    [SerializeField] private List<YYFActionPhase> splashStart_lowHealth = new List<YYFActionPhase>();
    [SerializeField] private List<YYFActionPhase> swingStart_lowHealth = new List<YYFActionPhase>();
    [SerializeField] private List<YYFActionPhase> bubbleTrapStart_lowHealth = new List<YYFActionPhase>();

    #region Unity Lifecycle

    public void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public override void Start()
    {
        base.Start();
        blackSprite = blackFishGFX.GetComponent<SpriteRenderer>();
        whiteSprite = whiteFishGFX.GetComponent<SpriteRenderer>();
        blackAnim = blackFishGFX.GetComponent<Animator>();
        whiteAnim = whiteFishGFX.GetComponent<Animator>();
        centerAnim = center.GetComponent<Animator>();
        movingTarget = player;
        EventInteract.SetActive(true);
        HealthUI.SetActive(false);

        StopRotate(false);
        canTakeDamage = false;

        blackFish.gameObject.SetActive(false);
        whiteFish.gameObject.SetActive(false);
    }

    private float playerDistanceDelta = 0f;
    private float playerDistanceTimer = 0f;

    private Queue<float> playerDistanceCache = new();

    private void Update()
    {
        distanceToPlayer = Mathf.Abs(transform.position.x - player.position.x);
        healthPercentage = currentHealth / maxHealth;
        playerDistanceTimer += Time.deltaTime;
        playerDistanceCache.Enqueue(distanceToPlayer);

        if (playerDistanceTimer >= 3)
        {
            float distance_three_secs_ago = playerDistanceCache.Dequeue();
            playerDistanceDelta = distanceToPlayer - distance_three_secs_ago;
        }

        white_rotateSpeed = Mathf.SmoothDamp(white_rotateSpeed, white_targetRotateSpeed, ref white_rotateVelocity, 0.2f);
        black_rotateSpeed = Mathf.SmoothDamp(black_rotateSpeed, black_targetRotateSpeed, ref black_rotateVelocity, 0.2f);

        blackOrigin.Rotate(new Vector3(0, 0, -1), black_rotateSpeed * Time.deltaTime);
        whiteOrigin.Rotate(new Vector3(0, 0, -1), white_rotateSpeed * Time.deltaTime);

        //if (!isActing && actionList.Count > 0)
        //{
        //    isActing = true;
        //    co_act = StartCoroutine(Act());
        //}
    }

    #endregion Unity Lifecycle

    #region Main AI Coroutines

    public IEnumerator IESprintToAngle(bool isBlack, float angle)
    {
        Transform origin = isBlack ? blackOrigin : whiteOrigin;
        Animator anim = isBlack ? blackAnim : whiteAnim;
        Transform fish = isBlack ? blackFish : whiteFish;
        float currentZ = origin.eulerAngles.z;
        anim.SetBool("isFast", true);

        // LocalAxisAdd adds the delta ON TOP of current rotation ?no absolute start needed
        SetFishRotateSpeed(isBlack, fastRotateSpeed, true);

        const float angleThreshold = 20f;
        yield return new WaitUntil(() =>
        {
            return Mathf.Abs(Mathf.DeltaAngle(origin.eulerAngles.z, angle)) <= angleThreshold;
        });

        anim.SetBool("isRotating", false);
        anim.Play("rotate_to_swim");
        fish.DOLocalRotate(new Vector3(0, 0, -20), 0.3f).SetEase(Ease.Linear);
        SetFishRotateSpeed(isBlack, 0, false);
    }

    public IEnumerator IESprintToAngleAndDive(bool isBlack, float angle)
    {
        Transform origin = isBlack ? blackOrigin : whiteOrigin;
        Transform fish = isBlack ? blackFish : whiteFish;

        yield return co_sprintToAngle = StartCoroutine(IESprintToAngle(isBlack, angle));

        yield return new WaitUntil(() =>
        {
            origin.position += fish.right * swimSpeed * Time.deltaTime;
            return IsUnderWater(isBlack);
        });
    }

    public IEnumerator IESingleFishDive(bool isBlack)
    {
        Transform fish = isBlack ? blackFish : whiteFish;
        Animator anim = isBlack ? blackAnim : whiteAnim;
        float z = -90;
        if (isBlack) { SetBlackRotateSpeed(0); black_rotateSpeed = 0; }
        else { SetWhiteRotateSpeed(0); white_rotateSpeed = 0; }

        Transform fishGFX = isBlack ? blackFishGFX : whiteFishGFX;
        Transform origin = isBlack ? blackOrigin : whiteOrigin;

        // Fast direct rotation instead of IESprintToAngle (which waits for gradual orbit)
        float currentZ = origin.eulerAngles.z;
        float rotationDuration = 0.4f;

        float targetAngle = Mathf.MoveTowardsAngle(currentZ, z, float.MaxValue);

        // Ensure the rotation is always clockwise by adding 360 if needed
        if (targetAngle > currentZ) targetAngle -= 360f;

        fishGFX.DORotate(new Vector3(0, 0, targetAngle), rotationDuration, RotateMode.Fast)
            .SetEase(Ease.InSine);
        yield return new WaitForSeconds(rotationDuration);

        float x = origin.position.x;
        bool moveDone = false;
        origin.DOMove(new Vector3(x, waterLevel.position.y - 7, 0), swimSpeed)
             .SetSpeedBased()
             .SetEase(Ease.Linear).OnComplete(() => moveDone = true);

        yield return new WaitUntil(() => moveDone);

        origin.localScale = new Vector3(1, 1, 1);
        origin.eulerAngles = new Vector3(0, 0, 0);
    }

    #endregion Main AI Coroutines

    #region AI Utility Methods

    public Transform CheckCloserFish()
    {
        if (blackFish.eulerAngles.z < whiteFish.eulerAngles.z) { return blackFish; }
        else { return whiteFish; }
    }

    public override void CancelAllAction()
    {
        TryStopCoroutine(co_sprintToAngle);
        TryStopCoroutine(co_multiCoroutine);
        TryStopCoroutine(co_multiActions);
        TryStopCoroutine(co_multiRun);
        //TryStopCoroutine(co_fishAppear);
        TryStopCoroutine(co_sprintToAngleAndDive);

        center.GetComponent<SpriteRenderer>().sortingOrder = 1;

        actionList.Clear();
        TryStopCoroutine(co_act);
        waterSpear.CancelAct();
        swing.CancelAct();
        bubbleTrap.CancelAct();
        gatling.CancelAct();
        blackOrigin.DOKill();
        whiteOrigin.DOKill();
        SetNormalRotateSpeed();
    }

    #endregion AI Utility Methods

    #region Phase & Ultimate Coroutines

    public IEnumerator Pre_SecondPhase()
    {
        if (!secondPhase)
        {
            DEAD = true;

            //clear
            actionList.Clear();
            CancelAllAction();
            selfPooler.SetPoolDisactive();

            //UI
            CharacterUIManager.ShowBlackEdge(true);

            SetWhiteRotateSpeed(idleRotateSpeed / 3);
            SetBlackRotateSpeed(idleRotateSpeed / 3);
            whiteAnim.Play("close_swim");
            blackAnim.Play("close_swim");

            //dive, move to center of map
            yield return StartCoroutine(ChangeYPos(-6));
            transform.DOMove(new Vector3(GetCenterXOfMap(), waterLevel.position.y - 6, 0), 1.8f);
            yield return new WaitForSeconds(1.8f);
            yield return StartCoroutine(ChangeYPos(2));// up

            centerAnim.Play("center_break");
            SoundManager.PlaySound("glass_break");
            secondPhase = true;
            EventInteract.SetActive(true);
        }
    }

    public IEnumerator secondPhaseAnim()
    {
        //disable player's actions
        InputMaster.instance.DisableAllActions();
        GameManager.instance.isInPerformingState = true;

        //event: player run to the left
        yield return StartCoroutine(playerController.RunToPositionCoroutine(transform.position - new Vector3(2, 0, 0)));
        playerController.FaceTarget(this.transform);
        yield return new WaitForSeconds(0.5f);

        //speed up fish rotate speed
        fastRotateSpeed *= 1.5f;
        idleRotateSpeed *= 1.5f;
        EventInteract.SetActive(false);

        InputMaster.instance._attackLeftAction.Enable();
        InputMaster.instance._attackRightAction.Enable();
        InputMaster.instance._defendAction.Enable();
        InputMaster.instance._attackDirectionAction.Enable();
        CharacterController2D.instance.FaceTarget(this.transform);
        StartCoroutine(ChangeYPos(4.5f));
        centerAnim.Play("center_fade");

        //circling
        StartCoroutine(EmojiDuringCircling());
        yield return StartCoroutine(Circling(3.5f));

        StartCoroutine(Ultimate());

        yield return null;
    }

    public IEnumerator Ultimate()
    {
        ////swing qte
        //yield return swing.act_routine = StartCoroutine(swing.Act_coroutine(1));

        ////move to left boundary
        //movingTarget = leftBoundary;
        //yield return dive.act_routine = StartCoroutine(dive.Act_coroutine());
        //yield return co_IEcloseSwim = StartCoroutine(IECloseSwim(false));
        //SetNormalRotateSpeed();

        ////double single swing and bubble gatling
        //singleSwing.act_routine = StartCoroutine(singleSwing.Act_coroutine(1));
        //yield return gatling.act_routine = StartCoroutine(gatling.Act_coroutine(1));

        ////water spear ultimate
        //yield return co_sprintBackEqual = StartCoroutine(IESprintBackEqual());
        //StartCoroutine(IESwimAway(waterSpearPos_black1.position, 10, true));
        //StartCoroutine(IESwimAway(waterSpearPos_white1.position, 10, false));
        //while (!blackPositioned || !whitePositioned) { yield return null; }
        //yield return StartCoroutine(IESprintSamePos());
        //StartCoroutine(waterSpear.Act_coroutine(1));
        //StartCoroutine(waterSpear.Act_coroutine(2));
        //while (!finishedWaterSpearUltimate) { yield return null; }

        ////swing ultimate
        //// move to player
        //movingTarget = player;
        //yield return dive.act_routine = StartCoroutine(dive.Act_coroutine());
        //yield return swing.act_routine = StartCoroutine(swing.Act_coroutine(2));

        ////move to right boundary
        //movingTarget = rightBoundary;
        //yield return dive.act_routine = StartCoroutine(dive.Act_coroutine());
        //yield return co_sprintBackEqual = StartCoroutine(IESprintBackEqual());

        //yield return new WaitForSeconds(1f);
        ////water ball ultimate
        //black_targetRotateSpeed = idleRotateSpeed / 3;
        //white_targetRotateSpeed = idleRotateSpeed / 3;

        ////circling
        //StartCoroutine(Circling(30f));
        //StartCoroutine(ChangeYPos(4));
        //yield return new WaitForSeconds(1f);

        ////ultimate wave show up
        //ultimateWave.SetActive(true);
        //ultimateWave.transform.position = new Vector3(transform.position.x, waterLevel.position.y, 0);

        ////charge up for 2 seconds
        //yield return new WaitForSeconds(2f);

        ////ultimate water ball bullets shooting
        //float[] bulletDelays = { 0f, 0.5f, 0.7f, 1.2f, 1.7f, 1.9f, 3f, 3.2f, 3.4f, 3.6f, 3.8f, 4f };
        //for (int i = 0; i < bulletDelays.Length; i++)
        //{
        //    StartCoroutine(SpawnUltimateBullet(i, bulletDelays[i]));
        //}

        ////water dragons
        //yield return new WaitForSeconds(1f);
        //waterDragon1.gameObject.SetActive(true);
        //yield return new WaitForSeconds(5f);

        ////player run to position
        //yield return StartCoroutine(playerController.RunToPositionCoroutine(transform.position - new Vector3(14f, 0, 0)));

        ////sword teleport jump qte
        //playerController.rb.velocity = Vector3.zero;
        //InputMaster.instance.StartMustSuccessQTE(InputKeyType.swordTeleport_key, player.transform.position + new Vector3(0, 4, 0),
        //    0.4f, () => { playerController.DesignatedPositionTeleport(player.transform.position + new Vector3(5, 7.5f, 0)); });

        //yield return new WaitUntil(() => !InputMaster.instance.isQTE);
        //playerController.EnableGravity(false);
        //playerController.rb.velocity = Vector3.zero;

        //yield return new WaitForSeconds(0.4f);
        //playerController.anim.Play("slash_pre");
        //yield return new WaitForSeconds(0.1f);

        ////slash qte

        //InputMaster.instance.StartMustSuccessQTE(InputKeyType.right_attack_key, player.transform.position + new Vector3(2, 2, 0),
        //   0.4f, () =>
        //   {
        //       playerController.anim.Play("slash_end");
        //   });
        //yield return new WaitUntil(() => !InputMaster.instance.isQTE);
        //yield return new WaitForSeconds(0.2f);

        //player.position += new Vector3(12, -7);
        //playerController.EnableGravity(true);
        ////slash_effect.transform.position = new Vector3(transform.position.x, waterLevel.position.y, 0f);
        ////slash_effect.SetActive(true);

        //blackAnim.speed = 0;
        //whiteAnim.speed = 0;
        //blackSprite.sprite = black_tex; whiteSprite.sprite = white_tex;
        //VFXManager.instance.SlowTimeForSeconds(0.5f, 0);
        //yield return new WaitForSeconds(0.1f);

        ////end
        //SetWhiteRotateSpeed(0); white_rotateSpeed = 0;
        //SetBlackRotateSpeed(0); black_rotateSpeed = 0;

        ////whiteAnim.SetTrigger("circling_end"); blackAnim.SetTrigger("circling_end");

        //ultimateWave.GetComponent<Animator>().SetTrigger("end");

        //yield return new WaitForSeconds(3f);
        //blackSprite.enabled = false;
        //whiteSprite.enabled = false;
        //black_particle.SetActive(true); white_particle.SetActive(true);
        //yield return new WaitForSeconds(5f);
        //black_particle.SetActive(false); white_particle.SetActive(false);

        ////slash_effect.SetActive(false);

        yield return null;
        //endCanvas.gameObject.SetActive(true);
    }

    public IEnumerator Circling(float duration)
    {
        center.GetComponent<SpriteRenderer>().sortingOrder = -1;
        string whiteClip = "circling_down_pre";
        string blackClip = "circling_up_pre";
        if (whiteFish.position.y > blackFish.position.y)
        {
            whiteClip = "circling_up_pre";
            blackClip = "circling_down_pre";
        }
        whiteAnim.Play(whiteClip);
        blackAnim.Play(blackClip);

        yield return new WaitForSeconds(duration);
        whiteAnim.SetTrigger("circling_end"); blackAnim.SetTrigger("circling_end");
        yield return new WaitForSeconds(0.3f);
        center.GetComponent<SpriteRenderer>().sortingOrder = 1;
        while (whiteAnim.GetCurrentAnimatorStateInfo(0).IsName("white_circling_down_end") ||
            blackAnim.GetCurrentAnimatorStateInfo(0).IsName("black_circling_down_end"))
        { yield return null; }
        yield return null;
    }

    private IEnumerator Dissolve()
    {
        float elapsedTime = 0f;
        float dissolveTime = 1.5f;

        black_particle.SetActive(true);
        white_particle.SetActive(true);
        while (elapsedTime < dissolveTime)
        {
            elapsedTime += Time.deltaTime;
            float lerpedDissolve = Mathf.Lerp(0, 1f, (elapsedTime / dissolveTime));
            blackFishGFX.GetComponent<SpriteRenderer>().material.SetFloat(Shader.PropertyToID("_DissolveAmount"), lerpedDissolve);
            whiteFishGFX.GetComponent<SpriteRenderer>().material.SetFloat(Shader.PropertyToID("_DissolveAmount"), lerpedDissolve);
            yield return null;
        }

        yield return new WaitForSeconds(2f);
        black_particle.SetActive(false);
        white_particle.SetActive(false);
    }

    public IEnumerator FishAppear(bool dive)
    {
        bool opposite = RandomFishBool();

        ResetAllFish();
        blackFishGFX.localPosition = new Vector3(-5, 0, 0);
        whiteFishGFX.localPosition = new Vector3(-5, 0, 0);
        blackOrigin.eulerAngles = opposite ? Vector3.zero : new Vector3(0, 0, 180);
        whiteOrigin.eulerAngles = opposite ? new Vector3(0, 0, 180) : Vector3.zero;
        SetBothRotateSpeed(0, false);
        //play fish appear animation
        blackFish.gameObject.SetActive(true);
        whiteFish.gameObject.SetActive(true);
        blackAnim.SetBool("isRotating", false);
        whiteAnim.SetBool("isRotating", false);

        blackAnim.Play("fast_down");
        whiteAnim.Play("fast_down");
        //set alpha to 0, then fade in
        blackFishGFX.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0);
        whiteFishGFX.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0);
        blackFishGFX.GetComponent<SpriteRenderer>().DOFade(1, 0.7f);
        whiteFishGFX.GetComponent<SpriteRenderer>().DOFade(1, 0.7f);
        blackFishGFX.DOLocalMoveX(0, 0.7f).SetEase(Ease.OutSine);
        whiteFishGFX.DOLocalMoveX(0, 0.7f).SetEase(Ease.OutSine);
        yield return new WaitForSeconds(0.5f);
        SetBothRotateSpeed(idleRotateSpeed, false);
        blackAnim.SetBool("isRotating", true);
        whiteAnim.SetBool("isRotating", true);
        yield return new WaitForSeconds(0.2f);
        blackFish.DOLocalRotate(new Vector3(0, 0, -20), 0.5f);
        whiteFish.DOLocalRotate(new Vector3(0, 0, -20), 0.5f);
        yield return new WaitForSeconds(0.5f);
        if (dive)
        {
            //sprint to angle
            yield return co_multiCoroutine = StartCoroutine(StartMultipleCoroutines(new List<IEnumerator> {
                IESprintToAngleAndDive(true, Random.Range(200f,300f)),
                IESprintToAngleAndDive(false,  Random.Range(200f,300f))
            }));
            // dive
        }
    }

    private void ResetAllFish()
    {
        ResetOrigin();
        ResetFishOrigin();
        ResetFish();
        ResetFishGFX();
    }

    /// <summary>
    /// reset fishOrigin
    /// </summary>
    public void ResetOrigin()
    {
        fish_origin.localPosition = Vector3.zero;
        fish_origin.eulerAngles = Vector3.zero;
    }

    /// <summary>
    /// reset blackOrigin and WhiteOrigin
    /// </summary>
    public void ResetFishOrigin(float factor = 2)
    {
        if (factor == 0 || factor == 2)
        {
            blackOrigin.eulerAngles = Vector3.zero;
            blackOrigin.localPosition = Vector3.zero;
        }
        if (factor == 1 || factor == 2)
        {
            whiteOrigin.eulerAngles = Vector3.zero;
            whiteOrigin.localPosition = Vector3.zero;
        }
    }

    /// <summary>
    /// reset blackFish and whiteFish
    /// </summary>
    public void ResetFish(float factor = 2)
    {
        if (factor == 0 || factor == 2)
        {
            blackFish.localEulerAngles = Vector3.zero;
            blackFish.localPosition = new Vector3(0, 3.5f, 0);
        }
        if (factor == 1 || factor == 2)
        {
            whiteFish.localEulerAngles = Vector3.zero;
            whiteFish.localPosition = new Vector3(0, 3.5f, 0);
        }
    }

    /// <summary>
    /// reset fishGFX
    /// </summary>
    public void ResetFishGFX(float factor = 2)
    {
        if (factor == 0 || factor == 2)
        {
            blackFishGFX.localEulerAngles = Vector3.zero;
            blackFishGFX.localPosition = new Vector3(0, 0, 0);
        }
        if (factor == 1 || factor == 2)
        {
            whiteFishGFX.localEulerAngles = Vector3.zero;
            whiteFishGFX.localPosition = new Vector3(0, 0, 0);
        }
    }

    private IEnumerator EmojiDuringCircling()
    {
        yield return new WaitForSeconds(0.2f);
        Emoji.instance.PlayEmotion(EmotionType.Sigh);
        yield return new WaitForSeconds(0.5f);
        playerAttack.anim.Play("defend");
        yield return null;
    }

    public IEnumerator ChangeYPos(float offset)
    {
        float y_value = waterLevel.position.y + offset;
        bool finished = false;
        transform.DOMoveY(y_value, 3f).SetEase(Ease.InOutSine).OnComplete(() => { finished = true; });
        while (!finished) { yield return null; }
        yield return null;
    }

    public void SummonFishAroundCenter()
    {
        fish_origin.localPosition = Vector3.zero;
        //black fish summon and reset
        blackFishGFX.localPosition = Vector3.zero;
        blackFish.localPosition = new Vector3(1, 0, 0);
        blackOrigin.localPosition = Vector3.zero;
        anim.Play("black_idle");

        //white fish summon and reset
        whiteFishGFX.localPosition = Vector3.zero;
        whiteFish.localPosition = new Vector3(1, 0, 0);
        whiteOrigin.localPosition = Vector3.zero;
        anim.Play("white_idle");
        SetNormalRotateSpeed();
    }

    #endregion Phase & Ultimate Coroutines

    #region IEnemyController Overrides

    public override IEnumerator Act()
    {
        //if the fish is still around the center, make the fish swim into the water first. then decide the actions.

        if (secondPhase) { yield break; }

        while (actionList[0] != null)
        {
            foreach (List<ActionCaller> list in actionList)
            {
                yield return co_multiActions = StartCoroutine(StartMultipleActions(list));
            }
            StartAction();
            yield return co_fishAppear = StartCoroutine(FishAppear(true));
        }
    }

    public override void StartAction()
    {
        if (secondPhase || DEAD) { return; }

        //initalize
        actionList.Clear();

        //logic: after fish is under the water, fish starts actions,
        //when calls the action method, put in the factor to determine which fish to use for the action:
        //0:black fish,
        //1:white fish,
        //2:both fish

        ///TEST********

        int randomInitial = Random.Range(0, 4);

        if (randomInitial == 0) initialAction = waterSpear;
        else if (randomInitial == 1) initialAction = splash;
        else if (randomInitial == 2) initialAction = swing;
        else if (randomInitial == 3) initialAction = bubbleTrap;

        // Re-roll until the new action differs from the last one
        while (initialAction == lastAction)
        {
            randomInitial = Random.Range(0, 4);
            if (randomInitial == 0) initialAction = waterSpear;
            else if (randomInitial == 1) initialAction = splash;
            else if (randomInitial == 2) initialAction = swing;
            else if (randomInitial == 3) initialAction = bubbleTrap;
        }

        //water spear as first action
        if (initialAction == waterSpear)
        {
            int randomNum = Random.Range(0, 3);

            // above 50% hp
            if (healthPercentage >= 0.5f)
            {
                //First Combo
                if (randomNum == 0) ApplyActionList(centerStart1);
                else if (randomNum == 1) ApplyActionList(centerStart2);
                else ApplyActionList(centerStart3);
            }
            else
            {
                if (randomNum == 0) ApplyActionList(centerStart1_lowHealth);
                else if (randomNum == 1) ApplyActionList(centerStart2_lowHealth);
                else ApplyActionList(centerStart3_lowHealth);
            }
        }
        else if (initialAction == splash)
        {
            ApplyActionList(healthPercentage >= 0.5f ? splashStart : splashStart_lowHealth);
        }
        else if (initialAction == swing)
        {
            ApplyActionList(healthPercentage >= 0.5f ? swingStart : swingStart_lowHealth);
        }
        else if (initialAction == bubbleTrap)
        {
            ApplyActionList(healthPercentage >= 0.5f ? bubbleTrapStart : bubbleTrapStart_lowHealth);
        }

        if (enableTest)
        {
            ApplyActionList(TEST);
        }
        //DebugPrintActionList();

        //start action
        if (initialAction != null)
        {
            //InsertAction(initialAction, 0);
            lastAction = initialAction;
            //co_act = StartCoroutine(Act());
        }
        else { StartAction(); return; }
    }

    public void ApplyActionList(List<YYFActionPhase> actions)
    {
        actionList.Clear();
        foreach (YYFActionPhase phase in actions)
        {
            List<ActionCaller> group = new List<ActionCaller>();
            int lastResolvedFish = 0;

            // For each Index value, randomly pick one entry among those sharing it
            var indexedGroups = phase.actionCombo
                .Where(e => e.UseIndex)
                .GroupBy(e => e.Index)
                .ToDictionary(g => g.Key, g => g.ToList());

            var selectedIndexedEntries = new HashSet<YYFActionEntry>();
            foreach (var kvp in indexedGroups)
            {
                YYFActionEntry chosen = kvp.Value[Random.Range(0, kvp.Value.Count)];
                selectedIndexedEntries.Add(chosen);
            }

            foreach (YYFActionEntry entry in phase.actionCombo)
            {
                // Skip indexed entries that weren't selected
                if (entry.UseIndex && !selectedIndexedEntries.Contains(entry))
                    continue;

                group.Add(entry.ToActionCaller(this, ref lastResolvedFish));
            }

            if (group.Count > 0)
                actionList.Add(group);
        }
    }

    public override IEnumerator IE_Activate()
    {
        // center interact and flowing upward animation
        Vector3 pos = new Vector3(center.position.x, 0, 0);
        interaction.transform.localPosition = Vector3.zero;
        leftBoundary.gameObject.SetActive(true);
        rightBoundary.gameObject.SetActive(true);
        float timer = 0f;
        center.DOLocalMove(Vector3.zero, 2f).SetEase(Ease.InOutSine).SetDelay(1f);
        while (timer <= 2f)
        {
            timer += Time.deltaTime;

            if (timer > 1f)
            {
                centerAnim.Play("center_rumbling");
                VFXManager.instance.Rumble(timer / 7, timer / 7);
            }
            yield return null;
        }
        VFXManager.instance.StopRumble();

        yield return new WaitForSeconds(2f);

        //setting before appear
        CameraFollow.instance.targets.Add(blackFish);
        CameraFollow.instance.targets.Add(whiteFish);

        //fish appear
        yield return co_fishAppear = StartCoroutine(FishAppear(true));

        IN_COMBAT = true;
        HealthUI.SetActive(true);

        StartAction();
        StartCoroutine(Act());
    }

    public override int Damage(float damageAmount, Transform sender, float stunDuration = 0, bool damageFlash = true, float stunValue = 0)
    {
        print(damageAmount + " dealt by " + (sender != null ? sender.name : "unknown"));
        if (DEAD || !canTakeDamage) { return 0; }

        int attackId = sender != null ? sender.GetInstanceID() : 0;
        if (lastAttackId == attackId && Time.time - lastAttackTime < attackCooldown) return 0;

        lastAttackId = attackId;
        lastAttackTime = Time.time;

        if (damageFlash) flash.OnDamageFlash();

        if (!isBossBreaking) { damageAmount *= 0.5f; }

        currentHealth -= damageAmount;

        if (!IN_COMBAT)
        {
            IN_COMBAT = true;
            HealthUI.SetActive(true);
            EventInteract.SetActive(false);
        }

        healthBar.UpdateBar(currentHealth);
        DecreaseStun(stunValue);

        if (currentHealth <= 0)
        {
            StartCoroutine(Pre_SecondPhase());
            DEAD = true;
        }

        return 0;
    }

    public override void Repel(float force, bool left)
    {
    }

    public override int SubObjectDamage(float damageAmount, Transform sender = null, float stunDuration = 0, float stunValue = 0)
    {
        return Damage(damageAmount, sender, stunDuration, false, stunValue);
    }

    #endregion IEnemyController Overrides

    #region Utility & Interaction

    public Quaternion CalculateWantedRotation(Vector3 startPos, Vector3 _targetPos)
    {
        float angle = Mathf.Atan2(_targetPos.y - startPos.y, _targetPos.x - startPos.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(new Vector3(0, 0, angle));
        return targetRotation;
    }

    public void Interact()
    {
        if (!secondPhase)
        {
            camLimit.UpdateLimit();
            ///CameraFollow.instance.targets.Add(center);
            StartCoroutine(IE_Activate());
            EventInteract.SetActive(false);
        }
        else
        {
            StartCoroutine(secondPhaseAnim());
        }
    }

    public override IEnumerator BossBreak()
    {
        isBossBreaking = true;
        CancelAllAction();
        blackAnim.Play("break"); whiteAnim.Play("break");
        SetBothRotateSpeed(0);
        VFXManager.instance.BulletTime();
        float duration = stunDuration;
        float elapsed = 0f;
        float startStun = currentStun;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime; // Use unscaled time to be immune to bullet time
            float t = Mathf.Clamp01(elapsed / duration);
            currentStun = Mathf.Lerp(startStun, maxStun, t);

            if (stunBar != null && maxStun > 0)
                stunBar.UpdateBar(currentStun);

            yield return null;
        }

        currentStun = maxStun;
        if (stunBar != null && maxStun > 0)
            stunBar.UpdateBar(maxStun);

        VFXManager.instance.UnBulletTime();
        isBossBreaking = false;

        yield return co_fishAppear = StartCoroutine(FishAppear(true));
        StartAction();
        StartCoroutine(Act());
        yield return null;
    }

    public void SetNormalRotateSpeed()
    {
        black_targetRotateSpeed = idleRotateSpeed;
        white_targetRotateSpeed = idleRotateSpeed;
    }

    public void StopRotate(bool smooth)
    {
        black_targetRotateSpeed = 0;
        white_targetRotateSpeed = 0;
        if (!smooth) { black_rotateSpeed = 0; white_rotateSpeed = 0; }
    }

    //public void SetBlackBusy() => isBlackBusy = true;

    //public void SetWhiteBusy() => isWhiteBusy = true;

    //public void SetBlackNotBusy() => isBlackBusy = false;

    //public void SetWhiteNotBusy() => isWhiteBusy = false;

    //public void SetNotBusy(bool isBlack)
    //{
    //    if (isBlack) { SetBlackNotBusy(); }
    //    else { SetWhiteNotBusy(); }
    //}

    //public void SetBusy(bool isBlack)
    //{
    //    if (isBlack) { SetBlackBusy(); }
    //    else { SetWhiteBusy(); }
    //}

    //Target Rotate Speed
    public void SetFishRotateSpeed(bool isBlack, float speed, bool smooth = true)
    {
        if (isBlack)
        {
            if (!smooth) black_rotateSpeed = speed;
            black_targetRotateSpeed = speed;
        }
        else
        {
            if (!smooth) white_rotateSpeed = speed;
            white_targetRotateSpeed = speed;
        }
    }

    public void SetBlackRotateSpeed(float speed, bool smooth = true)
    {
        if (!smooth) black_rotateSpeed = speed;
        black_targetRotateSpeed = speed;
    }

    public void SetWhiteRotateSpeed(float speed, bool smooth = true)
    {
        if (!smooth) white_rotateSpeed = speed;
        white_targetRotateSpeed = speed;
    }

    public void SetBothRotateSpeed(float speed, bool smooth = true)
    {
        SetBlackRotateSpeed(speed, smooth);
        SetWhiteRotateSpeed(speed, smooth);
    }

    //Target Rotate Speed

    public int RandomFish() => Random.Range(0, 2);

    public bool RandomFishBool() => Random.Range(0, 2) == 0;

    public bool IsUnderWater(bool isBlack)
    {
        Transform fish = isBlack ? blackFish : whiteFish;
        return fish.position.y < waterLevel.position.y - 7;
    }

    #endregion Utility & Interaction

    private void DebugPrintActionList()
    {
        if (actionList == null || actionList.Count == 0)
        {
            Debug.Log("[YingYangFish_AI] actionList is empty");
            return;
        }

        var sb = new StringBuilder();
        sb.AppendLine("[YingYangFish_AI] Selected actionList:");
        for (int phase = 0; phase < actionList.Count; phase++)
        {
            var group = actionList[phase];
            sb.AppendFormat(" Phase {0} (count={1}):", phase, group?.Count ?? 0).AppendLine();
            if (group == null) continue;

            for (int i = 0; i < group.Count; i++)
            {
                var ac = group[i];
                string actionName = ac?.action != null ? ac.action.GetType().Name : "null";
                sb.AppendFormat("  - [{0},{1}] Action: {2}, factor: {3}, delay: {4}", phase, i, actionName, ac?.factor ?? 0f, ac?.delay ?? 0f).AppendLine();
            }
        }

        Debug.Log(sb.ToString());
    }

    public Vector3 CreateWaterLevelYAxis(Vector3 position)
    {
        return new Vector3(position.x, waterLevel.position.y, 0);
    }

    public Vector3 CreateWaterLevelYAxis(float x)
    {
        return new Vector3(x, waterLevel.position.y, 0);
    }

    public Vector3 CreateWaterLevelYAxis(Transform target)
    {
        return new Vector3(target.position.x, waterLevel.position.y, 0);
    }
}

public enum YYFActionType
{
    WaterSpear,
    Gatling,
    BubbleTrap,
    Swing,
    Splash
}

public enum YYFFishTarget
{
    Black = 0,
    White = 1,
    Both = 2,
    Random = 3,
    Opposite = 4
}

public class YYFActionCaller : ActionCaller
{
    public YYFActionCaller(IEnemyAction _action, int _factor = 0, float _delay = 0) : base(_action, _factor, _delay)
    { }

    public YYFActionCaller(IEnemyAction _action, bool isBlack, float _delay = 0)
     : base(_action, isBlack ? 0f : 1f, _delay) { }
}

[System.Serializable]
public class YYFActionEntry
{
    [HorizontalGroup("Row", Width = 15)]
    [HideLabel]
    public bool UseIndex = false;

    [HorizontalGroup("Row", Width = 30)]
    [ShowIf(nameof(UseIndex))]
    [HideLabel]
    public int Index;

    [HorizontalGroup("Row", Width = 230)]
    [HideLabel] public YYFActionType actionType;

    // Add any YYFActionType values here that should expose raw factor instead of fishTarget
    private bool UseFactorMode => actionType == YYFActionType.Gatling || actionType == YYFActionType.WaterSpear;

    [HorizontalGroup("Row", Width = 400)]
    [HideLabel]
    [HideIf(nameof(UseFactorMode))]
    public YYFFishTarget fishTarget;

    [HorizontalGroup("Row", Width = 400)]
    [HideLabel]
    [ShowIf(nameof(UseFactorMode))]
    public int factor = 0;

    [HorizontalGroup("Row")]
    [HideLabel]
    [PropertyRange(0, 10)]
    public float delay = 0f;

    public YYFActionCaller ToActionCaller(YingYangFish_AI ai, ref int lastResolvedFish)
    {
        IEnemyAction action = actionType switch
        {
            YYFActionType.WaterSpear => ai.waterSpear,
            YYFActionType.Swing => ai.swing,
            YYFActionType.BubbleTrap => ai.bubbleTrap,
            YYFActionType.Gatling => ai.gatling,
            YYFActionType.Splash => ai.splash,
            _ => null
        };
        int resolvedFactor;

        switch (fishTarget)
        {
            case YYFFishTarget.Random:
                resolvedFactor = ai.RandomFish();
                lastResolvedFish = resolvedFactor;
                break;

            case YYFFishTarget.Opposite:
                resolvedFactor = lastResolvedFish == 0 ? 1 : 0;
                lastResolvedFish = resolvedFactor;
                break;

            default:
                resolvedFactor = UseFactorMode ? factor : (int)fishTarget;
                lastResolvedFish = resolvedFactor;
                break;
        }

        return new YYFActionCaller(action, resolvedFactor, delay);
    }
}

[System.Serializable]
public class YYFActionPhase
{
    public List<YYFActionEntry> actionCombo = new List<YYFActionEntry>();
}