using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Playables;
using static UnityEngine.UI.Image;
using Random = UnityEngine.Random;

public class YingYangFish_AI : IEnemyController
{
    [HideInInspector] public Coroutine co_sprintToAngle;
    [HideInInspector] public Coroutine co_fishAppear;
    [HideInInspector] public Coroutine co_sprintToAngleAndDive;
    [HideInInspector] public Coroutine co_return_singleFishDive;

    [HideInInspector] public Vector3 Dir;

    //[HideInInspector] public SpriteRenderer blackSprite;
    //[HideInInspector] public SpriteRenderer whiteSprite;
    //[HideInInspector] public Animator blackAnim;
    //[HideInInspector] public Animator whiteAnim;
    [HideInInspector] public Animator centerAnim;

    [HideInInspector] public bool closerFish_Black;

    private float white_rotateVelocity;
    private float black_rotateVelocity;

    [SerializeField] public Transform center;
    [SerializeField] public Transform fish_origin;

    [SerializeField] public float idleRotateSpeed;
    [SerializeField] public float fastRotateSpeed;
    [SerializeField] public float swimSpeed = 30f;
    [SerializeField] public float fastSwimSpeed = 60f;

    [HideInInspector] public Transform movingTarget;
    [HideInInspector] public float black_rotateSpeed;
    [HideInInspector] public float white_rotateSpeed;
    [HideInInspector] public float black_targetRotateSpeed;
    [HideInInspector] public float white_targetRotateSpeed;

    [SerializeField] public Transform endCanvas;
    [SerializeField] public Transform waterLevel;
    [SerializeField] public GameObject EventInteract;
    [SerializeField] public CameraLimit camLimit;
    [SerializeField] public PlayableDirector secondPhaseDirector;

    [HideInInspector] public bool secondPhase;

    [SerializeField] public YYF_WaterSpear waterSpear;
    [SerializeField] public YYF_Swing swing;
    [SerializeField] public YYF_BubbleTrap bubbleTrap;
    [SerializeField] public YYF_Gatling gatling;
    [SerializeField] public YYF_Splash splash;

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
        movingTarget = player;
        EventInteract.SetActive(true);
        HealthUI.SetActive(false);

        StopRotate(false);
        canTakeDamage = false;
        centerAnim = center.GetComponent<Animator>();
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
    }

    public IEnumerator IESprintToAngle(YYF_fish fishOrigin, float angle)
    {
        Transform fish = fishOrigin.fish;
        Animator anim = fishOrigin.anim;
        Transform fishGFX = fishOrigin.fishGFX;
        Transform origin = fishOrigin.transform;
        bool isBlack = fishOrigin.isBlack;

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

    public IEnumerator IESprintToAngleAndDive(YYF_fish fishOrigin, float angle)
    {
        Transform fish = fishOrigin.fish;
        Animator anim = fishOrigin.anim;
        Transform fishGFX = fishOrigin.fishGFX;
        Transform origin = fishOrigin.transform;

        yield return co_sprintToAngle = StartCoroutine(IESprintToAngle(fishOrigin, angle));

        yield return new WaitUntil(() =>
        {
            origin.position += fish.right * swimSpeed * Time.deltaTime;
            return IsUnderWater(fishOrigin);
        });
    }

    public IEnumerator IESingleFishDive(YYF_fish fishOrigin)
    {
        Transform fish = fishOrigin.fish;
        Animator anim = fishOrigin.anim;
        Transform fishGFX = fishOrigin.fishGFX;
        Transform origin = fishOrigin.transform;

        float z = -90;
        if (fishOrigin.isBlack) { SetBlackRotateSpeed(0); black_rotateSpeed = 0; }
        else { SetWhiteRotateSpeed(0); white_rotateSpeed = 0; }

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

        fishOrigin.gameObject.SetActive(false);
    }

    public override void CancelAllAction()
    {
        TryStopCoroutine(co_sprintToAngle);
        TryStopCoroutine(co_multiCoroutine);
        TryStopCoroutine(co_multiActions);
        TryStopCoroutine(co_multiRun);
        TryStopCoroutine(co_fishAppear);
        TryStopCoroutine(co_sprintToAngleAndDive);
        center.GetComponent<SpriteRenderer>().sortingOrder = 1;
        actionList.Clear();
        TryStopCoroutine(co_act);

        waterSpear.CancelAct();
        swing.CancelAct();
        bubbleTrap.CancelAct();
        splash.CancelAct();
        gatling.CancelAct();

        fish_origin.DOKill();
        center.DOKill();
        SetNormalRotateSpeed();
        selfPooler.SetPoolDisactive("whiteFish");
        selfPooler.SetPoolDisactive("blackFish");
    }

    public IEnumerator FishAppear()
    {
        bool opposite = RandomFishBool();

        //spawn new fish();
        YYF_fish newBlackFish = SpawnFish(true);
        YYF_fish newWhiteFish = SpawnFish(false);

        Transform blackFish = newBlackFish.fish;
        Animator blackAnim = newBlackFish.anim;
        Transform blackFishGFX = newBlackFish.fishGFX;
        Transform blackOrigin = newBlackFish.transform;
        Transform whiteFish = newWhiteFish.fish;
        Animator whiteAnim = newWhiteFish.anim;
        Transform whiteFishGFX = newWhiteFish.fishGFX;
        Transform whiteOrigin = newWhiteFish.transform;

        ResetFishCompletely(newBlackFish);
        ResetFishCompletely(newWhiteFish);

        ResetFishNearCenterWithDistance(newBlackFish);
        ResetFishNearCenterWithDistance(newWhiteFish);

        //logic
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

        //sprint to angle
        yield return co_multiCoroutine = StartCoroutine(StartMultipleCoroutines(new List<IEnumerator> {
                IESprintToAngleAndDive(newBlackFish, Random.Range(200f,300f)),
                IESprintToAngleAndDive(newWhiteFish,  Random.Range(200f,300f))
            }));
        // dive

        //disappear
        newBlackFish.gameObject.SetActive(false);
        newWhiteFish.gameObject.SetActive(false);
    }

    public YYF_fish SpawnFish(bool isBlack)
    {
        Vector3 spawnPos = new Vector3(0, waterLevel.position.y - 6, 0);
        YYF_fish fish = selfPooler.SpawnFromPool(isBlack ? "blackFish" : "whiteFish",
            spawnPos, Quaternion.identity).GetComponent<YYF_fish>();
        fish.transform.SetParent(fish_origin, true);
        fish.SetDamageableParent(this);
        ResetFishCompletely(fish);
        fish.transform.position = spawnPos;
        return fish;
    }

    private void ResetFishCompletely(YYF_fish fish)
    {
        ResetFishOrigin(fish);
        ResetFish(fish);
        ResetFishGFX(fish);
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
    public void ResetFishOrigin(YYF_fish fish)
    {
        fish.transform.eulerAngles = Vector3.zero;
        fish.transform.localPosition = Vector3.zero;
    }

    /// <summary>
    /// reset blackFish and whiteFish
    /// </summary>
    public void ResetFish(YYF_fish fish)
    {
        fish.fish.localEulerAngles = Vector3.zero;
        fish.fish.localPosition = new Vector3(0, 0, 0);
    }

    public void ResetFishNearCenterWithDistance(YYF_fish fish)
    {
        fish.fish.localEulerAngles = Vector3.zero;
        fish.fish.localPosition = new Vector3(0, 3.5f, 0);
    }

    /// <summary>
    /// reset fishGFX
    /// </summary>
    public void ResetFishGFX(YYF_fish fish)
    {
        fish.fishGFX.localEulerAngles = Vector3.zero;
        fish.fishGFX.localPosition = Vector3.zero;
    }

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
            yield return co_fishAppear = StartCoroutine(FishAppear());
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

        //fish appear
        yield return co_fishAppear = StartCoroutine(FishAppear());

        IN_COMBAT = true;
        HealthUI.SetActive(true);

        StartAction();
        co_act = StartCoroutine(Act());
    }

    public override int Damage(float damageAmount, Transform sender, float stunDuration = 0, bool damageFlash = true, float stunValue = 0)
    {
        if (DEAD || !canTakeDamage) { damageAmount = 0; }

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
        DoBreak(stunValue);

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
            InputMaster.instance.DisableAllActions();
            InputMaster.instance._attackDirectionAction.Enable();
            InputMaster.instance._attackLeftAction.Enable();
            InputMaster.instance._attackRightAction.Enable();
            InputMaster.instance._defendAction.Enable();
            GameManager.instance.isInPerformingState = true;
            playerController.RunToPosition(center.position - new Vector3(2, 0, 0), true, () => { StartCoroutine(secondPhaseAnim()); });
        }
    }

    public IEnumerator Pre_SecondPhase()
    {
        if (!secondPhase)
        {
            DEAD = true;
            secondPhase = true;
            //clear
            actionList.Clear();
            CancelAllAction();
            selfPooler.SetPoolDisactive();

            centerAnim.SetBool("secondPhase", true);
            CharacterUIManager.ShowBlackEdge(true);
            yield return new WaitForSeconds(2f);
            centerAnim.Play("center_break");
            yield return new WaitForSeconds(0.5f);
            center.DOLocalMove(new Vector3(0, -8.4f, 0), 0.5f).SetEase(Ease.InSine);
            yield return new WaitForSeconds(0.5f);
            //UI
            EventInteract.SetActive(true);
        }
    }

    public IEnumerator secondPhaseAnim()
    {
        //disable player's actions
        centerAnim.Play("center_fade");
        PlaySecondPhaseTimeLine();
        center.DOLocalMove(Vector3.zero, 2.5f).SetEase(Ease.InOutSine);
        yield return new WaitForSeconds(1f);
        Health.instance.RepelToPosition(new Vector3(-37, 0, 0));
    }

    public override IEnumerator BossBreak()
    {
        isBossBreaking = true;
        canTakeDamage = true;
        CancelAllAction();
        ResetOrigin();
        SetBothRotateSpeed(0);
        VFXManager.instance.BulletTime();
        float duration = breakDuration;
        float elapsed = 0f;
        float startStun = currentBreak;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime; // Use unscaled time to be immune to bullet time
            float t = Mathf.Clamp01(elapsed / duration);
            currentBreak = Mathf.Lerp(startStun, maxBreak, t);

            if (bossBreakBar != null && maxBreak > 0)
                bossBreakBar.UpdateBar(currentBreak);

            yield return null;
        }

        currentBreak = maxBreak;
        if (bossBreakBar != null && maxBreak > 0)
            bossBreakBar.UpdateBar(maxBreak);

        VFXManager.instance.UnBulletTime();
        isBossBreaking = false;

        yield return co_fishAppear = StartCoroutine(FishAppear());
        canTakeDamage = false;
        StartAction();
        co_act = StartCoroutine(Act());
        yield return null;
    }

    public void PlaySecondPhaseTimeLine()
    {
        secondPhaseDirector.Play();
        camLimit.UpdateLimit();
        EventInteract.SetActive(false);
        // center interact and flowing upward animation
        leftBoundary.gameObject.SetActive(true);
        rightBoundary.gameObject.SetActive(true);
        IN_COMBAT = true;
        HealthUI.SetActive(true);
        DEAD = true;
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

    public bool IsUnderWater(YYF_fish fish)
    {
        return fish.fish.position.y <= waterLevel.position.y - 6f;
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
        int resolvedFactor = factor;

        if (!UseFactorMode)
        {
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
        }

        return new YYFActionCaller(action, resolvedFactor, delay);
    }
}

[System.Serializable]
public class YYFActionPhase
{
    public List<YYFActionEntry> actionCombo = new List<YYFActionEntry>();
}