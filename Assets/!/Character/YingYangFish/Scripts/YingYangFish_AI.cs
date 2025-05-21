using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EditorAttributes;
using Unity.VisualScripting;
using JetBrains.Annotations;
using DG.Tweening;
using System;
using Void = EditorAttributes.Void;
using Random = UnityEngine.Random;
using static UnityEditor.PlayerSettings;

public class YingYangFish_AI : IEnemyController
{
    #region ATTRIBUTES

    [FoldoutGroup("Attributes", nameof(center), nameof(blackFish), nameof(blackFishGFX),
        nameof(whiteFish), nameof(whiteFishGFX), nameof(blackOrigin), nameof(whiteOrigin),
        nameof(idleRotateSpeed), nameof(sprintRotateSpeed), nameof(waterSpearPos_black1),
        nameof(waterSpearPos_black2), nameof(waterSpearPos_white1), nameof(waterSpearPos_white2), nameof(Dir),
        nameof(swimToCenterSpeed), nameof(minMaxDistanceTocenter), nameof(waterLevel),
        nameof(EventInteract), nameof(ultimateWave), nameof(waterDragon))
        ]
    public Void void2;

    [SerializeField, HideInInspector] public Transform YingYangFish;
    [SerializeField, HideInInspector] public Transform center;
    [SerializeField, HideInInspector] public Transform blackFish;
    [SerializeField, HideInInspector] public Transform whiteFish;
    [SerializeField, HideInInspector] public Transform blackFishGFX;
    [SerializeField, HideInInspector] public Transform whiteFishGFX;
    [SerializeField, HideInInspector] public Transform blackOrigin;
    [SerializeField, HideInInspector] public Transform whiteOrigin;
    [SerializeField, HideInInspector] public float idleRotateSpeed;
    [SerializeField, HideInInspector] public float sprintRotateSpeed;
    [SerializeField, HideInInspector] public Transform waterSpearPos_black1;
    [SerializeField, HideInInspector] public Transform waterSpearPos_black2;
    [SerializeField, HideInInspector] public Transform waterSpearPos_white1;
    [SerializeField, HideInInspector] public Transform waterSpearPos_white2;
    [HideInInspector] public float black_rotateSpeed;
    [HideInInspector] public float white_rotateSpeed;
    private float black_targetRotateSpeed;
    private float white_targetRotateSpeed;
    [SerializeField, HideInInspector] public Vector3 Dir;
    [SerializeField, HideInInspector] public float swimToCenterSpeed = 2f;
    [SerializeField, HideInInspector, MinMaxSlider(1f, 3f)] public Vector2 minMaxDistanceTocenter;
    [SerializeField, HideInInspector] public Transform waterLevel;
    [SerializeField, HideInInspector] public GameObject EventInteract;
    [SerializeField, HideInInspector] public GameObject ultimateWave;
    [SerializeField, HideInInspector] public GameObject waterDragon;
    public List<Transform> ultimate_bullets;

    [HideInInspector] public SpriteRenderer blackSprite;
    [HideInInspector] public SpriteRenderer whiteSprite;
    [HideInInspector] public Animator blackAnim;
    [HideInInspector] public Animator whiteAnim;
    [HideInInspector] public Animator centerAnim;
    [HideInInspector] public bool closerFish_Black;

    #endregion ATTRIBUTES

    #region DEBUG

    [FoldoutGroup("Debug", nameof(white_distanceToCenter),
        nameof(black_distanceToCenter), nameof(movingTarget), nameof(isCloseSwimming), nameof(secondPhase))]
    public Void void3;

    //[SerializeField, HideInInspector] public bool black_idling = true;
    //[SerializeField, HideInInspector] public bool white_idling = true;
    [SerializeField, HideInInspector] public float white_distanceToCenter = 0f;

    [SerializeField, HideInInspector] public float black_distanceToCenter = 0f;
    [SerializeField, HideInInspector] public Transform movingTarget;
    [SerializeField, HideInInspector] public bool isCloseSwimming;
    [SerializeField, HideInInspector] public bool secondPhase;

    #endregion DEBUG

    #region ACTIONS

    public bool Actions;
    [ShowField(nameof(Actions))][SerializeField, ButtonField("ForceDie", "ForceDie")] public Transform void112;
    [ShowField(nameof(Actions))][SerializeField, ButtonField("StartAction", "StartAction")] public Transform void11;
    [ShowField(nameof(Actions))][SerializeField, ButtonField("WaterSpear", "WaterSpear")] public Void void1;
    [ShowField(nameof(Actions))][SerializeField, ButtonField("Swing", "Swing")] public Void void8;
    [ShowField(nameof(Actions))][SerializeField, ButtonField("Splash_white", "Splash_white")] public Void void9;
    [ShowField(nameof(Actions))][SerializeField, ButtonField("Splash_black", "Splash_black")] public Void void13;
    [ShowField(nameof(Actions))][SerializeField, ButtonField("Dive", "Dive")] public Void void10;
    [ShowField(nameof(Actions))][SerializeField] public YYF_WaterSpear waterSpear;
    [ShowField(nameof(Actions))][SerializeField] public YYF_Swing swing;
    [ShowField(nameof(Actions))][SerializeField] public YYF_splash_white splash_white;
    [ShowField(nameof(Actions))][SerializeField] public YYF_splash_black splash_black;
    [ShowField(nameof(Actions))][SerializeField] public YYF_Dive dive;

    #endregion ACTIONS

    #region COROUTINES

    [HideInInspector] public Coroutine co_sprintStartPoint;
    [HideInInspector] public Coroutine co_IEcloseSwim;
    [HideInInspector] public Coroutine co_sprintBackEqual;

    #endregion COROUTINES

    // Update is called once per frame

    public override void Start()
    {
        base.Start();
        blackSprite = blackFish.GetComponent<SpriteRenderer>();
        whiteSprite = whiteFish.GetComponent<SpriteRenderer>();
        blackAnim = blackFishGFX.GetComponent<Animator>();
        whiteAnim = whiteFishGFX.GetComponent<Animator>();
        centerAnim = center.GetComponent<Animator>();
        movingTarget = player;
        EventInteract.SetActive(true);
        HealthUI.SetActive(false);

        white_rotateSpeed = idleRotateSpeed;
        SetWhiteTargetRotateSpeed(idleRotateSpeed);
        black_rotateSpeed = idleRotateSpeed;
        SetBlackTargetRotateSpeed(idleRotateSpeed);
    }

    private float white_rotateVelocity;
    private float black_rotateVelocity;

    private void Update()
    {
        distanceToPlayer = Mathf.Abs(transform.position.x - player.position.x);
        white_distanceToCenter = whiteFish.localPosition.magnitude;
        black_distanceToCenter = blackFish.localPosition.magnitude;

        float white_temp = (minMaxDistanceTocenter.y / white_distanceToCenter) * white_targetRotateSpeed;
        float black_temp = (minMaxDistanceTocenter.y / black_distanceToCenter) * black_targetRotateSpeed;

        white_rotateSpeed = Mathf.SmoothDamp(white_rotateSpeed, white_temp, ref white_rotateVelocity, 0.2f);
        black_rotateSpeed = Mathf.SmoothDamp(black_rotateSpeed, black_temp, ref black_rotateVelocity, 0.2f);

        blackOrigin.Rotate(new Vector3(0, 0, -1), black_rotateSpeed * Time.deltaTime);
        whiteOrigin.Rotate(new Vector3(0, 0, -1), white_rotateSpeed * Time.deltaTime);

        if (!isActing && actionList.Count > 0)
        {
            isActing = true;
            co_act = StartCoroutine(Act());
        }
    }

    #region Main Functions

    public IEnumerator SprintStartPoint(string fish = "null")
    {
        Transform closerFish = CheckCloserFish();
        if (fish == "white") { closerFish = whiteFish; }
        else if (fish == "black") { closerFish = blackFish; }
        bool finished = false;
        if (closerFish == blackFish)
        {
            closerFish_Black = true;
            SetBlackTargetRotateSpeed(0);
            blackAnim.Play("sprint");
            blackOrigin.DORotate(Vector3.zero, sprintRotateSpeed, RotateMode.FastBeyond360).SetSpeedBased(true).OnComplete(() => { finished = true; });
        }
        else
        {
            closerFish_Black = false;
            SetWhiteTargetRotateSpeed(0);
            whiteAnim.Play("sprint");
            whiteOrigin.DORotate(Vector3.zero, sprintRotateSpeed, RotateMode.FastBeyond360).SetSpeedBased(true).OnComplete(() => { finished = true; });
        }
        while (!finished) { yield return null; }
        yield return null;
    }

    /// <summary>
    /// designated fish runs faster to get back to equal position
    /// </summary>
    /// <param name="isBlack"></param>
    public IEnumerator SprintBackEqual()
    {
        float closerFish = Vector2.SignedAngle(blackFish.right, whiteFish.right);

        if (closerFish > 0 && closerFish <= 180)
        {
            SetBlackTargetRotateSpeed(sprintRotateSpeed);
            if (white_targetRotateSpeed == sprintRotateSpeed) { SetBlackTargetRotateSpeed(white_targetRotateSpeed * 2); }

            blackAnim.Play("sprint");
            float angle = Vector2.Angle(blackFish.right, whiteFish.right);
            while (angle > 180 || angle < 170)// reached start point
            {
                angle = Vector2.Angle(blackFish.right, whiteFish.right); yield return null;
            }
            SetBlackTargetRotateSpeed(idleRotateSpeed);
            black_rotateSpeed = idleRotateSpeed;
        }
        else
        {
            SetWhiteTargetRotateSpeed(sprintRotateSpeed);
            if (black_targetRotateSpeed == sprintRotateSpeed) { SetWhiteTargetRotateSpeed(black_targetRotateSpeed * 2); }
            whiteAnim.Play("sprint");
            float angle = Vector2.Angle(blackFish.right, whiteFish.right);
            while (angle > 180 || angle < 170)// reached start point
            {
                angle = Vector2.Angle(blackFish.right, whiteFish.right); yield return null;
            }
            SetWhiteTargetRotateSpeed(idleRotateSpeed);
            white_rotateSpeed = idleRotateSpeed;
        }
        yield return null;
    }

    public IEnumerator SprintSamePos()
    {
        float closerFish = Vector2.SignedAngle(blackFish.right, whiteFish.right);

        if (closerFish > 0 && closerFish <= 180)
        {
            SetWhiteTargetRotateSpeed(sprintRotateSpeed);
            if (black_targetRotateSpeed == sprintRotateSpeed) { SetWhiteTargetRotateSpeed(black_targetRotateSpeed * 2); }
            whiteAnim.Play("sprint");
            float angle = Vector2.Angle(blackFish.right, whiteFish.right);
            while (angle > 5)// reached start point
            {
                angle = Vector2.Angle(blackFish.right, whiteFish.right); yield return null;
            }
            SetWhiteTargetRotateSpeed(sprintRotateSpeed);
        }
        else
        {
            SetBlackTargetRotateSpeed(sprintRotateSpeed);
            if (white_targetRotateSpeed == sprintRotateSpeed) { SetBlackTargetRotateSpeed(white_targetRotateSpeed * 2); }

            blackAnim.Play("sprint");
            float angle = Vector2.Angle(blackFish.right, whiteFish.right);
            while (angle > 5)// reached start point
            {
                angle = Vector2.Angle(blackFish.right, whiteFish.right); yield return null;
            }
            SetBlackTargetRotateSpeed(sprintRotateSpeed);
        }
        yield return null;
    }

    public IEnumerator IECloseSwim(bool close, bool playAnim = true)
    {
        SetWhiteTargetRotateSpeed(sprintRotateSpeed);
        SetBlackTargetRotateSpeed(sprintRotateSpeed);

        if (close)
        {
            if (playAnim)
            {
                whiteAnim.Play("close_swim_pre");
                blackAnim.Play("close_swim_pre");
            }

            while (white_distanceToCenter > minMaxDistanceTocenter.x)
            {
                whiteFish.position -= whiteFish.up * swimToCenterSpeed * Time.deltaTime;
                blackFish.position -= blackFish.up * swimToCenterSpeed * Time.deltaTime;
                yield return null;
            }
        }
        else
        {
            blackAnim.Play("sprint"); whiteAnim.Play("sprint");

            while (white_distanceToCenter < minMaxDistanceTocenter.y)
            {
                whiteFish.position += whiteFish.up * swimToCenterSpeed * Time.deltaTime;
                blackFish.position += blackFish.up * swimToCenterSpeed * Time.deltaTime;
                yield return null;
            }
        }
    }

    public IEnumerator IE_SwimAway(Vector3 end, float centerOffset, bool isBlack)
    {
        if (isBlack) { blackPositioned = false; SetBlackTargetRotateSpeed(sprintRotateSpeed); }
        else { whitePositioned = false; SetWhiteTargetRotateSpeed(sprintRotateSpeed); }

        Transform origin = whiteOrigin;
        if (isBlack) { origin = blackOrigin; }

        float angle = Vector2.Angle(origin.right, (end - origin.position).normalized) + 60;
        while (58 > angle || angle > 62)// reached start point
        {
            angle = Vector2.Angle(origin.right, (end - origin.position).normalized) + 40;
            yield return null;
        }

        if (isBlack) { black_targetRotateSpeed = 0; }
        else { white_targetRotateSpeed = 0; }

        float elapsedTime = 0;
        Vector3 start = origin.position;
        Vector3 centerPoint = (start + end) * 0.5f;
        if (isBlack) { centerPoint -= blackFish.up * centerOffset; }
        else { centerPoint -= whiteFish.up * centerOffset; }
        Vector3 startRelCenter = start - centerPoint;
        Vector3 endRelCenter = end - centerPoint;
        float duration = Vector3.Distance(origin.position, end) / 10;
        Vector3 startAngle = origin.eulerAngles;
        Vector3 endAngle = origin.eulerAngles - new Vector3(0, 0, 100);

        while (elapsedTime <= duration)
        {
            origin.eulerAngles = Vector3.Lerp(startAngle, endAngle, elapsedTime / duration);
            origin.transform.position = Vector3.Slerp(startRelCenter, endRelCenter, elapsedTime / duration) + centerPoint;

            elapsedTime += Time.deltaTime;
            if (elapsedTime / duration >= 0.9)
            {
                if (isBlack) { SetBlackTargetRotateSpeed(sprintRotateSpeed); }
                else { SetWhiteTargetRotateSpeed(sprintRotateSpeed); }
            }
            yield return null;
        }
        if (isBlack) { blackPositioned = true; }
        else { whitePositioned = true; }

        yield return null;
    }

    public Transform CheckCloserFish()
    {
        if (blackFish.eulerAngles.z < whiteFish.eulerAngles.z) { return blackFish; }
        else { return whiteFish; }
    }

    public void CancelAllAction()
    {
        if (co_IEcloseSwim != null) TryStopCoroutine(co_IEcloseSwim);
        if (co_sprintBackEqual != null) TryStopCoroutine(co_sprintBackEqual);
        if (co_sprintStartPoint != null) TryStopCoroutine(co_sprintStartPoint);

        TryStopCoroutine(co_act);
        waterSpear.CancelAct();
        swing.CancelAct();
        splash_white.CancelAct();
        splash_black.CancelAct();
        dive.CancelAct();
        SetNormalRotateSpeed();
    }

    [HideInInspector] public bool blackPositioned = false;
    [HideInInspector] public bool whitePositioned = false;
    [HideInInspector] public bool finishedWaterSpearUltimate = false;

    public IEnumerator Pre_SecondPhase()
    {
        if (!secondPhase)
        {
            actionList.Clear();
            CancelAllAction();
            CharacterUIManager.ShowBlackEdge(true);
            yield return co_sprintBackEqual = StartCoroutine(SprintBackEqual());
            yield return co_IEcloseSwim = StartCoroutine(IECloseSwim(true));
            SetWhiteTargetRotateSpeed(idleRotateSpeed / 3);
            SetBlackTargetRotateSpeed(idleRotateSpeed / 3);
            whiteAnim.Play("close_swim");
            blackAnim.Play("close_swim");

            //yield return StartCoroutine(ChangeYPos(-6));
            //transform.DOMove(new Vector3(GetCenterXOfMap(), waterLevel.position.y - 6, 0), 1.8f);
            //yield return new WaitForSeconds(1.8f);
            yield return StartCoroutine(ChangeYPos(2));

            centerAnim.Play("center_break");
            SoundManager.PlaySound("glass_break");
            secondPhase = true;
            EventInteract.SetActive(true);
        }
    }

    public IEnumerator secondPhaseAnim()
    {
        InputMaster.instance.DisableAllActions();
        GameManager.instance.isInPerformingState = true;
        yield return StartCoroutine(playerController.RunToPosition(transform.position - new Vector3(2, 0, 0)));
        playerController.FaceTarget(this.transform);
        yield return new WaitForSeconds(0.5f);
        sprintRotateSpeed *= 1.5f;
        idleRotateSpeed *= 1.5f;
        EventInteract.SetActive(false);

        InputMaster.instance._attackLeftAction.Enable();
        InputMaster.instance._attackRightAction.Enable();
        InputMaster.instance._defendAction.Enable();
        InputMaster.instance._attackDirectionAction.Enable();
        CharacterController2D.instance.FaceTarget(this.transform);
        //StartCoroutine(ChangeYPos(4.5f));
        centerAnim.Play("center_fade");

        StartCoroutine(EmojiDuringCircling());
        //yield return StartCoroutine(Circling(3.5f));

        StartCoroutine(Ultimate());

        yield return null;
    }

    public IEnumerator Ultimate()
    {
        //swing qte
        //yield return swing.act_routine = StartCoroutine(swing.Act_coroutine(1));
        //yield return dive.act_routine = StartCoroutine(dive.Act_coroutine(1));

        yield return co_IEcloseSwim = StartCoroutine(IECloseSwim(false));
        //SetNormalRotateSpeed();

        //// splash four times
        //yield return splash_black.act_routine = StartCoroutine(splash_black.Act_coroutine(1));
        //yield return splash_white.act_routine = StartCoroutine(splash_white.Act_coroutine(1));
        //yield return splash_black.act_routine = StartCoroutine(splash_black.Act_coroutine(1));
        //yield return splash_white.act_routine = StartCoroutine(splash_white.Act_coroutine(1));

        ////water spear ultimate
        //yield return co_sprintBackEqual = StartCoroutine(SprintBackEqual());
        //StartCoroutine(IE_SwimAway(waterSpearPos_black1.position, 10, true));
        //StartCoroutine(IE_SwimAway(waterSpearPos_white1.position, 10, false));
        //while (!blackPositioned || !whitePositioned) { yield return null; }
        //yield return StartCoroutine(SprintSamePos());
        //StartCoroutine(waterSpear.Act_coroutine(1));
        //StartCoroutine(waterSpear.Act_coroutine(2));
        //while (!finishedWaterSpearUltimate) { yield return null; }

        ////swing ultimate
        //movingTarget = player;
        //yield return dive.act_routine = StartCoroutine(dive.Act_coroutine());
        //yield return swing.act_routine = StartCoroutine(swing.Act_coroutine(2));
        movingTarget = GetBoundaryFarOfPlayer();
        yield return dive.act_routine = StartCoroutine(dive.Act_coroutine());
        yield return co_sprintBackEqual = StartCoroutine(SprintBackEqual());

        yield return new WaitForSeconds(1f);
        //water ball ultimate
        black_targetRotateSpeed = idleRotateSpeed / 3;
        white_targetRotateSpeed = idleRotateSpeed / 3;
        StartCoroutine(Circling(30f));
        StartCoroutine(ChangeYPos(4));
        yield return new WaitForSeconds(1f);
        ultimateWave.SetActive(true);
        ultimateWave.transform.position = new Vector3(transform.position.x, waterLevel.position.y, 0);

        yield return new WaitForSeconds(2f);
        float[] bulletDelays = { 0f, 0.5f, 0.7f, 1.2f, 1.7f, 1.9f, 3f, 3.2f, 3.4f, 3.6f, 3.8f, 4f };
        for (int i = 0; i < bulletDelays.Length; i++)
        {
            StartCoroutine(SpawnUltimateBullet(i, bulletDelays[i]));
        }
        yield return new WaitForSeconds(6f);

        waterDragon.transform.position = new Vector3(transform.position.x, waterLevel.position.y, 0);
        waterDragon.SetActive(true);

        yield return StartCoroutine(playerController.RunToPosition(transform.position - new Vector3(14f, 0, 0)));

        playerController.rb.velocity = Vector3.zero;
        InputMaster.instance.StartMustSuccessQTE(InputKeyType.swordTeleport_key, player.transform.position + new Vector3(0, 4, 0),
            0.4f, () => { playerController.DesignatedPositionTeleport(player.transform.position + new Vector3(5, 7.5f, 0)); });

        yield return new WaitUntil(() => !InputMaster.instance.isQTE);
        playerController.EnableGravity(false);
        playerController.rb.velocity = Vector3.zero;
        yield return new WaitForSeconds(0.5f);

        //slash animation pr

        InputMaster.instance.StartMustSuccessQTE(InputKeyType.right_attack_key, player.transform.position + new Vector3(2, 2, 0),
           0.4f, () =>
           {
               print("Slash");
           });

        yield return new WaitUntil(() => !InputMaster.instance.isQTE);
    }

    private IEnumerator SpawnUltimateBullet(int index, float delay)
    {
        yield return new WaitForSeconds(delay);
        IProjectile bullet = selfPooler.SpawnFromPool("ult_bullet", ultimate_bullets[index].position, false).GetComponent<IProjectile>();
        yield return new WaitForSeconds(1.4f);
        bullet.SetUp(transform.right, this.gameObject, 0, _followTarget: false, _target: Health.instance, true, 2, 500);
        GameObject effect = selfPooler.SpawnFromPool("ultimate_bullet_effect", ultimate_bullets[index].position, false);
        effect.transform.eulerAngles = bullet.transform.eulerAngles;
        yield return new WaitForSeconds(2f);
        effect.SetActive(false);
        yield return null;
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

    #endregion Main Functions

    #region Overrides

    public override IEnumerator Act()
    {
        if (secondPhase) { yield return null; }
        else
        {
            while (actionList.Count > 0 && actionList[0] != null)
            {
                IEnemyAction action = actionList[0];
                yield return action.act_routine = StartCoroutine(action.Act_coroutine());
                yield return null;
            }
            isActing = false;
            if (!secondPhase && currentActionBreakAmount >= maxActionBreakCapacity) { yield return StartCoroutine(Break()); }//break
            else { StartAction(); }//startover
            yield return null;
        }
    }

    public override void StartAction()
    {
        if (secondPhase) { return; }
        List<IEnemyAction> possibleActions = new List<IEnemyAction>();
        if (playerEnergy.currentEnergy <= 5)
        {
            possibleActions.Add(waterSpear);
        }
        if (distanceToPlayer >= swing.swingRange - 1)
        {
            if (distanceToPlayer <= 12)
            {
                float i = Random.Range(0, 10);
                if (i < 3) { possibleActions.Add(waterSpear); }
                else if (i < 6) { possibleActions.Add(splash_white); }
                else if (i < 10) { possibleActions.Add(dive); }//moving
            }
            else
            {
                float i = Random.Range(0, 10);
                if (i < 5) { possibleActions.Add(waterSpear); }
                else if (i < 10) { possibleActions.Add(dive); }//moving
            }
        }
        else
        {
            possibleActions.Add(swing);
        }

        if (!playerController.isGrounded)
        {
            possibleActions.Add(splash_black);
        }

        int index = Random.Range(0, possibleActions.Count);
        initialAction = possibleActions[index];

        actionList.Add(initialAction);
        if (initialAction == dive)
        {
            if ((float)playerEnergy.currentEnergy / (float)playerEnergy.maxEnergy <= 0.7f || playerAttack.currentHS_point < 2)
            {
                InsertAction(swing);
                movingTarget = player;
            }
            else
            {
                movingTarget = GetFarTargetOutOfTwo(player, GetBoundaryFarOfPlayer());
                if (Possibility(50)) { InsertAction(waterSpear); }
                else { InsertAction(splash_white); InsertAction(waterSpear); }
            }
        }
    }

    public override IEnumerator IE_Activate()
    {
        IN_COMBAT = true;
        //play start animation
        co_sprintBackEqual = StartCoroutine(SprintBackEqual());
        yield return co_IEcloseSwim = StartCoroutine(IECloseSwim(false));
        HealthUI.SetActive(true);
        StartAction();
        //start action loops
    }

    public override int Damage(int damageAmount, Transform sender, float stunDuration = 0)
    {
        if (DEAD) { return 0; }

        flash.OnDamageFlash();
        currentHealth -= damageAmount;
        healthBar.fillAmount = (float)currentHealth / (float)maxHealth;

        if (currentHealth <= 0)
        {
            StartCoroutine(Pre_SecondPhase());
        }//death

        return 0;
    }

    public override int SubObjectDamage(int damageAmount, Transform sender = null, float stunDuration = 0)
    {
        if (DEAD) { return 0; }

        currentHealth -= damageAmount;
        healthBar.fillAmount = (float)currentHealth / (float)maxHealth;

        if (currentHealth <= 0)
        {
            StartCoroutine(Pre_SecondPhase());
        }//death

        return 0;
    }

    #endregion Overrides

    #region Small Functions

    public Quaternion CalculateWantedRotation(Vector3 startPos, Vector3 _targetPos)
    {
        float angle = Mathf.Atan2(_targetPos.y - startPos.y, _targetPos.x - startPos.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(new Vector3(0, 0, angle));
        return targetRotation;
    }

    public void Interact()
    {
        //play start animation
        //start fight
        if (!secondPhase)
        {
            StartCoroutine(IE_Activate());
            EventInteract.SetActive(false);
        }
        else
        {
            //start second phase

            StartCoroutine(secondPhaseAnim());
        }
    }

    public void WaterSpear()
    {
        InsertAction(waterSpear);
    }

    public override void ForceDie()
    {
        base.ForceDie();
    }

    public void Swing()
    {
        InsertAction(swing);
    }

    public void Splash_black()
    {
        InsertAction(splash_black);
    }

    public void Splash_white()
    {
        InsertAction(splash_white);
    }

    public void Dive()
    {
        InsertAction(dive);
    }

    public void SetNormalRotateSpeed()
    {
        black_targetRotateSpeed = idleRotateSpeed;
        white_targetRotateSpeed = idleRotateSpeed;
    }

    public void StopRotate()
    {
        black_targetRotateSpeed = 0;
        white_targetRotateSpeed = 0;
    }

    public void SetBlackTargetRotateSpeed(float speed)
    {
        black_targetRotateSpeed = speed;
    }

    public void SetWhiteTargetRotateSpeed(float speed)
    {
        white_targetRotateSpeed = speed;
    }

    #endregion Small Functions
}