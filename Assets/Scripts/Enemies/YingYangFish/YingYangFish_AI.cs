using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EditorAttributes;
using Unity.VisualScripting;
using JetBrains.Annotations;

public class YingYangFish_AI : IEnemyController
{
    #region ATTRIBUTES

    [FoldoutGroup("Attributes", nameof(center), nameof(blackFish), nameof(blackFishGFX),
        nameof(whiteFish), nameof(whiteFishGFX),
        nameof(idleRotateSpeed), nameof(sprintRotateSpeed), nameof(Dir),
        nameof(swimToCenterSpeed), nameof(minMaxDistanceTocenter), nameof(waterLevel),
        nameof(EventInteract))
        ]
    public Void void2;

    [SerializeField, HideInInspector] public Transform YingYangFish;
    [SerializeField, HideInInspector] public Transform center;
    [SerializeField, HideInInspector] public Transform blackFish;
    [SerializeField, HideInInspector] public Transform whiteFish;
    [SerializeField, HideInInspector] public Transform blackFishGFX;
    [SerializeField, HideInInspector] public Transform whiteFishGFX;
    [SerializeField, HideInInspector] public float idleRotateSpeed;
    [SerializeField, HideInInspector] public float sprintRotateSpeed;
    [SerializeField, HideInInspector] public Vector3 Dir;
    [SerializeField, HideInInspector] public float swimToCenterSpeed = 2f;
    [SerializeField, HideInInspector, MinMaxSlider(1f, 3f)] public Vector2 minMaxDistanceTocenter;
    [SerializeField, HideInInspector] public Transform waterLevel;
    [SerializeField, HideInInspector] public GameObject EventInteract;

    [HideInInspector] public SpriteRenderer blackSprite;
    [HideInInspector] public SpriteRenderer whiteSprite;
    [HideInInspector] public Animator blackAnim;
    [HideInInspector] public Animator whiteAnim;

    #endregion ATTRIBUTES

    #region DEBUG

    [FoldoutGroup("Debug", nameof(black_idling), nameof(white_idling), nameof(white_distanceToCenter),
        nameof(black_distanceToCenter), nameof(movingTarget), nameof(isCloseSwimming), nameof(secondPhase))]
    public Void void3;

    [SerializeField, HideInInspector] public bool black_idling = true;
    [SerializeField, HideInInspector] public bool white_idling = true;
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
    [ShowField(nameof(Actions))][SerializeField, ButtonField("Splash", "Splash")] public Void void9;
    [ShowField(nameof(Actions))][SerializeField, ButtonField("Dive", "Dive")] public Void void10;
    [ShowField(nameof(Actions))][SerializeField] public YYF_WaterSpear waterSpear;
    [ShowField(nameof(Actions))][SerializeField] public YYF_Swing swing;
    [ShowField(nameof(Actions))][SerializeField] public YYF_splash splash;
    [ShowField(nameof(Actions))][SerializeField] public YYF_Dive dive;

    #endregion ACTIONS

    [HideInInspector] public Coroutine co_sprintStartPoint;
    [HideInInspector] public Coroutine co_IEcloseSwim;
    [HideInInspector] public Coroutine co_sprintBackEqual;

    // Update is called once per frame

    public override void Start()
    {
        base.Start();
        blackSprite = blackFish.GetComponent<SpriteRenderer>();
        whiteSprite = whiteFish.GetComponent<SpriteRenderer>();
        blackAnim = blackFishGFX.GetComponent<Animator>();
        whiteAnim = whiteFishGFX.GetComponent<Animator>();
        movingTarget = player;
        EventInteract.SetActive(true);
        HealthUI.SetActive(false);
    }

    private void Update()
    {
        distanceToPlayer = Mathf.Abs(transform.position.x - player.position.x);
        white_distanceToCenter = Vector2.Distance(whiteFish.position, center.position);
        black_distanceToCenter = Vector2.Distance(blackFish.position, center.position);
        float tempRotateSpeed = (minMaxDistanceTocenter.y / white_distanceToCenter) * idleRotateSpeed;
        if (tempRotateSpeed >= idleRotateSpeed * 2) { tempRotateSpeed = idleRotateSpeed * 2; }

        if (black_idling) { blackFish.RotateAround(transform.position, Dir, tempRotateSpeed * Time.deltaTime); blackAnim.SetFloat("swim_speed", 1f); }
        if (white_idling) { whiteFish.RotateAround(transform.position, Dir, tempRotateSpeed * Time.deltaTime); whiteAnim.SetFloat("swim_speed", 1f); }

        if (!isActing && actionList.Count > 0)
        {
            isActing = true;
            co_act = StartCoroutine(Act());
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

    public void Splash()
    {
        InsertAction(splash);
    }

    public void Dive()
    {
        InsertAction(dive);
    }

    public IEnumerator Pre_SecondPhase()
    {
        if (!secondPhase)
        {
            actionList.Clear();
            CancelAllAction();

            co_IEcloseSwim = StartCoroutine(IECloseSwim(true));
            CharacterUIManager.ShowBlackEdge(true);
            yield return StartCoroutine(ChangeYPos(false));
            idleRotateSpeed /= 3;
            secondPhase = true;
            EventInteract.SetActive(true);
        }
    }

    public override IEnumerator IE_Activate()
    {
        IN_COMBAT = true;
        //play start animation
        yield return co_IEcloseSwim = StartCoroutine(IECloseSwim(false));
        HealthUI.SetActive(true);
        StartAction();
        //start action loops
    }

    public override void StartAction()
    {
        if (secondPhase) { return; }
        List<IEnemyAction> possibleActions = new List<IEnemyAction>();
        if (playerEnergy.currentEnergy <= 5)
        {
            possibleActions.Add(waterSpear);
        }
        if (Mathf.Abs(player.position.x - transform.position.x) >= swing.swingRange + 1)
        {
            float i = Random.Range(0, 10);
            if (i < 3) { possibleActions.Add(waterSpear); }
            else if (i < 6) { possibleActions.Add(splash); }
            else if (i < 10) { possibleActions.Add(dive); }//moving
        }
        else
        {
            possibleActions.Add(swing);
        }

        if (!playerController.isGrounded)
        {
            possibleActions.Add(splash);
        }

        int index = Random.Range(0, possibleActions.Count);
        initialAction = possibleActions[index];

        actionList.Add(initialAction);
        if (initialAction == dive)
        {
            if ((float)playerEnergy.currentEnergy / (float)playerEnergy.maxEnergy <= 0.6f || playerAttack.currentHS_point < 2)
            {
                InsertAction(swing);
            }
            else
            {
                movingTarget = GetBoundaryFarOfPlayer();
                if (Possibility(50)) { InsertAction(waterSpear); }
                else { InsertAction(splash); InsertAction(waterSpear); }
            }
        }
    }

    /// <summary>
    /// designated fish runs faster to get to start point(top) for next action
    /// </summary>
    /// <param name="isBlack"></param>
    public IEnumerator SprintStartPoint()
    {
        black_idling = true;
        white_idling = true;
        Transform closerFish = CheckCloserFish();

        if (closerFish == blackFish)
        {
            black_idling = false;
            blackAnim.SetFloat("swim_speed", sprintRotateSpeed / idleRotateSpeed);
            while (blackFish.eulerAngles.z > 5 && blackFish.eulerAngles.z < 355)
            {
                blackFish.RotateAround(transform.position, Dir, sprintRotateSpeed * Time.deltaTime);
                yield return null;
            }
        }
        else
        {
            white_idling = false;
            whiteAnim.SetFloat("swim_speed", sprintRotateSpeed / idleRotateSpeed);
            while (whiteFish.eulerAngles.z > 5 && whiteFish.eulerAngles.z < 355)// reached start point
            {
                whiteFish.RotateAround(transform.position, Dir, sprintRotateSpeed * Time.deltaTime);
                yield return null;
            }
        }

        yield return null;
    }

    /// <summary>
    /// designated fish runs faster to get back to equal position
    /// </summary>
    /// <param name="isBlack"></param>
    public IEnumerator SprintBackEqual()
    {
        black_idling = true;
        white_idling = true;
        float closerFish = Vector2.SignedAngle(blackFish.right, whiteFish.right);

        if (closerFish > 0 && closerFish <= 180)
        {
            black_idling = false;
            blackAnim.SetFloat("swim_speed", sprintRotateSpeed / idleRotateSpeed);
            float angle = Vector2.Angle(blackFish.right, whiteFish.right);
            while (angle > 180 || angle < 175)// reached start point
            {
                angle = Vector2.Angle(blackFish.right, whiteFish.right);
                blackFish.RotateAround(transform.position, Dir, sprintRotateSpeed * Time.deltaTime);
                yield return null;
            }
            black_idling = true;
        }
        else
        {
            white_idling = false;
            whiteAnim.SetFloat("swim_speed", sprintRotateSpeed / idleRotateSpeed);
            float angle = Vector2.Angle(blackFish.right, whiteFish.right);
            while (angle > 180 || angle < 175)// reached start point
            {
                angle = Vector2.Angle(blackFish.right, whiteFish.right);
                whiteFish.RotateAround(transform.position, Dir, sprintRotateSpeed * Time.deltaTime);
                yield return null;
            }
            white_idling = true;
        }
    }

    public IEnumerator IECloseSwim(bool close)
    {
        if (close)
        {
            float elapsedTime = 0f;
            whiteAnim.Play("close_swim_pre");
            blackAnim.Play("close_swim_pre");
            while (white_distanceToCenter > minMaxDistanceTocenter.x)
            {
                whiteFish.position -= whiteFish.up * swimToCenterSpeed * Time.deltaTime;
                blackFish.position -= blackFish.up * swimToCenterSpeed * Time.deltaTime;
                whiteFishGFX.localEulerAngles = Vector3.Lerp(new Vector3(0, 0, 0), new Vector3(0, 0, -25), elapsedTime / 0.5f);
                whiteFishGFX.localPosition = Vector3.Lerp(new Vector3(0, 0, 0), new Vector3(1, 0, 0), elapsedTime / 0.5f);
                blackFishGFX.localEulerAngles = Vector3.Lerp(new Vector3(0, 0, 0), new Vector3(0, 0, -25), elapsedTime / 0.5f);
                blackFishGFX.localPosition = Vector3.Lerp(new Vector3(0, 0, 0), new Vector3(1, 0, 0), elapsedTime / 0.5f);

                elapsedTime += Time.deltaTime;
                yield return null;
            }
        }
        else
        {
            float elapsedTime = 0f;
            float originalRotateSpeed = idleRotateSpeed;
            idleRotateSpeed *= 1.5f;
            whiteAnim.Play("white_idle");
            blackAnim.Play("black_idle");
            while (elapsedTime < 2f)
            {
                if (white_distanceToCenter < minMaxDistanceTocenter.y)
                {
                    whiteFish.position += whiteFish.up * swimToCenterSpeed * Time.deltaTime;
                    blackFish.position += blackFish.up * swimToCenterSpeed * Time.deltaTime;
                }
                whiteFishGFX.localEulerAngles = Vector3.Lerp(new Vector3(0, 0, -25), new Vector3(0, 0, 0), elapsedTime / 0.5f);
                whiteFishGFX.localPosition = Vector3.Lerp(new Vector3(1, 0, 0), new Vector3(0, 0, 0), elapsedTime / 2f);
                blackFishGFX.localEulerAngles = Vector3.Lerp(new Vector3(0, 0, -25), new Vector3(0, 0, 0), elapsedTime / 0.5f);
                blackFishGFX.localPosition = Vector3.Lerp(new Vector3(1, 0, 0), new Vector3(0, 0, 0), elapsedTime / 2f);

                elapsedTime += Time.deltaTime;
                yield return null;
            }
            idleRotateSpeed = originalRotateSpeed;
        }
    }

    /// <summary>
    /// compare black fish and white fish's X position
    /// </summary>
    /// <returns>the one closer to the start point </returns>
    public Transform CheckCloserFish()
    {
        if (blackFish.eulerAngles.z < whiteFish.eulerAngles.z) { return blackFish; }
        else { return whiteFish; }
    }

    public override int Damage(int damageAmount, Transform sender, float stunDuration = 0)
    {
        if (DEAD) { return 0; }

        flash.OnDamageFlash();
        currentHealth -= damageAmount;
        healthBar.fillAmount = (float)currentHealth / (float)maxHealth;

        if (currentHealth <= 0)
        {
            //Invoke("Death", 3f);
            //DEAD = true;
            //rb.velocity = Vector3.zero;
            //rb.gravityScale = 0f;
            //rb.isKinematic = true;
            //GetComponent<BoxCollider2D>().enabled = false;
            //HealthUI.SetActive(false);
            //gameObject.layer = 0;
            //anim.Play("death");

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
            //Invoke("Death", 3f);
            //DEAD = true;
            //rb.velocity = Vector3.zero;
            //rb.gravityScale = 0f;
            //rb.isKinematic = true;
            //GetComponent<BoxCollider2D>().enabled = false;
            //HealthUI.SetActive(false);
            //gameObject.layer = 0;
            //anim.Play("death");
            StartCoroutine(Pre_SecondPhase());
        }//death

        return 0;
    }

    public Quaternion CalculateWantedRotation(Vector3 _targetPos)
    {
        float angle = Mathf.Atan2(_targetPos.y - transform.position.y, _targetPos.x - transform.position.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(new Vector3(0, 0, angle));
        return targetRotation;
    }

    public void CancelAllAction()
    {
        if (co_IEcloseSwim != null) StopCoroutine(co_IEcloseSwim);
        if (co_sprintBackEqual != null) StopCoroutine(co_sprintBackEqual);
        if (co_sprintStartPoint != null) StopCoroutine(co_sprintStartPoint);

        StopCoroutine(co_act);
        waterSpear.CancelAct();
        swing.CancelAct();
        splash.CancelAct();
        dive.CancelAct();

        print("cancel all actions");

        white_idling = true;
        black_idling = true;
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

            EventInteract.SetActive(false);
            idleRotateSpeed *= 3;
            co_IEcloseSwim = StartCoroutine(IECloseSwim(false));
            StartCoroutine(ChangeYPos(true));
        }
    }

    public IEnumerator ChangeYPos(bool up)
    {
        if (up)
        {
            while (transform.position.y < waterLevel.position.y + 4.5) { transform.position += new Vector3(0, 1, 0) * Time.deltaTime * 3; yield return null; }
        }
        else
        {
            while (transform.position.y > waterLevel.position.y + 2) { transform.position -= new Vector3(0, 1, 0) * Time.deltaTime * 3; yield return null; }
        }
    }
}