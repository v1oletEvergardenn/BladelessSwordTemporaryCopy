using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static UnityEngine.UI.Image;
using Random = UnityEngine.Random;

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
    [HideInInspector] public Coroutine co_circling;

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

    [FoldoutGroup("Fish References")]
    [SerializeField] public Transform center;

    [FoldoutGroup("Fish References")]
    [SerializeField] public Transform blackFish;

    [FoldoutGroup("Fish References")]
    [SerializeField] public Transform whiteFish;

    [FoldoutGroup("Fish References")]
    [SerializeField] public Transform blackFishGFX;

    [FoldoutGroup("Fish References")]
    [SerializeField] public Transform whiteFishGFX;

    [FoldoutGroup("Fish References")]
    [SerializeField] public Transform blackOrigin;

    [FoldoutGroup("Fish References")]
    [SerializeField] public Transform whiteOrigin;

    #endregion FISH REFERENCES

    #region ROTATION & MOVEMENT

    [FoldoutGroup("Rotation & Movement")]
    [SerializeField] public float idleRotateSpeed;

    [FoldoutGroup("Rotation & Movement")]
    [SerializeField] public float sprintRotateSpeed;

    [FoldoutGroup("Rotation & Movement")]
    [SerializeField] public float swimToCenterSpeed = 2f;

    [FoldoutGroup("Rotation & Movement")]
    [SerializeField, MinMaxSlider(1f, 3f, true)] public Vector2 minMaxDistanceTocenter;

    [FoldoutGroup("Rotation & Movement")]
    [SerializeField] public float far_distance_threshhold = 30f;

    [FoldoutGroup("Rotation & Movement")]
    [SerializeField] public float close_distance_threshhold = 10f;

    [HideInInspector] public float white_distanceToCenter = 0f;
    [HideInInspector] public float black_distanceToCenter = 0f;
    [HideInInspector] public Transform movingTarget;

    [HideInInspector] public bool isCloseSwimming;
    [HideInInspector] public float black_rotateSpeed;
    [HideInInspector] public float white_rotateSpeed;
    [HideInInspector] public float black_targetRotateSpeed;
    [HideInInspector] public float white_targetRotateSpeed;

    public bool isBlackBusy = false;

    public bool isWhiteBusy = false;

    #endregion ROTATION & MOVEMENT

    #region GENERAL REFERENCES

    [FoldoutGroup("General References")]
    [SerializeField] public Transform endCanvas;

    [FoldoutGroup("General References")]
    [SerializeField] public Transform waterLevel;

    [FoldoutGroup("General References")]
    [SerializeField] public GameObject EventInteract;

    [FoldoutGroup("General References")]
    [SerializeField] public GameObject black_particle;

    [FoldoutGroup("General References")]
    [SerializeField] public GameObject white_particle;

    [FoldoutGroup("General References")]
    [SerializeField] public Sprite black_tex;

    [FoldoutGroup("General References")]
    [SerializeField] public Sprite white_tex;

    [FoldoutGroup("General References")]
    [SerializeField] public GameObject swimEffect;

    [FoldoutGroup("General References")]
    [SerializeField] public Transform fish_origin;

    [FoldoutGroup("General References")]
    [SerializeField] public CameraLimit camLimit;

    [FoldoutGroup("General References")]
    [SerializeField] public GeneralEventInteraction interaction;

    #endregion GENERAL REFERENCES

    #region ULTIMATE REFERENCES

    [FoldoutGroup("Ultimate")]
    [SerializeField] public GameObject ultimateWave;

    [FoldoutGroup("Ultimate")]
    [SerializeField] public GameObject slash_effect;

    [FoldoutGroup("Ultimate")]
    [SerializeField] public Transform waterDragon1;

    [FoldoutGroup("Ultimate")]
    [SerializeField] public Transform waterSpearPos_black1;

    [FoldoutGroup("Ultimate")]
    [SerializeField] public Transform waterSpearPos_black2;

    [FoldoutGroup("Ultimate")]
    [SerializeField] public Transform waterSpearPos_white1;

    [FoldoutGroup("Ultimate")]
    [SerializeField] public Transform waterSpearPos_white2;

    [HideInInspector] public bool secondPhase;
    [HideInInspector] public bool blackPositioned = false;
    [HideInInspector] public bool whitePositioned = false;
    [HideInInspector] public bool finishedWaterSpearUltimate = false;
    public List<Transform> ultimate_bullets;

    #endregion ULTIMATE REFERENCES

    #region Action Fields

    [FoldoutGroup("Action References")]
    [SerializeField] public YYF_WaterSpear waterSpear;

    [FoldoutGroup("Action References")]
    [SerializeField] public YYF_Swing swing;

    [FoldoutGroup("Action References")]
    [SerializeField] public YYF_SingleSwing singleSwing;

    [FoldoutGroup("Action References")]
    [SerializeField] public YYF_BubbleTrap bubbleTrap;

    [FoldoutGroup("Action References")]
    [SerializeField] public YYF_Dive dive;

    [FoldoutGroup("Action References")]
    [SerializeField] public YYF_Gatling gatling;

    [FoldoutGroup("Action References")]
    [SerializeField] public YYF_Splash splash;

    [FoldoutGroup("Debug Actions")]
    [Button("Force Die")]
    private void DebugForceDie() => ForceDie();

    [FoldoutGroup("Debug Actions")]
    [Button("Force Stun")]
    private void DebugForceStun() => ForceStun();

    [FoldoutGroup("Debug Actions")]
    [Button("Start Action")]
    private void DebugStartAction() => StartAction();

    [FoldoutGroup("Debug Actions")]
    [Button("Water Spear")]
    private void DebugWaterSpear() => WaterSpear();

    [FoldoutGroup("Debug Actions")]
    [Button("Swing")]
    private void DebugSwing() => Swing();

    [FoldoutGroup("Debug Actions")]
    [Button("Splash")]
    private void DebugSplash() => Splash();

    [FoldoutGroup("Debug Actions")]
    [Button("Dive")]
    private void DebugDive() => Dive();

    [FoldoutGroup("Debug Actions")]
    [Button("Single Swing")]
    private void DebugSingleSwing() => SingleSwing();

    [FoldoutGroup("Debug Actions")]
    [Button("Bubble Trap")]
    private void DebugBubbleTrap() => BubbleTrap();

    [FoldoutGroup("Debug Actions")]
    [Button("Gatling")]
    private void DebugGatling() => Gatling();

    #endregion Action Fields

    public static YingYangFish_AI instance;

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

        white_rotateSpeed = idleRotateSpeed;
        SetWhiteTargetRotateSpeed(idleRotateSpeed);
        black_rotateSpeed = idleRotateSpeed;
        SetBlackTargetRotateSpeed(idleRotateSpeed);
        canTakeDamage = false;
        isCloseSwimming = true;
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

        white_distanceToCenter = whiteFish.localPosition.magnitude;
        black_distanceToCenter = blackFish.localPosition.magnitude;

        float white_temp = (minMaxDistanceTocenter.y / white_distanceToCenter) * white_targetRotateSpeed;
        float black_temp = (minMaxDistanceTocenter.y / black_distanceToCenter) * black_targetRotateSpeed;

        white_rotateSpeed = Mathf.SmoothDamp(white_rotateSpeed, white_temp, ref white_rotateVelocity, 0.2f);
        black_rotateSpeed = Mathf.SmoothDamp(black_rotateSpeed, black_temp, ref black_rotateVelocity, 0.2f);

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

    /// <summary>
    /// check closest fish and rotate it to the start point.
    /// </summary>
    /// <param name="assignedFish"> designate a specific fish to swim to start point</param>
    /// <returns></returns>
    public IEnumerator IESprintStartPoint(string assignedFish = "null")
    {
        Transform closerFish = null;
        if (assignedFish == "null") while (closerFish == null) { closerFish = CheckCloserFish(); yield return null; }
        else if (assignedFish == "white") { closerFish = whiteFish; }
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

    /// <summary>
    /// Adjusts the rotation speeds and animations of two entities to align their directions within a specified angle
    /// range.
    /// </summary>
    /// <remarks>This coroutine determines which entity is closer in alignment to the other and adjusts their
    /// rotation speeds      and animations accordingly. The method ensures that the entities' directions align within a
    /// range of 170 to 180 degrees.     The rotation speed is reset to idle once alignment is achieved.</remarks>
    /// <returns>An enumerator that can be used to control the coroutine's execution.</returns>
    public IEnumerator IESprintBackEqual()
    {
        float closerFish = Vector2.SignedAngle(blackFish.right, whiteFish.right);

        if (closerFish > 0 && closerFish <= 180)
        {
            SetBlackTargetRotateSpeed(sprintRotateSpeed);
            if (white_targetRotateSpeed == sprintRotateSpeed) { SetBlackTargetRotateSpeed(white_targetRotateSpeed * 2); }

            blackAnim.Play("sprint");
            float angle = Vector2.Angle(blackFish.right, whiteFish.right);
            while (angle > 180 || angle < 175)
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
            while (angle > 180 || angle < 175)
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

    public bool isReturnDive_black = false;
    public bool isReturnDive_white = false;
    public Coroutine co_return_singleFishDive;

    public IEnumerator IESingleFishDive(bool isBlack, bool isleft)
    {
        //if (isBlack) { blackAnim.Play("sprint"); SetBlackTargetRotateSpeed(sprintRotateSpeed); }
        //else { whiteAnim.Play("sprint"); SetWhiteTargetRotateSpeed(sprintRotateSpeed); }

        //// Quickly tween the fish outward to max distance instead of frame-by-frame swimming
        Transform fish = isBlack ? blackFish : whiteFish;
        Animator anim = isBlack ? blackAnim : whiteAnim;
        //float currentDist = isBlack ? black_distanceToCenter : white_distanceToCenter;
        //if (currentDist < minMaxDistanceTocenter.y)
        //{
        //    float remaining = minMaxDistanceTocenter.y - currentDist;
        //    float swimOutDuration = remaining / (swimToCenterSpeed * 4f); // 4x faster
        //    bool swimOutDone = false;
        //    Vector3 targetPos = fish.position + fish.up * remaining;
        //    fish.DOMove(targetPos, swimOutDuration).SetEase(Ease.OutSine).OnComplete(() => swimOutDone = true);
        //    yield return new WaitUntil(() => swimOutDone);
        //}

        float z = -90;

        if (isBlack) { SetBlackTargetRotateSpeed(0); black_rotateSpeed = 0; }
        else { SetWhiteTargetRotateSpeed(0); white_rotateSpeed = 0; }

        Transform fishGFX = isBlack ? blackFishGFX : whiteFishGFX;
        Transform origin = isBlack ? blackOrigin : whiteOrigin;

        // Fast direct rotation instead of IESprintToAngle (which waits for gradual orbit)
        float currentZ = origin.eulerAngles.z;
        anim.Play("rotate");
        float rotationDuration = 0.4f;

        float targetAngle = Mathf.MoveTowardsAngle(currentZ, z, float.MaxValue);

        // Ensure the rotation is always clockwise by adding 360 if needed
        if (targetAngle > currentZ) targetAngle -= 360f;

        fishGFX.DORotate(new Vector3(0, 0, targetAngle), rotationDuration, RotateMode.Fast)
            .SetEase(Ease.InSine);
        yield return new WaitForSeconds(rotationDuration);

        float x = origin.position.x;
        bool moveDone = false;
        origin.DOMove(new Vector3(x, waterLevel.position.y - 6, 0), 30f)
             .SetSpeedBased()
             .SetEase(Ease.Linear).OnComplete(() => moveDone = true);

        yield return new WaitUntil(() => moveDone);

        origin.localScale = new Vector3(1, 1, 1);
        isReturnDive_black = false;
        isReturnDive_white = false;
        origin.eulerAngles = new Vector3(0, 0, 0);
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

    public bool isReturningToCenter_black = false;
    public bool isReturningToCenter_white = false;
    private Coroutine co_return_singleJumpToPos;

    public IEnumerator IEReturnToCenter(bool isBlack)
    {
        if (isBlack) isReturningToCenter_black = true;
        else isReturningToCenter_white = true;
        yield return co_return_singleJumpToPos = StartCoroutine(IESingleFishJumpOut(isBlack, transform, Vector3.zero));
        if (isBlack) isReturningToCenter_black = false;
        else isReturningToCenter_white = false;
    }

    public IEnumerator IEReturnToInitialState(bool isBlack)
    {
        Transform origin = isBlack ? blackOrigin : whiteOrigin;
        Transform fish = isBlack ? blackFish : whiteFish;
        if (origin.localPosition != Vector3.zero)
        {
            // if fish is above water, dive first
            if (!(isBlack ? isReturnDive_black : isReturnDive_white))
            {
                if (fish.position.y > waterLevel.position.y)
                {
                    yield return StartCoroutine(IESingleFishDive(isBlack, origin.position.x >= transform.position.x));
                }
            }

            //
            if (!(isBlack ? isReturningToCenter_black : isReturningToCenter_white))
            {
                yield return new WaitUntil(() => !(isBlack ? isReturnDive_black : isReturnDive_white));
                yield return StartCoroutine(IEReturnToCenter(isBlack));
            }
            yield return new WaitUntil(() => !(isBlack ? isReturningToCenter_black : isReturningToCenter_white) && !(isBlack ? isReturnDive_black : isReturnDive_white));
        }
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
        TryStopCoroutine(co_circling);
        TryStopCoroutine(co_multiCoroutine);
        TryStopCoroutine(co_multiActions);
        TryStopCoroutine(co_multiRun);

        isReturnDive_black = false;
        isReturnDive_white = false;
        isReturningToCenter_black = false;
        isReturningToCenter_white = false;

        center.GetComponent<SpriteRenderer>().sortingOrder = 1;

        actionList.Clear();
        TryStopCoroutine(co_act);
        waterSpear.CancelAct();
        swing.CancelAct();
        dive.CancelAct();
        singleSwing.CancelAct();
        bubbleTrap.CancelAct();
        gatling.CancelAct();
        SetBlackNotBusy();
        SetWhiteNotBusy();
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

            // wait until fish back to initial state
            StartCoroutine(IEReturnToInitialState(true));
            StartCoroutine(IEReturnToInitialState(false));
            yield return new WaitUntil(() => blackOrigin.localPosition == Vector3.zero && whiteOrigin.localPosition == Vector3.zero);
            yield return co_sprintBackEqual = StartCoroutine(IESprintBackEqual());
            yield return co_IEcloseSwim = StartCoroutine(IECloseSwim(true));
            SetWhiteTargetRotateSpeed(idleRotateSpeed / 3);
            SetBlackTargetRotateSpeed(idleRotateSpeed / 3);
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

        //circling
        StartCoroutine(EmojiDuringCircling());
        yield return StartCoroutine(Circling(3.5f));

        StartCoroutine(Ultimate());

        yield return null;
    }

    public IEnumerator Ultimate()
    {
        //swing qte
        yield return swing.act_routine = StartCoroutine(swing.Act_coroutine(1));

        //move to left boundary
        movingTarget = leftBoundary;
        yield return dive.act_routine = StartCoroutine(dive.Act_coroutine());
        yield return co_IEcloseSwim = StartCoroutine(IECloseSwim(false));
        SetNormalRotateSpeed();

        //double single swing and bubble gatling
        singleSwing.act_routine = StartCoroutine(singleSwing.Act_coroutine(1));
        yield return gatling.act_routine = StartCoroutine(gatling.Act_coroutine(1));

        yield return new WaitUntil(() => !isBlackBusy && !isWhiteBusy);
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
        // move to player
        movingTarget = player;
        yield return dive.act_routine = StartCoroutine(dive.Act_coroutine());
        yield return swing.act_routine = StartCoroutine(swing.Act_coroutine(2));

        //move to right boundary
        movingTarget = rightBoundary;
        yield return dive.act_routine = StartCoroutine(dive.Act_coroutine());
        yield return co_sprintBackEqual = StartCoroutine(IESprintBackEqual());

        yield return new WaitForSeconds(1f);
        //water ball ultimate
        black_targetRotateSpeed = idleRotateSpeed / 3;
        white_targetRotateSpeed = idleRotateSpeed / 3;

        //circling
        StartCoroutine(Circling(30f));
        StartCoroutine(ChangeYPos(4));
        yield return new WaitForSeconds(1f);

        //ultimate wave show up
        ultimateWave.SetActive(true);
        ultimateWave.transform.position = new Vector3(transform.position.x, waterLevel.position.y, 0);

        //charge up for 2 seconds
        yield return new WaitForSeconds(2f);

        //ultimate water ball bullets shooting
        float[] bulletDelays = { 0f, 0.5f, 0.7f, 1.2f, 1.7f, 1.9f, 3f, 3.2f, 3.4f, 3.6f, 3.8f, 4f };
        for (int i = 0; i < bulletDelays.Length; i++)
        {
            StartCoroutine(SpawnUltimateBullet(i, bulletDelays[i]));
        }

        //water dragons
        yield return new WaitForSeconds(1f);
        waterDragon1.gameObject.SetActive(true);
        yield return new WaitForSeconds(5f);

        //player run to position
        yield return StartCoroutine(playerController.RunToPositionCoroutine(transform.position - new Vector3(14f, 0, 0)));

        //sword teleport jump qte
        playerController.rb.velocity = Vector3.zero;
        InputMaster.instance.StartMustSuccessQTE(InputKeyType.swordTeleport_key, player.transform.position + new Vector3(0, 4, 0),
            0.4f, () => { playerController.DesignatedPositionTeleport(player.transform.position + new Vector3(5, 7.5f, 0)); });

        yield return new WaitUntil(() => !InputMaster.instance.isQTE);
        playerController.EnableGravity(false);
        playerController.rb.velocity = Vector3.zero;

        yield return new WaitForSeconds(0.4f);
        playerController.anim.Play("slash_pre");
        yield return new WaitForSeconds(0.1f);

        //slash qte

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

        //end
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
            yield return co_multiCoroutine = StartCoroutine(StartMultipleCoroutines(new List<IEnumerator>()
            {
                whiteFish.transform.position.y >= waterLevel.position.y? IESingleFishDive(false, true):null,
                blackFish.transform.position.y >= waterLevel.position.y? IESingleFishDive(true, false):null
            }));

            foreach (List<ActionCaller> list in actionList)
            {
                yield return co_multiActions = StartCoroutine(StartMultipleActions(list));
            }
            if (!secondPhase && currentActionBreakAmount >= maxActionBreakCapacity) { yield return StartCoroutine(Break()); }
            else { StartAction(); }
            yield return null;
        }
    }

    public override void StartAction()
    {
        if (secondPhase || DEAD) { return; }

        //initalize
        actionList.Clear();
        List<ActionCaller> possibleActions = new List<ActionCaller>();

        //logic: after fish is under the water, fish starts actions,
        //when calls the action method, put in the factor to determine which fish to use for the action:
        //0:black fish,
        //1:white fish,
        //2:both fish

        ///TEST********

        int randomInitial = Random.Range(0, 4);

        if (randomInitial == 0)//water spear
        {
            initialAction = waterSpear;
        }
        else if (randomInitial == 1)
        {
            initialAction = splash;
        }
        else if (randomInitial == 2)
        {
            initialAction = swing;
        }
        else if (randomInitial == 3)
        {
            initialAction = bubbleTrap;
        }

        //water spear as first action
        if (initialAction == waterSpear)
        {
            int randomNum = Random.Range(0, 3);

            //First Scenario
            if (randomNum == 0)
            {
                bool isBlack = RandomFishBool();
                actionList = new List<List<ActionCaller>>
                    {
                        new List<ActionCaller> {
                            //***ADD water spear charging
                            RandomChoice(new ActionCaller(swing, isBlack),
                            new ActionCaller(splash,  isBlack))
                        },
                        new List<ActionCaller> {
                            RandomChoice(new ActionCaller(swing,!isBlack),
                            new ActionCaller(splash,  !isBlack)),
                            new ActionCaller(waterSpear)
                        },
                    };
            }

            //Second Scenario
            else if (randomNum == 1)
            {
                actionList = new List<List<ActionCaller>>
                    {
                        new List<ActionCaller> {
                            //***ADD water spear charging
                           new ActionCaller(swing, 2),
                           new ActionCaller(waterSpear,0,1f)
                        }
                    };
            }

            //Third Scenario
            else
            {
                actionList = new List<List<ActionCaller>>
                    {
                        new List<ActionCaller> {
                            //***ADD water spear charging
                            new ActionCaller(bubbleTrap, RandomFish())
                        },
                        new List<ActionCaller> {
                            RandomChoice(new ActionCaller(swing, RandomFish()),
                            new ActionCaller(splash,  RandomFish())),
                            new ActionCaller(waterSpear)
                        },
                    };
            }
        }
        else if (initialAction == splash)
        {
            bool isBlack = RandomFishBool();
            actionList = new List<List<ActionCaller>>
                    {
                        new List<ActionCaller> {
                            new ActionCaller(splash,isBlack)
                        },
                        new List<ActionCaller> {
                            new ActionCaller(splash,!isBlack)
                        },
                        new List<ActionCaller> {
                            new ActionCaller(swing,2)
                        }
                    };
        }
        else if (initialAction == swing)
        {
            bool isBlack = RandomFishBool();
            actionList = new List<List<ActionCaller>>
                    {
                        new List<ActionCaller> {
                            new ActionCaller(swing,isBlack)
                        },
                        new List<ActionCaller> {
                            new ActionCaller(swing,!isBlack)
                        },
                        new List<ActionCaller> {
                            new ActionCaller(splash,2)
                        }
                    };
        }
        else if (initialAction == bubbleTrap)
        {
            bool isBlack = RandomFishBool();
            actionList = new List<List<ActionCaller>>
                    {
                        new List<ActionCaller> {
                            new ActionCaller(bubbleTrap,isBlack)
                        },
                        new List<ActionCaller> {
                            RandomChoice(new ActionCaller(swing, RandomFish()),
                            new ActionCaller(splash,  RandomFish())),
                            new ActionCaller(gatling,0,2)
                        }
                    };
        }

        DebugPrintActionList();

        //start action
        if (initialAction != null)
        {
            //InsertAction(initialAction, 0);
            lastAction = initialAction;
            //co_act = StartCoroutine(Act());
        }
        else { StartAction(); return; }
    }

    public override IEnumerator IE_Activate()
    {
        Vector3 pos = new Vector3(center.position.x, 0, 0);
        interaction.transform.localPosition = Vector3.zero;
        leftBoundary.gameObject.SetActive(true);
        rightBoundary.gameObject.SetActive(true);
        float timer = 0f;
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

        fish_origin.transform.position = new Vector3(pos.x, waterLevel.position.y, 0);

        // prepare to jump out

        whiteOrigin.eulerAngles = Vector3.zero;
        blackOrigin.eulerAngles = Vector3.zero;
        StopRotate();
        whiteAnim.Play("white_dive_1");
        blackAnim.Play("black_dive_1");
        blackFish.localPosition = new Vector3(0, 1, 0);
        whiteFish.localPosition = new Vector3(0, 1, 0);

        whiteFishGFX.DOLocalRotate(new Vector3(0, 0, 0), 0.1f);
        whiteFishGFX.DOLocalMove(new Vector3(0, 0, 0), 0.1f);
        blackFishGFX.DOLocalRotate(new Vector3(0, 0, 0), 0.1f);
        blackFishGFX.DOLocalMove(new Vector3(0, 0, 0), 0.1f);

        float angle = 270;
        blackOrigin.Rotate(Dir, angle);
        whiteOrigin.Rotate(Dir, angle);

        fish_origin.DOLocalMove(Vector3.zero, 1f).SetEase(Ease.OutCubic);

        while (fish_origin.position.y < waterLevel.position.y + 4.5f)
        {
            if (fish_origin.position.y > waterLevel.position.y + 1)
            {
                VFXManager.instance.Rumble(1f, 1f);
                IN_COMBAT = true;
                HealthUI.SetActive(true);
                canTakeDamage = true;
            }
            if (fish_origin.position.y > waterLevel.position.y + 2)
            {
                center.DOLocalMove(Vector3.zero, 0.3f).SetEase(Ease.Linear);
                getHitPosition = Vector3.zero;
                blackAnim.SetBool("dive_end", true);
                whiteAnim.SetBool("dive_end", true);
            }
            yield return null;
        }
        VFXManager.instance.StopRumble();
        yield return new WaitForSeconds(0.5f);

        blackAnim.SetBool("dive_end", false);
        whiteAnim.SetBool("dive_end", false);
        SetBlackNotBusy();
        SetWhiteNotBusy();

        CameraFollow.instance.targets.Add(blackFish);
        CameraFollow.instance.targets.Add(whiteFish);

        yield return co_sprintBackEqual = StartCoroutine(IESprintBackEqual());
        co_IEcloseSwim = StartCoroutine(IECloseSwim(false));
        yield return new WaitForSeconds(2f);
        StartAction();
        StartCoroutine(Act());
    }

    public override int Damage(float damageAmount, Transform sender, float stunDuration = 0, bool damageFlash = true, float stunValue = 0)
    {
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

    public override IEnumerator Break()
    {
        // summon two new fish around the center.

        SummonFishAroundCenter();

        //far swim
        StartCoroutine(IECloseSwim(false));

        //break for a while
        yield return new WaitForSeconds(breakDuration);
        currentActionBreakAmount = 0;

        //start action
        StartAction();
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

    public void WaterSpear()
    {
        AddAction(waterSpear);
    }

    public override void ForceDie()
    {
        base.ForceDie();
    }

    public void Swing() => AddAction(swing);

    public void Splash() => AddAction(splash);

    public void Dive() => AddAction(dive);

    public void SingleSwing() => AddAction(singleSwing);

    public void BubbleTrap() => AddAction(bubbleTrap);

    public void Gatling() => AddAction(gatling);

    public override IEnumerator BossBreak()
    {
        isBossBreaking = true;
        CancelAllAction();
        blackAnim.Play("break"); whiteAnim.Play("break");
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
        yield return new WaitForSeconds(1f);

        SummonFishAroundCenter();
        StartAction();
        StartCoroutine(Act());
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

    public int RandomFish() => Random.Range(0, 2);

    public bool RandomFishBool() => Random.Range(0, 2) == 0;

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
}