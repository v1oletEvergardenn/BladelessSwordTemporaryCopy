using DG.Tweening;
using EditorAttributes;
using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.PlayerSettings;
using static UnityEngine.UI.Image;
using Random = UnityEngine.Random;
using Void = EditorAttributes.Void;

public class YingYangFish_AI : IEnemyController
{
    #region COROUTINES

    [HideInInspector] public Coroutine co_sprintStartPoint;
    [HideInInspector] public Coroutine co_IEcloseSwim;
    [HideInInspector] public Coroutine co_sprintBackEqual;
    [HideInInspector] public Coroutine co_sprintToAngle;
    [HideInInspector] public Coroutine co_singleFishDive;
    [HideInInspector] public Coroutine co_singleReturnToCenter;
    [HideInInspector] public Coroutine co_singleJumpToPos;

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

    [FoldoutGroup("Fish References", nameof(center), nameof(blackFish), nameof(whiteFish),
        nameof(blackFishGFX), nameof(whiteFishGFX), nameof(blackOrigin), nameof(whiteOrigin))]
    public Void fishRefsGroup;

    [SerializeField, HideInInspector] public Transform center;
    [SerializeField, HideInInspector] public Transform blackFish;
    [SerializeField, HideInInspector] public Transform whiteFish;
    [SerializeField, HideInInspector] public Transform blackFishGFX;
    [SerializeField, HideInInspector] public Transform whiteFishGFX;
    [SerializeField, HideInInspector] public Transform blackOrigin;
    [SerializeField, HideInInspector] public Transform whiteOrigin;

    #endregion FISH REFERENCES

    #region ROTATION & MOVEMENT

    [FoldoutGroup("Rotation & Movement", nameof(idleRotateSpeed), nameof(sprintRotateSpeed),
        nameof(swimToCenterSpeed), nameof(minMaxDistanceTocenter))]
    public Void rotationMoveGroup;

    [SerializeField, HideInInspector] public float idleRotateSpeed;
    [SerializeField, HideInInspector] public float sprintRotateSpeed;
    [SerializeField, HideInInspector] public float swimToCenterSpeed = 2f;
    [SerializeField, HideInInspector, MinMaxSlider(1f, 3f)] public Vector2 minMaxDistanceTocenter;

    [HideInInspector] public float white_distanceToCenter = 0f;
    [HideInInspector] public float black_distanceToCenter = 0f;
    [HideInInspector] public Transform movingTarget;

    [HideInInspector] public bool isCloseSwimming;
    [HideInInspector] public float black_rotateSpeed;
    [HideInInspector] public float white_rotateSpeed;
    [HideInInspector] public float black_targetRotateSpeed;
    [HideInInspector] public float white_targetRotateSpeed;

    public bool isBlackBusy { get; private set; } = false;

    public bool isWhiteBusy { get; private set; } = false;

    #endregion ROTATION & MOVEMENT

    #region GENERAL REFERENCES

    [FoldoutGroup("General References", nameof(waterLevel), nameof(endCanvas),
        nameof(EventInteract), nameof(black_particle), nameof(white_particle),
       nameof(black_tex), nameof(white_tex))]
    public Void refenereceGroup;

    [SerializeField, HideInInspector] public Transform endCanvas;
    [SerializeField, HideInInspector] public Transform waterLevel;
    [SerializeField, HideInInspector] public GameObject EventInteract;
    [SerializeField, HideInInspector] public GameObject black_particle;
    [SerializeField, HideInInspector] public GameObject white_particle;
    [SerializeField, HideInInspector] public Sprite black_tex;
    [SerializeField, HideInInspector] public Sprite white_tex;

    #endregion GENERAL REFERENCES

    #region ULTIMATE REFERENCES

    [FoldoutGroup("Ultimate", nameof(ultimateWave), nameof(slash_effect),
        nameof(waterDragon1),
        nameof(waterSpearPos_black1), nameof(waterSpearPos_black2),
        nameof(waterSpearPos_white1), nameof(waterSpearPos_white2))]
    public Void voidWaterDragon;

    [SerializeField, HideInInspector] public GameObject ultimateWave;
    [SerializeField, HideInInspector] public GameObject slash_effect;
    [SerializeField, HideInInspector] public Transform waterDragon1;
    [SerializeField, HideInInspector] public Transform waterSpearPos_black1;
    [SerializeField, HideInInspector] public Transform waterSpearPos_black2;
    [SerializeField, HideInInspector] public Transform waterSpearPos_white1;
    [SerializeField, HideInInspector] public Transform waterSpearPos_white2;

    [HideInInspector] public bool secondPhase;
    [HideInInspector] public bool blackPositioned = false;
    [HideInInspector] public bool whitePositioned = false;
    [HideInInspector] public bool finishedWaterSpearUltimate = false;
    public List<Transform> ultimate_bullets;

    #endregion ULTIMATE REFERENCES

    #region Action Fields

    [FoldoutGroup("Action References", nameof(waterSpear), nameof(swing),
        nameof(singleSwing), nameof(bubbleTrap), nameof(dive),
        nameof(gatling), nameof(splash))]
    public Void actionRefsGroup;

    [SerializeField, HideInInspector] public YYF_WaterSpear waterSpear;
    [SerializeField, HideInInspector] public YYF_Swing swing;
    [SerializeField, HideInInspector] public YYF_SingleSwing singleSwing;
    [SerializeField, HideInInspector] public YYF_BubbleTrap bubbleTrap;
    [SerializeField, HideInInspector] public YYF_Dive dive;
    [SerializeField, HideInInspector] public YYF_Gatling gatling;
    [SerializeField, HideInInspector] public YYF_Splash splash;

    public bool Actions;
    [ShowField(nameof(Actions))][SerializeField, ButtonField("ForceDie", "ForceDie")] public Transform void112;
    [ShowField(nameof(Actions))][SerializeField, ButtonField("ForceStun", "ForceStun")] public Transform void114;
    [ShowField(nameof(Actions))][SerializeField, ButtonField("StartAction", "StartAction")] public Transform void11;
    [ShowField(nameof(Actions))][SerializeField, ButtonField("WaterSpear", "WaterSpear")] public Void void1;
    [ShowField(nameof(Actions))][SerializeField, ButtonField("Swing", "Swing")] public Void void8;
    [ShowField(nameof(Actions))][SerializeField, ButtonField("Splash", "Splash")] public Void void9;
    [ShowField(nameof(Actions))][SerializeField, ButtonField("Dive", "Dive")] public Void void10;
    [ShowField(nameof(Actions))][SerializeField, ButtonField("SingleSwing", "SingleSwing")] public Void voidSingleswing;
    [ShowField(nameof(Actions))][SerializeField, ButtonField("BubbleTrap", "BubbleTrap")] public Void voidbubble;
    [ShowField(nameof(Actions))][SerializeField, ButtonField("Gatling", "Gatling")] public Void voidgatling;

    #endregion Action Fields

    #region Unity Lifecycle

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

        white_rotateSpeed = idleRotateSpeed;
        SetWhiteTargetRotateSpeed(idleRotateSpeed);
        black_rotateSpeed = idleRotateSpeed;
        SetBlackTargetRotateSpeed(idleRotateSpeed);

        isCloseSwimming = true;
    }

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

    #endregion Unity Lifecycle

    #region Main AI Coroutines

    /// <summary>
    /// check closest fish and rotate it to the start point.
    /// </summary>
    /// <param name="assignedFish"> designate a specific fish to swim to start point</param>
    /// <returns></returns>
    public IEnumerator IESprintStartPoint(string assignedFish = "null")
    {
        Transform closerFish = null;
        while (closerFish == null) { closerFish = CheckCloserFish(); yield return null; }
        if (assignedFish == "white") { closerFish = whiteFish; }
        else if (assignedFish == "black") { closerFish = blackFish; }
        closerFish_Black = closerFish == blackFish ? true : false;
        bool isBlack = closerFish == blackFish ? true : false;
        yield return co_sprintToAngle = StartCoroutine(IESprintToAngle(isBlack, 0));
    }

    public IEnumerator IESprintToAngle(bool isBlack, float angle)
    {
        bool finished = false;

        if (isBlack) { SetBlackTargetRotateSpeed(0); } else { SetWhiteTargetRotateSpeed(0); }
        if (isBlack) { blackAnim.Play("sprint"); } else { whiteAnim.Play("sprint"); }
        Transform origin = isBlack ? blackOrigin : whiteOrigin;
        float currentZ = origin.eulerAngles.z;

        float delta = ((angle - currentZ - 360f) % 360f);

        origin.DORotate(new Vector3(0, 0, delta), sprintRotateSpeed * 2, RotateMode.WorldAxisAdd)
           .SetSpeedBased(true)
           .SetEase(Ease.Linear)
           .OnComplete(() => { finished = true; });

        yield return new WaitUntil(() => finished);
    }

    public IEnumerator IESprintBackEqual()
    {
        float closerFish = Vector2.SignedAngle(blackFish.right, whiteFish.right);

        if (closerFish > 0 && closerFish <= 180)
        {
            SetBlackTargetRotateSpeed(sprintRotateSpeed);
            if (white_targetRotateSpeed == sprintRotateSpeed) { SetBlackTargetRotateSpeed(white_targetRotateSpeed * 2); }

            blackAnim.Play("sprint");
            float angle = Vector2.Angle(blackFish.right, whiteFish.right);
            while (angle > 180 || angle < 170)
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
            while (angle > 180 || angle < 170)
            {
                angle = Vector2.Angle(blackFish.right, whiteFish.right); yield return null;
            }
            SetWhiteTargetRotateSpeed(idleRotateSpeed);
            white_rotateSpeed = idleRotateSpeed;
        }
        yield return null;
    }

    public IEnumerator IESprintSamePos()
    {
        float closerFish = Vector2.SignedAngle(blackFish.right, whiteFish.right);

        if (closerFish > 0 && closerFish <= 180)
        {
            SetWhiteTargetRotateSpeed(sprintRotateSpeed);
            if (black_targetRotateSpeed == sprintRotateSpeed) { SetWhiteTargetRotateSpeed(black_targetRotateSpeed * 2); }
            whiteAnim.Play("sprint");
            float angle = Vector2.Angle(blackFish.right, whiteFish.right);
            while (angle > 5)
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
            while (angle > 5)
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
            isCloseSwimming = true;
            if (playAnim)
            {
                whiteAnim.Play("close_swim_pre");
                blackAnim.Play("close_swim_pre");
            }

            while (white_distanceToCenter > minMaxDistanceTocenter.x || black_distanceToCenter > minMaxDistanceTocenter.x)
            {
                if (white_distanceToCenter > minMaxDistanceTocenter.x) whiteFish.position -= whiteFish.up * swimToCenterSpeed * Time.deltaTime;
                if (black_distanceToCenter > minMaxDistanceTocenter.x) blackFish.position -= blackFish.up * swimToCenterSpeed * Time.deltaTime;
                yield return null;
            }
        }
        else
        {
            blackAnim.Play("sprint"); whiteAnim.Play("sprint");
            isCloseSwimming = false;
            while (white_distanceToCenter < minMaxDistanceTocenter.y || black_distanceToCenter < minMaxDistanceTocenter.y)
            {
                if (white_distanceToCenter < minMaxDistanceTocenter.y) whiteFish.position += whiteFish.up * swimToCenterSpeed * Time.deltaTime;
                if (black_distanceToCenter < minMaxDistanceTocenter.y) blackFish.position += blackFish.up * swimToCenterSpeed * Time.deltaTime;

                yield return null;
            }
        }

        SetNormalRotateSpeed();
    }

    public IEnumerator IESwimAway(Vector3 end, float centerOffset, bool isBlack)
    {
        if (isBlack) { blackPositioned = false; SetBlackTargetRotateSpeed(sprintRotateSpeed); }
        else { whitePositioned = false; SetWhiteTargetRotateSpeed(sprintRotateSpeed); }

        Transform origin = whiteOrigin;
        if (isBlack) { origin = blackOrigin; }

        float angle = Vector2.Angle(origin.right, (end - origin.position).normalized) + 60;
        while (58 > angle || angle > 62)
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

    public IEnumerator IESingleFishDive(bool isBlack, bool isleft)
    {
        if (isBlack) { blackAnim.Play("sprint"); SetBlackTargetRotateSpeed(sprintRotateSpeed); }
        else { whiteAnim.Play("sprint"); SetWhiteTargetRotateSpeed(sprintRotateSpeed); }

        while ((isBlack ? black_distanceToCenter : white_distanceToCenter) < minMaxDistanceTocenter.y)
        {
            if (isBlack) { blackFish.position += blackFish.up * swimToCenterSpeed * Time.deltaTime; }
            else { whiteFish.position += whiteFish.up * swimToCenterSpeed * Time.deltaTime; }
            yield return null;
        }

        float z = isleft ? -90 : 30;

        bool finished = false;
        Transform origin = isBlack ? blackOrigin : whiteOrigin;
        yield return co_sprintToAngle = StartCoroutine(IESprintToAngle(isBlack, z));

        if (isBlack) { SetBlackTargetRotateSpeed(sprintRotateSpeed); black_rotateSpeed = sprintRotateSpeed * 2; }
        else { SetWhiteTargetRotateSpeed(sprintRotateSpeed); white_rotateSpeed = sprintRotateSpeed * 2; }

        float x = isleft ? origin.position.x - 8 : origin.position.x + 10;
        origin.DOMove(new Vector3(x, waterLevel.position.y - 4, 0), 1f)
            .SetEase(Ease.InSine).OnComplete(() => finished = true);

        yield return new WaitUntil(() => finished);
    }

    public IEnumerator IESingleFishJumpOut(bool isBlack, Transform _target, Vector3 offset)
    {
        float animTime = 0.49f;
        Animator anim = isBlack ? blackAnim : whiteAnim;
        Transform fish = isBlack ? blackFish : whiteFish;
        Transform origin = isBlack ? blackOrigin : whiteOrigin;
        Transform fishGFX = isBlack ? blackFishGFX : whiteFishGFX;
        Vector3 target = _target.position + offset;
        bool toLeft = target.x < origin.position.x;

        // move to appropriate x position
        if (toLeft) { origin.DOMove(new Vector3(target.x + 8, waterLevel.position.y - 6, 0), 1f); }
        else { origin.DOMove(new Vector3(target.x - 8, waterLevel.position.y - 6, 0), 1f); }
        yield return new WaitForSeconds(1);

        //reset to initial
        if (isBlack) { SetBlackTargetRotateSpeed(0); black_rotateSpeed = 0; }
        else { SetWhiteTargetRotateSpeed(0); white_rotateSpeed = 0; }
        origin.eulerAngles = Vector3.zero;
        if (isBlack) { blackAnim.Play("black_dive_1"); }
        else { whiteAnim.Play("white_dive_2"); }
        fish.localPosition = new Vector3(0, isCloseSwimming ? 1 : 3, 0);
        fishGFX.DOLocalRotate(new Vector3(0, 0, 0), 0.1f);
        fishGFX.DOLocalMove(new Vector3(0, 0, 0), 0.1f);

        // rotate to target position angle
        float angle = 210;
        if (!toLeft) { angle += 90; }
        origin.Rotate(Dir, angle);

        //jump out
        origin.DOMove(target, 1f).SetEase(Ease.OutCubic);

        yield return new WaitForSeconds(animTime);
        anim.SetBool("dive_end", true);
        yield return new WaitForSeconds(0.2f);
        if (isBlack) { SetBlackTargetRotateSpeed(idleRotateSpeed); black_rotateSpeed = sprintRotateSpeed; }
        else { SetWhiteTargetRotateSpeed(sprintRotateSpeed); white_rotateSpeed = sprintRotateSpeed; }
        anim.Play(isBlack ? "black_idle" : "white_idle");
        anim.SetBool("dive_end", false);
    }

    public IEnumerator IEReturnToCenter(bool isBlack)
    {
        yield return co_singleJumpToPos = StartCoroutine(IESingleFishJumpOut(isBlack, transform, Vector3.zero));
    }

    #endregion Main AI Coroutines

    #region AI Utility Methods

    public Transform CheckCloserFish()
    {
        if (isBlackBusy && isWhiteBusy) { return null; }
        if (!isBlackBusy && !isWhiteBusy)
        {
            if (blackFish.eulerAngles.z < whiteFish.eulerAngles.z) { return blackFish; }
            else { return whiteFish; }
        }
        if (!isBlackBusy) return blackFish;
        if (!isWhiteBusy) return whiteFish;
        return null;
    }

    public override void CancelAllAction()
    {
        TryStopCoroutine(co_sprintStartPoint);
        TryStopCoroutine(co_IEcloseSwim);
        TryStopCoroutine(co_sprintBackEqual);
        TryStopCoroutine(co_sprintToAngle);
        TryStopCoroutine(co_singleFishDive);
        TryStopCoroutine(co_singleReturnToCenter);
        TryStopCoroutine(co_singleJumpToPos);

        actionList.Clear();
        TryStopCoroutine(co_act);
        waterSpear.CancelAct();
        swing.CancelAct();
        dive.CancelAct();

        blackAnim.Play("black_idle");
        whiteAnim.Play("white_idle");

        SetBlackNotBusy();
        SetWhiteNotBusy();

        SetNormalRotateSpeed();
    }

    #endregion AI Utility Methods

    #region Phase & Ultimate Coroutines

    public IEnumerator Pre_SecondPhase()
    {
        if (!secondPhase)
        {
            DEAD = true;
            actionList.Clear();
            CancelAllAction();
            CharacterUIManager.ShowBlackEdge(true);
            yield return co_sprintBackEqual = StartCoroutine(IESprintBackEqual());
            yield return co_IEcloseSwim = StartCoroutine(IECloseSwim(true));
            SetWhiteTargetRotateSpeed(idleRotateSpeed / 3);
            SetBlackTargetRotateSpeed(idleRotateSpeed / 3);
            whiteAnim.Play("close_swim");
            blackAnim.Play("close_swim");

            yield return StartCoroutine(ChangeYPos(-6));
            transform.DOMove(new Vector3(GetCenterXOfMap(), waterLevel.position.y - 6, 0), 1.8f);
            yield return new WaitForSeconds(1.8f);
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
        StartCoroutine(ChangeYPos(4.5f));
        centerAnim.Play("center_fade");

        StartCoroutine(EmojiDuringCircling());
        yield return StartCoroutine(Circling(3.5f));

        StartCoroutine(Ultimate());

        yield return null;
    }

    public IEnumerator Ultimate()
    {
        //swing qte
        yield return swing.act_routine = StartCoroutine(swing.Act_coroutine(1));
        yield return dive.act_routine = StartCoroutine(dive.Act_coroutine(1));

        yield return co_IEcloseSwim = StartCoroutine(IECloseSwim(false));
        SetNormalRotateSpeed();

        //water spear ultimate
        yield return co_sprintBackEqual = StartCoroutine(IESprintBackEqual());
        StartCoroutine(IESwimAway(waterSpearPos_black1.position, 10, true));
        StartCoroutine(IESwimAway(waterSpearPos_white1.position, 10, false));
        while (!blackPositioned || !whitePositioned) { yield return null; }
        yield return StartCoroutine(IESprintSamePos());
        StartCoroutine(waterSpear.Act_coroutine(1));
        StartCoroutine(waterSpear.Act_coroutine(2));
        while (!finishedWaterSpearUltimate) { yield return null; }

        //swing ultimate
        movingTarget = player;
        yield return dive.act_routine = StartCoroutine(dive.Act_coroutine());
        yield return swing.act_routine = StartCoroutine(swing.Act_coroutine(2));
        movingTarget = GetBoundaryFarOfPlayer();
        yield return dive.act_routine = StartCoroutine(dive.Act_coroutine());
        yield return co_sprintBackEqual = StartCoroutine(IESprintBackEqual());

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
        yield return new WaitForSeconds(1f);
        waterDragon1.gameObject.SetActive(true);
        yield return new WaitForSeconds(5f);

        yield return StartCoroutine(playerController.RunToPosition(transform.position - new Vector3(14f, 0, 0)));

        playerController.rb.velocity = Vector3.zero;
        InputMaster.instance.StartMustSuccessQTE(InputKeyType.swordTeleport_key, player.transform.position + new Vector3(0, 4, 0),
            0.4f, () => { playerController.DesignatedPositionTeleport(player.transform.position + new Vector3(5, 7.5f, 0)); });

        yield return new WaitUntil(() => !InputMaster.instance.isQTE);
        playerController.EnableGravity(false);
        playerController.rb.velocity = Vector3.zero;

        yield return new WaitForSeconds(0.4f);
        playerController.anim.Play("slash_pre");
        yield return new WaitForSeconds(0.1f);

        //slash animation pr

        InputMaster.instance.StartMustSuccessQTE(InputKeyType.right_attack_key, player.transform.position + new Vector3(2, 2, 0),
           0.4f, () =>
           {
               playerController.anim.Play("slash_end");
           });
        yield return new WaitUntil(() => !InputMaster.instance.isQTE);
        yield return new WaitForSeconds(0.2f);

        player.position += new Vector3(12, -7);
        playerController.EnableGravity(true);
        slash_effect.transform.position = new Vector3(transform.position.x, waterLevel.position.y, 0f);
        slash_effect.SetActive(true);

        blackAnim.speed = 0;
        whiteAnim.speed = 0;
        blackSprite.sprite = black_tex; whiteSprite.sprite = white_tex;
        VFXManager.instance.SlowTimeForSeconds(0.5f, 0);
        yield return new WaitForSeconds(0.1f);
        //CancelAllAction();
        SetWhiteTargetRotateSpeed(0); white_rotateSpeed = 0;
        SetBlackTargetRotateSpeed(0); black_rotateSpeed = 0;

        //whiteAnim.SetTrigger("circling_end"); blackAnim.SetTrigger("circling_end");

        ultimateWave.GetComponent<Animator>().SetTrigger("end");

        yield return new WaitForSeconds(3f);
        blackSprite.enabled = false;
        whiteSprite.enabled = false;
        black_particle.SetActive(true); white_particle.SetActive(true);
        yield return new WaitForSeconds(5f);
        black_particle.SetActive(false); white_particle.SetActive(false);

        slash_effect.SetActive(false);

        yield return new WaitForSeconds(3f);
        endCanvas.gameObject.SetActive(true);
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

    #endregion Phase & Ultimate Coroutines

    #region IEnemyController Overrides

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
            if (!secondPhase && currentActionBreakAmount >= maxActionBreakCapacity) { yield return StartCoroutine(Break()); }
            else { StartAction(); }
            yield return null;
        }
    }

    public override void StartAction()
    {
        if (secondPhase || DEAD) { return; }
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
                else if (i < 10) { possibleActions.Add(dive); }
            }
            else
            {
                float i = Random.Range(0, 10);
                if (i < 5) { possibleActions.Add(waterSpear); }
                else if (i < 10) { possibleActions.Add(dive); }
            }
        }
        else
        {
            possibleActions.Add(swing);
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
                else { InsertAction(waterSpear); }
            }
        }
    }

    public override IEnumerator IE_Activate()
    {
        co_sprintBackEqual = StartCoroutine(IESprintBackEqual());
        yield return co_IEcloseSwim = StartCoroutine(IECloseSwim(false));
        StartAction();
    }

    public override int Damage(float damageAmount, Transform sender, float stunDuration = 0, bool damageFlash = true, float stunValue = 0)
    {
        if (DEAD) { return 0; }

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

        healthBar.fillAmount = (float)currentHealth / (float)maxHealth;
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
            StartCoroutine(IE_Activate());
            IN_COMBAT = true;
            HealthUI.SetActive(true);
            EventInteract.SetActive(false);
        }
        else
        {
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

    public void Swing() => InsertAction(swing);

    public void Splash() => InsertAction(splash);

    public void Dive() => InsertAction(dive);

    public void SingleSwing() => InsertAction(singleSwing);

    public void BubbleTrap() => InsertAction(bubbleTrap);

    public void Gatling() => InsertAction(gatling);

    public override IEnumerator BossBreak()
    {
        isBossBreaking = true;
        CancelAllAction();
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
                stunBar.fillAmount = Mathf.Clamp01(currentStun / maxStun);

            yield return null;
        }

        currentStun = maxStun;
        if (stunBar != null && maxStun > 0)
            stunBar.fillAmount = 1f;

        VFXManager.instance.UnBulletTime();
        isBossBreaking = false;
        yield return new WaitForSeconds(1f);
        yield return StartCoroutine(IESprintBackEqual());
        StartAction();
        yield return null;
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

    public void SetBlackBusy() => isBlackBusy = true;

    public void SetWhiteBusy() => isWhiteBusy = true;

    public void SetBlackNotBusy() => isBlackBusy = false;

    public void SetWhiteNotBusy() => isWhiteBusy = false;

    public void SetNotBusy(bool isBlack)
    {
        if (isBlack) { SetBlackNotBusy(); }
        else { SetWhiteNotBusy(); }
    }

    public void SetBusy(bool isBlack)
    {
        if (isBlack) { SetBlackBusy(); }
        else { SetWhiteBusy(); }
    }

    public void SetFishTargetRotateSpeed(bool isBlack, float speed)
    {
        if (isBlack) { black_targetRotateSpeed = speed; }
        else { white_targetRotateSpeed = speed; }
    }

    public void SetBlackTargetRotateSpeed(float speed)
    {
        black_targetRotateSpeed = speed;
    }

    public void SetWhiteTargetRotateSpeed(float speed)
    {
        white_targetRotateSpeed = speed;
    }

    #endregion Utility & Interaction
}